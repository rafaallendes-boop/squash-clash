using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SquashClash.Core.Rally;
using SquashClash.Core.Shot;
using SquashClash.Runtime.Input;
using SquashClash.Runtime.Menu;
using SquashClash.Runtime.Rally;
using SquashClash.Runtime.Shot;
using SquashClash.Runtime.Visual;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
namespace SquashClash.Runtime.Tests
{
    // First-match hints (spec front 2, 2026-11-22), the game side: the window, the four hints, what counts as seen, what is written and
    // that no time is added. The hints are ON in every test here except the two the spec declares (section 4).
    // The player's own match goes through autopilot in most of these tests, which the window excludes, so ForceWindowForTests lifts only that.
    public sealed class ZFirstMatchHintsTests : ZFirstRunFixture
    {
        private const string GuideText="Sacas tú: toca el frontis antes de que se acabe la barra. Si no tocas, sale un globo.";
        private const string BannerServeText="Cuando saques tú, toca el frontis durante la pausa: arriba sale un globo; abajo, un saque rápido.";
        private const string VolleyText="Anillo ámbar: toca y volea en el aire. Anillo blanco: espera el bote y golpea.";
        private const string BoastText="Dos paredes: si tocas alto en la lateral, puede quedar larga.";
        private const string CrossText="Pared lateral cerca de ti: la pelota cruza al otro lado.";
        private static readonly string[] AllHintKeys={"sc.hints.serve","sc.hints.volley","sc.hints.boast","sc.hints.boastCross","sc.hints.window"};
        private double clock;
        [Serializable] private sealed class Line {public string kind,payload;}
        [Serializable] private sealed class Payload {public string matchId,id,action,where,kind;public bool chosen,hintSeen;}
        [UnitySetUp] public IEnumerator Setup()
        {
            FirstMatchHints.ResetForTests();yield return Boot(true);
            clock=100;FirstMatchHints.Now=()=>clock;FirstMatchHints.ForceWindowForTests=true;
        }
        [UnityTearDown] public IEnumerator Cleanup(){FirstMatchHints.ResetForTests();PointEndFreeze.Reset();yield return Shutdown();}
        // ---------- helpers ----------
        private IEnumerator Settle(){Refresh();yield return null;yield return null;Refresh();yield return null;}
        private static bool AnyKey()=>AllHintKeys.Any(PlayerPrefs.HasKey);
        private Payload[] Events(string kind)=>File.ReadAllLines(app.Metrics.CurrentFilePath).Select(JsonUtility.FromJson<Line>).Where(r=>r.kind==kind).Select(r=>JsonUtility.FromJson<Payload>(r.payload)).ToArray();
        private static void Preset(int serve=0,int volley=0,int boast=0,int cross=0)
        {
            foreach(var (key,value) in new[]{("sc.hints.serve",serve),("sc.hints.volley",volley),("sc.hints.boast",boast),("sc.hints.boastCross",cross)})if(value!=0)PlayerPrefs.SetInt(key,value);
        }
        // Everything seen except the volley: only H1 can show.
        private static void OnlyVolleyLeft()=>Preset(serve:1,boast:2,cross:1);
        private void StartHints(bool autopilot,long seed=3,double volleyEnabled=1)
        {
            flow.ClosePractice();
            rally.Autopilot=autopilot;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=seed;rally.StyleOverride=RivalStyleId.Fondista;
            rally.SetParams(new RallyParams{ServeKindOn=1,VolleyEnabled=volleyEnabled});
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();
        }
        private bool ReachGuide()=>Drive(()=>rally.HumanServePause,900);
        private bool ReachRivalFirstPause()=>Drive(()=>rally.Phase==RallyPhase.PointOver&&!rally.Match.Finished&&rally.Match.Server==PlayerId.Rival,300);
        private bool Ordinary=>rally.Phase==RallyPhase.PointOver&&!rally.HumanServePause&&!rally.Match.Finished;
        private bool ReachOrdinaryPause()=>Drive(()=>Ordinary&&rally.LastPoint.HasValue,900);
        // Lets the guide fade in and lays everything out, as a few real frames do.
        private IEnumerator ShowGuide(){Drive(()=>Find("serve-guide").style.opacity.value>=1,.5);yield return Settle();}
        private ClosedGesture Touch(float y,string id){double now=app.Host.ObservedRealTime;return new ClosedGesture(app.Host.LaunchId,id,0,now-.2,now,true,GestureOutcome.Swing,new Vector2(3.2f,y),.5f,controller.Snapshot.Reception,"test",now);}
        private Label BannerHint=>Find<Label>("rally-banner-hint-text");
        private Label GuideHint=>Find<Label>("serve-guide-hint-text");
        // The banner's own fade-in, as the HUD's Tick does it, plus a layout pass.
        private IEnumerator FadeBannerIn(){hud.Tick(RallyHud.BannerInSeconds);yield return Settle();}

