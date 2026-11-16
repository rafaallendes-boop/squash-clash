using System;
using System.Linq;
using SquashClash.Core.Config;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("SquashClash.Runtime.Tests")]
namespace SquashClash.AutoQA
{
    // Spec v0.3 §7.1 and the approved First Shot closure table; no game decision APIs.
    internal static class Oracle
    {
        internal static Expected Expect(Scenario s,ParamSet config)=>Expect(s,config,double.NaN,double.NaN);
        internal static Expected Expect(Scenario s,ParamSet config,double kInMs,double kOutMs)
        {
            string closure=s.kind=="rejected"?"Rejected":Closure(false,false,false,s.pullMm>config.Get("power.pullDeadZoneMm"),s.pullMm,s.pullMm,s.durationMs,config);
            var e=new Expected{closure=closure,result="NoOrder",rationale="Non-Swing closures do not consume an attempt or produce contact presentation"};
            if(closure=="Swing")
            {
                double delta=s.releaseOffsetMs-config.Get("timing.latencyCompMs");
                bool perfect=Math.Abs(delta)<=config.Get("timing.assistMs");
                bool limited=config.GetBool("timing.validLimit");
                if(!limited&&(double.IsNaN(kInMs)||double.IsNaN(kOutMs)||double.IsInfinity(kInMs)||double.IsInfinity(kOutMs)||kInMs>kOutMs))
                    throw new InvalidOperationException("T3 requires an independent finite K window");
                bool withinK=limited||delta>=kInMs&&delta<=kOutMs;
                perfect&=withinK;
                bool valid=withinK&&(!limited||Math.Abs(delta)<=config.Get("timing.validMs"));
                e.result=perfect?"Perfect":valid?"Valid":"Failed";e.applied=perfect||valid;e.presentations=1;
                e.fail=e.applied?"None":(!limited?delta<kInMs:delta<0)?"Air":"Late";e.scheduled=perfect&&delta<=0;e.checkScheduled=perfect;
                e.oracle=limited?"timing-window/1":"timing-k/1";
                e.rationale=limited?"Independent configured A/V windows; unchanged V oracle":"Independent physical K first, then configured A; no production contact decisions";
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
