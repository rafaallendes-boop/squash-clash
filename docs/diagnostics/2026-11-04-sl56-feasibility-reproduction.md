# Reproducción del mapa offline de factibilidad

[Método predeclarado](2026-11-04-sl56-feasibility-method.md), commits **c000f85/7b5d943 anteriores al cálculo**. [Resumen sanitizado](../validation/evidence/2026-11-04-sl56-feasibility-summary.json), [informe](../reviews/2026-11-04-first-shot-sl56-feasibility-map.md).

## Entradas, herramientas y salidas

Reconstruir positions.tsv desde contacts del JSON: cabecera id,x,y,z separada porTAB, coordenadas position con17dígitos significativos, UTF-8/LF. Verificar positionsTsvSha256. El arnés fija la rejilla del método; no consume originalP/originalVelocity para calcular salidas.

Compilar con el Mono/csc de Unity6000.3.27f1 instalado, opciones -langversion:latest -target:exe -optimize+, referencias a UnityEngine.CoreModule.dll y fachada netstandard.dll. Incluir en el mismo assembly los13fuentesCore del [arnés anterior](2026-11-04-sl56-p2-reproduction.md), más **ShotSolver.cs y AnalyticBallSimulationFactory.cs originales**. Ningún stub, Editor, suite o archivo de producto modificado. Comando exacto resuelto y hashes en Logs/feasibility-20261104/provenance.json; hashes relativos en el JSON.

Ejecutar con arguments positions.tsv combinations.tsv, MONO_PATH apuntando al Managed/UnityEngine de Unity. Repetir el mismo binario con positions.tsv combinations-repeat.tsv; comparar byte a byte. Ambas ejecuciones completas fueron idénticas. El archivo completo contiene79.497filas y cabecera,38columnas, incluidos primer bote(x,z) y clasificaciones de cada combinación. Se preserva ignorado en Logs/feasibility-20261104, no se versiona como log.

El resumen se deriva de ese archivo: puntoIndex=ix*11+iy; cada máscara tiene11bits, biti representaP=i/10. Columnas de máscara en pointMaskColumns, posición en position, rangos observados en inCourtDepthRanges. U=solved sin ajustes, A=solved con SpeedRaised/TargetLowered. Para reconstruir exactamente una celda, filtrar elTSV por id/ix/iy y leer sus11ip, sin ejecutar de nuevo elsolver para resumir.

El empaquetador local package_map.py conserva el código de derivación y write_report.py las tablas; las máscaras se verificaron contra todos los registros completos, no contra redondeos de Markdown. Fuentes/primarios con hashes antes/después y método anterior archivados. No se promete igualdad bit a bit entre Mono eIL2CPP/ARM64 ni factibilidad entre muestras.

## Arnés exacto ejecutado

```csharp
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Core.Shot;
public static class FeasibilityMap {
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
 public static int Main(string[] args) {
  var physics=new PhysicsParams {Gravity=9.81f,StepHz=120,MaxImpactsPerStep=4,BallRadius=.020f,BallMass=.024f,
   Floor=new SurfaceMaterial{Restitution=.45f,TangentialFriction=.30f},FrontWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},
   SideWalls=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},BackWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f}};
  var court=CourtDimensions.Standard();var bands=CourtLines.Build(court);
  var factory=new AnalyticBallSimulationFactory(.1f);var hits=new List<ContactEvent>(4);
  using(var w=new StreamWriter(args[1],false,new System.Text.UTF8Encoding(false))) {
   w.WriteLine("id\tix\tiy\tip\tsolved\treason\traised\tlowered\trequestedSpeed\tspeed\tusedX\tusedY\tfirstSurface\tfirstT\tfrontX\tfrontY\tfloorT\tfloorX\tfloorZ\tfloorInside\toutBefore\toutT\toutSurface\ttinBefore\ttinT\tescapeBefore\tescapeT\tescapeAny\tpassedRearBefore\tpassedRearReturn\tbackBefore\tsideBefore\tsideAny\tplaneOutsideBefore\tcapCount\tstopReason\tinCourt\tsideOutsideBefore");
   var lines=File.ReadAllLines(args[0]);
   for(int caseNo=1;caseNo<lines.Length;caseNo++) {
    if(string.IsNullOrWhiteSpace(lines[caseNo]))continue;
    var a=lines[caseNo].Split('\t');
    var pos=new Vector3(float.Parse(a[1],C),float.Parse(a[2],C),float.Parse(a[3],C));
    for(int ix=0;ix<X.Length;ix++)for(int iy=0;iy<Y.Length;iy++)for(int ip=0;ip<11;ip++) {
     float power=ip/10f;
     var solution=ShotSolver.Solve(new ShotRequest(pos,new Vector2(X[ix],Y[iy]),power),6f,28f,32f,physics.Gravity,physics.BallRadius,court,factory,physics);
     if(factory.LiveInstances!=0)throw new InvalidOperationException("Solver leaked an instance.");
     var row=new object[38];row[0]=a[0];row[1]=ix;row[2]=iy;row[3]=ip;row[4]=solution.Solved;row[5]=solution.Reason;
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
     w.WriteLine(string.Join("\t",row));
    }
    w.Flush();Console.WriteLine("Completed "+a[0]+"; "+caseNo+"/"+(lines.Length-1));
   }
  }
  if(factory.LiveInstances!=0)throw new InvalidOperationException("Outstanding instances.");
  return 0;
 }
}
```

SHA-256 del arnés: **888ffccb4d6e383d516f3992fc7cea43312b9a8d5496b38b9c33b5f0ccad5593**.

SHA-256 del TSV completo: **913551248ac43ad9e029dfbcd5d6551f9cf19cf9775faeb0e7b3a2aaaba69b43**.
