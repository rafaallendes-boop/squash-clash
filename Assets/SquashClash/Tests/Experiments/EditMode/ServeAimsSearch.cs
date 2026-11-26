using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Court;
using SquashClash.Core.Rally;
using SquashClash.Core.Simulation;
using UnityEngine;
namespace SquashClash.Experiments.EditMode
{
    // Serve positions, option 1 of the PO (26 nov 17:44): with the server positions of ae97194 unchanged, the front-wall aims of the
    // lob and the drive (RallyParams ServeKind*AimX/AimY; speeds unchanged) nearest to today's that meet the current criteria, unchanged:
    //  - ServeTests.Lob: legal, front wall at 3.4 m or higher, side wall before the bounce, bounce at z >= 9, < 0.5 m from the
    //    receiver's side wall;
    //  - ServeTests.Drive: legal, front wall at 2.6 m or lower, no side wall, bounce shorter than the lob's by more than 0.4 m, faster;
    //  - ServePositionsTests: bounce at least 0.3 m inside the quarter and no pass through the receiver's capsule.
    // A box keeps today's aim when it already passes. Explicit, outside regression; never overwrites evidence.
    // It also rewrites the frozen RallySim replay references with the exact contexts and seeds of the three replay tests.
    public sealed class ServeAimsSearch
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private const float Step=.05f;
        private static string F(double v)=>double.IsNaN(v)?"NaN":v.ToString("0.####",C);
        private struct Flight{public bool Legal;public float FrontY,BounceX,BounceZ,Inside,Pass,Horizontal;public bool SideBefore;}
        private static Flight Fly(RallyContext ctx,ServiceBox box,ServeType type)
        {
            var f=new Flight{FrontY=float.NaN,BounceX=float.NaN,BounceZ=float.NaN,Inside=float.NaN,Pass=float.NaN};BallState serve;
            try{serve=ctx.Serve(box,type);}catch(InvalidOperationException){return f;}
            f.Legal=ServePlanner.Forecast(ctx,serve,box,out var bounce);if(!f.Legal)return f;
            f.Inside=ServePositionsSearch.Inside(ctx.Court,box,bounce);
            var t=ctx.Predict(serve);bool front=false;
            foreach(var c in t.Contacts)
            {
                if(c.Persistent)continue;
                if(!front){if(c.Surface==SurfaceId.FrontWall){front=true;f.FrontY=c.Point.y;}continue;}
                if(c.Surface==SurfaceId.Floor){f.BounceX=c.Point.x;f.BounceZ=c.Point.z;break;}
                if(c.Surface==SurfaceId.LeftWall||c.Surface==SurfaceId.RightWall)f.SideBefore=true;
            }
            f.Pass=ServePositionsExperiment.Pass(ctx,t,ServePlanner.ReceiverPosition(ctx,box)).minInside;
            f.Horizontal=new Vector2(serve.Velocity.x,serve.Velocity.z).magnitude;
            return f;
        }
        private static bool Clear(RallyContext ctx,Flight f)=>f.Inside>=.3f&&f.Pass>=ServePositionsExperiment.CapsuleRadius+ctx.Physics.BallRadius;
        private static bool LobOk(RallyContext ctx,ServiceBox box,Flight f)
        {
            if(!f.Legal)return false;float wall=box==ServiceBox.Right?ctx.Court.Width-f.BounceX:f.BounceX;
            return f.FrontY>=3.4f&&f.SideBefore&&f.BounceZ>=9f&&wall<.5f&&Clear(ctx,f);
        }
        private static bool DriveOk(RallyContext ctx,Flight d,Flight l)=>d.Legal&&l.Legal&&d.FrontY<=2.6f&&!d.SideBefore&&d.BounceZ<l.BounceZ-.4f&&d.Horizontal>l.Horizontal&&Clear(ctx,d);
        private static void SetAim(RallyParams p,ServiceBox box,ServeType type,double x,double y)
        {
            bool r=box==ServiceBox.Right;
            if(type==ServeType.Lob){if(r){p.ServeKindLobRightAimX=x;p.ServeKindLobRightAimY=y;}else{p.ServeKindLobLeftAimX=x;p.ServeKindLobLeftAimY=y;}}
            else{if(r){p.ServeKindDriveRightAimX=x;p.ServeKindDriveRightAimY=y;}else{p.ServeKindDriveLeftAimX=x;p.ServeKindDriveLeftAimY=y;}}
        }
        private static (double x,double y) Aim(RallyParams p,ServiceBox box,ServeType type)
        {
            bool r=box==ServiceBox.Right;
            return type==ServeType.Lob?(r?(p.ServeKindLobRightAimX,p.ServeKindLobRightAimY):(p.ServeKindLobLeftAimX,p.ServeKindLobLeftAimY))
                :(r?(p.ServeKindDriveRightAimX,p.ServeKindDriveRightAimY):(p.ServeKindDriveLeftAimX,p.ServeKindDriveLeftAimY));
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteAimSearch()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string dir=Path.Combine(root,"Logs/experiments/serve-positions/aims");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var config=EffectiveConfig.From(preset);var today=new RallyParams();var chosen=today.Clone();
            RallyContext Ctx(RallyParams p)=>new RallyContext(config,p,factory);
            using(var w=new StreamWriter(Path.Combine(dir,"grid.tsv"),false,Utf8))
            {
                w.WriteLine("box\ttype\taimX\taimY\tdist\tlegal\tfrontY\tsideBefore\tbounceX\tbounceZ\tinsideM\tminPassM\thorizontal\tok");
                void Row(ServiceBox box,ServeType type,double x,double y,double dist,Flight f,bool ok)=>w.WriteLine(string.Join("\t",new[]{box.ToString(),type.ToString(),F(x),F(y),F(dist),
                    f.Legal?"1":"0",F(f.FrontY),f.SideBefore?"1":"0",F(f.BounceX),F(f.BounceZ),F(f.Inside),F(f.Pass),F(f.Horizontal),ok?"1":"0"}));
                foreach(ServiceBox box in new[]{ServiceBox.Right,ServiceBox.Left})
                {
                    // Lob first (the drive is judged against it).
                    var (lx,ly)=Aim(today,box,ServeType.Lob);var tc=Ctx(chosen);var tf=Fly(tc,box,ServeType.Lob);
                    bool keep=LobOk(tc,box,tf);Row(box,ServeType.Lob,lx,ly,0,tf,keep);
                    if(!keep)
                    {
                        (double x,double y,double d)? best=null;
                        for(int i=-30;i<=30;i++)for(int j=0;j<=24;j++)
                        {
                            double x=Math.Round(lx+i*Step,2),y=Math.Round(3.4+j*Step,2),d=Math.Sqrt((x-lx)*(x-lx)+(y-ly)*(y-ly));
                            var p=chosen.Clone();SetAim(p,box,ServeType.Lob,x,y);var c=Ctx(p);var f=Fly(c,box,ServeType.Lob);bool ok=LobOk(c,box,f);Row(box,ServeType.Lob,x,y,d,f,ok);
                            if(ok&&(best==null||Math.Round(d,6)<Math.Round(best.Value.d,6)))best=(x,y,d);
                        }
                        Assert.That(best.HasValue,Is.True,"a lob aim for the "+box+" box");SetAim(chosen,box,ServeType.Lob,best.Value.x,best.Value.y);
                    }
                    var (dx,dy)=Aim(today,box,ServeType.Drive);var dc=Ctx(chosen);var df=Fly(dc,box,ServeType.Drive);var lf=Fly(dc,box,ServeType.Lob);
                    keep=DriveOk(dc,df,lf);Row(box,ServeType.Drive,dx,dy,0,df,keep);
                    if(!keep)
                    {
                        (double x,double y,double d)? best=null;
                        for(int i=-30;i<=30;i++)for(int j=0;j<=20;j++)
                        {
                            double x=Math.Round(dx+i*Step,2),y=Math.Round(1.6+j*Step,2),d=Math.Sqrt((x-dx)*(x-dx)+(y-dy)*(y-dy));
                            var p=chosen.Clone();SetAim(p,box,ServeType.Drive,x,y);var c=Ctx(p);var f=Fly(c,box,ServeType.Drive);bool ok=DriveOk(c,f,Fly(c,box,ServeType.Lob));Row(box,ServeType.Drive,x,y,d,f,ok);
                            if(ok&&(best==null||Math.Round(d,6)<Math.Round(best.Value.d,6)))best=(x,y,d);
                        }
                        Assert.That(best.HasValue,Is.True,"a drive aim for the "+box+" box");SetAim(chosen,box,ServeType.Drive,best.Value.x,best.Value.y);
                    }
                }
            }
            // Final check of the chosen set, both boxes and both kinds, plus the Auto serve (switch off) which aims do not touch.
            var final=Ctx(chosen);var sb=new StringBuilder();
            foreach(ServiceBox box in new[]{ServiceBox.Right,ServiceBox.Left})
            {
                var l=Fly(final,box,ServeType.Lob);var d=Fly(final,box,ServeType.Drive);
                Assert.That(LobOk(final,box,l),Is.True,"final lob "+box);Assert.That(DriveOk(final,d,l),Is.True,"final drive "+box);
                foreach(var type in new[]{ServeType.Lob,ServeType.Drive})
                {
                    var (bx,by)=Aim(today,box,type);var (ax,ay)=Aim(chosen,box,type);var f=type==ServeType.Lob?l:d;
                    sb.Append(box+"\t"+type+"\tbefore "+F(bx)+","+F(by)+"\tafter "+F(ax)+","+F(ay)+"\tfrontY "+F(f.FrontY)+"\tbounce "+F(f.BounceX)+","+F(f.BounceZ)+"\tinside "+F(f.Inside)+"\tminPass "+F(f.Pass)+"\n");
                }
            }
            var off=chosen.Clone();off.ServeKindOn=0;
            sb.Append("RallyParams before "+today.Hash+" after "+chosen.Hash+"\n");
            sb.Append("RallyParams switch off before "+new RallyParams{ServeKindOn=0}.Hash+" after "+off.Hash+"\n");
            File.WriteAllText(Path.Combine(dir,"chosen.txt"),sb.ToString(),Utf8);
        }
        // The frozen RallySim replay references with the exact contexts and seeds of FatigueIntegrationTests.Off_... ("off", also read by
        // RivalAiTests.Fondista_..., first two matches) and FatigueArmRallyTests.A1_... ("arm0"): automatic serve (switch off), four matches,
        // same column layout as the 18 Nov points.tsv (signature in column 9).
        [Test,Explicit,Category("Experiments"),Timeout(7200000)]
        public void WriteFatigueReplayReferences()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string dir=Path.Combine(root,"Logs/experiments/serve-positions/fatigue-replay");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");
            foreach(var (name,edited) in new[]{("off",preset.With("fatigue.enabled",0)),
                ("arm0",preset.With("fatigue.armSpeedMax",0).With("fatigue.positionGainMax",0).With("fatigue.costPerMeter",.4).With("fatigue.recoverBetween",.25))})
            {
                var ctx=new RallyContext(EffectiveConfig.From(edited),new RallyParams{ServeKindOn=0},factory);
                Directory.CreateDirectory(Path.Combine(dir,name));
                using(var writer=new StreamWriter(Path.Combine(dir,name,"points.tsv"),false,Utf8))
                {
                    writer.WriteLine("match\tpoint\thumanBefore\trivalBefore\tserver\twinner\tshots\tsteps\trealSeconds\tsignature");
                    for(int i=0;i<4;i++)
                    using(var sim=new RallySim(ctx,new BotBrain(ctx,30,20261117+2*i),new FondoAi(ctx,AiDifficulty.For(ctx.Params,AiLevel.Medium),20261118+2*i)))
                    {
                        var score=new MatchScore(7,2,i%2==0?PlayerId.Human:PlayerId.Rival);int point=0;
                        while(!score.Finished)
                        {
                            var o=sim.PlayPoint(score.Server,score.Box);
                            writer.WriteLine(string.Join("\t",F(i),F(point),F(score.Human),F(score.Rival),score.Server.ToString(),o.Winner?.ToString()??"",
                                F(o.Shots),F(o.Steps),F(o.RealSeconds),FatigueMatchExperiment.Signature(o,sim)));
                            score.PointWon(o.Winner.Value);point++;
                        }
                    }
                }
            }
        }
    }
}
