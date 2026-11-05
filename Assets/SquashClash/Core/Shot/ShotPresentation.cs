using System.Collections.Generic;
using UnityEngine;
namespace SquashClash.Core.Shot
{
    public enum PresentationCategory {Perfect,Valid,Failed}
    // One immutable final-result presentation per (LaunchId, AttemptId); every S3 channel consumes this value.
    public readonly struct ShotPresentation
    {
        public readonly long LaunchId,ContactStep;
        public readonly string AttemptId,Fail,Side;
        public readonly PresentationCategory Category;
        public readonly int Version;
        public readonly double DeltaMs;
        public readonly bool HasContact;
        public readonly Vector3 ContactPosition;
        public ShotPresentation(long launchId,string attemptId,PresentationCategory category,int version,string fail,double deltaMs,bool hasContact,long contactStep,Vector3 contactPosition,string side)
        {LaunchId=launchId;AttemptId=attemptId;Category=category;Version=version;Fail=fail;DeltaMs=deltaMs;HasContact=hasContact;ContactStep=contactStep;ContactPosition=contactPosition;Side=side;}
    }
    public sealed class ShotPresentationGate
    {
        private readonly Dictionary<string,(PresentationCategory category,int version)> delivered=new Dictionary<string,(PresentationCategory,int)>();
        private readonly Dictionary<string,string> results=new Dictionary<string,string>();
        public int DiscrepancyCount {get;private set;}
        public string LastDiscrepancy {get;private set;}
        public void Reset(long launchId){delivered.Clear();results.Clear();DiscrepancyCount=0;LastDiscrepancy=null;}
        public bool TryDeliver(ShotRecord record,long currentLaunchId,out ShotPresentation presentation,out string rejection)
        {presentation=default;rejection="not-implemented";return false;}
    }
}
