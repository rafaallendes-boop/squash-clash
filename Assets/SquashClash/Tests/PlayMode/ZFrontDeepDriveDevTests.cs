using System.Collections;
using NUnit.Framework;
using SquashClash.Runtime.Shot;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // Front deep drive (design 2026-11-20): the DEV switch the PO uses to compare with and without the human side-wall guard.
    public sealed class ZFrontDeepDriveDevTests : ZFirstRunFixture
    {
        [UnitySetUp] public IEnumerator Setup()=>Boot(true);
        [UnityTearDown] public IEnumerator Cleanup(){ShotController.FrontDeepDriveOn=true;return Shutdown();}
        [UnityTest] public IEnumerator DevPanel_FrontDeepDriveToggle_DefaultsOn_AndFlipsTheSwitch()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            var button=root.Q<Button>("front-deep-drive-toggle");Assert.That(button,Is.Not.Null,"DEV panel has the front deep drive toggle");
            Assert.That(ShotController.FrontDeepDriveOn,Is.True);Assert.That(button.text,Does.Contain("sí"));
            Click("front-deep-drive-toggle");Assert.That(ShotController.FrontDeepDriveOn,Is.False);Assert.That(button.text,Does.Contain("no"));
            Click("front-deep-drive-toggle");Assert.That(ShotController.FrontDeepDriveOn,Is.True);
        }
    }
}
