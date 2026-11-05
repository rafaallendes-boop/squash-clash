# Reproducción del diagnóstico offline SL5/SL6

Método confirmado antes del primer cálculo en **975b597/2141676**; auditoría de endpoints **4a55d80**. El arnés siguiente es documentación, no un archivo de producto o prueba ni una ejecución automática. Se compiló junto con los 13 fuentes Core originales enumerados en el [JSON derivado](../validation/evidence/2026-11-04-sl56-p2-offline.json), sobre producto 9131b1c.

Usar Mono, csc, UnityEngine.CoreModule y la fachada netstandard de Unity 6000.3.27f1 ya instalado. No requiere abrir Editor, correr suites, acceder al iPhone o instalar dependencias. No se promete igualdad bit a bit con IL2CPP nativo.

## Entradas y ejecución

Generar inputs.tsv UTF-8/LF desde cases del JSON versionado, cabecera **id,x,y,z,vx,vy,vz** separada por TAB. Cada fila usa position/velocity con 17 dígitos significativos; no recalcular solver ni sustituir estados. Verificar inputsTsvSha256. Guardar el arnés fuera de Assets/tests en una ruta diagnóstica ignorada.

Compilar con **-langversion:latest -target:exe -optimize+**, referencias a UnityEngine.CoreModule.dll y netstandard.dll del Mono incluido en Unity. Incluir en el mismo assembly estos fuentes originales para acceder al constructor internal, sin modificarlos:

- Core/Simulation: AnalyticBallSimulation.cs, Ballistics.cs, BallState.cs, PhysicsParams.cs, SurfaceMaterial.cs, IBallSimulation.cs, ContactEvent.cs.
- Core/Court: CourtDimensions.cs, SurfaceId.cs, CourtLines.cs, LineBand.cs, LineId.cs, LineContactClassifier.cs.

Ejecutar OfflineTrajectory.exe con argumentos **inputs.tsv events.tsv**, usando el Mono de Unity y MONO_PATH apuntando a Managed/UnityEngine. Repetir con **inputs.tsv events-repeat.tsv** y comparar ambos archivos byte a byte. Los comandos exactos resueltos están preservados en Logs/offline-sl56-20261104/execution-provenance.json; los hashes de fuentes/DLL/arnés/binario están en el JSON derivado. No se publica ruta de usuario ni identidad de equipo.

El primer intento de compilar faltaba una referencia a netstandard; se preservó compile.log y no produjo trayectorias. Se añadió la fachada ya instalada, sin cambiar P2 ni usar un adaptador Vector3 propio. La ejecución usa los vectores/clasificador de las bibliotecas originales. El análisis preliminar de out fuera de la extensión de pared se conserva aparte y no es el derivado final.

## Arnés exacto final

