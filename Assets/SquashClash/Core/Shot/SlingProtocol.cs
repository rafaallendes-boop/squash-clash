using System;
namespace SquashClash.Core.Shot
{
    public enum ProtocolResponse { Ok, Premature, Failed }
    // DEV owns its lifetime and persistence. No gameplay attempt is consumed here.
    public sealed class SlingProtocol
    {
        public string Block {get;private set;}="preview";
        public bool AbsoluteSupport {get;set;}
        public ProtocolResponse? LastResponse {get;private set;}
        public int Session {get;private set;}
        public int Completed {get;private set;}
        public bool AwaitingFeedback {get;private set;}
        public int PendingTrial {get;private set;}
        private int Limit=>Block=="B0"?int.MaxValue:Block=="B4"?5:10;
        public bool Running=>Block!="preview"&&Completed<Limit;
        public int TargetId
        {
            get
            {
                int index=AwaitingFeedback?PendingTrial-1:Completed;
                if(Block=="B1"||Block=="B5")return index%5+1;
                if(Block=="B3")return index%2+1;
                return Block=="B0"||Block=="preview"?0:3;
            }
        }
        public string RequestedPower=>Block=="B1"&&AbsoluteSupport?"none":Block=="B2-soft"||Block=="B1"?"soft":Block=="B2-medium"||Block=="B3"?"medium":
            Block=="B2-strong"?"strong":Block=="B4"?"cancel":Block=="B5"?new[]{"soft","medium","strong"}[(AwaitingFeedback?PendingTrial-1:Completed)%3]:"practice";
        public bool Arm(string block)
        {
            if(AwaitingFeedback)return false;
            if(block!="B0"&&block!="B1"&&block!="B2-soft"&&block!="B2-medium"&&block!="B2-strong"&&block!="B3"&&block!="B4"&&block!="B5")
                throw new ArgumentOutOfRangeException(nameof(block));
            Block=block;Session++;Completed=PendingTrial=0;return true;
        }
        public void CompleteAttempt()
        {
            if(!Running||AwaitingFeedback)return;
            Completed++;PendingTrial=Completed;AwaitingFeedback=Block=="B1"||Block=="B3";
        }
        public bool Respond(bool premature)=>Respond(premature?ProtocolResponse.Premature:ProtocolResponse.Ok);
        public bool Respond(ProtocolResponse response)
        {
            if(!AwaitingFeedback||!Enum.IsDefined(typeof(ProtocolResponse),response)||response==ProtocolResponse.Failed&&!AbsoluteSupport)return false;
            LastResponse=response;AwaitingFeedback=false;return true;
        }
        public void EndPractice(){if(Block=="B0")Block="preview";}
    }
}
