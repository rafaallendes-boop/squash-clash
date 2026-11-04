# Reproducción de la evaluación offline de la candidata

Método predeclarado en **140d0ca**, anterior al cálculo. [Informe](../reviews/2026-11-04-first-shot-sl56-lowering-candidate.md), [método](2026-11-04-sl56-lowering-rule-method.md). Dos ejecuciones completas byte a byte idénticas. No se modificó ShotSolver/P2 ni se ejecutó una build de producto, Editor, suite o iPhone.

## Entradas, compilación y salidas

Requiere las fuentes originales del head de producto9131b1c, Unity6000.3.27f1 ya instalado y las posiciones/TSV del [mapa aprobado y su reproducción](2026-11-04-sl56-feasibility-reproduction.md). La tabla positions.tsv se conserva idéntica, y se puede reconstruir desde contactos/position del JSON del mapa, sin usar iPhone. Para cotejar baseline se conserva el TSV aprobado o se reproduce primero con aquel arnés. No se cambia la fuente del producto para compilar este ejecutable diagnóstico.

Compilar en un directorio ignorado nuevo. Usar mono y Roslyn/csc.exe de MonoBleedingEdge de esa versión deUnity, referencias UnityEngine.CoreModule.dll y Facades/netstandard.dll; opciones `-langversion:latest -target:exe -optimize+`. Incluir el arnés de abajo y estos fuentes originales:

- `Assets/SquashClash/Core/Simulation/AnalyticBallSimulation.cs`
- `Assets/SquashClash/Core/Simulation/Ballistics.cs`
- `Assets/SquashClash/Core/Simulation/BallState.cs`
- `Assets/SquashClash/Core/Simulation/PhysicsParams.cs`
- `Assets/SquashClash/Core/Simulation/SurfaceMaterial.cs`
- `Assets/SquashClash/Core/Simulation/IBallSimulation.cs`
- `Assets/SquashClash/Core/Simulation/ContactEvent.cs`
- `Assets/SquashClash/Core/Court/CourtDimensions.cs`
- `Assets/SquashClash/Core/Court/SurfaceId.cs`
- `Assets/SquashClash/Core/Court/CourtLines.cs`
- `Assets/SquashClash/Core/Court/LineBand.cs`
- `Assets/SquashClash/Core/Court/LineId.cs`
- `Assets/SquashClash/Core/Court/LineContactClassifier.cs`
- `Assets/SquashClash/Core/Shot/ShotSolver.cs`
- `Assets/SquashClash/Core/Simulation/AnalyticBallSimulationFactory.cs`

Ejecutar (rutas al compilador y UnityEngine.CoreModule según instalación indicada):

```sh
MONO_PATH="$unity_scripting/Managed/UnityEngine" "$unity_scripting/MonoBleedingEdge/bin/mono" LoweringEvaluation.exe positions.tsv run1
MONO_PATH="$unity_scripting/Managed/UnityEngine" "$unity_scripting/MonoBleedingEdge/bin/mono" LoweringEvaluation.exe positions.tsv run2
```

`unity_scripting` es la carpeta Contents/Resources/Scripting del Editor indicado; no se abre el Editor. Los argumentos son exclusivamente archivos locales. Cada contacto tiene caché propio por bitsfloat32(x,y,P). `evaluations.tsv` conserva cada evaluación única con su id local; `attempts.tsv` enlaza cada intento/candidato con esa evaluación; `results.tsv` incluye solicitud/brazo/estado/final/último candidato/contador y métricas; `baseline.tsv` reproduce exactamente el formato anterior; `cache.tsv` conserva conteos. No se versionan losTSV completos. No-solution no tiene solicitud/resultado final ni datos físicos ficticios.

SHA-256 de las salidas deambas ejecuciones:

| Archivo | SHA-256 | Bytes |
| --- | --- | --- |
| attempts.tsv | b3136e422c2f7d228e6487212ed0be0b673f95f12a2b2ab518bdbbdc1e1cda77 | 8371279 |
| baseline.tsv | 913551248ac43ad9e029dfbcd5d6551f9cf19cf9775faeb0e7b3a2aaaba69b43 | 26058592 |
| cache.tsv | 42fe6a17bdfed03a0216e270b6fb3270e8235ad53fa179aedfa44884a1514f85 | 1176 |
| evaluations.tsv | 1fcd95975250fe934231ae7a5931b8ad6c1e3634967affec788e8901b2a66494 | 75124793 |
| results.tsv | 3cf1962487dbdd63cc8103c981fcf174c7f7406243a4deed0fc8a3b21ec6f504 | 90875053 |

SHA-256 arnés: `be9c9ed0c54686fd535675f269c7177f7d695fa952ad7e266820f7110d6945e0`; posiciones: `68ccf9fc4f1733a9199b3430f45a6623658977723e44941a8e3474ccc3fb82c1`; método en el commit: `61a8ba87153d6a37a299ba135891c9f391cdd09aa7cbcdb1df50b07e8d40398b`.

