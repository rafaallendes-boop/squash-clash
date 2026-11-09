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
import base64,datetime,hashlib,json,os,re,subprocess,sys,time
from pathlib import Path
from xml.etree import ElementTree as ET
root,run=map(Path,sys.argv[1:3]);unity=sys.argv[3];start=time.time_ns()
now=lambda:datetime.datetime.now(datetime.timezone.utc).isoformat()
started=now();xml=run/'tests.xml';log=run/'unity.log';summary=None;guard_failed=False
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
def failure(kind,message,integrity=False):
    # Preserve the harness output before producing a wrapper ERROR summary.
    existing=run/'summary.json';harness=run/'summary.harness.json';harness_report=run/'report.harness.md'
    if existing.is_file() and not harness.exists():
        existing.rename(harness)
        if (run/'report.md').is_file():(run/'report.md').rename(harness_report)
    # Unknown checks stay null if the execution never reached them; they are not asserted true.
    s={'schema':'autoqa-summary/1','runId':run.name,'gitHash':os.environ['AUTOQA_GIT_HASH'],
       'dirty':os.environ['AUTOQA_DIRTY']=='true','command':os.environ['AUTOQA_COMMAND'],'startedAt':started,'finishedAt':now(),
       'gate':'FAIL' if integrity else 'ERROR','counts':{k:0 for k in ['PASS','FAIL','INCONCLUSIVE','NOT_EVALUATED']},
       'canary':{'expectedFail':True,'observed':None},'determinism':{},
       'integrity':{k:None for k in ['hashesVerified','presetsUnchanged','guardPassed','evidenceSha256Ok']},
       'executionError':{'kind':kind,'message':message},'escalationLevel':3 if integrity else 2,
       'escalationReasons':['guard-failed' if integrity else 'execution-error'],'exitCode':3 if integrity else 4}
    if integrity:s['integrity']['guardPassed']=False
    attempts=run/'attempts.jsonl'
    if not attempts.exists():attempts.write_text('')
    report=run/'report.md';report.write_text('# AutoQA MVP\n\ngate: '+s['gate']+'\n\n'+kind+': '+message+'\n\nNo verifica audibilidad, háptica percibida, lectura visual, latencia toque→imagen, sensación ni diversión.\n')
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
            previous_error=summary.get('executionError') if summary else None
            previous_level=summary.get('escalationLevel',2) if summary else 2
            summary=failure('dirty-after-run','Working tree status changed or restoration failed; see restoration.json')
            summary['escalationLevel']=max(previous_level,2)
            if previous_error:summary['executionError']['previous']=previous_error
            summary['escalationReasons'].append('dirty-after-run')
    except Exception as exc:
        summary=failure('dirty-after-run','Could not verify restoration: '+str(exc))
    if summary is None:summary=failure('execution','Execution ended without a summary')
    if (run/'restoration.json').is_file():
        summary['artifacts']['restoration']={'path':'restoration.json','sha256':sha(run/'restoration.json')}
    write_summary(summary)
print('RESULT='+summary['gate']);print('ESCALATION_LEVEL='+str(summary['escalationLevel']))
sys.exit(int(summary['exitCode']))
PY
