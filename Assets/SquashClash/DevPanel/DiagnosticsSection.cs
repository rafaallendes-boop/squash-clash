using System;
using System.Collections.Generic;
using System.Globalization;
using SquashClash.Core.Court;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using UnityEngine;
using UnityEngine.UIElements;
namespace SquashClash.DevPanel
{
    public sealed class DiagnosticsSection : IDisposable
    {
        private readonly AppBootstrap app;
        private readonly Queue<string> recent=new Queue<string>();
        private readonly Label summary=new Label {name="diagnostics-summary"}, contacts=new Label {name="diagnostics-contacts"};
        private long previousFrames;
        private double previousTime=Time.realtimeSinceStartupAsDouble;
        public VisualElement Root {get;}=new VisualElement {name="diagnostics"};
        public string Summary=>summary.text;
        public string ContactText=>contacts.text;
        public DiagnosticsSection(AppBootstrap app)
        {
            this.app=app;Root.Add(summary);Root.Add(new Label("Últimos 8 contactos · huella r = 0,010 m"));Root.Add(contacts);
            summary.style.whiteSpace=contacts.style.whiteSpace=WhiteSpace.Normal;app.Host.ContactRaised+=Record;Refresh();
        }
        public void Record(ContactEvent contact)
        {
            var lines=new List<LineId>();LineContactClassifier.Overlapping(CourtLines.Build(app.Config.Court),contact.Surface,contact.UV,.010f,lines);
            recent.Enqueue(FormattableString.Invariant($"#{contact.Step} {contact.Surface} UV({contact.UV.x:0.000},{contact.UV.y:0.000}) {contact.Timing} líneas: {string.Join(",",lines)}{(contact.Persistent?" · persistente":"")}"));
            while(recent.Count>8)recent.Dequeue();contacts.text=string.Join("\n",recent);
        }
        public void Refresh()
        {
            var stats=app.FrameStats;var now=Time.realtimeSinceStartupAsDouble;var dt=now-previousTime;
            double fps=dt>0?(stats.SessionFrames-previousFrames)/dt:double.NaN;previousTime=now;previousFrames=stats.SessionFrames;
            var timing=app.GetComponent<FrameTimingSampler>();var thermal=app.GetComponent<ThermalMonitor>();
            summary.text=$"{Number(fps)} FPS\nVentana ms: p50 {Number(stats.WindowPercentile(50))} · p95 {Number(stats.WindowPercentile(95))} · p99 {Number(stats.WindowPercentile(99))}\n"+
                $"Sesión ms: p50 {Number(stats.SessionPercentile(50))} · p95 {Number(stats.SessionPercentile(95))} · p99 {Number(stats.SessionPercentile(99))}\n"+
                $">33 ms/min: {Number(stats.SessionLongFramesPerMinute(33))}\nCPU {Number(timing.CpuMs)} ms · GPU {Number(timing.GpuMs)} ms (N/D = sin muestra)\n"+
                $"Térmico {thermal.Current} · peor {thermal.WorstThisSession}\nPredictionMatchesBitwise: {app.Host.PredictionMatchesBitwise}\nDroppedSteps: {app.Clock.DroppedSteps}\n"+
                (app.Clock.MetricsInvalid?"MÉTRICAS NO VÁLIDAS · cámara lenta usada":"")+
                (app.Metrics.LastError==null?"":"\nError de log: "+app.Metrics.LastError);
        }
        private static string Number(double v)=>double.IsNaN(v)?"N/D":double.IsPositiveInfinity(v)?">250":v.ToString("0.##",CultureInfo.InvariantCulture);
        public void Dispose(){if(app!=null)app.Host.ContactRaised-=Record;}
    }
}
