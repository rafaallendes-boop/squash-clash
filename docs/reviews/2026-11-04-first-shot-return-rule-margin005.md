# Margen de 0,05 m: comparación previa al código

Método **f901ea7** commiteado antes del cálculo; único cambio del arnés aprobado: límite TinTop+0,05f en vez deTinTop+0,01f. ShotSolver/P2/clasificación/guardia/brazos/P/grid/horizonte intactos. Dos ejecuciones completas idénticas byte a byte; base idéntica a aprobada; brazoP idéntico en resultados físicos/estados (solo pueden cambiar ids internos de caché). No código de producto ni tests implementados antes de esta comparación.

| Grupo | N | Y dentro anterior→0,05 | Sin solución anterior→0,05 | Bote trasero anterior→0,05 | BajarP dentro | BajarP sin solución |
|---|---:|---:|---:|---:|---:|---:|
| focal | 26136 | 26125→26116 | 11→20 | 6270→6270 | 26099 | 37 |
| control | 53361 | 53247→53201 | 114→160 | 22831→22831 | 53089 | 272 |

Se pierden9retornos focales. Encontroles47cambios antes exitosos quedan sin solución y1caso antes sin solución pasa a retorno: saldo−46. Ese caso puede aparecer porque el extremo muestreado cambió de≈0,49 a≈0,53m; no se refinó el paso tras resultados. Todos los cambios restantes conservan el resultado anterior. Las66447bases dentro siguen idénticas por guardia. EnC el resultado de cadaP/brazo coincide exactamente con la evaluación aprobada, incluidos24/24botes traseros focalesY desdeP0,3 y17/24enP.

El margen limita candidatos nuevos; no impone una corrección a bases ya dentro. Por eso pueden preservarse puntosy0,50 por debajo deTinTop+0,05≈0,53: no son un tin/out nuevo ni incumplimiento de la guardia. La huella clasificadora sigue0,010f.

## Distribución Δy de cambios exitosos

| Grupo | Margen | n | min | p5 | p50 | p95 | p99 | max |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| focal | 0,01 | 8722 | 0.009999990463256836 | 0.10000002384185791 | 1.25 | 3.3000001907348633 | 3.8000001907348633 | 4.000000178813934 |
| focal | 0,05 | 8713 | 0.04999995231628418 | 0.10000002384185791 | 1.25 | 3.3000001907348633 | 3.8000001907348633 | 4.000000178813934 |
| control | 0,01 | 4203 | 0.009999990463256836 | 0.04999995231628418 | 0.5 | 1.500000238418579 | 1.8000001907348633 | 2.200000286102295 |
| control | 0,05 | 4157 | 0.04999995231628418 | 0.04999995231628418 | 0.5 | 1.500000238418579 | 1.8000001907348633 | 2.200000286102295 |

## Continuidad, sin umbral nuevo

| Grupo | Eje | Pares con2salidas antes→ahora | p95 antes→ahora | p99 antes→ahora | Máximo antes→ahora |
|---|---|---:|---:|---:|---:|
| focal | P | 23749→23740 | 0.5499999523162842→0.5499999523162842 | 1.4000000953674316→1.4000000953674316 | 2.9500001668930054→2.9500001668930054 |
| focal | x | 23221→23212 | 0.7999999523162842→0.7999999523162842 | 1.3999998569488525→1.3999998569488525 | 2.2500457763671875→2.2500457763671875 |
| focal | y | 23749→23740 | 0.5999999046325684→0.5999999046325684 | 0.7000000476837158→0.7000000476837158 | 2.7500457763671875→2.7500457763671875 |
| control | P | 48362→48296 | 0.2500002384185791→0.2500002384185791 | 0.7049438953399658→0.7063474655151367 | 3.8638450503349304→3.8638450503349304 |
| control | x | 47318→47272 | 0.3000001907348633→0.3000001907348633 | 1.1000001430511475→1.1000001430511475 | 3.9138452410697937→3.9138452410697937 |
| control | y | 48396→48350 | 0.7000000476837158→0.7000000476837158 | 1.1275482177734375→1.1275482177734375 | 3.8978471755981445→3.8978471755981445 |

Ausencias se excluyen de diferencias numéricas; no se rellenan con0. JSON conserva las distribuciones/transiciones/top20 completos con los mismos denominadores porgrupo/eje. No se clasifica percepción ni «brusco» con corte nuevo.

## Lectura para la autorización condicionada

**Juicio de Codex:** no cambia el panorama de la recomendación. Y mantiene ventaja frente a bajarP en retornos dentro, profundidad y menos casos sin solución en ambos grupos; la columnaC y la profundidad se mantienen. El margen elimina candidatos próximos al tin del tramo de búsqueda sin introducir un nuevo máximo de discontinuidad. Se conservan los riesgos ya aceptados/observados de guardia, lateral y pared trasera. No es una aprobación física ni un criterio de aceptación nuevo. Se continúa elTDD autorizado, con la regla desactivada por defecto y sin instalar.

[Resumen](../validation/evidence/2026-11-04-sl56-margin005-summary.json), [continuidad](../validation/evidence/2026-11-04-sl56-margin005-continuity.json), [tin](../validation/evidence/2026-11-04-sl56-margin005-tin.json), [método](../diagnostics/2026-11-04-sl56-margin005-method.md). LosTSV completos ignorados están enLogs/margin005-20261104/run1 yrun2. Arnésexacto: mismo aprobado be9c9ed0… salvo la única línea documentada; se adjunta acontinuación para reproducción.

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
   double limit=(double)court.TinTop+(double).05f;
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

SHA-256 de salidas de ambas ejecuciones:

| Archivo | SHA-256 |
|---|---|
| attempts.tsv | 8f088fa8a374eeb753fef2597b496db090eb95bd1c3b29b1c1943b0ca4a4d3b6 |
| baseline.tsv | 913551248ac43ad9e029dfbcd5d6551f9cf19cf9775faeb0e7b3a2aaaba69b43 |
| cache.tsv | e3280b278de7460fdf0a9bf8ad97ee8a6b16c951da1248a58e06399053f13938 |
| evaluations.tsv | 929b8ce90a4b64941d1e3ff151288951673d698e15941ce40cb677f91e1b8fa7 |
| results.tsv | 91415ebf22dea748b8dc5892620a21bf911f5562c74f4e1bf9c465ae2bf569d9 |
