using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using SquashClash.Core.Config;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.App;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace SquashClash.Capture
{
    // Backhand-pose diagnosis (experiment, not regression): how often each stroke side is chosen and drawn in seeded Demo rallies.
    // Writes backhand-diagnosis.tsv under SQUASH_CAPTURE_DIR/<set>. Separate capture assembly: no regression counts.
    public sealed class ZBackhandDiagnosis
    {
        private AppBootstrap app;private ShotController controller;private SwingView swing;private StrokeFx fx;private RallyDirector rally;
        private CharacterView human,rival;private bool wasLeft;private double StepReal=>1d/app.Config.Physics.StepHz/app.Config.Tempo;
        private readonly StringBuilder rows=new StringBuilder();
        private IEnumerator Boot(bool left,int seed)
        {
            wasLeft=Handedness.Left;Handedness.Left=left;FirstRunFlow.OpenAtBoot=false;
            if(AppBootstrap.Instance!=null){UnityEngine.Object.Destroy(AppBootstrap.Instance.gameObject);yield return null;}
            yield return SceneManager.LoadSceneAsync("Main");yield return null;yield return null;
            app=AppBootstrap.Instance;app.GetComponent<ResumeOverlay>().Continue();app.Clock.Paused=false;app.DevTools=false;
            app.Tuning.LoadPreset(PresetCodec.FromSet(app.Tuning.Defaults,"backhand diagnosis",app.Build.Label),new List<string>());
            app.Tuning.Set("launcher.repeatSeconds",0);app.LaunchCurrent();
            controller=app.GetComponent<ShotController>();swing=app.GetComponent<SwingView>();fx=app.GetComponent<StrokeFx>();rally=app.Rally;
            human=app.Players.Human.Find("Figure").GetComponent<CharacterView>();rival=app.Players.Rival.Find("Figure").GetComponent<CharacterView>();
            yield return null;yield return null;
            app.enabled=app.Host.enabled=controller.enabled=swing.enabled=fx.enabled=rally.enabled=human.enabled=rival.enabled=false;
            app.GetComponent<ImpactFx>().enabled=false;
            controller.PresentationDelivered+=p=>{if(p.HasContact)Row("human",swing.ShoulderSign<0?StrokeSide.Drive:StrokeSide.Backhand,app.Players.Human,app.Host.RenderPosition,seed);};
            rally.RivalSwing+=(side,contact)=>{if(contact)Row("rival",side,app.Players.Rival,app.Host.RenderPosition,seed);};
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Medium;rally.Seed=seed;
            Assert.That(rally.StartMode(),Is.True);
            yield return null;yield return null;
        }
        private void Row(string who,StrokeSide side,Transform player,Vector3 ball,int seed)
        {
            float dx=ball.x-player.position.x;
            // The drawn figure: across = the arm in use reaches over the body (the human's chosen hand; the rival is always right-handed).
            float sign=side==StrokeSide.Drive?-1:1;float holds=who=="human"&&Handedness.Left?1:-1;
            rows.AppendLine(string.Join("\t",who,Handedness.Left?"zurdo":"diestro",seed,side,dx.ToString("F2"),holds!=sign?"across":"same-side"));
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            var metrics=app!=null?app.Metrics?.CurrentFilePath:null;
            if(app!=null)UnityEngine.Object.Destroy(app.gameObject);yield return null;
            if(metrics!=null&&File.Exists(metrics))File.Delete(metrics);
            Handedness.Left=wasLeft;FirstRunFlow.ResetOpenAtBoot();
        }
        [UnityTest] public IEnumerator TallyStrokeSides()
        {
            string set=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_SET")??"backhand-diagnosis";
            string root=Environment.GetEnvironmentVariable("SQUASH_CAPTURE_DIR")??Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Logs","capture");
            var directory=Path.Combine(root,set);Directory.CreateDirectory(directory);
            rows.AppendLine("who\thand\tseed\tside\tball_dx_m\tdrawn");
            foreach(bool left in new[]{false,true})
            foreach(int seed in new[]{9,11,13})
            {
                yield return Boot(left,seed);
                int limit=(int)(150/StepReal);
                for(int frame=0;frame<limit;frame++)
                {
                    float dt=(float)StepReal;
                    app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();
                    swing.Tick(dt);fx.Tick(dt);human.Animate(dt);rival.Animate(dt);
                    if(frame%240==0)yield return null;
                }
                yield return Cleanup();
            }
            File.WriteAllText(Path.Combine(directory,"backhand-diagnosis.tsv"),rows.ToString());
            Debug.Log("[SquashClash] backhand diagnosis written to "+directory);
        }
    }
}
