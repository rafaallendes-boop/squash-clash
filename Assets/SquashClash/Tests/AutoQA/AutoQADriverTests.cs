using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace SquashClash.AutoQA
{
    public sealed class AutoQADriverTests
    {
        [UnityTest] public IEnumerator Driver_RoutesRealGesture()
        {
#if UNITY_EDITOR
            using(var view=new NativeGameView())
            {
                yield return null;yield return null;yield return null;
                using(var driver=new DeterministicDriver())
                {
                    var s=new Scenario{id="driver-integration",kind="swing",launch=4,releaseOffsetMs=-10};
                    yield return driver.Prepare(s);
                    driver.Pump(s,ScriptedPlayer.Build(s,driver.Anchor,driver.Target,driver.Environment.mmPerPx));
                    string dir=Path.GetFullPath(System.Environment.GetEnvironmentVariable("AUTOQA_DRIVER_DIR"));
                    Assert.That(dir.StartsWith(Path.GetFullPath("Logs/autoqa")+Path.DirectorySeparatorChar),Is.True);Directory.CreateDirectory(dir);
                    EvidenceCollector.Copy(driver.App.Metrics.CurrentFilePath,dir,"driver");
                    Assert.That(driver.Controller.LastRecord,Is.Not.Null,"Touch script must reach real shot controller");
                    Assert.That(driver.Controller.LastRecord.Result,Is.EqualTo("Perfect"));
                    Assert.That(driver.Controller.LastRecord.Applied,Is.True);
                }
                yield return null;
            }
#endif
        }
    }
}