        // ---------- texts ----------
        [UnityTest] public IEnumerator Texts_AreTheOnesDecidedByScLead_AndKeepTheSquashVocabulary()
        {
            Assert.That(FirstMatchHints.Text(HintId.Serve,HintWhere.Guide),Is.EqualTo(GuideText));
            Assert.That(FirstMatchHints.Text(HintId.Serve,HintWhere.Banner),Is.EqualTo(BannerServeText));
            Assert.That(FirstMatchHints.Text(HintId.Volley,HintWhere.Banner),Is.EqualTo(VolleyText));
            Assert.That(FirstMatchHints.Text(HintId.Boast,HintWhere.Banner),Is.EqualTo(BoastText));
            Assert.That(FirstMatchHints.Text(HintId.BoastCross,HintWhere.Banner),Is.EqualTo(CrossText));
            foreach(var text in new[]{GuideText,BannerServeText,VolleyText,BoastText,CrossText})
            {
                Assert.That(text,Does.Not.Contain("la línea"),"«la línea» is confused with the lines of the front wall");
                Assert.That(text,Does.Not.Contain("Lateral cerca"),"the full term is «pared lateral»");
            }
            Assert.That(BannerServeText,Does.Contain("arriba sale un globo").And.Contain("abajo, un saque rápido"));
            yield return null;
        }

