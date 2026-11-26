using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    // Serve positions (spec docs/superpowers/specs/2026-11-26-posiciones-saque.md). Explicit, outside regression and AutoQA; it is run
    // once on the base (before) and once on the branch (after) with the same seeds, the default preset and the default RallyParams.
    //  1. reception.tsv: RallySim at Medium against the three rival styles (newcomer sigma 90 ms as the human, the lob as its serve, as
    //     the M1 measurement), the first return of every serve, split by receiver (the human receives the rival's serve and vice versa);
    //  2. geometry.tsv: every serve of the product (Auto with «Saque nuevo» off, Lob and Drive) from the server position the code uses
    //     now, and the Lob and Drive replayed from the proposed corner, against the receiver's proposed start: the closest horizontal
    //     pass of the ball inside the 1.80 m reference capsule (radius 0.28 m) before the first floor bounce.
    // Never overwrites evidence.
    public sealed class ServePositionsExperiment
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static readonly UTF8Encoding Utf8=new UTF8Encoding(false);
        private static readonly RivalStyleId[] Styles={RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo};
        internal const float CapsuleRadius=.28f,CapsuleHeight=1.8f;
        private static string F(double v)=>double.IsNaN(v)?"NaN":v.ToString("0.####",C);
        // The positions the spec proposes, written out here so the base run can preview them before the code has them.
        internal static Vector2 ProposedServer(CourtDimensions court,ServiceBox box)
        {
            float x=court.ServiceBoxInner-.3f,z=court.Length-court.ShortLineNearBackEdgeFromBack+.3f;
            return new Vector2(box==ServiceBox.Right?x:court.Width-x,z);
        }
        internal static Vector2 ProposedReceiver(CourtDimensions court,ServiceBox box)
            =>new Vector2(box==ServiceBox.Right?court.Width-1.6f:1.6f,court.Length-2.1f);
        // The same construction as ServePlanner.Typed, from any origin.
        private static BallState TypedFrom(RallyContext ctx,Vector2 at,ServiceBox box,ServeType type)
        {
            var p=ctx.Params;bool right=box==ServiceBox.Right,lob=type==ServeType.Lob;
            double x=lob?(right?p.ServeKindLobRightAimX:p.ServeKindLobLeftAimX):(right?p.ServeKindDriveRightAimX:p.ServeKindDriveLeftAimX);
            double y=lob?(right?p.ServeKindLobRightAimY:p.ServeKindLobLeftAimY):(right?p.ServeKindDriveRightAimY:p.ServeKindDriveLeftAimY);
            double speed=lob?(right?p.ServeKindLobRightSpeed:p.ServeKindLobLeftSpeed):(right?p.ServeKindDriveRightSpeed:p.ServeKindDriveLeftSpeed);
            var from=new Vector3(at.x,(float)p.ServeHandHeight,at.y);
            var center=Ballistics.CenterForContact(SurfaceId.FrontWall,new Vector3((float)x,(float)y,0),ctx.Physics.BallRadius);
            return new BallState{Position=from,Velocity=Ballistics.SolveCenterTo(from,center,(float)speed,ctx.Physics.Gravity)};
        }
        // Closest horizontal pass of the ball centre to the receiver while the ball is inside the capsule's height, up to the first floor
        // bounce; NaN when the ball never comes below the capsule's top.
        internal static (float minInside,float heightAtMin,bool firstBounce) Pass(RallyContext ctx,Trajectory t,Vector2 receiver)
        {
            long bounce=long.MaxValue;
            foreach(var c in t.Contacts)if(!c.Persistent&&c.Surface==SurfaceId.Floor){bounce=c.Step;break;}
            float best=float.NaN,height=float.NaN;float r=ctx.Physics.BallRadius;
            foreach(var s in t.States)
            {
                if(s.Step>bounce)break;
                if(s.Position.y-r>CapsuleHeight)continue;
                float d=Vector2.Distance(new Vector2(s.Position.x,s.Position.z),receiver);
                if(float.IsNaN(best)||d<best){best=d;height=s.Position.y;}
            }
            return (best,height,bounce!=long.MaxValue);
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void MeasureServePositions()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string run=Environment.GetEnvironmentVariable("SC_POSITIONS_RUN")??"before";
            int matches=int.Parse(Environment.GetEnvironmentVariable("SC_POSITIONS_MATCHES")??"100",C);
            string dir=Path.Combine(root,"Logs/experiments/serve-positions/"+run);
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            Assert.That(ctx.ServeKindOn,Is.True,"default RallyParams: «Saque nuevo» on");
            var auto=new RallyContext(EffectiveConfig.From(preset),new RallyParams{ServeKindOn=0},factory);
            var watch=Stopwatch.StartNew();
            using(var w=new StreamWriter(Path.Combine(dir,"geometry.tsv"),false,Utf8))
            {
                w.WriteLine("origin\tbox\ttype\tserverX\tserverZ\treceiverX\treceiverZ\tminInsideM\theightAtMinM\tclearM\tthrough");
                foreach(ServiceBox box in new[]{ServiceBox.Right,ServiceBox.Left})
                {
                    var receiver=ProposedReceiver(ctx.Court,box);
                    void Row(string origin,RallyContext c,ServeType type,in BallState serve)
                    {
                        var (min,h,_)=Pass(c,c.Predict(serve),receiver);float clear=CapsuleRadius+c.Physics.BallRadius;
                        w.WriteLine(string.Join("\t",new[]{origin,box.ToString(),type.ToString(),F(serve.Position.x),F(serve.Position.z),F(receiver.x),F(receiver.y),F(min),F(h),F(clear),(!float.IsNaN(min)&&min<clear)?"1":"0"}));
                    }
                    Row("code",auto,ServeType.Auto,auto.Serve(box,ServeType.Auto));
                    foreach(var type in new[]{ServeType.Lob,ServeType.Drive})
                    {
                        Row("code",ctx,type,ctx.Serve(box,type));
                        Row("proposed",ctx,type,TypedFrom(ctx,ProposedServer(ctx.Court,box),box,type));
                    }
                }
            }
            using(var points=new StreamWriter(Path.Combine(dir,"points.tsv"),false,Utf8))
            {
                points.WriteLine(ServeDiagnosisExperiment.PointHeader);
                for(int i=0;i<Styles.Length;i++)ServeDiagnosisExperiment.PlayCell(ctx,Styles[i],matches,20261126L+100000L*i,points);
            }
            var rows=File.ReadAllLines(Path.Combine(dir,"points.tsv")).Skip(1).Select(l=>l.Split('\t')).ToList();
            using(var s=new StreamWriter(Path.Combine(dir,"reception.tsv"),false,Utf8))
            {
                s.WriteLine("receiver\tstyle\tserves\tcontact\trate\tperfect\tunreachable\tlate\tair\tnoSwing\treceiverWon");
                foreach(var receiver in new[]{"Human","Rival"})
                    foreach(var style in Styles.Select(x=>x.ToString()).Concat(new[]{"all"}))
                    {
                        var cell=rows.Where(r=>r[3]!=receiver&&(style=="all"||r[0]==style)).ToList();
                        int n=cell.Count,contact=cell.Count(r=>r[8]=="Contact");
                        s.WriteLine(string.Join("\t",new[]{receiver,style,n.ToString(C),contact.ToString(C),F(n==0?double.NaN:100.0*contact/n),
                            cell.Count(r=>r[9]=="1").ToString(C),cell.Count(r=>r[8]=="Unreachable").ToString(C),cell.Count(r=>r[8]=="Late").ToString(C),
                            cell.Count(r=>r[8]=="Air").ToString(C),cell.Count(r=>r[8]=="NoSwing").ToString(C),cell.Count(r=>r[5]==receiver).ToString(C)}));
                    }
            }
            File.WriteAllText(Path.Combine(dir,"DONE.txt"),"matches per style "+matches+"; seeds 20261126+100000*style; seconds "+watch.Elapsed.TotalSeconds.ToString("0.0",C)+"\n",Utf8);
        }
    }
}
