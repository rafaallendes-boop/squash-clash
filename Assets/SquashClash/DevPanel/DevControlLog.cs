using System;
using SquashClash.Runtime.App;
namespace SquashClash.DevPanel
{
    internal static class DevControlLog
    {
        internal static void Run(AppBootstrap app,string control,Action action)=>Try(app,control,()=>{action();return true;});
        internal static bool Try(AppBootstrap app,string control,Func<bool> action)
        {
            app.RecordDevControl(control,true,"invoked","activation");
            try {bool accepted=action();app.RecordDevControl(control,accepted,accepted?"completed":"operation-rejected");return accepted;}
            catch {app.RecordDevControl(control,false,"exception");throw;}
        }
    }
}
