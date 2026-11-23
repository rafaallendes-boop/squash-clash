using System.Collections;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First-match hints, criterion 10 (spec front 2): every hint fits the safe area on every phone, in two lines (three for H0), with no ellipsis,
    // and H0 in the serve guide stacks on the previous stroke's note without touching it, the line or the labels. The player's note is the same
    // with the hints on and off. Hints are ON; the one run with them off is the reference the spec declares (section 4).
    public sealed class ZFirstMatchHintsLayoutTests : ZDeviceFixture
    {
        private double clock=100;
        [UnityTearDown] public IEnumerator Cleanup(){FirstMatchHints.ResetForTests();PointEndFreeze.Reset();PlayerPrefs.DeleteKey("sc.hints.window");return Shutdown();}
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private void StartHintMatch(bool autopilot)
        {
            FirstMatchHints.ResetForTests();FirstMatchHints.Clear();clock=100;FirstMatchHints.Now=()=>clock;FirstMatchHints.ForceWindowForTests=true;
            app.Tuning.Set("fatigue.enabled",0);app.Clock.Paused=false;
            rally.Autopilot=autopilot;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;rally.SetParams(new RallyParams{ServeKindOn=1});
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();
        }
        private static void AssertLines(Label label,int max,string context)
        {
            float one=label.MeasureTextSize("Mg",0,VisualElement.MeasureMode.Undefined,0,VisualElement.MeasureMode.Undefined).y;
            Assert.That(label.contentRect.height,Is.LessThanOrEqualTo(max*one+2),context+": at most "+max+" lines ("+label.text+")");
            Assert.That(label.resolvedStyle.textOverflow,Is.Not.EqualTo(TextOverflow.Ellipsis),context+": no ellipsis");
            Assert.That(label.text,Is.Not.Empty);
        }
        [UnityTest] public IEnumerator BannerHints_FitInTheSafeArea_InTwoLines_AndH0InThree([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,false);StartHintMatch(false);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver&&!rally.Match.Finished&&rally.Match.Server==PlayerId.Rival,300),Is.True);
            foreach(var (id,lines) in new[]{(HintId.Serve,3),(HintId.Volley,2),(HintId.Boast,2),(HintId.BoastCross,2)})
            {
                hud.Hints.ForceShownForTests(id);hud.Tick(RallyHud.BannerInSeconds);yield return Settle();
                var label=Find<Label>("rally-banner-hint-text");Assert.That(Shown(Find("rally-banner-hint")),Is.True,phone+" "+id);
                Assert.That(label.text,Is.EqualTo(FirstMatchHints.Text(id,HintWhere.Banner)));
                AssertFits("rally-banner",phone+" banner with "+id);AssertLines(label,lines,phone+" "+id);
                Assert.That(Find("rally-banner-card").worldBound.yMin,Is.GreaterThanOrEqualTo(Find("boot-header").worldBound.yMax),"the banner does not cover the header");
            }
        }
        // The guide: H0 on the stroke note («¡Temprano!») and then with the notes off, on every phone.
        [UnityTest] public IEnumerator GuideHint_FitsInThreeLines_AndStacksOnAVisibleNoteWithoutTouchingIt([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,false);StartHintMatch(true);
            Assert.That(Drive(()=>rally.HumanServePause,900),Is.True);Assert.That(Drive(()=>Find("serve-guide").style.opacity.value>=1,.5),Is.True);
            var notes=controller.GetComponent<ShotNoteView>();
            foreach(bool noteOn in new[]{true,false})
            {
                notes.ShowText("¡Temprano!");notes.SetVisible(noteOn);yield return Settle();
                var hint=Find("serve-guide-hint");var label=Find<Label>("serve-guide-hint-text");
                Assert.That(Shown(hint),Is.True,phone+" note="+noteOn);Assert.That(label.text,Is.EqualTo(FirstMatchHints.Text(HintId.Serve,HintWhere.Guide)));
                AssertLines(label,3,phone+" guide note="+noteOn);
                var safe=Safe;var box=hint.worldBound;
                Assert.That(box.xMin,Is.GreaterThanOrEqualTo(safe.xMin-.5f));Assert.That(box.xMax,Is.LessThanOrEqualTo(safe.xMax+.5f));
                Assert.That(box.yMax,Is.LessThanOrEqualTo(safe.yMax+.5f),"not under the home indicator");
                var camera=UnityEngine.Camera.main;var court=app.Host.ActiveConfig.Court;
                float wallBottom=(1-camera.WorldToScreenPoint(new Vector3(court.Width,0,0)).y/phone.Height)*root.layout.height;
                Assert.That(box.yMin,Is.GreaterThanOrEqualTo(wallBottom-1),"never over the front wall");
                foreach(var other in new[]{"serve-guide-line","serve-guide-lob","serve-guide-drive"})
                    Assert.That(box.Overlaps(Find(other).worldBound),Is.False,phone+": the hint crosses "+other);
                var note=Find("shot-note");
                if(noteOn)
                {
                    Assert.That(Shown(note),Is.True);Assert.That(box.Overlaps(note.worldBound),Is.False,phone+": the hint crosses the note");
                    Assert.That(note.worldBound.yMin-box.yMax,Is.InRange(7f,9f),phone+": 8 px between the hint and the note");
                }
            }
        }
        [UnityTest] public IEnumerator TheNote_IsTheSameWithTheHintsOnAndOff([ValueSource(nameof(Phones))] PhoneScreen screen)
        {
            yield return Boot(screen,false);
            (string text,DisplayStyle display,Rect box) Capture()
            {
                var notes=controller.GetComponent<ShotNoteView>();notes.ShowText("¡Temprano!");notes.SetVisible(true);
                var note=Find<Label>("shot-note");return (note.text,note.resolvedStyle.display,note.worldBound);
            }
            StartHintMatch(true);FirstMatchHints.Enabled=false;
            Assert.That(Drive(()=>rally.HumanServePause,900),Is.True);Drive(()=>Find("serve-guide").style.opacity.value>=1,.5);
            var off=Capture();yield return Settle();off=Capture();yield return Settle();
            rally.StopMode();FirstMatchHints.Enabled=true;StartHintMatch(true);FirstMatchHints.Enabled=true;
            Assert.That(Drive(()=>rally.HumanServePause,900),Is.True);Drive(()=>Find("serve-guide").style.opacity.value>=1,.5);
            var on=Capture();yield return Settle();on=Capture();yield return Settle();
            Assert.That(Shown(Find("serve-guide-hint")),Is.True,"the reference run really had the hint");
            Assert.That(on.text,Is.EqualTo(off.text));Assert.That(on.display,Is.EqualTo(off.display));Assert.That(on.box,Is.EqualTo(off.box),"the note does not move");
        }
    }
    // Look of the hints (spec front 2, section 6): the player's surfaces draw with Rubik Regular; H1's ring is the court's amber ring.
    public sealed class ZFirstMatchHintsLookTests : ZFirstRunFixture
    {
        private double clock=100;
        [UnitySetUp] public IEnumerator Setup(){FirstMatchHints.ResetForTests();yield return Boot(true);clock=100;FirstMatchHints.Now=()=>clock;FirstMatchHints.ForceWindowForTests=true;}
        [UnityTearDown] public IEnumerator Cleanup(){FirstMatchHints.ResetForTests();PointEndFreeze.Reset();yield return Shutdown();}
        private static string FontName(VisualElement e)
        {
            var d=e.resolvedStyle.unityFontDefinition;string name=d.font!=null?d.font.name:d.fontAsset!=null?d.fontAsset.name:"";return name.EndsWith(" SDF")?name.Substring(0,name.Length-4):name;
        }
        private static void Close(float actual,float expected){Assert.That(actual,Is.EqualTo(expected).Within(.07f*expected+.05f));}
        private static void Near(Color a,Color b){Assert.That(a.r,Is.EqualTo(b.r).Within(.01f));Assert.That(a.g,Is.EqualTo(b.g).Within(.01f));Assert.That(a.b,Is.EqualTo(b.b).Within(.01f));}
        [UnityTest] public IEnumerator HintTexts_UseRubikRegular_AndTheVolleyRingHasTheCourtsAmber()
        {
            flow.ClosePractice();rally.Autopilot=false;rally.Level=AiLevel.Easy;rally.Seed=3;rally.SetParams(new RallyParams{ServeKindOn=1});
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.PointOver&&!rally.Match.Finished&&rally.Match.Server==PlayerId.Rival,300),Is.True);
            hud.Hints.ForceShownForTests(HintId.Volley);hud.Tick(RallyHud.BannerInSeconds);Refresh();yield return null;yield return null;Refresh();
            Assert.That(FontName(Find("rally-banner-hint-text")),Is.EqualTo("Rubik-Regular"));
            var ring=Find("rally-banner-hint-icon");Near(ring.resolvedStyle.borderLeftColor,ShotController.VolleyTint);Close(ring.resolvedStyle.borderLeftWidth,2f);
            Close(ring.resolvedStyle.width,14f);
            Close(Find("rally-banner-hint").resolvedStyle.borderTopWidth,1f);
            Close(Find("rally-banner-hint-text").resolvedStyle.fontSize,14f);Close(Find("rally-banner-hint").resolvedStyle.maxWidth.value,280f);
            hud.Hints.ForceShownForTests(HintId.Boast);Refresh();yield return null;Refresh();
            var corner=Find("rally-banner-hint-icon");Close(corner.resolvedStyle.borderLeftWidth,2f);Close(corner.resolvedStyle.borderTopWidth,2f);Assert.That(corner.resolvedStyle.borderBottomWidth,Is.EqualTo(0f));
            Near(corner.resolvedStyle.borderLeftColor,new Color(234/255f,243/255f,243/255f));
        }
        [UnityTest] public IEnumerator GuideHint_UsesRubikRegular_GlassWithAOnePixelEdge_AndTheGuideLineInMiniature()
        {
            flow.ClosePractice();rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;rally.SetParams(new RallyParams{ServeKindOn=1});
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();Assert.That(Drive(()=>rally.HumanServePause,900),Is.True);Drive(()=>Find("serve-guide").style.opacity.value>=1,.5);
            Refresh();yield return null;yield return null;Refresh();
            var pill=Find("serve-guide-hint");Assert.That(FontName(Find("serve-guide-hint-text")),Is.EqualTo("Rubik-Regular"));
            Close(pill.resolvedStyle.borderTopWidth,1f);Close(pill.resolvedStyle.maxWidth.value,280f);
            Assert.That(pill.resolvedStyle.backgroundColor.a,Is.GreaterThan(.8f),"glass");
            var bar=Find("serve-guide-hint-icon");Close(bar.resolvedStyle.width,14f);Close(bar.resolvedStyle.height,3f);
            Assert.That(pill.pickingMode,Is.EqualTo(PickingMode.Ignore),"the front wall stays free to touch");
        }
    }
}
