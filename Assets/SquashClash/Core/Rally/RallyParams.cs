using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
namespace SquashClash.Core.Rally
{
    // [EXPERIMENTO] First Rally starting values. Kept outside DefaultPreset.json on purpose: AutoQA anchors the
    // 107-key default hash, so these live here with their own hash until the PO decides to move them to the catalog.
    // Times are simulation seconds and distances metres unless the name says otherwise.
    public sealed class RallyParams
    {
        // Rules (spec 8.7.3). 20 mm matches the footprint the existing contact planner already uses.
        public double FootprintRadius=.02;
        // Character kinematics (spec 9.1), identical for human and rival.
        public double ReactionS=.18,Accel=12,MaxSpeed=4.8,Brake=16,ArriveEpsilon=.01;
        // Contact candidate scoring (spec 9.1).
        public double CandidateIntervalS=.02,SlackCapS=.4,WeightSlack=1,WeightHeight=.5,WeightWall=.3;
        // Prefer stances whose planned contact sits well inside the reachable run (margin on both sides of T*).
        public double WeightWindow=1.5,WindowMarginCapS=.09;
        public double BodyWallMargin=.35,PlanAttempts=6;
        // Waiting positions (spec 9.2, 11.4). The lateral offset only keeps both bodies readable at the T.
        public double HumanWaitBehindT=0,RivalWaitBehindT=.5,WaitLateralOffset=.4;
        // Neutral launch (spec 15): a soft feed to the front wall that comes back to the receiver's side.
        public double LaunchHeight=1.2,LaunchZ=6.5,LaunchLateral=.9,LaunchTargetHeight=2.3,LaunchHorizontalSpeed=13;
        // Rally loop.
        public double RestartSeconds=1.6,MaxRallySeconds=900,MaxShots=200;
        // A flight with no racket contact and no terminal event for this long is a block (must never happen).
        public double StallSeconds=20;
        // Match (spec 5.4, 5.2): point-a-rally to 7 by 2, pause between points in real seconds.
        public double MatchTarget=7,MatchMargin=2,MatchPauseSeconds=2.5;
        // Automatic serve: height of the hand and preferred landing spot in the receiver's quarter.
        public double ServeHandHeight=1.1,ServeLandingDepth=8,ServeLandingFromWall=1.4;
        // Serve kinds (design 2026-11-20-serve). ServeKindOn: 0 = the original automatic serve, 1 = lob or drive (default).
        // Each kind is one fixed serve per box (front-wall aim x, height, horizontal speed), chosen from the diag01 grid
        // by the predeclared rule (P+V of the newcomer closest to 85 %); every one is checked against the real serve rules.
        public double ServeKindOn=1;
        // Real milliseconds of early allowance, only for the human returning the rival's serve.
        public double HumanServeEarlyMs=25;
        public double ServeKindLobRightAimX=3.6,ServeKindLobRightAimY=3.6,ServeKindLobRightSpeed=18,ServeKindLobLeftAimX=2.8,ServeKindLobLeftAimY=3.45,ServeKindLobLeftSpeed=18;
        public double ServeKindDriveRightAimX=2.8,ServeKindDriveRightAimY=2.55,ServeKindDriveRightSpeed=22,ServeKindDriveLeftAimX=3.6,ServeKindDriveLeftAimY=2.4,ServeKindDriveLeftSpeed=22;
        // The human's choice: an aim at or above this front-wall height (m) is the lob, below it the drive.
        public double ServeKindLobMinAimY=3;
        // Real seconds before a human serve after a point, with serve choice enabled.
        public double ServeKindHumanPauseSeconds=3;
        // The rival's choice: probability of the drive per style (own random stream, apart from its strokes).
        public double ServeKindDriveFondo=.25,ServeKindDriveShooter=.75,ServeKindDriveAllRound=.5;
        // AI difficulty (spec 11.5): Easy / Medium / Hard. Only these change with difficulty.
        public double EasyPerceptionS=.08,EasyStanceSigma=.12,EasyAimSigma=.28,EasyPowerSigma=.12,EasyTimingSigmaMs=55,EasyTemperature=1.2;
        public double MediumPerceptionS=.04,MediumStanceSigma=.06,MediumAimSigma=.16,MediumPowerSigma=.08,MediumTimingSigmaMs=38,MediumTemperature=.6;
        public double HardPerceptionS=0,HardStanceSigma=.02,HardAimSigma=.08,HardPowerSigma=.04,HardTimingSigmaMs=22,HardTemperature=.3;
        // Fondo style weights (spec 11.4).
        public double FondoLongWeight=1.2,FondoAwayWeight=.8,FondoTimeWeight=.8,FondoDropWeight=.9,FondoRiskAversion=1.6;
        public double FondoDefenseSlackS=.12,FondoDropMinSlackS=.3;
        // Keep the release mean inside its own contact window without changing timing sigma.
        public double FondoReleaseMarginSigma=1.5;
        // Option geometry (spec 11.3).
        public double AimLow=.95,AimMid=1.7,AimHigh=3,DropHeight=.75,PowerDrop=.12,PowerMedium=.5,PowerStrong=.85,CornerInset=.6;
        // Intentions never ask for a launch steeper than this: from the front court a fixed height would send the ball out.
        public double MaxAimElevationDeg=22;
        // Volley v1 (design 2026-11-18): on/off, legal heights (spec 7.3.1), separation from the bounce window in real seconds
        // (spec 7.3.2) and the shortest window offered, in simulation seconds (v1 stand-in for q_min_rel).
        public double VolleyEnabled=1,VolleyMinHeight=.6,VolleyMaxHeight=1.9,VolleyMinSeparationS=.25,VolleyMinWindowS=.05;
        // Volley energy (19 nov): a contact before the bounce costs the striker this many energy points (0-100 scale) on top of the
        // stroke, because the player anticipates. Charged only with fatigue on; RallySim bots never volley by default.
        public double VolleyEnergyCost=2;
        // Volley v2, human only (design 2026-11-20): 2 = the player chooses the volley (touch while the amber ring shows sends the body to an
        // intercept stance), 1 = v1 (M1 path). Shortest volley window offered (simulation seconds) and the human's return-of-serve volley.
        // The rival and RallySim never read these fields.
        public double HumanVolleyMode=3,HumanVolleyMinWindowS=.05,HumanVolleyServe=1;
        // Modes: 1 = v1, 2 = v2, 3 = v2.2 (shipped by the predeclared rule, design section 8), 4 = v2.3 lunge (measured, not shipped).
        // v2.2: a touch after S and up to S' still chooses the volley with a sprint of at most this speed factor, and costs this many extra
        // energy points at the contact, times its lateness (S..S' as 0..1).
        public double HumanVolleySprintMax=2,HumanVolleySprintCost=4;
        // Fatigue feel (design 2026-11-20-fatigue-feel, section 1), human only: 1 = the human's energy at the reception start limits the sprint,
        // 1 + (HumanVolleySprintMax - 1)(1 - f): the full sprint at or above the onset, none at energy 0. 0 = v2.2 as shipped (DEV toggle).
        public double HumanVolleySprintFatigue=1;
        // Rival styles (design 2026-11-19). A style changes only how the AI chooses (utility weights, risk aversion, volley
        // willingness, wait W and recovery cover), never physics, rules, reach, windows, errors or information (spec 11.2, 11.5).
        // Fondista reuses the Fondo* weights above. StyleRivalVolley switches the rival volley off (control); a cover depth < 0 is "to the T".
        public double StyleRivalVolley=1,StyleCoverBounceZ=3.5;
        public double StyleFondoVolleyBias=-1,StyleFondoVolleyMinKS=.1,StyleFondoCoverDepth=-1;
        public double StyleShooterLongWeight=.5,StyleShooterAwayWeight=.8,StyleShooterTimeWeight=.3,StyleShooterDropWeight=1.6,StyleShooterRiskAversion=.6;
        public double StyleShooterDefenseSlackS=.06,StyleShooterDropMinSlackS=.3,StyleShooterDropZoneAhead=1,StyleShooterAttackBase=.8,StyleShooterAttackAhead=.4;
        public double StyleShooterVolleyBias=.8,StyleShooterVolleyMinKS=.05,StyleShooterVolleySteal=0,StyleShooterWaitBehindT=0,StyleShooterCoverDepth=-1;
        public double StyleAllRoundLongWeight=1,StyleAllRoundAwayWeight=.8,StyleAllRoundTimeWeight=.6,StyleAllRoundDropWeight=1.1,StyleAllRoundRiskAversion=1.1;
        public double StyleAllRoundDefenseSlackS=.12,StyleAllRoundDropMinSlackS=.3,StyleAllRoundDropZoneAhead=0,StyleAllRoundAttackBase=0,StyleAllRoundAttackAhead=1;
        // Difficulty per style (Claude, 19 nov, after search02): the styles differ in strength, so each (style, level) scales the
        // level's five execution/perception noises by its own k (temperature unchanged). Chosen from the search02 grid; Definidor Hard
        // stays at 1 (deviation: easier than the 20 % target, never below the shared noise).
        public double StyleNoiseKFondoEasy=1.5,StyleNoiseKFondoMedium=1.75,StyleNoiseKFondoHard=2.5;
        public double StyleNoiseKShooterEasy=1.25,StyleNoiseKShooterMedium=1,StyleNoiseKShooterHard=1;
        public double StyleNoiseKAllRoundEasy=1.5,StyleNoiseKAllRoundMedium=1.5,StyleNoiseKAllRoundHard=1.25;
        public double StyleAllRoundVolleyBias=-.8,StyleAllRoundVolleyMinKS=.08,StyleAllRoundVolleySteal=.6,StyleAllRoundWaitBehindT=.2,StyleAllRoundCoverDepth=-1;
        // Two-wall shot (design 2026-11-20-boast-predeclaration, section 5). BoastRival: the rival may boast (DEV «Dos paredes», on by default
        // since round two: M4 within 3 pp with new seeds; the human's
        // gesture is always on). Open from the back quarter (contact z at or beyond BoastBackFraction of the length) with the contact within
        // BoastWallMax of the near side wall; the side-wall point is BoastAheadZ in front of the contact at the low or mid height. Utility
        // weight per style (Fondista least, Definidor most).
        public double BoastRival=1,BoastBackFraction=.75,BoastWallMax=2,BoastAheadZ=1,BoastAimLow=.9,BoastAimMid=1.5;
        public double BoastWeightFondo=0,BoastWeightShooter=-.5,BoastWeightAllRound=0;
        // [EXPERIMENTO] set17 (27 nov; local branch, never shipped). Metres: when > 0, after its own stroke the rival whose ball bounces deep
        // (z >= StyleCoverBounceZ) recovers to T.x -/+ shift towards the side of that bounce, at its style's wait depth. 0 = today's behaviour.
        public double RivalRecoverySideShift=1.6;
        // [EXPERIMENTO] set21 (27 nov; local branch, never shipped). The rival reads the human's swing: RivalSwingReadLeadMs (real ms; 0 = off,
        // today's behaviour) before the human's contact it runs to where the intended shot will land; RivalSwingMisreadP is the probability
        // that read is wrong (direction or length), only used when the lead is > 0.
        public double RivalSwingReadLeadMs=0,RivalSwingMisreadP=0;
        // [EXPERIMENTO] set22 (27 nov; local branch, never shipped). 1 = the rival reads only long swings (RivalSwingRead.LongSwing) and a Length misread is always the drop; 0 = set21.
        public double RivalSwingReadLongOnly=0;
        // [EXPERIMENTO] set25 (28 nov; local branch, never shipped). 1 = the rival reads the swing only when its level (IRallyLevel) is Medium or Hard; Easy or unknown: no read. 0 = set22 (no level distinction).
        public double RivalSwingReadMinLevel=0;
        public string Hash
        {
            get
            {
                var canonical=new StringBuilder();
                foreach(var field in typeof(RallyParams).GetFields(BindingFlags.Public|BindingFlags.Instance).OrderBy(f=>f.Name,StringComparer.Ordinal))
                    canonical.Append(field.Name).Append('=').Append(((double)field.GetValue(this)).ToString("R",CultureInfo.InvariantCulture)).Append('\n');
                using(var sha=SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())),0,6).Replace("-","").ToLowerInvariant();
            }
        }
        public RallyParams Clone()=>(RallyParams)MemberwiseClone();
    }
}
