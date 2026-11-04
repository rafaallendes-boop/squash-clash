# Reproducción de la regresión de producto y costo

Fuente `a048e71`, método de margenf901ea7, comparaciónbf89e75 antes del código. [Informe](../reviews/2026-11-04-first-shot-return-rule-implementation.md). Ejecutable diagnóstico con las fuentes originales, ReturnShotResolver/ReturnFlightClassifier y tipos deconfiguración usadospor ellos. Misma física/fábrica/evaluador completo del arnés aprobado dePR#31, sin modificar producto para correrlo. Mono/Roslyn yUnityEngine.CoreModule.dll deUnity6000.3.27f1, referencias yflags del método previo, incluidosnetstandard.dll ylangversionlatest.

TSVpositions idéntico al aprobado, hash68ccf9fc4f1733a9199b3430f45a6623658977723e44941a8e3474ccc3fb82c1. Inputs/métricas de comparación del [método de margen](2026-11-04-sl56-margin005-method.md). Ejecutar conMONO_PATH aManaged/UnityEngine, argumentos positions.tsv ycarpeta de salidas nueva ignorada. El métodoMain deabajo escribeproduct-enabled.tsv/product-disabled.tsv/product-states.tsv yStopwatchFrequency; redirigir stdout aproduct-regression.log enesa carpeta. No usar el iPhone, Editor o unbuild nativo para esta regresión.

## Fuentes del ejecutable

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
- `Assets/SquashClash/Core/Shot/ReturnShotResolver.cs`
- `Assets/SquashClash/Core/Shot/ReturnFlightClassifier.cs`
- `Assets/SquashClash/Core/Config/ParamSet.cs`
- `Assets/SquashClash/Core/Config/ParamCatalog.cs`
- `Assets/SquashClash/Core/Config/ParamDef.cs`
- `Assets/SquashClash/Core/Config/ParamKind.cs`
- `Assets/SquashClash/Core/Config/ApplyClass.cs`

## Arnés exacto

```csharp
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Shot;
public static class ProductRegression {
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

 static readonly PhysicsParams physics=new PhysicsParams {Gravity=9.81f,StepHz=120,MaxImpactsPerStep=4,BallRadius=.020f,BallMass=.024f,
 Floor=new SurfaceMaterial{Restitution=.45f,TangentialFriction=.30f},FrontWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},
 SideWalls=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},BackWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f}};
 static readonly CourtDimensions court=CourtDimensions.Standard();
 static readonly IReadOnlyList<LineBand> bands=CourtLines.Build(court);
 static readonly AnalyticBallSimulationFactory factory=new AnalyticBallSimulationFactory(.1f);
 static readonly List<ContactEvent> hits=new List<ContactEvent>(4);
 static object[] Evaluate(Vector3 pos,float x,float y,float power,ShotSolution solution) {

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

 public static int Main(string[] args) {
  var lines=File.ReadAllLines(args[0]);
  using(var enabled=new StreamWriter(Path.Combine(args[1],"product-enabled.tsv")))
  using(var disabled=new StreamWriter(Path.Combine(args[1],"product-disabled.tsv")))
  using(var states=new StreamWriter(Path.Combine(args[1],"product-states.tsv"))) {
   enabled.WriteLine("id	ix	iy	ip	solved	reason	raised	lowered	requestedSpeed	speed	usedX	usedY	firstSurface	firstT	frontX	frontY	floorT	floorX	floorZ	floorInside	outBefore	outT	outSurface	tinBefore	tinT	escapeBefore	escapeT	escapeAny	passedRearBefore	passedRearReturn	backBefore	sideBefore	sideAny	planeOutsideBefore	capCount	stopReason	inCourt	sideOutsideBefore");disabled.WriteLine("id	ix	iy	ip	solved	reason	raised	lowered	requestedSpeed	speed	usedX	usedY	firstSurface	firstT	frontX	frontY	floorT	floorX	floorZ	floorInside	outBefore	outT	outSurface	tinBefore	tinT	escapeBefore	escapeT	escapeAny	passedRearBefore	passedRearReturn	backBefore	sideBefore	sideAny	planeOutsideBefore	capCount	stopReason	inCourt	sideOutsideBefore");states.WriteLine("id\tix\tiy\tip\tstate\taskedY\taskedP\tcalls\tuncachedTicks\tcachedTicks\tcachedCalls");
   for(int c=1;c<lines.Length;c++) {
    var a=lines[c].Split('\t');var pos=new Vector3(float.Parse(a[1],C),float.Parse(a[2],C),float.Parse(a[3],C));
    var on=new ReturnShotResolver(6,28,32,court,physics,factory,new ReturnRuleSettings(true));
    var off=new ReturnShotResolver(6,28,32,court,physics,factory,new ReturnRuleSettings(false));
    for(int ix=0;ix<X.Length;ix++)for(int iy=0;iy<Y.Length;iy++)for(int ip=0;ip<11;ip++) {
     float power=ip/10f;var request=new ShotRequest(pos,new Vector2(X[ix],Y[iy]),power);
     long t=System.Diagnostics.Stopwatch.GetTimestamp();var result=on.Resolve(request);long elapsed=System.Diagnostics.Stopwatch.GetTimestamp()-t;
     t=System.Diagnostics.Stopwatch.GetTimestamp();var cached=on.Resolve(request);long cacheElapsed=System.Diagnostics.Stopwatch.GetTimestamp()-t;
     var baseResult=off.Resolve(request);
     foreach(var pair in new[]{Tuple.Create(enabled,result.Solution),Tuple.Create(disabled,baseResult.Solution)}) {
      var row=Evaluate(pos,X[ix],Y[iy],power,pair.Item2);row[0]=a[0];row[1]=ix;row[2]=iy;row[3]=ip;pair.Item1.WriteLine(string.Join("\t",row));
     }
     states.WriteLine(a[0]+"\t"+ix+"\t"+iy+"\t"+ip+"\t"+result.State+"\t"+R(result.AdjustedRequest.AimOnFrontWall.y)+"\t"+R(result.AdjustedRequest.P)+"\t"+result.SolverCalls+"\t"+elapsed+"\t"+cacheElapsed+"\t"+cached.SolverCalls);
    }
    enabled.Flush();disabled.Flush();states.Flush();Console.WriteLine("Completed "+a[0]);
   }
  }
  Console.WriteLine("StopwatchFrequency="+System.Diagnostics.Stopwatch.Frequency);return 0;
 }
}
```