## Arnés C# exacto ejecutado

```csharp
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Shot;
public static class LoweringEvaluation {
 static readonly CultureInfo C=CultureInfo.InvariantCulture;
 static readonly float[] X={.02f,.35f,1.3f,2.6f,3.2f,3.8f,5.1f,6.05f,6.38f};
 static readonly float[] Y={.5f,.7f,.9f,1.2f,1.5f,1.8f,2.2f,2.8f,3.5f,4f,4.55f};
 static string R(double n)=>double.IsNaN(n)?"NA":n.ToString("R",C);
 static bool OutBand(LineId id)=>id==LineId.FrontOut||id==LineId.BackOut||id==LineId.LeftSideOut||id==LineId.RightSideOut;
 static bool FiniteWall(ContactEvent h,CourtDimensions court) =>
  (h.Surface==SurfaceId.FrontWall||h.Surface==SurfaceId.BackWall)?h.Point.x>=0&&h.Point.x<=court.Width:
  (h.Surface==SurfaceId.LeftWall||h.Surface==SurfaceId.RightWall)?h.Point.z>=0&&h.Point.z<=court.Length:true;
 static bool IsOut(ContactEvent h,IReadOnlyList<LineBand> bands) {
  foreach(var b in bands)if(b.Surface==h.Surface&&OutBand(b.Id)) {
   var e=b.B-b.A;float len=e.magnitude;float along=Vector2.Dot(h.UV-b.A,e/len);
   if((along>=0&&along<=len&&Vector2.Dot(h.UV-b.A,b.Growth)>=0)||LineContactClassifier.Touches(in b,h.UV,.01f))return true;
  }
  return false;
 }
 static bool IsTin(ContactEvent h,CourtDimensions court,IReadOnlyList<LineBand> bands) {
  if(h.Surface!=SurfaceId.FrontWall||!FiniteWall(h,court))return false;
  if(h.Point.y<=court.TinTop)return true;
  foreach(var b in bands)if(b.Id==LineId.Tin&&LineContactClassifier.Touches(in b,h.UV,.01f))return true;
  return false;
 }

 static readonly double[] DecimalY={.50,.70,.90,1.20,1.50,1.80,2.20,2.80,3.50,4.00,4.55};
 static readonly PhysicsParams physics=new PhysicsParams {Gravity=9.81f,StepHz=120,MaxImpactsPerStep=4,BallRadius=.020f,BallMass=.024f,
 Floor=new SurfaceMaterial{Restitution=.45f,TangentialFriction=.30f},FrontWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},
 SideWalls=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},BackWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f}};
 static readonly CourtDimensions court=CourtDimensions.Standard();
 static readonly IReadOnlyList<LineBand> bands=CourtLines.Build(court);
 static readonly AnalyticBallSimulationFactory factory=new AnalyticBallSimulationFactory(.1f);
 static readonly List<ContactEvent> hits=new List<ContactEvent>(4);
 static object[] Evaluate(Vector3 pos,float x,float y,float power) {
     var solution=ShotSolver.Solve(new ShotRequest(pos,new Vector2(x,y),power),6f,28f,32f,physics.Gravity,physics.BallRadius,court,factory,physics);
     if(factory.LiveInstances!=0)throw new InvalidOperationException("Solver leaked an instance.");
     var row=new object[38];row[4]=solution.Solved;row[5]=solution.Reason;
     row[6]=solution.SpeedRaised;row[7]=solution.TargetLowered;row[8]=R(6f+power*22f);row[9]=R(solution.Speed);row[10]=R(solution.TargetUsed.x);row[11]=R(solution.TargetUsed.y);
     string firstSurface="none",outSurface="none",stop=solution.Solved?"horizon-10s":"unsolved";
     double first=double.NaN,frontX=double.NaN,frontY=double.NaN,floor=double.NaN,fx=double.NaN,fz=double.NaN;
     double outT=double.PositiveInfinity,tinT=double.PositiveInfinity,escapeT=double.PositiveInfinity,rear=double.PositiveInfinity,returnRear=double.PositiveInfinity,back=double.PositiveInfinity;
     int sideAll=0,sideBefore=0,planeOutsideBefore=0,sideOutsideBefore=0;long cap=0;bool inside=false;
     if(solution.Solved)using(var sim=factory.Create(physics,court)) {
      sim.Reset(new BallState{Position=pos,Velocity=solution.Velocity,Step=0});int floors=0;
      if(pos.z>=court.Length/2f)rear=0;
      bool hadFront=false;
      for(int step=1;step<=1200;step++) {
       sim.Step(hits);
       foreach(var h in hits) {
        if(h.Persistent)continue;
        double time=(h.Step-1)/120.0+h.TimeInStep;
        if(double.IsNaN(first)){first=time;firstSurface=h.Surface.ToString();}
        if(h.Surface==SurfaceId.FrontWall&&!hadFront){hadFront=true;frontX=h.Point.x;frontY=h.Point.y;}
        if(IsOut(h,bands)&&time<outT){outT=time;outSurface=h.Surface.ToString();}
        if(IsTin(h,court,bands)&&time<tinT)tinT=time;
        if(h.Surface==SurfaceId.Floor) {
         floors++;
         if(double.IsNaN(floor)){floor=time;fx=h.Point.x;fz=h.Point.z;inside=fx>=0&&fx<=court.Width&&fz>=0&&fz<=court.Length;}
        }
        if(double.IsNaN(floor)||time<=floor) {
         if(h.Point.z>=court.Length/2f){rear=Math.Min(rear,time);if(hadFront)returnRear=Math.Min(returnRear,time);}
         if(h.Surface==SurfaceId.BackWall&&FiniteWall(h,court))back=Math.Min(back,time);
         if(h.Surface==SurfaceId.LeftWall||h.Surface==SurfaceId.RightWall){sideBefore++;if(!FiniteWall(h,court))sideOutsideBefore++;}
         if(h.Surface!=SurfaceId.Floor&&!FiniteWall(h,court))planeOutsideBefore++;
        }
        if(h.Surface==SurfaceId.LeftWall||h.Surface==SurfaceId.RightWall)sideAll++;
       }
       var s=sim.State;double end=s.Step/120.0;
       if(s.Position.y>court.WallHeight||s.Position.x<0||s.Position.x>court.Width||s.Position.z<0||s.Position.z>court.Length)escapeT=Math.Min(escapeT,end);
       if(double.IsNaN(floor)&&s.Position.z>=court.Length/2f) {
        rear=Math.Min(rear,end);if(hadFront)returnRear=Math.Min(returnRear,end);
       }
       if(floors>=2){stop="second-floor-step-complete";break;}
      }
      cap=((AnalyticBallSimulation)sim).ImpactCapReachedCount;
     }
     bool hasFloor=!double.IsNaN(floor),outBefore=hasFloor&&outT<=floor,tinBefore=hasFloor&&tinT<=floor,escapeBefore=hasFloor&&escapeT<=floor;
     bool inCourt=solution.Solved&&firstSurface=="FrontWall"&&hasFloor&&inside&&!outBefore&&!tinBefore&&!escapeBefore;
     row[12]=firstSurface;row[13]=R(first);row[14]=R(frontX);row[15]=R(frontY);row[16]=R(floor);row[17]=R(fx);row[18]=R(fz);row[19]=inside;
     row[20]=outBefore;row[21]=double.IsInfinity(outT)?"NA":R(outT);row[22]=outSurface;row[23]=tinBefore;row[24]=double.IsInfinity(tinT)?"NA":R(tinT);
     row[25]=escapeBefore;row[26]=double.IsInfinity(escapeT)?"NA":R(escapeT);row[27]=!double.IsInfinity(escapeT);
     row[28]=hasFloor&&rear<=floor;row[29]=hasFloor&&returnRear<=floor;row[30]=hasFloor&&back<=floor;row[31]=sideBefore;row[32]=sideAll;row[33]=planeOutsideBefore;row[34]=cap;row[35]=stop;row[36]=inCourt;row[37]=sideOutsideBefore;

 return row;
 }
 static readonly Dictionary<string,Entry> cache=new Dictionary<string,Entry>();
 sealed class Entry {public int Id;public object[] Row;}
 static StreamWriter trace,attempts;static string contact;static Vector3 position;static long totalHits,totalEvaluations;
 static Entry Get(float x,float y,float p) {
  string key=BitConverter.SingleToInt32Bits(x)+","+BitConverter.SingleToInt32Bits(y)+","+BitConverter.SingleToInt32Bits(p);
  if(cache.TryGetValue(key,out var found)){totalHits++;return found;}
  var e=new Entry{Id=cache.Count+1,Row=Evaluate(position,x,y,p)};cache.Add(key,e);totalEvaluations++;
  var a=new object[5+34];a[0]=contact;a[1]=e.Id;a[2]=R(x);a[3]=R(y);a[4]=R(p);Array.Copy(e.Row,4,a,5,34);
  trace.WriteLine(string.Join("\t",a));return e;
 }
 static void WriteResult(StreamWriter w,int ix,int iy,int ip,string arm,string status,int tried,Entry e,float? y,float? p,float? lastY,float? lastP) {
  var a=new object[12+34];a[0]=contact;a[1]=ix;a[2]=iy;a[3]=ip;a[4]=arm;a[5]=status;a[6]=tried;
  a[7]=e==null?"NA":e.Id.ToString(C);a[8]=y.HasValue?R(y.Value):"NA";a[9]=p.HasValue?R(p.Value):"NA";
  a[10]=lastY.HasValue?R(lastY.Value):"NA";a[11]=lastP.HasValue?R(lastP.Value):"NA";
  if(e!=null)Array.Copy(e.Row,4,a,12,34);else for(int j=12;j<a.Length;j++)a[j]="NA";
  w.WriteLine(string.Join("\t",a));
 }
 static void Attempt(int ix,int iy,int ip,string arm,int n,Entry e) {
  attempts.WriteLine(contact+"\t"+ix+"\t"+iy+"\t"+ip+"\t"+arm+"\t"+n+"\t"+e.Id);
 }
 public static int Main(string[] args) {
  Directory.CreateDirectory(args[1]);
  string metrics="id\tix\tiy\tip\tsolved\treason\traised\tlowered\trequestedSpeed\tspeed\tusedX\tusedY\tfirstSurface\tfirstT\tfrontX\tfrontY\tfloorT\tfloorX\tfloorZ\tfloorInside\toutBefore\toutT\toutSurface\ttinBefore\ttinT\tescapeBefore\tescapeT\tescapeAny\tpassedRearBefore\tpassedRearReturn\tbackBefore\tsideBefore\tsideAny\tplaneOutsideBefore\tcapCount\tstopReason\tinCourt\tsideOutsideBefore".Substring("id\tix\tiy\tip\t".Length);
  using(var b=new StreamWriter(Path.Combine(args[1],"baseline.tsv"),false,new System.Text.UTF8Encoding(false)))
  using(var w=new StreamWriter(Path.Combine(args[1],"results.tsv"),false,new System.Text.UTF8Encoding(false)))
  using(trace=new StreamWriter(Path.Combine(args[1],"evaluations.tsv"),false,new System.Text.UTF8Encoding(false)))
  using(attempts=new StreamWriter(Path.Combine(args[1],"attempts.tsv"),false,new System.Text.UTF8Encoding(false)))
  using(var stats=new StreamWriter(Path.Combine(args[1],"cache.tsv"),false,new System.Text.UTF8Encoding(false))) {
   b.WriteLine("id\tix\tiy\tip\tsolved\treason\traised\tlowered\trequestedSpeed\tspeed\tusedX\tusedY\tfirstSurface\tfirstT\tfrontX\tfrontY\tfloorT\tfloorX\tfloorZ\tfloorInside\toutBefore\toutT\toutSurface\ttinBefore\ttinT\tescapeBefore\tescapeT\tescapeAny\tpassedRearBefore\tpassedRearReturn\tbackBefore\tsideBefore\tsideAny\tplaneOutsideBefore\tcapCount\tstopReason\tinCourt\tsideOutsideBefore");w.WriteLine("id\tix\tiy\tip\tarm\tstatus\ttried\tevalId\taskedY\taskedP\tlastY\tlastP\t"+metrics);
   trace.WriteLine("id\tevalId\tx\ty\tp\t"+metrics);attempts.WriteLine("id\tix\tiy\tip\tarm\ttrial\tevalId");stats.WriteLine("id\tuniqueEvaluations\tcacheHits");
   var lines=File.ReadAllLines(args[0]);
   double limit=(double)court.TinTop+(double).01f;
   float endpoint=(float)limit;if(endpoint<=limit)endpoint=BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(endpoint)+1);
   for(int c=1;c<lines.Length;c++) {
    if(string.IsNullOrWhiteSpace(lines[c]))continue;
    var input=lines[c].Split('\t');contact=input[0];position=new Vector3(float.Parse(input[1],C),float.Parse(input[2],C),float.Parse(input[3],C));
    cache.Clear();long previousHits=totalHits;
    for(int ix=0;ix<X.Length;ix++)for(int iy=0;iy<Y.Length;iy++)for(int ip=0;ip<11;ip++) {
     float x=X[ix],y=Y[iy],p=ip/10f;var original=Get(x,y,p);
     var baseRow=(object[])original.Row.Clone();baseRow[0]=contact;baseRow[1]=ix;baseRow[2]=iy;baseRow[3]=ip;b.WriteLine(string.Join("\t",baseRow));
     bool preserve=(bool)original.Row[36];WriteResult(w,ix,iy,ip,"base","base",0,original,y,p,null,null);
     if(preserve) {WriteResult(w,ix,iy,ip,"Y","preserved",0,original,y,p,null,null);WriteResult(w,ix,iy,ip,"P","preserved",0,original,y,p,null,null);continue;}
     Entry chosen=null;float? last=null;int n=0;var seen=new HashSet<int>();
     for(int k=1;;k++) {
      double candidate=DecimalY[iy]-k*.05;if(candidate<=limit)break;float yf=(float)candidate;if(yf<=limit)continue;
      if(!seen.Add(BitConverter.SingleToInt32Bits(yf)))continue;
      last=yf;n++;var e=Get(x,yf,p);Attempt(ix,iy,ip,"Y",n,e);if((bool)e.Row[36]){chosen=e;break;}
     }
     if(chosen==null&&endpoint<y&&seen.Add(BitConverter.SingleToInt32Bits(endpoint))) {
      last=endpoint;n++;var e=Get(x,endpoint,p);Attempt(ix,iy,ip,"Y",n,e);if((bool)e.Row[36])chosen=e;
     }
     WriteResult(w,ix,iy,ip,"Y",chosen==null?"no-solution":"changed",n,chosen,chosen==null?(float?)null:last,chosen==null?(float?)null:p,last,last.HasValue?(float?)p:null);
     chosen=null;last=null;n=0;
     for(int j=1;j<=2*ip;j++) {
      float pf=(2*ip-j)/20f;last=pf;n++;var e=Get(x,y,pf);Attempt(ix,iy,ip,"P",n,e);if((bool)e.Row[36]){chosen=e;break;}
     }
     WriteResult(w,ix,iy,ip,"P",chosen==null?"no-solution":"changed",n,chosen,chosen==null?(float?)null:y,chosen==null?(float?)null:last,last.HasValue?(float?)y:null,last);
    }
    stats.WriteLine(contact+"\t"+cache.Count+"\t"+(totalHits-previousHits));w.Flush();b.Flush();trace.Flush();attempts.Flush();stats.Flush();
    Console.WriteLine("Completed "+contact+" "+c+"/"+(lines.Length-1)+" unique="+cache.Count);
   }
  }
  if(factory.LiveInstances!=0)throw new InvalidOperationException("Outstanding instances.");
  Console.WriteLine("Unique="+totalEvaluations+" cacheHits="+totalHits);return 0;
 }
}
```

