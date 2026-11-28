using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Read-only diagnosis of the stop of 26 nov (serve positions): every contact of the Lob and Drive serves, built as ServePlanner.Typed
    // does from the current BoxPosition, judged by the serve rules until the serve is judged or closed. Nothing in production changes.
    public sealed class ServePositionsDriveDiagnosis
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static string F(float v)=>v.ToString("0.####",C);
        [Test,Explicit,Category("Experiments")]
        public void WriteServeContacts()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string path=Path.Combine(root,"Logs/experiments/serve-positions/serve-contacts.tsv");
            Assert.That(File.Exists(path),Is.False,"Never overwrite evidence.");
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);var p=ctx.Params;
            using(var w=new StreamWriter(path,false,new UTF8Encoding(false)))
            {
                w.WriteLine("box\ttype\tfromX\tfromZ\tstep\tsurface\tx\ty\tz\tpersistent\tserving\tclosed\tcause");
                foreach(ServiceBox box in new[]{ServiceBox.Right,ServiceBox.Left})
                    foreach(var type in new[]{ServeType.Lob,ServeType.Drive})
                    {
                        bool right=box==ServiceBox.Right,lob=type==ServeType.Lob;
                        double x=lob?(right?p.ServeKindLobRightAimX:p.ServeKindLobLeftAimX):(right?p.ServeKindDriveRightAimX:p.ServeKindDriveLeftAimX);
                        double y=lob?(right?p.ServeKindLobRightAimY:p.ServeKindLobLeftAimY):(right?p.ServeKindDriveRightAimY:p.ServeKindDriveLeftAimY);
                        double speed=lob?(right?p.ServeKindLobRightSpeed:p.ServeKindLobLeftSpeed):(right?p.ServeKindDriveRightSpeed:p.ServeKindDriveLeftSpeed);
                        var at=ServePlanner.BoxPosition(ctx,box);var from=new Vector3(at.x,(float)p.ServeHandHeight,at.y);
                        var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3((float)x,(float)y,0),ctx.Physics.BallRadius);
                        var serve=new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,(float)speed,ctx.Physics.Gravity)};
                        var rules=new PointRules(ctx.Court,(float)p.FootprintRadius);rules.BeginServe(PlayerId.Human,box);
                        using(var sim=ctx.Factory.Create(ctx.Physics,ctx.Court))
                        {
                            sim.Reset(serve);var events=new List<ContactEvent>(4);bool done=false;
                            for(int step=0;step<ctx.StepHz*8&&!done;step++)
                            {
                                events.Clear();sim.Step(events);
                                foreach(var e in events)
                                {
                                    rules.OnBallContact(e);
                                    w.WriteLine(string.Join("\t",new[]{box.ToString(),type.ToString(),F(at.x),F(at.y),e.Step.ToString(C),e.Surface.ToString(),F(e.Point.x),F(e.Point.y),F(e.Point.z),e.Persistent?"1":"0",rules.Serving?"1":"0",rules.Closed?"1":"0",rules.Cause.ToString()}));
                                    if(rules.Closed||!rules.Serving){done=true;break;}
                                }
                            }
                        }
                    }
            }
        }
    }
}
