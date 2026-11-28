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
    // Serve positions, path 3 (SC-PM, 26 nov): the server position inside the box closest to the inner front corner from which the
    // Drive, the Lob and the Auto serve stay legal with the current aims (RallyParams untouched), with a margin. Explicit, outside
    // regression; nothing in production changes. The candidate is a mirrored pair of insets (from the box's inner line and behind the
    // short line), judged in both boxes:
    //  - every serve is legal by the serve rules (ServePlanner.Forecast; Auto by ServePlanner.Find from the candidate);
    //  - its first bounce lies at least Margin inside what the rules require: past the half-court line and behind the short line;
    //  - before that bounce the ball never enters the 1.80 m reference capsule at the receiver position.
    // The body stays Inset inside the box lines as the spec's corner (0.30 m) and inside the body wall margin.
    // Never overwrites evidence.
    public sealed class ServePositionsSearch
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        internal const float Margin=.3f,Inset=.3f,Step=.05f;
        private static string F(double v)=>double.IsNaN(v)?"NaN":v.ToString("0.####",C);
        internal static Vector2 At(CourtDimensions court,ServiceBox box,float fromInner,float behindShort)
        {
            float x=court.ServiceBoxInner-fromInner;
            return new Vector2(box==ServiceBox.Right?x:court.Width-x,court.Length-court.ShortLineNearBackEdgeFromBack+behindShort);
        }
        // How far inside the required landing area the first bounce is (negative: outside).
        internal static float Inside(CourtDimensions court,ServiceBox box,Vector3 bounce)
        {
            float mid=court.Width*.5f,shortLine=court.Length-court.ShortLineNearBackEdgeFromBack;
            float side=box==ServiceBox.Right?bounce.x-mid:mid-bounce.x;
            return Mathf.Min(side,bounce.z-shortLine);
        }
        [Test,Explicit,Category("Experiments"),Timeout(14400000)]
        public void WriteServerSearch()
        {
            var (root,preset,factory)=ServeDiagnosisExperiment.Setup();
            string dir=Path.Combine(root,"Logs/experiments/serve-positions/search");
            Assert.That(Directory.Exists(dir),Is.False,"Never overwrite evidence.");Directory.CreateDirectory(dir);
            var ctx=new RallyContext(EffectiveConfig.From(preset),new RallyParams(),factory);
            var auto=new RallyContext(EffectiveConfig.From(preset),new RallyParams{ServeKindOn=0},factory);
            var court=ctx.Court;float clear=ServePositionsExperiment.CapsuleRadius+ctx.Physics.BallRadius;
            float maxInner=court.ServiceBoxInner-(float)ctx.Params.BodyWallMargin,maxBehind=court.ServiceBoxInner-Inset;
            var rows=new List<(float fi,float bs,float dist,bool ok,string detail)>();
            using(var w=new StreamWriter(Path.Combine(dir,"grid.tsv"),false,new UTF8Encoding(false)))
            {
                w.WriteLine("fromInner\tbehindShort\tdistToCorner\tbox\ttype\tlegal\tbounceX\tbounceZ\tinsideM\tminPassM\tok");
                for(int i=0;Inset+i*Step<=maxInner+1e-4f;i++)
                    for(int j=0;Inset+j*Step<=maxBehind+1e-4f;j++)
                    {
                        float fi=(float)Math.Round(Inset+i*Step,2),bs=(float)Math.Round(Inset+j*Step,2);
                        float dist=Mathf.Sqrt((fi-Inset)*(fi-Inset)+(bs-Inset)*(bs-Inset));bool all=true;
                        foreach(ServiceBox box in new[]{ServiceBox.Right,ServiceBox.Left})
                        {
                            var at=At(court,box,fi,bs);var receiver=ServePlanner.ReceiverPosition(ctx,box);
                            foreach(var type in new[]{ServeType.Drive,ServeType.Lob,ServeType.Auto})
                            {
                                var c=type==ServeType.Auto?auto:ctx;BallState serve=default;bool legal;Vector3 bounce=default;
                                try{serve=type==ServeType.Auto?ServePlanner.Find(c,box,at):ServePlanner.Typed(c,box,type,at);legal=ServePlanner.Forecast(c,serve,box,out bounce);}
                                catch(InvalidOperationException){legal=false;}
                                float inside=legal?Inside(court,box,bounce):float.NaN;float pass=float.NaN;
                                if(legal&&inside>=Margin)pass=ServePositionsExperiment.Pass(c,c.Predict(serve),receiver).minInside;
                                bool ok=legal&&inside>=Margin&&(float.IsNaN(pass)||pass>=clear);
                                all&=ok;
                                w.WriteLine(string.Join("\t",new[]{F(fi),F(bs),F(dist),box.ToString(),type.ToString(),legal?"1":"0",legal?F(bounce.x):"NaN",legal?F(bounce.z):"NaN",F(inside),F(pass),ok?"1":"0"}));
                                if(!ok)break;
                            }
                            if(!all)break;
                        }
                        rows.Add((fi,bs,dist,all,""));
                    }
            }
            var chosen=rows.Where(r=>r.ok).OrderBy(r=>Math.Round(r.dist,4)).ThenBy(r=>r.bs).ThenBy(r=>r.fi).ToList();
            File.WriteAllText(Path.Combine(dir,"chosen.txt"),chosen.Count==0?"none\n":
                "fromInner "+F(chosen[0].fi)+"\tbehindShort "+F(chosen[0].bs)+"\tdistToCorner "+F(chosen[0].dist)+"\tcandidates "+rows.Count+"\tok "+chosen.Count+"\n"+
                string.Join("\n",chosen.Take(10).Select(r=>F(r.fi)+"\t"+F(r.bs)+"\t"+F(r.dist)))+"\n",new UTF8Encoding(false));
        }
    }
}