## Agregación exacta de los derivados

Python estándar, desde raíz del worktree; entradas completas solo enLogs/lowering-evaluation-20261104, el mapa aprobado en docs. Estos scripts se incluyen como documentación de reproducción; no son producto ni tests. El segundo agrega auditoría y todos los casos sin solución al resumen del primero. El reporte usa esosJSON; redondea tablas a9cifras significativas sin alterar los datos.

```python
import csv,json,math,hashlib,collections,itertools
from pathlib import Path
root=Path.cwd(); out=root/'Logs/lowering-evaluation-20261104'; run=out/'run1'; dest=root/'docs/validation/evidence'; fmap=json.loads((dest/'2026-11-04-sl56-feasibility-summary.json').read_text()); contacts={v['id']:v for v in fmap['contacts']}; grid=fmap['grid'];
print('grid',grid)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dist(values):
 a=sorted(v for v in values if v is not None and math.isfinite(v));n=len(a)
 return {'n':n,'min':a[0] if n else None,**{k:a[max(0,math.ceil(p*n)-1)] if n else None for k,p in [('p5',.05),('p50',.5),('p95',.95),('p99',.99)]},'max':a[-1] if n else None}
def num(v):return None if v=='NA' else float(v)
def truth(v):return v=='True'
rows=[];bykey={}
for r in csv.DictReader((run/'results.tsv').open(),delimiter='\t'):
 for k in ['ix','iy','ip','tried']:r[k]=int(r[k])
 for k in ['askedY','askedP','lastY','lastP','requestedSpeed','speed','usedX','usedY','firstT','frontX','frontY','floorT','floorX','floorZ','outT','tinT','escapeT']:r[k]=num(r[k])
 for k in ['solved','raised','lowered','floorInside','outBefore','tinBefore','escapeBefore','escapeAny','passedRearBefore','passedRearReturn','backBefore','inCourt']:r[k]=truth(r[k])
 for k in ['sideBefore','sideAny','sideOutsideBefore','planeOutsideBefore','capCount']:r[k]=None if r[k]=='NA' else int(r[k])
 r['group']=contacts[r['id']]['group'];r['hasResult']=r['solved']
 r['originalY']=grid['y'][r['iy']] if 'y' in grid else None
 rows.append(r);bykey[r['id'],r['ix'],r['iy'],r['ip'],r['arm']]=r
# Inputs are serialized under lowercase x/y/p in approved map.
X=grid['x'];Y=grid['y'];P=grid['p']
limit=0.47999998927116394+0.009999999776482582
for r in rows:
 b=bykey[r['id'],r['ix'],r['iy'],r['ip'],'base'];r['base']=b if r['arm']!='base' else None
 r['originalY']=b['askedY'];r['originalP']=b['askedP'];r['deltaY']=None if r['askedY'] is None else b['askedY']-r['askedY'];r['deltaP']=None if r['askedP'] is None else b['askedP']-r['askedP'];r['totalDeltaY']=None if not r['hasResult'] else b['askedY']-r['usedY'];r['targetTinClearance']=None if not r['hasResult'] else r['usedY']-limit;r['frontTinClearance']=None if r['frontY'] is None else r['frontY']-limit

def counts(a):
 a=list(a);avail=[r for r in a if r['hasResult']];good=[r for r in a if r['inCourt']]
 c={'n':len(a),'preserved':sum(r['status']=='preserved' for r in a),'searchActivated':sum(r['status'] in ['changed','no-solution'] for r in a),'changed':sum(r['status']=='changed' for r in a),'noSolution':sum(r['status']=='no-solution' for r in a),'hasResult':len(avail),'noTrajectory':len(a)-len(avail),'inCourt':len(good),'rearBounceInCourt':sum(r['floorZ']>=4.875 for r in good),'backBeforeInCourt':sum(r['backBefore'] for r in good),'passRearReturnInCourt':sum(r['passedRearReturn'] for r in good),'sideFiniteBeforeInCourt':sum(r['sideBefore']>r['sideOutsideBefore'] for r in good)}
 for name in ['outBefore','tinBefore','escapeBefore','escapeAny','backBefore','passedRearReturn','raised','lowered']:c[name]=sum(r[name] for r in avail)
 c.update(floorOutside=sum(r['floorT'] is not None and not r['floorInside'] for r in avail),sideFiniteBefore=sum(r['sideBefore']>r['sideOutsideBefore'] for r in avail),sideAny=sum(r['sideAny']>0 for r in avail),planeOutsideBefore=sum(r['planeOutsideBefore']>0 for r in avail),capReached=sum(r['capCount']>0 for r in avail),horizonCensored=sum(r['stopReason']=='horizon-10s' for r in avail),unSolved=sum(not r['solved'] for r in a if r['arm']=='base'))
 return c

def metrics(a):
 a=list(a);d={}
 for status in ['all','preserved','changed','no-solution']:
  section=[r for r in a if status=='all' or r['status']==status]
  d[status]={'n':len(section),'counts':counts(section),'distributions':{k:dist(r[k] for r in section) for k in ['deltaY','deltaP','totalDeltaY','targetTinClearance','frontTinClearance','floorZ','tried']}}
 return d
summary={'schema':'sl56-lowering-evaluation-v1','methodCommit':'140d0ca','inputs':{'positions':73,'focal':24,'control':49,'requestsPerContact':1089,'requests':79497,'arms':['base','Y','P'],'rows':len(rows),'grid':grid,'tinTopPlusFootprint':limit},'group':{},'byPower':{},'contacts':{},'drops':{},'columnC':{},'solverInteraction':{}}
for g in ['focal','control','all']:
 section=[r for r in rows if g=='all' or r['group']==g]
 summary['group'][g]={arm:metrics(r for r in section if r['arm']==arm) for arm in ['base','Y','P']}
 summary['byPower'][g]={str(ip):{arm:metrics(r for r in section if r['arm']==arm and r['ip']==ip) for arm in ['base','Y','P']} for ip in range(11)}
 summary['drops'][g]={arm:metrics(r for r in section if r['arm']==arm and r['iy']<=2 and r['ip']<=2) for arm in ['base','Y','P']}
 summary['columnC'][g]={str(ip):{arm:counts(r for r in section if r['arm']==arm and r['ip']==ip and r['ix']==4 and r['iy']==6) for arm in ['base','Y','P']} for ip in range(11)}
 summary['solverInteraction'][g]={}
 for arm in ['Y','P']:
  a=[r for r in section if r['arm']==arm];v=[r for r in a if r['hasResult']];ch=[r for r in v if r['status']=='changed']
  summary['solverInteraction'][g][arm]={'n':len(a),'withFinal':len(v),'changed':len(ch),'raisedTransitions':dict(collections.Counter(str(r['base']['raised'])+'→'+str(r['raised']) for r in v)),'loweredTransitions':dict(collections.Counter(str(r['base']['lowered'])+'→'+str(r['lowered']) for r in v)),'finalSpeedMinusBase':dist(r['speed']-r['base']['speed'] for r in ch if r['base']['solved']),'targetUsedDiffersFromRequest':sum(r['usedY']!=r['askedY'] for r in v),'changedWithExistingRaised':sum(r['raised'] for r in ch),'changedWithExistingLowered':sum(r['lowered'] for r in ch)}
for ident,c in contacts.items():
 a=[r for r in rows if r['id']==ident]
 summary['contacts'][ident]={'group':c['group'],'block':c['block'],'position':c['position'],'arms':{arm:counts(r for r in a if r['arm']==arm) for arm in ['base','Y','P']},'byPower':{str(ip):{arm:counts(r for r in a if r['arm']==arm and r['ip']==ip) for arm in ['base','Y','P']} for ip in range(11)}}
# Small stable row references; full requests/evaluations are reproducible ignored output.
def ref(r):
 return {k:r[k] for k in ['id','ix','iy','ip','status','askedY','askedP','usedY','deltaY','deltaP','totalDeltaY','floorX','floorZ','raised','lowered','targetTinClearance','frontTinClearance','evalId']}
near={g:{arm:{} for arm in ['base','Y','P']} for g in ['focal','control']}
for g in near:
 for arm in near[g]:
  for category in ['all','highPower','p8','p9','p10']:
   a=[r for r in rows if r['group']==g and r['arm']==arm and r['hasResult'] and (category=='all' or category=='highPower' and r['ip']>=8 or category.startswith('p') and r['ip']==int(category[1:]))]
   near[g][arm][category]={'n':len(a),'targetTinClearance':dist(r['targetTinClearance'] for r in a),'frontTinClearance':dist(r['frontTinClearance'] for r in a),'closest20Target':[ref(r) for r in sorted(a,key=lambda r:(r['targetTinClearance'],r['id'],r['ix'],r['iy'],r['ip']))[:20]],'closest20Front':[ref(r) for r in sorted([r for r in a if r['frontTinClearance'] is not None],key=lambda r:(r['frontTinClearance'],r['id'],r['ix'],r['iy'],r['ip']))[:20]]}
# Neighbor statistics, no categorical abruptness threshold.
continuity={};allpairrows=[]
for g in ['focal','control']:
 continuity[g]={}
 for arm in ['base','Y','P']:
  continuity[g][arm]={}
  for axis in ['P','x','y']:
   entries=[];trans=collections.Counter();resulttrans=collections.Counter();missing=0
   for ident in contacts:
    if contacts[ident]['group']!=g:continue
    for ix in range(9):
     for iy in range(11):
      for ip in range(11):
       jx,jy,jp=ix+(axis=='x'),iy+(axis=='y'),ip+(axis=='P')
       if jx>=9 or jy>=11 or jp>=11:continue
       a=bykey[ident,ix,iy,ip,arm];b=bykey[ident,jx,jy,jp,arm];gap=(X[jx]-X[ix]) if axis=='x' else (Y[jy]-Y[iy]) if axis=='y' else .1
       trans[a['status']+'→'+b['status']]+=1;resulttrans[str(a['hasResult'])+'→'+str(b['hasResult'])]+=1
       if not a['hasResult'] or not b['hasResult']:missing+=1;continue
       e={'a':[ident,ix,iy,ip],'b':[ident,jx,jy,jp],'inputGap':gap,'statusA':a['status'],'statusB':b['status']}
       for k in ['askedY','askedP','usedY','deltaY','deltaP']:
        d=b[k]-a[k];e['diff_'+k]=d;e['abs_'+k]=abs(d);e['slope_'+k]=d/gap
       e['excessUsedY']=e['diff_usedY']-(b['originalY']-a['originalY']);e['absExcessUsedY']=abs(e['excessUsedY']);e['slopeExcessUsedY']=e['excessUsedY']/gap
       entries.append(e)
   keys=[k for k in entries[0] if k.startswith(('diff_','abs_','slope_'))]+['excessUsedY','absExcessUsedY','slopeExcessUsedY'] if entries else []
   continuity[g][arm][axis]={'pairs':sum(trans.values()),'bothHaveResult':len(entries),'missingAtLeastOne':missing,'statusTransitions':dict(trans),'resultTransitions':dict(resulttrans),'distributions':{k:dist(e[k] for e in entries) for k in keys},'top20UsedY':[e for e in sorted(entries,key=lambda e:(-e['abs_usedY'],e['a']))[:20]],'top20AskedY':sorted(entries,key=lambda e:(-e['abs_askedY'],e['a']))[:20],'top20AskedP':sorted(entries,key=lambda e:(-e['abs_askedP'],e['a']))[:20]}
   print('continuity',g,arm,axis,len(entries),missing)
   for e in entries:allpairrows.append([g,arm,axis,e])
summary['provenance']={'fullOutputSha256':{p.name:sha(p) for p in sorted(run.glob('*.tsv'))},'baselineByteIdenticalToApprovedMap':(run/'baseline.tsv').read_bytes()==(root/'Logs/feasibility-20261104/combinations.tsv').read_bytes(),'positionsSha256':sha(out/'positions.tsv'),'methodSha256':sha(root/'docs/diagnostics/2026-11-04-sl56-lowering-rule-method.md'),'harnessSha256':sha(out/'LoweringEvaluation.cs'),'uniqueEvaluations':sum(int(r['uniqueEvaluations']) for r in csv.DictReader((run/'cache.tsv').open(),delimiter='\t')),'cacheHits':sum(int(r['cacheHits']) for r in csv.DictReader((run/'cache.tsv').open(),delimiter='\t'))}
for name,data in [('summary',summary),('continuity',continuity),('tin',near)]:
 (dest/f'2026-11-04-sl56-lowering-{name}.json').write_text(json.dumps(data,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
# Reusable compact in-memory rows, no self-referential base.
(out/'parsed-rows.json').write_text(json.dumps([{k:v for k,v in r.items() if k!='base'} for r in rows],separators=(',',':')))
print('COUNTS',json.dumps({g:{a:summary['group'][g][a]['all']['counts'] for a in ['base','Y','P']} for g in ['focal','control']},ensure_ascii=False))
```

