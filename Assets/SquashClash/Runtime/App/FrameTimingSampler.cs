using UnityEngine;
namespace SquashClash.Runtime.App
{
    [DefaultExecutionOrder(-100)]
    public sealed class FrameTimingSampler : MonoBehaviour
    {
        private readonly FrameTiming[] timings=new FrameTiming[1];
        private ulong lastTimestamp;
        private int discardFrames;
        private bool paused,focused=true;
        public double CpuMs { get; private set; }=double.NaN;
        public double GpuMs { get; private set; }=double.NaN;
        public long SampleSerial { get; private set; }
        private void Update()
        {
            CpuMs=GpuMs=double.NaN;
            if(paused||!focused||!FrameTimingManager.IsFeatureEnabled())return;
            FrameTimingManager.CaptureFrameTimings();
            if(discardFrames>0){discardFrames--;return;}
            if(FrameTimingManager.GetLatestTimings(1,timings)>0)AcceptTiming(timings[0]);
        }
        public bool AcceptTiming(FrameTiming timing)
        {
            CpuMs=GpuMs=double.NaN;
            if(timing.frameStartTimestamp==0||timing.frameStartTimestamp<=lastTimestamp)return false;
            lastTimestamp=timing.frameStartTimestamp;
            if(Valid(timing.cpuFrameTime))CpuMs=timing.cpuFrameTime;
            if(Valid(timing.gpuFrameTime))GpuMs=timing.gpuFrameTime;
            if(double.IsNaN(CpuMs)&&double.IsNaN(GpuMs))return false;
            SampleSerial++;return true;
        }
        // FrameTimingManager is delayed; discard the first four captures at phase/resume boundaries.
        public void RestartCapture(){discardFrames=4;CpuMs=GpuMs=double.NaN;}
        private void OnApplicationPause(bool value){paused=value;RestartCapture();}
        private void OnApplicationFocus(bool value){focused=value;RestartCapture();}
        private static bool Valid(double v)=>v>0&&!double.IsNaN(v)&&!double.IsInfinity(v);
    }
}