        // ---------- criterion 1: H0 in the guide ----------
        [UnityTest] public IEnumerator Guide_ShowsH0_AndMarksItSeenInOneFrameOf0_18Seconds_WritingOneLine()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True,"the human wins a point");yield return ShowGuide();
            Assert.That(rally.WaitBetweenPoints,Is.EqualTo(3d),"the pause is not stretched to fit the hint");
            Assert.That(Shown(Find("serve-guide-hint")),Is.True);Assert.That(GuideHint.text,Is.EqualTo(GuideText));Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));
            Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False,"visible for no real time yet");Assert.That(Events("hint"),Is.Empty);
            clock+=.18;Refresh();
            Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1));
            var lines=Events("hint");Assert.That(lines.Length,Is.EqualTo(1));Assert.That(lines[0].id,Is.EqualTo("serve"));Assert.That(lines[0].action,Is.EqualTo("seen"));Assert.That(lines[0].where,Is.EqualTo("guide"));
            clock+=2;Refresh();Refresh();Assert.That(Events("hint").Length,Is.EqualTo(1),"written once");
        }
        [UnityTest] public IEnumerator Guide_AddsUpRealTimeOverSeveralFrames()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            for(int i=0;i<3;i++){clock+=.05;Refresh();}
            Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False,"0.15 s is not enough");
            clock+=.05;Refresh();Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1),"0.20 s is");
        }
        [UnityTest] public IEnumerator Guide_ThePausedGameDoesNotAddVisibleTime()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            app.Clock.Paused=true;clock+=5;Refresh();clock+=5;Refresh();
            Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False,"a paused game is not time the player saw it");
            app.Clock.Paused=false;
        }
        [UnityTest] public IEnumerator ServeChoice_RecordsATouchAbove_ATouchBelow_AndNoTouch()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            clock+=.18;Refresh();Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1));
            controller.Enqueue(Touch(3.5f,"serve-high"));controller.ProcessPending();Refresh();
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Lob));Assert.That(rally.LastServeChosen,Is.True);Assert.That(rally.LastServeFromPause,Is.True);
            var first=Events("serve-choice");Assert.That(first.Length,Is.EqualTo(1));Assert.That(first[0].chosen,Is.True);Assert.That(first[0].kind,Is.EqualTo("Lob"));
            Assert.That(first[0].hintSeen,Is.False,"H0 became seen during that very pause, not before it");
            Assert.That(ReachGuide(),Is.True,"the human wins another point");yield return ShowGuide();
            Assert.That(Shown(Find("serve-guide-hint")),Is.False,"seen once, never again");
            controller.Enqueue(Touch(1.2f,"serve-low"));controller.ProcessPending();Refresh();
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Drive));Assert.That(rally.LastServeChosen,Is.True);
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            Assert.That(Drive(()=>!rally.HumanServePause,4),Is.True,"no touch: the pause runs out");Refresh();
            Assert.That(rally.LastServeType,Is.EqualTo(ServeType.Lob));Assert.That(rally.LastServeChosen,Is.False);
            var all=Events("serve-choice");Assert.That(all.Length,Is.EqualTo(3));
            Assert.That(all[1].chosen,Is.True);Assert.That(all[1].kind,Is.EqualTo("Drive"));Assert.That(all[1].hintSeen,Is.True);
            Assert.That(all[2].chosen,Is.False);Assert.That(all[2].kind,Is.EqualTo("Lob"));Assert.That(all[2].hintSeen,Is.True);
        }

        // ---------- criterion 2: H1 in the banner ----------
        [UnityTest] public IEnumerator Volley_IsArmedWhenTheAmberRingShowedWithoutAVolley_FromRealPlay()
        {
            OnlyVolleyLeft();bool armed=false;
            for(long seed=1;seed<=8&&!armed;seed++)
            {
                PlayerPrefs.DeleteKey("sc.hints.window");StartHints(true,seed);
                armed=Drive(()=>hud.Hints.State(HintId.Volley)==HintState.Armed||rally.Phase==RallyPhase.MatchOver,1500)&&hud.Hints.State(HintId.Volley)==HintState.Armed;
            }
            Assert.That(armed,Is.True,"an offered volley that autopilot (no touch) did not take, within eight matches");
            Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False,"armed in memory only: nothing is written");
            yield return null;
        }
        [UnityTest] public IEnumerator Volley_ShowsInTheOrdinaryPause_AndIsSeenWhenTheOpacityReachesOne_NotBefore()
        {
            OnlyVolleyLeft();StartHints(false);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachRivalFirstPause(),Is.True);Refresh();
            yield return Settle();
            Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Volley));Assert.That(Find("rally-banner-hint").style.display.value,Is.EqualTo(DisplayStyle.Flex));
            Assert.That(BannerHint.text,Is.EqualTo(VolleyText));
            hud.Tick(.05f);yield return Settle();Assert.That(hud.BannerOpacity,Is.LessThan(1f));
            Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False,"not before the opacity is 1");
            hud.Tick(RallyHud.BannerInSeconds);Refresh();yield return null;Refresh();
            Assert.That(hud.BannerOpacity,Is.EqualTo(1f));Assert.That(PlayerPrefs.GetInt("sc.hints.volley"),Is.EqualTo(1));
            var lines=Events("hint");Assert.That(lines.Length,Is.EqualTo(1));Assert.That(lines[0].id,Is.EqualTo("volley"));Assert.That(lines[0].where,Is.EqualTo("banner"));
        }

        // ---------- criterion 3: H2 and H3, armed in memory, retired ----------
        [UnityTest] public IEnumerator FrontWallStrokes_ArmBoast_InMemoryOnly()
        {
            Preset(serve:1,volley:1,cross:1);StartHints(true);
            Assert.That(Drive(()=>hud.Hints.State(HintId.Boast)==HintState.Armed,900),Is.True,"an applied front-wall stroke with the side aim open");
            Assert.That(PlayerPrefs.HasKey("sc.hints.boast"),Is.False);Assert.That(Events("hint"),Is.Empty);
            Assert.That(controller.ServeAim,Is.Null);yield return null;
        }
        private IEnumerator HumanBoast(float dz)
        {
            Assert.That(Drive(()=>rally.Phase==RallyPhase.HumanReception&&controller.Plan.Exists&&!controller.AttemptConsumed,300),Is.True,"a reception to hit");
            Assert.That(app.Host.ClockMap.TrySimToReal(controller.Plan.TStarStep,0,out var star),Is.True);
            Drive(()=>app.Host.ObservedRealTime+StepReal>=star-1e-10,30);
            double left=star-app.Host.ObservedRealTime;if(left>0)app.Host.Advance(left);
            var p=controller.Plan.PStar;var court=app.Host.ActiveConfig.Court;var wall=SideAim.NearWall(p.x,court);var side=new SideAim(wall,Mathf.Max(.5f,p.z+dz),1.4f);
            controller.Enqueue(new ClosedGesture(app.Host.LaunchId,Guid.NewGuid().ToString("N"),1,app.Host.LaunchRealTime+.01,star,true,GestureOutcome.Swing,side.UV,.8f,app.Host.ActiveConfig.Source,"fixture",star,side:side));
            yield return null;
        }
        [UnityTest] public IEnumerator ATwoWallShot_RetiresBoast_ArmsTheCross_InTheFrameOfShotResolved_WithNothingVisible()
        {
            Preset(serve:1,volley:1);bool done=false;
            foreach(float dz in new[]{-.8f,-.5f,-1.1f,-1.4f,-.3f,-1.7f})
            {
                StartHints(false);bool resolvedWithBoast=false;string phaseInFrame=null;bool bannerHint=true,guideHint=true;
                Action<ShotRecord> probe=r=>{if(controller.LastStrokeBoast){resolvedWithBoast=true;phaseInFrame=rally.Phase.ToString();hud.Refresh();bannerHint=Shown(Find("rally-banner-hint"));guideHint=Shown(Find("serve-guide-hint"));}};
                controller.ShotResolved+=probe;
                yield return HumanBoast(dz);controller.ProcessPending();
                Drive(()=>controller.LastRecord!=null&&controller.LastRecord.LaunchId==app.Host.LaunchId&&!controller.WaitingForContact,5);
                controller.ShotResolved-=probe;
                if(!resolvedWithBoast){rally.StopMode();continue;}
                done=true;
                Assert.That(phaseInFrame,Is.Not.EqualTo(RallyPhase.PointOver.ToString()),"in play, not in a pause");Assert.That(bannerHint||guideHint,Is.False,"both hint rows are hidden in that frame");
                Assert.That(PlayerPrefs.GetInt("sc.hints.boast"),Is.EqualTo(2),"retired: key = 2");
                var lines=Events("hint");Assert.That(lines.Count(l=>l.id=="boast"&&l.action=="retired"),Is.EqualTo(1));Assert.That(lines.First(l=>l.id=="boast").where,Is.Empty);
                Assert.That(hud.Hints.State(HintId.BoastCross),Is.EqualTo(HintState.Armed),"the same shot arms the cross");Assert.That(PlayerPrefs.HasKey("sc.hints.boastCross"),Is.False);
                break;
            }
            Assert.That(done,Is.True,"one of the side orders must be a solved, applied two-wall shot");
        }
        [UnityTest] public IEnumerator AFallbackBoast_ArmsBoastLikeAFrontWallStroke_AndNotTheCross()
        {
            Preset(serve:1,volley:1);StartHints(false);
            yield return HumanBoast(+.9f);controller.ProcessPending();
            Drive(()=>controller.LastRecord!=null&&!controller.WaitingForContact&&controller.LastRecord.LaunchId==app.Host.LaunchId,5);
            Assert.That(controller.LastCausal.FellBack,Is.True);Assert.That(controller.LastStrokeBoast,Is.False);
            Assert.That(hud.Hints.State(HintId.BoastCross),Is.EqualTo(HintState.Unseen));
            if(controller.LastRecord.Applied)Assert.That(hud.Hints.State(HintId.Boast),Is.EqualTo(HintState.Armed),"counted as a front-wall stroke");
            Assert.That(PlayerPrefs.HasKey("sc.hints.boast"),Is.False);
        }

        // ---------- criterion 4: priority, carry over, visibility ----------
        [UnityTest] public IEnumerator VolleyAndBoastArmed_ShowVolleyFirst_AndBoastInTheNextOrdinaryPauseOfTheMatch()
        {
            Preset(serve:1,cross:1);StartHints(false);hud.Hints.ArmForTests(HintId.Boast);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();
            Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Volley));Assert.That(BannerHint.text,Is.EqualTo(VolleyText));
            yield return FadeBannerIn();Assert.That(PlayerPrefs.GetInt("sc.hints.volley"),Is.EqualTo(1));Assert.That(PlayerPrefs.HasKey("sc.hints.boast"),Is.False,"one hint per pause");
            Assert.That(Drive(()=>Ordinary&&rally.PointsPlayed>=2&&rally.LastPoint.HasValue,900),Is.True,"the next ordinary pause");yield return Settle();
            Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Boast));Assert.That(BannerHint.text,Is.EqualTo(BoastText));
        }
        [UnityTest] public IEnumerator AnArmedHint_IsNotShownNorSeenInTheHumanServePause_AndComesInTheOrdinaryOne()
        {
            Preset(serve:1,boast:2,cross:1);StartHints(true);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            Assert.That(hud.Hints.Shown,Is.Null,"H0 is seen: the guide shows no hint and the armed one waits");
            Assert.That(Shown(Find("rally-banner-hint")),Is.False);Assert.That(Shown(Find("serve-guide-hint")),Is.False);
            clock+=1;hud.Tick(1f);Refresh();Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False,"not seen where it was hidden");
            Assert.That(hud.Hints.State(HintId.Volley),Is.EqualTo(HintState.Armed));
            Assert.That(Drive(()=>hud.Hints.Shown==HintId.Volley&&Ordinary,900),Is.True,"it shows in the next ordinary pause");
        }
        // ---------- criterion 5: the pause ends before the oracle ----------
        [UnityTest] public IEnumerator Banner_ALongFrameLeavesTheOpacityBelowOne_AndSeguirDoesNotMarkIt()
        {
            OnlyVolleyLeft();StartHints(false);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Volley));
            clock+=.18;hud.Tick(.05f);Refresh();yield return null;Refresh();
            Assert.That(hud.BannerOpacity,Is.LessThan(1f),"a clipped frame: 0.18 s real, 0.05 s of fade");
            Click("rally-next");Refresh();
            Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False);Assert.That(Events("hint"),Is.Empty);Assert.That(hud.Hints.State(HintId.Volley),Is.EqualTo(HintState.Armed),"it stays armed");
            Assert.That(Drive(()=>hud.Hints.Shown==HintId.Volley&&Ordinary,900),Is.True,"and comes in the next ordinary pause of the same match");
        }
        [UnityTest] public IEnumerator Banner_WithFullOpacityBeforeSeguir_TheHintIsSeen()
        {
            OnlyVolleyLeft();StartHints(false);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();yield return FadeBannerIn();
            Click("rally-next");Refresh();Assert.That(PlayerPrefs.GetInt("sc.hints.volley"),Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator Guide_ATouchOrSeguirBefore0_18Seconds_DoesNotMarkH0_AndItComesBackInTheNextServePause()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            clock+=.05;Refresh();controller.Enqueue(Touch(3.5f,"early-touch"));controller.ProcessPending();Refresh();
            Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False);Assert.That(Events("hint"),Is.Empty);
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();Assert.That(Shown(Find("serve-guide-hint")),Is.True,"armed again by its own pause");
            clock+=.1;Refresh();Click("rally-next");Refresh();Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False,"Seguir at 0.10 s");
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();clock+=.18;Refresh();Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1));
        }

        // ---------- criterion 6: early abandonment ----------
        [UnityTest] public IEnumerator Abandon_At00BeforeAnyPause_WritesNoKey_AndTheNextMatchIsStillInTheWindow()
        {
            StartHints(false);Assert.That(hud.Hints.InWindowMatch,Is.True);
            rally.StopMode();Refresh();Assert.That(AnyKey(),Is.False);Assert.That(hud.Hints.InWindowMatch,Is.False);
            StartHints(false);Assert.That(hud.Hints.InWindowMatch,Is.True,"the next match from the menu is in the window");
            app.UserEvents.EndSession();Assert.That(AnyKey(),Is.False,"closing the app writes nothing either");
            yield return null;
        }
        [UnityTest] public IEnumerator Abandon_WithAnArmedHint_DoesNotBringItBack_UnlessTheTriggerRepeats()
        {
            // PO approval, 28 nov: VolleyEnabled=0 in this test's configuration.
            OnlyVolleyLeft();StartHints(false,volleyEnabled:0);hud.Hints.ArmForTests(HintId.Volley);rally.StopMode();Refresh();
            Assert.That(AnyKey(),Is.True,"only the preset keys");Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False);
            StartHints(false,volleyEnabled:0);Assert.That(hud.Hints.State(HintId.Volley),Is.EqualTo(HintState.Unseen),"armed hints are not restored");
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();Assert.That(hud.Hints.Shown,Is.Null,"nothing shows: the trigger did not repeat");
            Assert.That(Shown(Find("rally-banner-hint")),Is.False);
            hud.Hints.ArmForTests(HintId.Volley);Assert.That(Drive(()=>hud.Hints.Shown==HintId.Volley&&Ordinary,900),Is.True,"when it repeats it arms and shows in the next ordinary pause");
        }
        [UnityTest] public IEnumerator Abandon_AndASceneRestart_KeepTheStoredKeysAndRestoreNothingArmed()
        {
            // PO approval, 28 nov: VolleyEnabled=0 in this test's configuration.
            Preset(serve:1,boast:2,cross:1);StartHints(false,volleyEnabled:0);hud.Hints.ArmForTests(HintId.Volley);
            var kept=AllHintKeys.Where(PlayerPrefs.HasKey).ToDictionary(k=>k,k=>PlayerPrefs.GetInt(k));
            yield return Shutdown();yield return Boot(true);
            clock=100;FirstMatchHints.Now=()=>clock;FirstMatchHints.ForceWindowForTests=true;
            foreach(var pair in kept)PlayerPrefs.SetInt(pair.Key,pair.Value);
            StartHints(false,volleyEnabled:0);
            Assert.That(hud.Hints.State(HintId.Serve),Is.EqualTo(HintState.Seen));Assert.That(hud.Hints.State(HintId.Boast),Is.EqualTo(HintState.Retired));
            Assert.That(hud.Hints.State(HintId.Volley),Is.EqualTo(HintState.Unseen),"lo armado no se restaura");
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();Assert.That(hud.Hints.Shown,Is.Null);
            Assert.That(PlayerPrefs.HasKey("sc.hints.window"),Is.False,"the window keeps its state");
        }
        [UnityTest] public IEnumerator Abandon_AfterH0WasSeenInTheBanner_NeverShowsItAgain()
        {
            // PO approval, 28 nov: VolleyEnabled=0 in this test's configuration.
            StartHints(false,volleyEnabled:0);Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();
            Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));yield return FadeBannerIn();Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1));
            rally.StopMode();Refresh();
            StartHints(false,volleyEnabled:0);Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();
            Assert.That(hud.Hints.Shown,Is.Null);Assert.That(Shown(Find("rally-banner-hint")),Is.False,"not in the banner");
            rally.Autopilot=true;Assert.That(ReachGuide(),Is.True);yield return ShowGuide();Assert.That(Shown(Find("serve-guide-hint")),Is.False,"nor in the guide");
        }
        [UnityTest] public IEnumerator Abandon_WhenH0ReachesTheGuideNextMatch_ItArmsAgain()
        {
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();clock+=.05;Refresh();
            rally.StopMode();Refresh();Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False);
            StartHints(true);Assert.That(ReachGuide(),Is.True);yield return ShowGuide();Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));Assert.That(Shown(Find("serve-guide-hint")),Is.True);
        }

        // ---------- criterion 7: the window ----------
        [UnityTest] public IEnumerator Window_ClosesWhenAWindowMatchEndsNormally_AndTheNextOneShowsNothing()
        {
            Preset(serve:1,volley:1,boast:2,cross:0);StartHints(true);
            Assert.That(Drive(()=>rally.Phase==RallyPhase.MatchOver,1500),Is.True);Refresh();
            Assert.That(PlayerPrefs.GetInt("sc.hints.window"),Is.EqualTo(1));
            StartHints(true);Assert.That(hud.Hints.InWindowMatch,Is.False,"the next match from the menu is outside the window");
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();Assert.That(Shown(Find("serve-guide-hint")),Is.False);Assert.That(Shown(Find("rally-banner-hint")),Is.False);
        }
        [UnityTest] public IEnumerator Window_AutopilotRematchDevMatchAndPracticeNeverShowHints()
        {
            FirstMatchHints.ForceWindowForTests=false;
            PlayerPrefs.SetInt("sc.firstRun.tutorialSeen",1);rally.Autopilot=true;rally.AutopilotTimingSigmaMs=0;rally.Level=AiLevel.Easy;rally.Seed=3;
            flow.Play();Refresh();Assert.That(rally.Match,Is.Not.Null,"a match from the menu");
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            Assert.That(hud.Hints.InWindowMatch,Is.False,"autopilot excludes the match");Assert.That(Shown(Find("serve-guide-hint")),Is.False);clock+=1;Refresh();Assert.That(AnyKey(),Is.False);
            // The DEV «Partido» button, a rematch and the practice court do not come from the menu.
            rally.StopMode();rally.Autopilot=false;hud.StartMatch();Assert.That(hud.Hints.InWindowMatch,Is.False,"the DEV Partido");
            FirstMatchHints.ForceWindowForTests=true;StartHints(false);Assert.That(hud.Hints.InWindowMatch,Is.True);rally.Rematch();Refresh();Assert.That(hud.Hints.InWindowMatch,Is.False,"a rematch leaves the window");
            rally.StopMode();Refresh();Assert.That(rally.StartMode(),Is.True);Refresh();Assert.That(hud.Hints.InWindowMatch,Is.False,"practice");
        }
        [UnityTest] public IEnumerator Window_ClosesWhenTheLastHintIsRetired_WithoutAnythingVisible()
        {
            Preset(serve:1,volley:1,cross:1);StartHints(false);Assert.That(hud.Hints.InWindowMatch,Is.True);
            hud.Hints.ArmForTests(HintId.BoastCross);// a two-wall shot: retires the boast, the fourth hint done
            Assert.That(PlayerPrefs.GetInt("sc.hints.boast"),Is.EqualTo(2));Assert.That(PlayerPrefs.GetInt("sc.hints.window"),Is.EqualTo(1));
            Assert.That(Events("hint").Count(l=>l.id=="boast"&&l.action=="retired"),Is.EqualTo(1));
            yield return null;
        }

        // ---------- criterion 8: never in play ----------
        [UnityTest] public IEnumerator NeverInPlay_BothHintRowsAreHiddenInEveryFrameThatIsNotAPause()
        {
            StartHints(true);bool everShown=false;int checkedFrames=0;
            hud.Hints.ArmForTests(HintId.Volley);hud.Hints.ArmForTests(HintId.Boast);
            // One simulation step per iteration; real frames are given to the layout while a pause is on, so the rows can really show.
            for(int guard=0;guard<400000&&rally.Phase!=RallyPhase.MatchOver;guard++)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();Refresh();hud.Tick((float)StepReal);
                bool shown=Shown(Find("rally-banner-hint"))||Shown(Find("serve-guide-hint"));
                if(shown)everShown=true;
                if(rally.Phase!=RallyPhase.PointOver){checkedFrames++;Assert.That(shown,Is.False,"a hint row is visible in "+rally.Phase);}
                else if(guard%6==0)yield return null;
            }
            Assert.That(rally.Phase,Is.EqualTo(RallyPhase.MatchOver));
            Assert.That(checkedFrames,Is.GreaterThan(100));Assert.That(everShown,Is.True,"the check is not vacuous: the rows do show in a pause");
        }

        // ---------- criterion 9: no added time ----------
        private sealed class RunTrace {public readonly List<string> Points=new List<string>(),Waits=new List<string>(),Starts=new List<string>();}
        private RunTrace Run(bool on)
        {
            FirstMatchHints.Enabled=on;rally.StopMode();var trace=new RunTrace();
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=25;rally.Level=AiLevel.Easy;rally.Seed=5;rally.RandomStyles=false;rally.StyleOverride=RivalStyleId.Fondista;
            rally.SetParams(new RallyParams{ServeKindOn=1});
            Action<PointResult> ended=r=>{trace.Points.Add(r.Winner+"/"+r.Cause+"/"+r.Shots);trace.Waits.Add(rally.WaitBetweenPoints.ToString("R"));};
            Action started=()=>trace.Starts.Add(app.Host.LaunchId+"/"+app.Host.Current.Step+"/"+rally.LastServeType);
            rally.PointEnded+=ended;rally.RallyStarted+=started;
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();long firstLaunch=app.Host.LaunchId;
            Assert.That(Drive(()=>rally.PointsPlayed>=5||rally.Phase==RallyPhase.MatchOver,900),Is.True);
            rally.PointEnded-=ended;rally.RallyStarted-=started;
            for(int i=0;i<trace.Starts.Count;i++){var parts=trace.Starts[i].Split('/');trace.Starts[i]=(long.Parse(parts[0])-firstLaunch)+"/"+parts[1]+"/"+parts[2];}
            return trace;
        }
        [UnityTest] public IEnumerator NoAddedTime_OnMatchesOff()
        {
            flow.ClosePractice();
            var on=Run(true);var off=Run(false);
            Assert.That(on.Points.Count,Is.GreaterThanOrEqualTo(3));
            Assert.That(on.Points,Is.EqualTo(off.Points),"PointResult sequence");Assert.That(on.Waits,Is.EqualTo(off.Waits),"each WaitBetweenPoints, exactly");Assert.That(on.Starts,Is.EqualTo(off.Starts),"start step of each point");
            yield return null;
        }
        // Criterion 9, stricter (PR #103 round 2): the step of the ball at RallyStarted is local to each launch (every serve starts at 0), so it
        // cannot show a later start. This counter is the harness' own: simulation steps advanced since the start of the run, continuous across
        // points, so a point that begins later shows up even if WaitBetweenPoints keeps its value. Same inputs in both runs, exact equality.
        private sealed class SteppedTrace
        {
            public readonly List<string> Points=new List<string>(),Starts=new List<string>(),Ends=new List<string>();public bool HintShown;
        }
        private SteppedTrace RunStepped(bool on)
        {
            FirstMatchHints.Enabled=on;rally.StopMode();var trace=new SteppedTrace();long steps=0;
            rally.Autopilot=true;rally.AutopilotTimingSigmaMs=25;rally.Level=AiLevel.Easy;rally.Seed=5;rally.RandomStyles=false;rally.StyleOverride=RivalStyleId.Fondista;
            rally.SetParams(new RallyParams{ServeKindOn=1});
            Action<PointResult> ended=r=>{trace.Points.Add(r.Winner+"/"+r.Cause+"/"+r.Shots+"/"+r.Reception);trace.Ends.Add(steps+"/"+rally.WaitBetweenPoints.ToString("R"));};
            Action started=()=>trace.Starts.Add(steps+"/"+rally.LastServeType+"/"+rally.Match.Server);
            rally.PointEnded+=ended;rally.RallyStarted+=started;
            Assert.That(rally.StartMatch(),Is.True);hud.Hints.BeginMatch();
            while(steps<400000&&rally.PointsPlayed<6&&rally.Phase!=RallyPhase.MatchOver)
            {
                app.Host.Advance(StepReal);controller.ProcessPending();rally.Tick();scape.Tick((float)StepReal);Refresh();steps++;
                if(hud.Hints.Shown.HasValue)trace.HintShown=true;
            }
            rally.PointEnded-=ended;rally.RallyStarted-=started;
            return trace;
        }
        [UnityTest] public IEnumerator NoAddedTime_EachPointStartsAtTheSameContinuousStep_OnAndOff()
        {
            flow.ClosePractice();
            var on=RunStepped(true);var off=RunStepped(false);
            Assert.That(on.Points.Count,Is.GreaterThanOrEqualTo(5),"enough points to compare");Assert.That(on.Starts.Count,Is.GreaterThanOrEqualTo(5));
            Assert.That(on.HintShown,Is.True,"the run with the hints on really showed one: the comparison is not vacuous");Assert.That(off.HintShown,Is.False);
            Assert.That(on.Points,Is.EqualTo(off.Points),"PointResult (winner, cause, shots, reception)");
            Assert.That(on.Ends,Is.EqualTo(off.Ends),"the step at which each point ended and its WaitBetweenPoints");
            Assert.That(on.Starts,Is.EqualTo(off.Starts),"the continuous step at which each point began, and its serve");
            yield return null;
        }
        [UnityTest] public IEnumerator Off_NothingIsWrittenOrShown()
        {
            FirstMatchHints.Enabled=false;StartHints(true);
            Assert.That(ReachGuide(),Is.True);yield return ShowGuide();clock+=1;Refresh();
            Assert.That(hud.Hints.Shown,Is.Null);Assert.That(Shown(Find("serve-guide-hint")),Is.False);Assert.That(AnyKey(),Is.False);Assert.That(Events("hint"),Is.Empty);Assert.That(Events("serve-choice"),Is.Empty);
            controller.Enqueue(Touch(3.5f,"off-touch"));controller.ProcessPending();Refresh();Assert.That(Events("serve-choice"),Is.Empty,"the serve-choice line belongs to the hints");
        }

        // ---------- criterion 11: persistence and the DEV button ----------
        [UnityTest] public IEnumerator DevButton_ResetsOnlyTheHintKeys()
        {
            for(int i=0;i<4;i++){Refresh();yield return null;}
            Preset(serve:1,volley:1,boast:2,cross:1);PlayerPrefs.SetInt("sc.hints.window",1);PlayerPrefs.SetInt("sc.records.longestRally",9);PlayerPrefs.SetInt("sc.firstRun.level",2);
            var button=root.Q<Button>("hints-reset");Assert.That(button,Is.Not.Null,"the DEV panel has «Reiniciar pistas»");Assert.That(button.text,Is.EqualTo("Reiniciar pistas"));
            Click("hints-reset");
            Assert.That(AnyKey(),Is.False,"every hint key is gone");Assert.That(PlayerPrefs.GetInt("sc.records.longestRally"),Is.EqualTo(9));Assert.That(PlayerPrefs.GetInt("sc.firstRun.level"),Is.EqualTo(2));
            Assert.That(FirstMatchHints.AllKeys(),Is.EquivalentTo(AllHintKeys));
            PlayerPrefs.DeleteKey("sc.records.longestRally");PlayerPrefs.DeleteKey("sc.firstRun.level");
        }

        // ---------- criterion 13 [H0-aviso] ----------
        [UnityTest] public IEnumerator H0InTheBanner_ShowsBeforeTheRivalsFirstServe_AndIsSeenAtOpacityOne()
        {
            StartHints(false);Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();
            Assert.That(rally.WaitBetweenPoints,Is.EqualTo(2.5d));Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));Assert.That(BannerHint.text,Is.EqualTo(BannerServeText));
            hud.Tick(.05f);yield return Settle();Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False);
            hud.Tick(RallyHud.BannerInSeconds);Refresh();yield return null;Refresh();
            Assert.That(PlayerPrefs.GetInt("sc.hints.serve"),Is.EqualTo(1));var lines=Events("hint");Assert.That(lines.Length,Is.EqualTo(1));
            Assert.That(lines[0].id,Is.EqualTo("serve"));Assert.That(lines[0].action,Is.EqualTo("seen"));Assert.That(lines[0].where,Is.EqualTo("banner"));
            // The next serve pause: the key is shared, so the guide shows no hint and stays as it is today.
            rally.Autopilot=true;Assert.That(ReachGuide(),Is.True);yield return ShowGuide();
            Assert.That(Shown(Find("serve-guide")),Is.True,"the guide itself is unchanged");Assert.That(Shown(Find("serve-guide-hint")),Is.False);Assert.That(Events("hint").Length,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator H0InTheBanner_SeguirBeforeOpacityOne_LeavesItForTheGuide_NeverTheBannerAgain()
        {
            StartHints(false);Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));
            hud.Tick(.05f);Refresh();Click("rally-next");Refresh();Assert.That(PlayerPrefs.HasKey("sc.hints.serve"),Is.False);Assert.That(Events("hint"),Is.Empty);
            Assert.That(Drive(()=>Ordinary&&rally.PointsPlayed>=2,600),Is.True,"the human never swings: the rival keeps winning");yield return Settle();
            Assert.That(hud.Hints.Shown,Is.Not.EqualTo(HintId.Serve),"the rival already served: H0 does not come back to the banner (the ring that showed with no stroke arms H1)");
            rally.Autopilot=true;Assert.That(ReachGuide(),Is.True);yield return ShowGuide();Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve),"but the guide shows it");Assert.That(GuideHint.text,Is.EqualTo(GuideText));
        }
        [UnityTest] public IEnumerator H0InTheBanner_IfItWasSeenInTheGuide_TheRivalsFirstServePauseShowsNothing()
        {
            // PO approval, 28 nov: VolleyEnabled=0 in this test's configuration.
            Preset(serve:1);StartHints(false,volleyEnabled:0);Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();
            Assert.That(hud.Hints.Shown,Is.Null);Assert.That(Shown(Find("rally-banner-hint")),Is.False);
        }
        [UnityTest] public IEnumerator H0InTheBanner_GoesBeforeAnArmedVolley_WhichWaitsForTheNextOrdinaryPause()
        {
            Preset(boast:2,cross:1);StartHints(false);hud.Hints.ArmForTests(HintId.Volley);
            Assert.That(ReachRivalFirstPause(),Is.True);yield return Settle();Assert.That(hud.Hints.Shown,Is.EqualTo(HintId.Serve));
            yield return FadeBannerIn();Assert.That(PlayerPrefs.HasKey("sc.hints.volley"),Is.False);
            Assert.That(Drive(()=>hud.Hints.Shown==HintId.Volley&&Ordinary,900),Is.True,"H1 waited for the next ordinary pause");
        }
    }
}