```csharp
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
public static class OfflineTrajectory {
 static readonly CultureInfo C=CultureInfo.InvariantCulture;
 static string R(double x)=>x.ToString("R",C);
 static string R(float x)=>x.ToString("R",C);
 static string V(Vector3 v)=>R(v.x)+"\t"+R(v.y)+"\t"+R(v.z);
 static float F(string s)=>float.Parse(s,C);
 static bool IsOut(LineId x)=>x==LineId.FrontOut||x==LineId.BackOut||x==LineId.LeftSideOut||x==LineId.RightSideOut;
 public static int Main(string[] args) {
  var p=new PhysicsParams {Gravity=9.81f,StepHz=120,MaxImpactsPerStep=4,BallRadius=.020f,BallMass=.024f,
   Floor=new SurfaceMaterial{Restitution=.45f,TangentialFriction=.30f},FrontWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},
   SideWalls=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f},BackWall=new SurfaceMaterial{Restitution=.55f,TangentialFriction=.20f}};
  var court=CourtDimensions.Standard();var bands=CourtLines.Build(court);
  using(var w=new StreamWriter(args[1],false,new System.Text.UTF8Encoding(false))) {
   w.WriteLine("id\ttype\tstep\ttimeSim\tsurface\tpx\tpy\tpz\tvx\tvy\tvz\tpersistent\toutDistance\tpointOut\tfootprintOut\ttouchesOutBand\tfirstFloor\tstopReason\tcapCount");
   var lines=File.ReadAllLines(args[0]);
   for(int k=1;k<lines.Length;k++) {
    if(string.IsNullOrWhiteSpace(lines[k]))continue;
    var a=lines[k].Split('\t');var id=a[0];
    var state=new BallState {Position=new Vector3(F(a[1]),F(a[2]),F(a[3])),Velocity=new Vector3(F(a[4]),F(a[5]),F(a[6])),Step=0};
    int floors=0;bool escaped=false;string stop="horizon-10s";
    var hits=new List<ContactEvent>(4);
    using(var sim=new AnalyticBallSimulation(p,court,.1f,()=>{},()=>{})) {
     sim.Reset(state);
     for(int i=0;i<1200;i++) {
      sim.Step(hits);
      foreach(var h in hits) {
       bool first=false;
       if(h.Surface==SurfaceId.Floor&&!h.Persistent){floors++;first=floors==1;}
       double distance=double.NaN;bool pointOut=false,footprintOut=false,touches=false;
       foreach(var b in bands)if(b.Surface==h.Surface&&IsOut(b.Id)) {
        distance=Vector2.Dot(h.UV-b.A,b.Growth);
        var edge=b.B-b.A; float along=Vector2.Dot(h.UV-b.A,edge/edge.magnitude);
        pointOut=distance>=0&&along>=0&&along<=edge.magnitude;
        touches=LineContactClassifier.Touches(in b,h.UV,.01f);
        footprintOut=pointOut||touches;
       }
       double t=(h.Step-1)/120.0+h.TimeInStep;
       w.WriteLine(id+"\tcontact\t"+h.Step+"\t"+R(t)+"\t"+h.Surface+"\t"+V(h.Point)+"\t"+V(h.ApproachVelocity)+"\t"+h.Persistent+"\t"+R(distance)+"\t"+pointOut+"\t"+footprintOut+"\t"+touches+"\t"+first+"\t\t"+sim.ImpactCapReachedCount);
      }
      var s=sim.State;
      if(!escaped&&(s.Position.y>court.WallHeight||s.Position.x<0||s.Position.x>court.Width||s.Position.z<0||s.Position.z>court.Length)) {
       escaped=true;w.WriteLine(id+"\tescape\t"+s.Step+"\t"+R(s.Step/120.0)+"\tnone\t"+V(s.Position)+"\t"+V(s.Velocity)+"\tFalse\tNaN\tFalse\tFalse\tFalse\tFalse\tgeometry-envelope\t"+sim.ImpactCapReachedCount);
      }
      if(floors>=2){stop="second-floor-step-complete";break;}
     }
     var end=sim.State;
     w.WriteLine(id+"\tend\t"+end.Step+"\t"+R(end.Step/120.0)+"\tnone\t"+V(end.Position)+"\t"+V(end.Velocity)+"\tFalse\tNaN\tFalse\tFalse\tFalse\tFalse\t"+stop+"\t"+sim.ImpactCapReachedCount);
    }
   }
  }
  return 0;
 }
}
```

SHA-256 del arnés: **2f7deefe1a9619dc9902658bef78ffa1f60e6dfdfac02e2f128f79a7b374749a**.

SHA-256 events.tsv final: **7d3b10ab7f82e017f560cde0f6ed47a2cbc4300b56f0eb6a7d06711b23a0eed5**; repetición idéntica byte a byte. Salida final: 128 filas de contactos/fin/escape, más cabecera. El JSON derivado agrega contexto y primeros eventos manteniendo todas las filas; NaN de una distancia no aplicable se representa como null.
