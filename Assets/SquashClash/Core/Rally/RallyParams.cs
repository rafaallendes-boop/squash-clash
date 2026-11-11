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
        public double ReactionS=.15,Accel=14,MaxSpeed=5.5,Brake=18,ArriveEpsilon=.01;
        // Contact candidate scoring (spec 9.1).
        public double CandidateIntervalS=.02,SlackCapS=.4,WeightSlack=1,WeightHeight=.5,WeightWall=.3;
        public double BodyWallMargin=.35,PlanAttempts=6;
        // Waiting positions (spec 9.2, 11.4). The lateral offset only keeps both bodies readable at the T.
        public double HumanWaitBehindT=0,RivalWaitBehindT=.5,WaitLateralOffset=.4;
        // Neutral launch (spec 15): a soft feed to the front wall that comes back to the receiver's side.
        public double LaunchHeight=1.2,LaunchZ=6.5,LaunchLateral=.9,LaunchTargetHeight=2.3,LaunchHorizontalSpeed=13;
        // Rally loop.
        public double RestartSeconds=1.6,MaxRallySeconds=180,MaxShots=400;
        // AI difficulty (spec 11.5): Easy / Medium / Hard. Only these change with difficulty.
        public double EasyPerceptionS=.08,EasyStanceSigma=.12,EasyAimSigma=.28,EasyPowerSigma=.12,EasyTimingSigmaMs=55,EasyTemperature=1.2;
        public double MediumPerceptionS=.04,MediumStanceSigma=.06,MediumAimSigma=.16,MediumPowerSigma=.08,MediumTimingSigmaMs=38,MediumTemperature=.6;
        public double HardPerceptionS=0,HardStanceSigma=.02,HardAimSigma=.08,HardPowerSigma=.04,HardTimingSigmaMs=22,HardTemperature=.3;
        // Fondo style weights (spec 11.4).
        public double FondoLongWeight=1.2,FondoAwayWeight=.8,FondoTimeWeight=.8,FondoDropWeight=.9,FondoRiskAversion=1.6;
        public double FondoDefenseSlackS=.12,FondoDropMinSlackS=.3;
        // Option geometry (spec 11.3).
        public double AimLow=.95,AimMid=1.7,AimHigh=3,DropHeight=.75,PowerDrop=.12,PowerMedium=.5,PowerStrong=.85,CornerInset=.6;
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
