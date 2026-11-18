namespace SquashClash.Core.Shot
{
    // The striker's arm at one contact (spec 4): speed lost as a fraction and position-vector gain, both already scaled by f_s.
    public readonly struct FatigueArm
    {
        public readonly float SpeedLoss,PositionGain;public readonly double Factor;
        public FatigueArm(float speedLoss,float positionGain,double factor=0){SpeedLoss=speedLoss;PositionGain=positionGain;Factor=factor;}
        public static readonly FatigueArm None=default;
        public bool Active=>SpeedLoss>0||PositionGain>0;
    }
}