## Auditoría exacta realizada

```python
import csv,json,struct,math,hashlib,collections,itertools,subprocess
from pathlib import Path
root=Path.cwd();out=root/'Logs/lowering-evaluation-20261104';run=out/'run1'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def f32(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def bits(x):return struct.unpack('<I',struct.pack('<f',x))[0]
summary=json.loads((root/'docs/validation/evidence/2026-11-04-sl56-lowering-summary.json').read_text());grid=summary['inputs']['grid'];limit=f32(.48)+f32(.01);endpoint=f32(limit)
if endpoint<=limit:endpoint=struct.unpack('<f',struct.pack('<I',bits(endpoint)+1))[0]
evals={(r['id'],r['evalId']):r for r in csv.DictReader((run/'evaluations.tsv').open(),delimiter='\t')}
results={(r['id'],r['ix'],r['iy'],r['ip'],r['arm']):r for r in csv.DictReader((run/'results.tsv').open(),delimiter='\t')}
sequences=collections.defaultdict(list)
for r in csv.DictReader((run/'attempts.tsv').open(),delimiter='\t'):sequences[tuple(r[k] for k in ['id','ix','iy','ip','arm'])].append(r)
for key,r in results.items():
 ident,ix,iy,ip,arm=key;ix,iy,ip=map(int,[ix,iy,ip]);b=results[ident,str(ix),str(iy),str(ip),'base'];s=sequences.get(key,[])
 assert len(s)==int(r['tried'])
 if arm=='base':assert not s;continue
 if b['inCourt']=='True':
  assert r['status']=='preserved' and not s
  for k in b:
   if k not in ['arm','status']:assert b[k]==r[k],(key,k)
  continue
 assert r['status'] in ['changed','no-solution']
 expected=[]
 if arm=='Y':
  seen=set();k=1
  while True:
   candidate=grid['y'][iy]-k*.05;k+=1
   if candidate<=limit:break
   v=f32(candidate)
   if v<=limit or bits(v) in seen:continue
   expected.append(v);seen.add(bits(v))
  if endpoint<f32(grid['y'][iy]) and bits(endpoint) not in seen:expected.append(endpoint)
 else:expected=[f32((2*ip-j)/20) for j in range(1,2*ip+1)]
 assert len(s)<=len(expected)
 if r['status']=='no-solution':assert len(s)==len(expected) and r['evalId']=='NA' and r['askedY']=='NA' and r['askedP']=='NA'
 for j,trial in enumerate(s):
  assert int(trial['trial'])==j+1
  e=evals[ident,trial['evalId']];actual=float(e['y'] if arm=='Y' else e['p'])
  assert bits(actual)==bits(expected[j]),(key,j,actual,expected[j])
  assert bits(float(e['x']))==bits(f32(grid['x'][ix]))
  assert bits(float(e['p'] if arm=='Y' else e['y']))==bits(float(b['askedP'] if arm=='Y' else b['askedY']))
  assert (e['inCourt']=='True')==(r['status']=='changed' and j==len(s)-1),(key,j)
 if r['status']=='changed':
  assert r['evalId']==s[-1]['evalId'];e=evals[ident,r['evalId']]
  for k in e:
   if k not in ['x','y','p']:assert r[k]==e[k],(key,k)
 for k,v in [('lastY',None if not s else float(evals[ident,s[-1]['evalId']]['y'])),('lastP',None if not s else float(evals[ident,s[-1]['evalId']]['p']))]:assert (r[k]=='NA' and v is None) or float(r[k])==v
for p in sorted(run.glob('*.tsv')):assert p.read_bytes()==(out/'run2'/p.name).read_bytes()
assert (run/'baseline.tsv').read_bytes()==(root/'Logs/feasibility-20261104/combinations.tsv').read_bytes()
protected=json.loads((out/'protected-before.json').read_text())
for p,h in protected.items():assert sha(root/p)==h,p
prov=json.loads((out/'provenance.json').read_text())
for p,h in prov['sourceHashes'].items():assert sha(Path(p))==h,p
# Retain all no-solution references with original request and last candidate.
summary['noSolutionCases']=[{k:r[k] for k in ['id','ix','iy','ip','arm','tried','lastY','lastP']} for r in results.values() if r['status']=='no-solution']
summary['provenance'].update(repeatByteIdentical=True,baselineRowsVerified=79497,requestArmsVerified=len(results),candidateReferencesVerified=sum(map(len,sequences.values())),protectedSourceFiles=len(protected),methodCommitsBeforeCalculation=['140d0ca'])
(root/'docs/validation/evidence/2026-11-04-sl56-lowering-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2,allow_nan=False)+'\n')
record={'baselineRowsVerified':79497,'requestArmsVerified':len(results),'candidateReferencesVerified':sum(map(len,sequences.values())),'uniqueEvaluationsVerified':len(evals),'protectedSourceFiles':len(protected),'outputsIdentical':True,'methodCommitBeforeCalculation':'140d0ca','outputsSha256':{p.name:sha(p) for p in sorted(run.glob('*.tsv'))}}
(out/'audit.json').write_text(json.dumps(record,indent=2)+'\n');print(json.dumps(record,indent=2))
```

La auditoría verifica79497filas base,238491solicitudes/brazo,380725referencias de candidatos,195936evaluaciones únicas y521archivos protegidos; salidas repetidas idénticas. El JSON de tin y el de continuidad incluyen los rangos de20predeclarados y todas las distribuciones porsección. Las fuentes y los datos originales se conservan.
