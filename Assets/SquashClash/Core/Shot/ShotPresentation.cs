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
    // Decides, once per attempt identity and launch, whether a final ShotRecord yields a presentation. Version is traceability only.
    public sealed class ShotPresentationGate
    {
        private readonly Dictionary<string,(string result,int version)> delivered=new Dictionary<string,(string,int)>();
        public int DiscrepancyCount {get;private set;}
        public string LastDiscrepancy {get;private set;}
        public void Reset(long launchId){delivered.Clear();DiscrepancyCount=0;LastDiscrepancy=null;}
        public bool TryDeliver(ShotRecord record,long currentLaunchId,out ShotPresentation presentation,out string rejection)
        {
            presentation=default;rejection=null;
            if(record==null){rejection="no-record";return false;}
            if(record.LaunchId!=currentLaunchId){rejection="wrong-launch";return false;}
            if(string.IsNullOrEmpty(record.AttemptId)){rejection="no-attempt-id";return false;}
            string key=record.LaunchId+"/"+record.AttemptId;
            if(delivered.TryGetValue(key,out var first))
            {
                if(first.result!=record.Result){DiscrepancyCount++;LastDiscrepancy=key+" delivered "+first.result+" v"+first.version+", later "+record.Result+" v"+record.Version;}
                rejection="already-delivered";return false;
            }
            PresentationCategory category;
            if(record.Applied&&record.Result==nameof(PresentationCategory.Perfect))category=PresentationCategory.Perfect;
            else if(record.Applied&&record.Result==nameof(PresentationCategory.Valid))category=PresentationCategory.Valid;
            else if(!record.Applied&&record.Result==nameof(PresentationCategory.Failed))category=PresentationCategory.Failed;
            else{rejection="not-eligible";return false;}
            delivered[key]=(record.Result,record.Version);
            bool contact=category!=PresentationCategory.Failed;
            presentation=new ShotPresentation(record.LaunchId,record.AttemptId,category,record.Version,record.Fail,contact?record.DeltaMs:0,contact,contact?record.ContactStep:0,
                contact?new Vector3(record.ContactX,record.ContactY,record.ContactZ):Vector3.zero,contact?record.Side:null);
            return true;
        }
    }
}
