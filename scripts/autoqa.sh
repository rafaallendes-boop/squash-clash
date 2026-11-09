#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"
if [[ "${1:-}" != mvp ]]; then printf 'Uso: scripts/autoqa.sh mvp [--scenario ID] [--repetition 1|2|3]\n' >&2; exit 4; fi
shift
export AUTOQA_SCENARIO='' AUTOQA_REPETITION=''
while [[ $# -gt 0 ]]; do
    case "$1" in
        --scenario) [[ $# -ge 2 ]] || exit 4; export AUTOQA_SCENARIO="$2"; shift 2;;
        --repetition) [[ $# -ge 2 && "$2" =~ ^[123]$ ]] || exit 4; export AUTOQA_REPETITION="$2"; shift 2;;
        *) printf 'Argumento desconocido: %s\n' "$1" >&2; exit 4;;
    esac
done
export AUTOQA_GIT_HASH="$(git rev-parse HEAD)"
export AUTOQA_DIRTY=false
if [[ -n "$(git status --porcelain)" ]]; then export AUTOQA_DIRTY=true; fi
export AUTOQA_COMMAND="scripts/autoqa.sh mvp${AUTOQA_SCENARIO:+ --scenario $AUTOQA_SCENARIO}${AUTOQA_REPETITION:+ --repetition $AUTOQA_REPETITION}"
export AUTOQA_RUN_DIR="$ROOT/Logs/autoqa/$(date -u +%Y%m%dT%H%M%SZ)-$(git rev-parse --short HEAD)-$RANDOM"
mkdir -p "$AUTOQA_RUN_DIR"
export AUTOQA_POC_DIR="$AUTOQA_RUN_DIR/poc" AUTOQA_DRIVER_DIR="$AUTOQA_RUN_DIR/driver"
UNITY=''
env_status=0
source "$ROOT/scripts/unity-env.sh" > "$AUTOQA_RUN_DIR/environment.log" 2>&1 || env_status=$?
printf 'RUN_DIR=%s\n' "$AUTOQA_RUN_DIR"
python3 - "$ROOT" "$AUTOQA_RUN_DIR" "$UNITY" "$env_status" <<'PY'
import base64,copy,datetime,hashlib,json,os,re,subprocess,sys,time
from pathlib import Path
from xml.etree import ElementTree as ET
root,run=map(Path,sys.argv[1:3]);unity=sys.argv[3];start=time.time_ns()
now=lambda:datetime.datetime.now(datetime.timezone.utc).isoformat()
started=now();xml=run/'tests.xml';log=run/'unity.log';summary=None;guard_failed=False;harness_state=None;harness_loaded=False
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
# Snapshot both index/worktree status before Unity. A pre-existing change is never checked out.
transient_paths=['ProjectSettings/GraphicsSettings.asset','ProjectSettings/QualitySettings.asset']
def git_status(*paths):
    return subprocess.check_output(['git','status','--porcelain','-z','--',*paths],cwd=root)
status_before=git_status()
transients=[]
for name in transient_paths:
    path=root/name;prior=git_status(name)
    transients.append({'path':name,'cleanBefore':not prior,'statusBefore':prior.decode(errors='backslashreplace'),
                       'sha256Before':sha(path) if path.is_file() else None})
def restore_transients():
    errors=[]
    for item in transients:
        item['restoreAttempted']=item['cleanBefore']
        if item['cleanBefore']:
            result=subprocess.run(['git','checkout','--',item['path']],cwd=root,capture_output=True,text=True)
            item['restoreExitCode']=result.returncode
            if result.returncode:errors.append(item['path']+': '+result.stderr.strip())
        else:item['restoreExitCode']=None
        path=root/item['path'];item['statusAfter']=git_status(item['path']).decode(errors='backslashreplace')
        item['sha256After']=sha(path) if path.is_file() else None
    after=git_status()
    record={'schema':'autoqa-restoration/1','files':transients,
            'gitStatusBeforeBase64':base64.b64encode(status_before).decode(),
            'gitStatusAfterBase64':base64.b64encode(after).decode(),
            'statusUnchanged':status_before==after,'errors':errors}
    (run/'restoration.json').write_text(json.dumps(record,indent=2)+'\n')
    return record
def write_summary(s):
    (run/'summary.json').write_text(json.dumps(s,indent=2)+'\n')
def readable_summary(value):
    return isinstance(value,dict) and value.get('schema')=='autoqa-summary/1'
def load_harness(path):
    try:
        value=json.loads(path.read_text())
        return value if readable_summary(value) else None
    except (OSError,ValueError,TypeError):
        return None
def failure(kind,message,integrity=False):
    global harness_state,harness_loaded
    existing=run/'summary.json';harness=run/'summary.harness.json';harness_report=run/'report.harness.md'
    # Capture the original once, including when the exception happened before summary parsing.
    if not harness_loaded:
        harness_state=load_harness(harness if harness.exists() else existing)
        harness_loaded=True
    previous=summary if readable_summary(summary) else None
    sources=[value for value in (harness_state,previous) if value is not None]
    if existing.is_file() and not harness.exists():
        existing.rename(harness)
        if (run/'report.md').is_file():(run/'report.md').rename(harness_report)
    known={key:None for key in ['hashesVerified','presetsUnchanged','guardPassed','evidenceSha256Ok']}
    canary=None;level=2;exit_candidates=[4]
    reasons=['guard-failed' if integrity else 'execution-error']
    if kind=='dirty-after-run':reasons.append('dirty-after-run')
    demonstrated_integrity=integrity
    for value in sources:
        flags=value.get('integrity')
        if isinstance(flags,dict):
            for key in known:
                flag=flags.get(key)
                # Only booleans are knowledge; a demonstrated False cannot become True/null.
                if type(flag) is bool and known[key] is not False:known[key]=flag
        signal=value.get('canary');observed=signal.get('observed') if isinstance(signal,dict) else None
        if observed in ['PASS','FAIL','INCONCLUSIVE','NOT_EVALUATED']:
            if canary is None or observed=='PASS':canary=observed
            if observed=='PASS' or (observed!='FAIL' and value.get('gate')!='ERROR' and value.get('executionError') is None):
                demonstrated_integrity=True
                reasons.append('canary-not-failed')
        declared_level=value.get('escalationLevel');declared_exit=value.get('exitCode')
        if type(declared_level) is int and 1<=declared_level<=3:
            level=max(level,declared_level)
            if declared_level==3:
                demonstrated_integrity=True;reasons.append('known-level-3')
        if type(declared_exit) is int and declared_exit in [0,1,2,3,4]:exit_candidates.append(declared_exit)
        prior_reasons=value.get('escalationReasons')
        if isinstance(prior_reasons,list):reasons.extend(item for item in prior_reasons if isinstance(item,str))
    if integrity:known['guardPassed']=False
    for key,flag in known.items():
        if flag is False:
            demonstrated_integrity=True;reasons.append('integrity-'+key+'-failed')
    if demonstrated_integrity:level=3;exit_candidates.append(3)
    final_exit=max(exit_candidates,key={0:0,2:1,1:2,4:3,3:4}.__getitem__)
    if final_exit==3:level=3
    previous_error=previous.get('executionError') if previous is not None else None
    if previous_error is None and harness_state is not None:previous_error=harness_state.get('executionError')
    error={'kind':kind,'message':message}
    if previous_error is not None:error['previous']=copy.deepcopy(previous_error)
    s={'schema':'autoqa-summary/1','runId':run.name,'gitHash':os.environ['AUTOQA_GIT_HASH'],
       'dirty':os.environ['AUTOQA_DIRTY']=='true','command':os.environ['AUTOQA_COMMAND'],'startedAt':started,'finishedAt':now(),
       'gate':'FAIL' if integrity else 'ERROR','counts':{k:0 for k in ['PASS','FAIL','INCONCLUSIVE','NOT_EVALUATED']},
       'canary':{'expectedFail':True,'observed':canary},'determinism':{},'integrity':known,
       'executionError':error,'escalationLevel':level,'escalationReasons':list(dict.fromkeys(reasons)),'exitCode':final_exit,
       'harnessResults':None}
    if harness_state is not None:
        # Counts/determinism remain explicitly scoped to the archived harness, never to this ERROR.
        s['harnessResults']={'source':harness.name,'scope':'harness-only','gate':harness_state.get('gate'),
                             'counts':copy.deepcopy(harness_state.get('counts')) if isinstance(harness_state.get('counts'),dict) else None,
                             'determinism':copy.deepcopy(harness_state.get('determinism')) if isinstance(harness_state.get('determinism'),dict) else None}
    attempts=run/'attempts.jsonl'
    if not attempts.exists():attempts.write_text('')
    report=run/'report.md'
    report.write_text('# AutoQA MVP\n\ngate: '+s['gate']+'\nescalationLevel: '+str(level)+'\nexitCode: '+str(final_exit)+
                      '\n\n'+kind+': '+message+'\n\nIntegridad conocida: '+json.dumps(known,ensure_ascii=False)+
                      '\nCanario del arnés: '+json.dumps(canary)+'\nRazones: '+json.dumps(s['escalationReasons'],ensure_ascii=False)+
                      '\n\nLos recuentos y el determinismo del arnés, si existen, están separados en harnessResults y summary.harness.json; no son resultados del ERROR.\n'+
                      '\nNo verifica audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión.\n')
    s['artifacts']={'attemptsSha256':sha(attempts),'reportSha256':sha(report),'gameMetrics':[]}
    if harness.is_file():s['artifacts']['harnessSummary']={'path':harness.name,'sha256':sha(harness)}
    if harness_report.is_file():s['artifacts']['harnessReport']={'path':harness_report.name,'sha256':sha(harness_report)}
    write_summary(s);return s

try:
    if int(sys.argv[4]) or not unity:raise RuntimeError('environment: Unity environment unavailable; see environment.log')
    args=[unity,'-batchmode','-projectPath',str(root),'-runTests','-testPlatform','PlayMode','-assemblyNames','SquashClash.AutoQA','-testResults',str(xml),'-logFile',str(log)]
    with (run/'console.log').open('w') as console:
        proc=subprocess.run(args,stdout=console,stderr=subprocess.STDOUT,timeout=270)
    if not xml.is_file() or xml.stat().st_mtime_ns<=start:raise RuntimeError('missing-xml: No fresh Unity result')
    tree=ET.parse(xml).getroot();a=tree.attrib
    guard_failed=any('AutoQAGuardTests' in c.get('fullname','') and c.get('result')=='Failed' for c in tree.findall('.//test-case'))
    if int(a.get('total',0))<1 or int(a.get('failed',0)) or int(a.get('skipped',0)) or proc.returncode:
        raise RuntimeError('tests: Unity failure, zero tests or skipped tests; see tests.xml')
    if not log.is_file() or re.search(r'error CS\d+|Scripts have compiler errors',log.read_text(errors='replace')):raise RuntimeError('compilation: missing log or compiler errors')
    p=run/'summary.json'
    if not p.is_file() or p.stat().st_mtime_ns<=start:raise RuntimeError('missing-summary: No fresh summary')
    summary=json.loads(p.read_text())
    required={'schema','runId','gitHash','dirty','command','startedAt','finishedAt','gate','counts','canary','determinism','integrity','executionError','artifacts','escalationLevel','escalationReasons','exitCode'}
    if not required<=summary.keys() or summary['schema']!='autoqa-summary/1':raise RuntimeError('summary-schema: Missing fields')
    attempts=[json.loads(l) for l in (run/'attempts.jsonl').read_text().splitlines() if l]
    for item in attempts:
        if item['schema']!='autoqa-attempt/1':raise RuntimeError('attempt-schema: Wrong schema')
    pairs=[(run/'attempts.jsonl',summary['artifacts']['attemptsSha256']),(run/'report.md',summary['artifacts']['reportSha256'])]
    for item in summary['artifacts']['gameMetrics']:
        p=(run/item['path']).resolve()
        if run.resolve() not in p.parents:raise RuntimeError('artifact-path: Out-of-run evidence')
        pairs.append((p,item['sha256']))
    if any(not p.is_file() or sha(p)!=expected for p,expected in pairs):
        summary['integrity']['evidenceSha256Ok']=False;summary['gate']='FAIL';summary['exitCode']=3;summary['escalationLevel']=3
        summary['escalationReasons'].append('evidence-sha256-mismatch');write_summary(summary)
    if summary['exitCode']==0 and (summary['counts']!={'PASS':18,'FAIL':0,'INCONCLUSIVE':0,'NOT_EVALUATED':0} or summary['canary']['observed']!='FAIL'):
        raise RuntimeError('incomplete-run: PASS requires all 18 attempts and failed canary')
    print('TESTS='+json.dumps({k:int(a.get(k,0)) for k in ['total','passed','failed','skipped']}))
except subprocess.TimeoutExpired:
    summary=failure('environment','Unity timeout after 270 seconds')
except Exception as exc:
    message=str(exc);kind=message.split(':',1)[0] if ':' in message else 'execution'
    summary=failure(kind,message,guard_failed)
finally:
    try:
        restoration=restore_transients()
        if not restoration['statusUnchanged'] or restoration['errors']:
            summary=failure('dirty-after-run','Working tree status changed or restoration failed; see restoration.json')
    except Exception as exc:
        summary=failure('dirty-after-run','Could not verify restoration: '+str(exc))
    if summary is None:summary=failure('execution','Execution ended without a summary')
    if (run/'restoration.json').is_file():
        summary['artifacts']['restoration']={'path':'restoration.json','sha256':sha(run/'restoration.json')}
    write_summary(summary)
print('RESULT='+summary['gate']);print('ESCALATION_LEVEL='+str(summary['escalationLevel']))
sys.exit(int(summary['exitCode']))
PY
