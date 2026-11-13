using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Rally;
using UnityEngine;
namespace SquashClash.Core.Tests.Rally
{
    public sealed class RallySimTests
    {
        private sealed class NeverSwing : IRallyBrain
        {
            public int PerceptionSteps=>0;public Vector2 StanceNoise()=>Vector2.zero;
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own)=>default;
        }
        private sealed class Counting : IRallyBrain
        {
            private readonly IRallyBrain inner;public int Decisions;public Counting(IRallyBrain inner){this.inner=inner;}
            public int PerceptionSteps=>inner.PerceptionSteps;public Vector2 StanceNoise()=>inner.StanceNoise();
            public AiShot Decide(in AiObservation observation,in ReceptionPlan own){Decisions++;return inner.Decide(observation,own);}
        }
        internal static IRallyBrain Fondo(RallyContext context,long seed,AiLevel level=AiLevel.Medium)=>new FondoAi(context,AiDifficulty.For(context.Params,level),seed);
        internal static IRallyBrain Bot(RallyContext context,long seed,double sigmaMs=30)=>new BotBrain(context,sigmaMs,seed);
        private static string Signature(RallyOutcome o)=>o.End+"/"+o.Cause+"/"+o.Winner+"/"+o.Shots+"/"+o.Steps+"/"+string.Join(",",o.Receptions.Select(r=>r.Who+":"+r.Result+":"+r.ContactStep));