## Comparación exacta

El código siguiente usa las rutaslocales deejecución, conlos TSV aprobados delmargen ylos deproducto enproduct-final. NoSolution compara conbase; los demásestados conY. Además comprueba la salida desactivada entera ylas referencias decaché.

```python
from pathlib import Path
import csv,json,collections,hashlib,math
out=Path('Logs/margin005-20261104');a=list(csv.DictReader((out/'run1/results.tsv').open(),delimiter='\t'));gold={(r['id'],r['ix'],r['iy'],r['ip']):r for r in a if r['arm']=='Y'};base={(r['id'],r['ix'],r['iy'],r['ip']):r for r in a if r['arm']=='base'}
states=list(csv.DictReader((out/'product-final/product-states.tsv').open(),delimiter='\t'));physical=list(csv.DictReader((out/'product-final/product-enabled.tsv').open(),delimiter='\t'));mismatch=[];counts=collections.Counter()
for s,p in zip(states,physical):
 key=tuple(s[k] for k in ['id','ix','iy','ip']);g=gold[key];counts[s['state']]+=1
 if s['state']!={'preserved':'Preserved','changed':'Changed','no-solution':'NoSolution'}[g['status']]:mismatch.append([key,'state',s['state'],g['status']]);continue
 expected=base[key] if g['status']=='no-solution' else g
 if g['status']!='no-solution' and (s['askedY']!=g['askedY'] or s['askedP']!=g['askedP']):mismatch.append([key,'request',s['askedY'],g['askedY']])
 for k in p:
  if p[k]!=expected[k]:mismatch.append([key,k,p[k],expected[k]])
 assert s['cachedCalls']=='0'
assert len(states)==len(physical)==79497
same=(out/'product-final/product-disabled.tsv').read_bytes()==(out/'run1/baseline.tsv').read_bytes()
frequency=int((out/'product-final/product-regression.log').read_text().split('StopwatchFrequency=')[-1].strip())
def dist(a):
 a=sorted(a);n=len(a);return {'n':n,'min':a[0],'p50':a[math.ceil(n*.5)-1],'p95':a[math.ceil(n*.95)-1],'p99':a[math.ceil(n*.99)-1],'max':a[-1]}
r={'requests':len(states),'counts':dict(counts),'mismatchCount':len(mismatch),'firstMismatches':mismatch[:20],'disabledByteIdentical':same,'stopwatchFrequency':frequency,'uncachedMilliseconds':dist(int(v['uncachedTicks'])*1000/frequency for v in states),'cachedMilliseconds':dist(int(v['cachedTicks'])*1000/frequency for v in states),'solverCalls':dist(int(v['calls']) for v in states),'cachedCallsAllZero':True}
(out/'product-final/product-regression-summary.json').write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r,indent=2));assert not mismatch and same
```

## Costo de vista previa

Eltest PlayMode `ReturnPreview_CostSamples_NoNewPerformanceThreshold` registraSL1/SL5/SL6×P0…1, antes ydespués deuna repetición exacta, sinaserción de umbral. ElXML final301/301 contiene33líneasRETURN_PREVIEW_COST; los datos sanitizados están enelJSON deverificación junto albenchmark externo de79497solicitudes. No mideLineRenderer/GPU/H17/iPhone. Elcache se valida por0llamadas alsolver y resolución determinista, no porun presupuesto nuevo.
