using System;
using System.Linq;
using SquashClash.Core.Config;
namespace SquashClash.AutoQA
{
    // Spec v0.3 §7.1 and the approved First Shot closure table; no game decision APIs.
    internal static class Oracle
    {
        internal static Expected Expect(Scenario s,ParamSet config)
        {
            string closure=s.kind=="rejected"?"Rejected":Closure(false,false,false,s.pullMm>config.Get("power.pullDeadZoneMm"),s.pullMm,s.pullMm,s.durationMs,config);
            var e=new Expected{closure=closure,result="NoOrder",rationale="Non-Swing closures do not consume an attempt or produce contact presentation"};
            if(closure=="Swing")
            {
                double delta=s.releaseOffsetMs-config.Get("timing.latencyCompMs");
                bool perfect=Math.Abs(delta)<=config.Get("timing.assistMs");
                bool valid=!config.GetBool("timing.validLimit")||Math.Abs(delta)<=config.Get("timing.validMs");
                e.result=perfect?"Perfect":valid?"Valid":"Failed";e.applied=perfect||valid;e.presentations=1;
                e.fail=e.applied?"None":delta<0?"Air":"Late";e.scheduled=perfect&&delta<=0;e.checkScheduled=perfect;
                e.rationale="Independent configured A/V windows; geometric K bounds only invalidate margin, never select expectation";
            }
            if(!string.IsNullOrEmpty(s.deliberateExpectedResult))e.result=s.deliberateExpectedResult;
            return e;
        }
        internal static bool MarginValid(Scenario s,ParamSet config,double kInMs,double kOutMs)
        {
            if(s.kind!="swing")return true;
            double delta=s.releaseOffsetMs-config.Get("timing.latencyCompMs"),a=config.Get("timing.assistMs"),v=config.Get("timing.validMs");
            double margin=2000/config.Get("physics.stepHz")/config.Get("pace.tempo");
            var borders=config.GetBool("timing.validLimit")?new[]{-a,a,-v,v,kInMs,kOutMs}:new[]{-a,a,kInMs,kOutMs};
            return borders.All(b=>!double.IsNaN(b)&&!double.IsInfinity(b)&&Math.Abs(delta-b)>=margin)&&
                (Math.Abs(delta)>a||Math.Abs(delta)>=1);
        }
        internal static string Closure(bool rejected,bool invalid,bool canceled,bool everPulled,float pull,float maximum,double duration,ParamSet config)
        {
            if(canceled)return "SystemCanceled";if(rejected)return "Rejected";if(invalid)return "InvalidAim";
            if(maximum<config.Get("gesture.tapMaxMm")&&duration>=0&&duration<config.Get("gesture.tapMaxMs"))return "Tap";
            if(pull<=config.Get("power.pullDeadZoneMm"))return everPulled?"CanceledReturnToDeadZone":"AbandonedNoPull";
            return "Swing";
        }
    }
}
