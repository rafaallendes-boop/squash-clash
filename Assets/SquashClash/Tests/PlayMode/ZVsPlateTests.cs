using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Phase=UnityEngine.InputSystem.TouchPhase;
namespace SquashClash.Runtime.Tests
{
    // «vs» plate (spec front 4, 2026-11-22 rev 2): shown over the flight of the first serve of a match started from the menu, and nowhere else.
    // Criteria 1 to 7. The plate is ON in every test except the reference run of criterion 2, which the spec fixes beforehand.
    // One clock drives VsPlate.Now and TouchInputAdapter.ReadRealTime, so the plate's age is real time and never the HUD's clipped dt.
    public sealed class ZVsPlateTests : ZFirstRunFixture
    {
        private double clock;private bool booted;
        private TouchInputAdapter input;private GesturePreview preview;private Touchscreen screen;
        private readonly List<TouchSample> received=new List<TouchSample>();private readonly List<(TouchSample sample,string reason)> filtered=new List<(TouchSample,string)>();
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class Payload {public string screen,action;}
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(screen!=null){InputSystem.RemoveDevice(screen);screen=null;}
            VsPlate.Reset();RallyHud.VsPlateOn=true;if(booted)yield return Shutdown();booted=false;
        }
        // A test that boots twice shuts the first world down itself; the fixture's Shutdown must not run on a world that is not there.
        private IEnumerator Reboot(){if(booted)yield return Shutdown();booted=false;yield return Boot(true);booted=true;}
        // ---------- helpers ----------
        private void Install()
        {
            VsPlate.Reset();RallyHud.VsPlateOn=true;clock=app.Host.ObservedRealTime;VsPlate.Now=()=>clock;
            input=app.GetComponent<TouchInputAdapter>();preview=app.GetComponent<GesturePreview>();
            Assert.That(input,Is.Not.Null);Assert.That(preview,Is.Not.Null);
            input.ReadRealTime=()=>clock;input.InputAreaOverride=new Rect(0,0,5000,5000);
            received.Clear();filtered.Clear();input.SampleReceived+=received.Add;input.SampleFiltered+=(sample,reason)=>filtered.Add((sample,reason));
        }
        // The menu's own «Jugar» with the tutorial seen: the same call as a player's tap.
        private void Play(bool autopilot=false,long seed=3)
        {
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);
            rally.Seed=seed;rally.Autopilot=autopilot;rally.AutopilotTimingSigmaMs=0;
            Click("menu-play");Refresh();
        }
        // advanceClock=false holds the test clock still, so the plate cannot time out while the serve flies.
        private void Step(bool advanceClock=true){if(advanceClock)clock+=StepReal;app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick((float)StepReal);Refresh();}
        private bool PlateShown=>Shown(Find("vs-plate"));
        private Payload[] Menu(string screenName)=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(r=>r.kind=="menu").Select(r=>JsonUtility.FromJson<Payload>(r.payload)).Where(p=>p.screen==screenName).ToArray();
        private string[] VsActions()=>Menu("vs").Select(p=>p.action).ToArray();
        private static Vector2 Middle=>new Vector2(Screen.width/2f,Screen.height*.3f);
        private void Touch(Phase phase,int id,Vector2 position,double? at=null)
        {
            InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,phase=phase,position=position},at??clock);InputSystem.Update();
        }
        private void AddTouchscreen(){if(screen==null)screen=InputSystem.AddDevice<Touchscreen>();}

        // ---------- criterion 1: shown in the first frame of RivalReception ----------
        [UnityTest] public IEnumerator Plate_IsShownInTheFirstFrameOfRivalReception_WithTuVsRivalAndTheStyle()
        {
            yield return Reboot();Install();Play();
            Assert.That(rally.Match,Is.Not.Null);Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception));Assert.That(rally.PointsPlayed,Is.EqualTo(0));
            Assert.That(Shown(Find("vs-plate-layer")),Is.True);Assert.That(PlateShown,Is.True,"visible in the same frame the serve leaves");
            Assert.That(Find<Label>("vs-plate-you").text,Is.EqualTo("TÚ"));Assert.That(Find<Label>("vs-plate-mid").text,Is.EqualTo("vs"));
            Assert.That(Find<Label>("vs-plate-rival").text,Is.EqualTo("RIVAL"));
            Assert.That(Find<Label>("vs-plate-style").text,Is.EqualTo(RivalStyle.Name(rally.Style)));
            Assert.That(Find<Label>("vs-plate-style").text,Is.EqualTo(RallyText.VsStyle(rally.Style)));
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown"}));
        }
        [UnityTest] public IEnumerator Plate_ShowsEachOfTheThreeStyles([Values(RivalStyleId.Fondista,RivalStyleId.Definidor,RivalStyleId.Completo)] RivalStyleId style)
        {
            yield return Reboot();Install();rally.StyleOverride=style;Play();
            Assert.That(rally.Style,Is.EqualTo(style));Assert.That(Find<Label>("vs-plate-style").text,Is.EqualTo(RivalStyle.Name(style)));
        }

        // ---------- criterion 2: it adds no time ----------
        private sealed class Trace {public readonly List<long> LaunchSteps=new List<long>();public double PlayToFirstLaunch=-1;public readonly List<string> Points=new List<string>();public int ShownLines;}
        private IEnumerator Run(bool plateOn,Trace trace)
        {
            yield return Reboot();Install();RallyHud.VsPlateOn=plateOn;
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);PlayerPrefs.SetInt("sc.firstRun.rivalMatches",5);
            rally.Seed=7;rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;
            double playAt=clock;bool launched=false;
            app.Host.Launched+=_=>{trace.LaunchSteps.Add(app.Host.Current.Step);if(!launched){launched=true;trace.PlayToFirstLaunch=clock-playAt;}};
            rally.PointEnded+=p=>trace.Points.Add(p.Winner+"|"+p.Cause+"|"+p.Shots+"|"+p.Reception);
            Click("menu-play");Refresh();
            for(int i=0;i<40000&&trace.Points.Count<3;i++)Step();
            trace.ShownLines=Menu("vs").Count(p=>p.action=="shown");
        }
        [UnityTest] public IEnumerator Plate_AddsNoTime_SameLaunchStepSameElapsedTimeSamePoints()
        {
            var on=new Trace();var off=new Trace();
            yield return Run(true,on);yield return Run(false,off);
            Assert.That(on.ShownLines,Is.EqualTo(1),"the plate really ran in the first run");Assert.That(off.ShownLines,Is.EqualTo(0),"and not in the reference run");
            Assert.That(on.Points.Count,Is.GreaterThanOrEqualTo(3));Assert.That(off.Points.Count,Is.GreaterThanOrEqualTo(3));
            Assert.That(on.LaunchSteps.Count,Is.GreaterThan(0));
            Assert.That(on.LaunchSteps,Is.EqualTo(off.LaunchSteps),"every launch, the first serve included, at the same simulation step");
            Assert.That(on.PlayToFirstLaunch,Is.EqualTo(off.PlayToFirstLaunch),"same real time from «Jugar» to the serve");Assert.That(on.PlayToFirstLaunch,Is.GreaterThanOrEqualTo(0));
            Assert.That(on.Points.Take(3),Is.EqualTo(off.Points.Take(3)),"same PointResult sequence");
        }

        // ---------- criterion 3: closes by time and by phase ----------
        [UnityTest] public IEnumerator Plate_ClosesInTheSameFrameWhenTheClockJumpsOneAndAHalfSecondsAtOnce()
        {
            yield return Reboot();Install();Play();
            Assert.That(PlateShown,Is.True);
            clock+=1.5;Refresh();
            Assert.That(PlateShown,Is.False,"age is Now()-shownAt, not a sum of the HUD's clipped dt");Assert.That(Shown(Find("vs-plate-layer")),Is.False);
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown","timeout"}));
        }
        [UnityTest] public IEnumerator Plate_StaysWhileTheClockHasNotRunOutAndFadesOutAtTheEnd()
        {
            yield return Reboot();Install();double t0=clock;Play();
            clock=t0+(RallyText.VsInSeconds+RallyText.VsHoldSeconds)*.5;Refresh();Assert.That(PlateShown,Is.True);Assert.That(Find("vs-plate").resolvedStyle.opacity,Is.EqualTo(1f).Within(.01f),"the hold does not fade");
            clock=t0+RallyText.VsInSeconds+RallyText.VsHoldSeconds+RallyText.VsOutSeconds*.5;Refresh();Assert.That(PlateShown,Is.True);
            Assert.That(Find("vs-plate").resolvedStyle.opacity,Is.InRange(.01f,.99f),"half way through the fade-out");
        }
        [UnityTest] public IEnumerator Plate_ClosesInTheFrameHumanReceptionStarts()
        {
            yield return Reboot();Install();Play(true);
            Assert.That(PlateShown,Is.True);
            bool reached=false;
            for(int i=0;i<4000;i++)
            {
                Step(false);
                if(rally.Phase==RallyPhase.RivalReception){Assert.That(PlateShown,Is.True,"still in the serve's flight, frame "+i);continue;}
                reached=true;Assert.That(rally.Phase,Is.EqualTo(RallyPhase.HumanReception),"the first serve's rally goes on to the human");
                Assert.That(PlateShown,Is.False,"hidden in the very frame the hit window opens");break;
            }
            Assert.That(reached,Is.True,"the serve's flight ends");
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown","phase"}));
        }

        // ---------- criterion 4: no phantom touch, with the real adapter, the real UiBlocker and the real panel ----------
        [UnityTest] public IEnumerator Touch_OnThePlateHidesItAndNeverOpensAGesture_EvenWhenTheFingerOutlivesTheServe()
        {
            yield return Reboot();Install();AddTouchscreen();Play();
            Assert.That(input.UiBlocker,Is.Not.Null,"GesturePreview's own filter, not a stub");Assert.That(PlateShown,Is.True);
            var layer=Find("vs-plate-layer");Assert.That(layer.pickingMode,Is.EqualTo(PickingMode.Position));
            var panelPoint=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(Middle.x,Screen.height-Middle.y));
            Assert.That(root.panel.Pick(panelPoint),Is.SameAs(layer),"the panel's picking recognises the real layer there");
            Assert.That(input.UiBlocker(Middle),Is.True,"OverUi answers true over the plate");
            Touch(Phase.Began,7,Middle);Refresh();
            Assert.That(PlateShown,Is.False,"hidden in the same frame");Assert.That(VsActions(),Is.EqualTo(new[]{"shown","tapped"}));
            Assert.That(filtered.Count,Is.EqualTo(1));Assert.That(filtered[0].sample.Phase,Is.EqualTo(TouchPhaseKind.Began));Assert.That(filtered[0].reason,Is.EqualTo("suppressed-ui-or-contact"));
            Assert.That(received,Is.Empty);
            // The same finger moves during RivalReception.
            clock+=.01;Touch(Phase.Moved,7,Middle+new Vector2(4,6));Step();
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception));Assert.That(filtered.Count,Is.EqualTo(2));
            // The serve ends while the finger is still down.
            bool human=false;
            for(int i=0;i<4000&&!human;i++){Step();human=rally.Phase==RallyPhase.HumanReception;}
            Assert.That(human,Is.True,"the rally reaches the human's reception");
            clock+=.01;Touch(Phase.Moved,7,Middle+new Vector2(9,12));clock+=.01;Touch(Phase.Ended,7,Middle+new Vector2(9,12));
            Assert.That(filtered.Count,Is.EqualTo(4),"Moved and Ended of that same finger");
            Assert.That(filtered.All(f=>f.reason=="suppressed-ui-or-contact"),Is.True,string.Join(",",filtered.Select(f=>f.reason)));
            Assert.That(received,Is.Empty,"nothing of that finger reached the gesture");Assert.That(preview.Active,Is.False,"no gesture");
            Assert.That(controller.LastRecord,Is.Null,"and no attempt");
            // A new finger in HumanReception opens a normal gesture.
            double at=Math.Max(clock,app.Host.LaunchRealTime+.01);
            Touch(Phase.Began,8,Middle,at);
            Assert.That(received.Count,Is.EqualTo(1),"the new Began arrives through SampleReceived");Assert.That(received[0].Phase,Is.EqualTo(TouchPhaseKind.Began));
            Assert.That(filtered.Count,Is.EqualTo(4),"and is not filtered");Assert.That(preview.Active,Is.True,"it opens a normal gesture");
        }

        // ---------- criterion 5: pause ----------
        [UnityTest] public IEnumerator Pause_WithThePlateShown_PausesHidesItAndWritesPause_AndThePauseButtonsStillWork()
        {
            yield return Reboot();Install();Play();
            Assert.That(PlateShown,Is.True);
            Click("rally-pause");Refresh();
            Assert.That(app.Clock.Paused,Is.True);Assert.That(PlateShown,Is.False);Assert.That(Shown(Find("vs-plate-layer")),Is.False);
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown","pause"}));
            Click("pause-continue");Refresh();
            Assert.That(app.Clock.Paused,Is.False,"the pause screen's buttons work");Assert.That(PlateShown,Is.False,"it does not come back");
            clock+=5;Refresh();Assert.That(VsActions(),Is.EqualTo(new[]{"shown","pause"}),"one closing line only");
        }

        // ---------- criterion 6: where it does not appear ----------
        [UnityTest] public IEnumerator Plate_DoesNotAppearInTheRematch_TheDevMatchOrPractice()
        {
            yield return Reboot();Install();Play();
            clock+=1.5;Refresh();Assert.That(VsActions(),Is.EqualTo(new[]{"shown","timeout"}));
            rally.Rematch();hud.Refresh();Refresh();
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.RivalReception));Assert.That(Shown(Find("vs-plate-layer")),Is.False,"same rival: no plate in the rematch");
            hud.StartMatch();hud.Refresh();
            Assert.That(rally.Match,Is.Not.Null);Assert.That(Shown(Find("vs-plate-layer")),Is.False,"the DEV «Partido» has no plate");
            rally.StopMode();hud.Refresh();rally.StartMode();hud.Refresh();Refresh();
            Assert.That(rally.Active,Is.True);Assert.That(Shown(Find("vs-plate-layer")),Is.False,"practice has no plate");
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown","timeout"}),"no new line");
        }
        [UnityTest] public IEnumerator Plate_SwitchedOff_StartsTheMatchAsToday()
        {
            yield return Reboot();Install();RallyHud.VsPlateOn=false;Play();
            Assert.That(rally.Match,Is.Not.Null);Assert.That(Shown(Find("vs-plate-layer")),Is.False);Assert.That(VsActions(),Is.Empty);
        }

        // ---------- criterion 7: metrics, exactly one `shown` and one closing line with the right reason ----------
        private IEnumerator CloseBy(string reason)
        {
            yield return Reboot();Install();AddTouchscreen();
            Play(reason=="phase");
            Assert.That(PlateShown,Is.True,reason);
            switch(reason)
            {
                case "tapped":Touch(Phase.Began,3,Middle);Refresh();break;
                case "timeout":clock+=1.5;Refresh();break;
                case "pause":Click("rally-pause");Refresh();break;
                default:for(int i=0;i<4000&&rally.Phase==RallyPhase.RivalReception;i++)Step(false);break;
            }
            Assert.That(PlateShown,Is.False,reason);
            for(int i=0;i<30;i++)Step();clock+=5;Refresh();Refresh();
            Assert.That(VsActions(),Is.EqualTo(new[]{"shown",reason}),"exactly one shown and one closing line, closed by "+reason);
        }
        [UnityTest] public IEnumerator Metrics_Tapped(){yield return CloseBy("tapped");}
        [UnityTest] public IEnumerator Metrics_Timeout(){yield return CloseBy("timeout");}
        [UnityTest] public IEnumerator Metrics_Phase(){yield return CloseBy("phase");}
        [UnityTest] public IEnumerator Metrics_Pause(){yield return CloseBy("pause");}
    }
}