        [Test] public void Rally_EndsWithATerminalEvent()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,Bot(context,3),Fondo(context,4)))
            {
                var outcome=sim.Play(PlayerId.Human);
                Assert.That(outcome.End,Is.EqualTo(RallyEnd.Point));Assert.That(outcome.Cause,Is.Not.EqualTo(PointEndCause.None));Assert.That(outcome.Winner,Is.Not.Null);
                Assert.That(outcome.Steps,Is.GreaterThan(0));Assert.That(outcome.RealSeconds,Is.EqualTo(outcome.SimSeconds/context.Tempo).Within(1e-9));
                Assert.That(sim.Rules.Closed,Is.True);
            }
        }
        [Test] public void WithoutASwing_TheReceiverLosesOnTheSecondBounce()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,new NeverSwing(),Fondo(context,4)))
            {
                var outcome=sim.Play(PlayerId.Human);
                Assert.That(outcome.Cause,Is.EqualTo(PointEndCause.DoubleBounce));Assert.That(outcome.Winner,Is.EqualTo(PlayerId.Rival));
                Assert.That(outcome.Shots,Is.EqualTo(0));Assert.That(outcome.LastReception,Is.EqualTo(ReceptionResult.NoSwing));
            }
        }
        // B4 follow-up A1: the frozen causal reference belongs to one reception. A reception that does not swing
        // (here the rival never does) must not leave the previous reception's reference reachable.
        [Test] public void CausalReference_DoesNotSurviveIntoAReceptionWithoutASwing()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,Bot(context,1,0),new NeverSwing()))
            {
                var outcome=sim.Play(PlayerId.Human);
                Assert.That(outcome.Receptions[0].Who,Is.EqualTo(PlayerId.Human));Assert.That(outcome.Receptions[0].Result,Is.EqualTo(ReceptionResult.Contact),Signature(outcome));
                Assert.That(outcome.Receptions[0].Vectors,Is.Not.Null,"the human contact used its own frozen reference");
                var last=outcome.Receptions[outcome.Receptions.Count-1];
                Assert.That(last.Who,Is.EqualTo(PlayerId.Rival));Assert.That(last.Result,Is.EqualTo(ReceptionResult.NoSwing).Or.EqualTo(ReceptionResult.Unreachable));
                var field=typeof(RallySim).GetField("pendingCausal",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                Assert.That(field,Is.Not.Null);
                Assert.That(field.GetValue(sim),Is.Null,"a reception without a swing kept the previous reception's causal reference");
            }
        }
        [Test] public void PerfectTiming_ProducesAnExchange()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,Bot(context,1,0),Bot(context,2,0)))
            {
                var outcome=sim.Play(PlayerId.Human);
                Assert.That(outcome.Shots,Is.GreaterThanOrEqualTo(3),Signature(outcome));
                var contacts=outcome.Receptions.Where(r=>r.Result==ReceptionResult.Contact).ToArray();
                Assert.That(contacts.Length,Is.EqualTo(outcome.Shots));Assert.That(contacts.All(c=>c.Perfect),Is.True);
            }
        }
        [Test] public void Turns_Alternate_AndOnlyTheLastReceptionFails()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,Bot(context,5),Fondo(context,6)))
                for(int rally=0;rally<20;rally++)
                {
                    var first=rally%2==0?PlayerId.Human:PlayerId.Rival;var outcome=sim.Play(first);
                    Assert.That(outcome.Receptions.Count,Is.GreaterThan(0));
                    for(int i=0;i<outcome.Receptions.Count;i++)
                    {
                        Assert.That(outcome.Receptions[i].Who,Is.EqualTo(i%2==0?first:first.Other()),"turn "+i+" of "+Signature(outcome));
                        if(i<outcome.Receptions.Count-1)Assert.That(outcome.Receptions[i].Result,Is.EqualTo(ReceptionResult.Contact));
                    }
                    Assert.That(outcome.RejectedContacts,Is.EqualTo(0));
                }
        }
        [Test] public void EachReception_GetsExactlyOneAttempt()
        {
            var context=RallyFixtures.Context();var human=new Counting(Bot(context,7));var rival=new Counting(Fondo(context,8));int receptions=0;
            using(var sim=new RallySim(context,human,rival))
                for(int rally=0;rally<10;rally++)receptions+=sim.Play(PlayerId.Human).Receptions.Count;
            Assert.That(human.Decisions+rival.Decisions,Is.EqualTo(receptions));
        }
        [Test] public void SameSeeds_ReproduceTheSameRallies()
        {
            var context=RallyFixtures.Context();
            List<string> Run(long seed)
            {
                var list=new List<string>();
                using(var sim=new RallySim(context,Bot(context,seed),Fondo(context,seed+100)))
                    for(int rally=0;rally<12;rally++)list.Add(Signature(sim.Play(rally%2==0?PlayerId.Human:PlayerId.Rival)));
                return list;
            }
            Assert.That(Run(21),Is.EqualTo(Run(21)));Assert.That(Run(21),Is.Not.EqualTo(Run(22)));
        }
        [Test] public void Bodies_ApproachAndRecover_DuringTheRally()
        {
            var context=RallyFixtures.Context();
            using(var sim=new RallySim(context,Bot(context,1,0),Bot(context,2,0)))
            {
                var outcome=sim.Play(PlayerId.Human);Assert.That(outcome.Shots,Is.GreaterThanOrEqualTo(2));
                Assert.That(Vector2.Distance(sim.Human.Position,context.WaitPosition(PlayerId.Human))+Vector2.Distance(sim.Rival.Position,context.WaitPosition(PlayerId.Rival)),Is.GreaterThan(.2f));
                float margin=(float)context.Params.BodyWallMargin-1e-4f;
                foreach(var body in new[]{sim.Human,sim.Rival})
                {Assert.That(body.Position.x,Is.InRange(margin,context.Court.Width-margin));Assert.That(body.Position.y,Is.InRange(margin,context.Court.Length-margin));}
            }
        }
        [Test] public void ElapsedSteps_ShortenTheTimeAvailableToThePlanner()
        {
            var context=RallyFixtures.Context();var trajectory=context.Predict(context.NeutralLaunch(PlayerId.Human));
            var body=RallyFixtures.Body(context,PlayerId.Human,new Vector2(5.6f,3));var rules=RallyFixtures.Rules(context,PlayerId.Rival,PlayerId.Human);
            var onTime=ReceptionPlanner.Plan(context,trajectory,rules,body,18,1);var late=ReceptionPlanner.Plan(context,trajectory,rules,body,18,1,default,40);
            Assert.That(onTime.Reachable,Is.True);Assert.That(!late.Reachable||late.SlackS<onTime.SlackS,Is.True);
            if(late.Reachable)Assert.That(late.ArrivalStep,Is.GreaterThanOrEqualTo(onTime.ArrivalStep));
        }
        // R4 headless validation: AI (Fondo, Medium) against the scripted bot, at least 1000 rallies.
        [Test,Timeout(600000)] public void Simulation_OfAThousandRallies_AlwaysTerminates()
        {
            const int Rallies=1000;var context=RallyFixtures.Context();var outcomes=new List<RallyOutcome>(Rallies);
            var watch=System.Diagnostics.Stopwatch.StartNew();
            using(var sim=new RallySim(context,Bot(context,2026),Fondo(context,1111)))
                for(int rally=0;rally<Rallies;rally++)outcomes.Add(sim.Play(rally%2==0?PlayerId.Human:PlayerId.Rival));
            watch.Stop();
            string directory=RallyReport.Write(context,outcomes,"fondo-medium-vs-bot30","Bot σ=30 ms (paralelo, altura media, potencia media) contra Fondo Medio",watch.Elapsed.TotalSeconds);
            TestContext.WriteLine("RALLY_SIM="+directory);
            Assert.That(outcomes.Count(o=>o.End==RallyEnd.Stalled),Is.EqualTo(0),"blocked rallies");
            Assert.That(outcomes.Count(o=>o.End==RallyEnd.StepCap),Is.EqualTo(0),"rallies cut by the time limit");
            Assert.That(outcomes.Count(o=>o.End==RallyEnd.LaunchFault),Is.EqualTo(0),"neutral launch faults");
            Assert.That(outcomes.Count(o=>o.End==RallyEnd.Point),Is.GreaterThanOrEqualTo(Rallies*99/100),"rallies ending on a terminal event");
            Assert.That(outcomes.Sum(o=>o.RejectedContacts),Is.EqualTo(0));
            // Contacts in the very step that closed the point are ignored and counted; nothing after that step is simulated.
            Assert.That(outcomes.Max(o=>o.IgnoredAfterClose),Is.LessThanOrEqualTo(2));
            Assert.That(outcomes.All(o=>o.End!=RallyEnd.Point||(o.Winner.HasValue&&o.Cause!=PointEndCause.None)),Is.True);
        }
    }
    internal static class RallyReport
    {
        private static readonly CultureInfo C=CultureInfo.InvariantCulture;
        private static double Percentile(List<double> sorted,double p)=>sorted.Count==0?0:sorted[(int)Math.Min(sorted.Count-1,Math.Floor(p*sorted.Count))];
        internal static string Write(RallyContext context,List<RallyOutcome> outcomes,string name,string description,double wallSeconds)
        {
            string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
            string directory=Path.Combine(root,"Logs/rally-sim",DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'",C)+"-"+name);Directory.CreateDirectory(directory);
            int n=outcomes.Count;var shots=outcomes.Select(o=>(double)o.Shots).OrderBy(v=>v).ToList();var seconds=outcomes.Select(o=>o.RealSeconds).OrderBy(v=>v).ToList();
            var text=new StringBuilder();
            text.Append("# Simulación headless de rallies — "+name+"\n\n"+description+".\n\n");
            text.Append("- Rallies: "+n+"\n- Defaults: `"+context.Config.Hash+"`, SimulationKey `"+context.Config.SimulationKey+"`, RallyParams `"+context.Params.Hash+"`\n");
            text.Append("- Tiempo de cómputo: "+wallSeconds.ToString("0.0",C)+" s\n\n## Cómo terminan\n\n| Fin | Rallies | % |\n|---|---:|---:|\n");
            foreach(RallyEnd end in Enum.GetValues(typeof(RallyEnd)))
            {int k=outcomes.Count(o=>o.End==end);text.Append("| "+end+" | "+k+" | "+(100.0*k/n).ToString("0.0",C)+" |\n");}
            text.Append("\n## Causas del punto\n\n| Causa | Detalle de la última recepción | Rallies | % |\n|---|---|---:|---:|\n");
            foreach(var group in outcomes.Where(o=>o.End==RallyEnd.Point).GroupBy(o=>o.Cause+"|"+(o.LastReception?.ToString()??"—")).OrderByDescending(g=>g.Count()))
            {var parts=group.Key.Split('|');text.Append("| "+parts[0]+" | "+parts[1]+" | "+group.Count()+" | "+(100.0*group.Count()/n).ToString("0.0",C)+" |\n");}
            text.Append("\n## Pelotas fuera, tin y no llega (quién pegó y dónde terminó)\n\n| Causa | Autor | Superficie | Tras el bote | Rallies |\n|---|---|---|---|---:|\n");
            foreach(var group in outcomes.Where(o=>o.End==RallyEnd.Point&&o.Cause!=PointEndCause.DoubleBounce).GroupBy(o=>(o.Cause,o.Striker,o.TerminalSurface,o.OutAfterBounce)).OrderByDescending(g=>g.Count()))
                text.Append("| "+group.Key.Cause+" | "+(group.Key.Striker?.ToString()??"—")+" | "+group.Key.TerminalSurface+" | "+(group.Key.OutAfterBounce?"sí":"no")+" | "+group.Count()+" |\n");
            text.Append("\n## Ganador\n\n| Ganador | Rallies | % |\n|---|---:|---:|\n");
            foreach(var group in outcomes.GroupBy(o=>o.Winner?.ToString()??"Nadie").OrderBy(g=>g.Key))
                text.Append("| "+group.Key+" | "+group.Count()+" | "+(100.0*group.Count()/n).ToString("0.0",C)+" |\n");
            text.Append("\n## Duración\n\n| Medida | Golpes | Segundos reales |\n|---|---:|---:|\n");
            text.Append("| Media | "+shots.Average().ToString("0.00",C)+" | "+seconds.Average().ToString("0.00",C)+" |\n");
            foreach(var p in new[]{(.5,"Mediana"),(.9,"p90"),(.99,"p99")})
                text.Append("| "+p.Item2+" | "+Percentile(shots,p.Item1).ToString("0",C)+" | "+Percentile(seconds,p.Item1).ToString("0.00",C)+" |\n");
            text.Append("| Máximo | "+shots.Last().ToString("0",C)+" | "+seconds.Last().ToString("0.00",C)+" |\n");
            text.Append("\n## Recepciones por jugador\n\n| Jugador | Recepciones | Contacto | Perfecto | Al aire | Tarde | No llega | Sin golpe | Falla del sistema | Holgura media (s) |\n|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|\n");
            foreach(PlayerId who in new[]{PlayerId.Human,PlayerId.Rival})
            {
                var all=outcomes.SelectMany(o=>o.Receptions).Where(r=>r.Who==who).ToArray();if(all.Length==0)continue;
                int Count(ReceptionResult result)=>all.Count(r=>r.Result==result);
                text.Append("| "+who+" | "+all.Length+" | "+Count(ReceptionResult.Contact)+" | "+all.Count(r=>r.Perfect)+" | "+Count(ReceptionResult.Air)+" | "+Count(ReceptionResult.Late)+" | "+Count(ReceptionResult.Unreachable)+" | "+Count(ReceptionResult.NoSwing)+" | "+(Count(ReceptionResult.SystemFault)+Count(ReceptionResult.NotEvaluable))+" | "+all.Where(r=>r.Reachable).Select(r=>r.SlackS).DefaultIfEmpty(0).Average().ToString("0.000",C)+" |\n");
            }
            text.Append("\n## Golpes elegidos por la IA (contactos)\n\n| Línea | Altura | Potencia | Golpes | % |\n|---|---|---|---:|---:|\n");
            var chosen=outcomes.SelectMany(o=>o.Receptions).Where(r=>r.Who==PlayerId.Rival&&r.Result==ReceptionResult.Contact).ToArray();
            foreach(var group in chosen.GroupBy(r=>(r.Line,r.Height,r.Power)).OrderByDescending(g=>g.Count()))
                text.Append("| "+group.Key.Line+" | "+group.Key.Height+" | "+group.Key.Power+" | "+group.Count()+" | "+(100.0*group.Count()/Math.Max(1,chosen.Length)).ToString("0.0",C)+" |\n");
            text.Append("\nFallas del sistema: "+outcomes.Sum(o=>o.SystemFaults)+". Contactos rechazados por turno: "+outcomes.Sum(o=>o.RejectedContacts)+". Eventos tras el cierre: "+outcomes.Sum(o=>o.IgnoredAfterClose)+".\n");
            File.WriteAllText(Path.Combine(directory,"report.md"),text.ToString());
            var tsv=new StringBuilder("rally\tfirstReceiver\tend\tcause\twinner\tshots\tsteps\trealSeconds\tlastReception\tsystemFaults\n");
            for(int i=0;i<n;i++){var o=outcomes[i];tsv.Append(i+"\t"+o.FirstReceiver+"\t"+o.End+"\t"+o.Cause+"\t"+(o.Winner?.ToString()??"")+"\t"+o.Shots+"\t"+o.Steps+"\t"+o.RealSeconds.ToString("R",C)+"\t"+(o.LastReception?.ToString()??"")+"\t"+o.SystemFaults+"\n");}
            File.WriteAllText(Path.Combine(directory,"rallies.tsv"),tsv.ToString());
            return directory;
        }
    }
}
