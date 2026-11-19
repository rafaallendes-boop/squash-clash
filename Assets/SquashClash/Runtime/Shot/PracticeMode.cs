using System;
using SquashClash.Core.Config;
using SquashClash.Core.Shot;
using SquashClash.Core.Simulation;
using SquashClash.Runtime.App;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public enum PracticePhase { Reception, Resolved, Observing, Waiting, Unavailable }
    public sealed class PracticeMode : MonoBehaviour
    {
        private AppBootstrap app;
        private ShotController controller;
        private double waiting;
        private int selected=-1;
        public PracticePhase Phase {get;private set;}
        public bool Enabled=>app?.Host.ActiveConfig?.Source.GetBool("practice.enabled")??false;
        public void Initialize(AppBootstrap app,ShotController controller)
        {
            this.app=app;this.controller=controller;app.Host.Launched+=OnLaunch;controller.ShotResolved+=OnResolved;
            OnLaunch(app.Host.ActiveConfig);
        }
        private void OnLaunch(EffectiveConfig cfg){waiting=0;Phase=cfg.Source.GetBool("practice.enabled")&&cfg.Backend=="P2-Analytic"?PracticePhase.Reception:PracticePhase.Unavailable;}
        private void OnResolved(ShotRecord record)
        {
            if(record.LaunchId!=app.Host.LaunchId)return;
            Phase=PracticePhase.Resolved;
        }
        public bool TryLaunchCurrent()
        {
            if(app==null||!app.Tuning.Visible.GetBool("practice.enabled"))return false;
            var before=EffectiveConfig.From(app.Tuning.Visible);int index=before.Source.GetInt("practice.launch");
            var recipe=PracticeLaunchDefinitions.At(before,index);
            if(index!=selected)
            {
                // Recipe selection positions the fixed body; subsequent explicit human edits apply next launch.
                if((float)app.Tuning.Applied.Get("human.x")!=recipe.Body.PositionXZ.x)app.Tuning.Set("human.x",recipe.Body.PositionXZ.x);
                if((float)app.Tuning.Applied.Get("human.z")!=recipe.Body.PositionXZ.y)app.Tuning.Set("human.z",recipe.Body.PositionXZ.y);
                selected=index;
            }
            var cfg=EffectiveConfig.From(app.Tuning.ApplyPending());
            if(recipe.Volley)controller.SetNextVolley(VolleyFor(cfg));
            app.Host.Launch(cfg,recipe.Initial,cfg.Physics.StepHz*10,StopCondition.SecondFloorBounce);return true;
        }
        // VL1: the fixed practice body, the rally's volley limits and reaction delay (one source: RallyParams defaults).
        private static Func<Trajectory,ContactPlan,VolleyOpportunity> VolleyFor(EffectiveConfig cfg)
        {
            var p=new SquashClash.Core.Rally.RallyParams();var body=BodyFrame.From(cfg.Source);
            var legal=new LegalSettings((float)cfg.Source.Get("legal.minHeight"),(float)cfg.Source.Get("legal.maxHeight"),cfg.Physics.StepHz,cfg.Physics.Gravity,cfg.Physics.BallRadius,cfg.Court);
            var settings=new VolleySettings(true,(float)p.VolleyMinHeight,(float)p.VolleyMaxHeight,p.VolleyMinSeparationS,p.VolleyMinWindowS);
            int reaction=(int)Math.Round(p.ReactionS*cfg.Physics.StepHz);
            return (t,bounce)=>t==null||t.States.Count<2?VolleyOpportunity.None("no-trajectory"):
                VolleyPlanner.Plan(t,_=>body,cfg.Court,legal,settings,bounce,t.States[0].Step+reaction,cfg.Tempo,1);
        }
        public void Tick(double realSeconds)
        {
            if(double.IsNaN(realSeconds)||double.IsInfinity(realSeconds)||realSeconds<0)throw new ArgumentOutOfRangeException(nameof(realSeconds));
            if(app==null||!Enabled||!controller.Available)return;
            if(Phase==PracticePhase.Reception&&!app.Host.IsRunning)controller.CloseReception();
            if(controller.AttemptConsumed&&Phase==PracticePhase.Reception)Phase=PracticePhase.Resolved;
            if(Phase==PracticePhase.Resolved&&!controller.WaitingForContact)Phase=PracticePhase.Observing;
            if(Phase==PracticePhase.Observing)
            {
                var record=controller.LastRecord;
                bool limit=record?.Applied==true&&(app.Host.Current.Step-record.ContactStep)/(double)app.Clock.StepHz>=app.Host.ActiveConfig.Source.Get("practice.observeMaxSeconds");
                if(!app.Host.IsRunning||limit){Phase=PracticePhase.Waiting;waiting=0;}
                return;
            }
            if(Phase!=PracticePhase.Waiting||app.Clock.Paused||!app.DevHasInputFocus)return;
            waiting+=realSeconds;
            if(waiting>=app.Tuning.Applied.Get("practice.repeatSeconds")){waiting=0;app.LaunchCurrent();}
        }
        private void Update()=>Tick(Time.unscaledDeltaTime);
        private void OnDestroy(){if(app!=null)app.Host.Launched-=OnLaunch;if(controller!=null)controller.ShotResolved-=OnResolved;}
    }
}
