using SquashClash.Runtime.App;
using SquashClash.Runtime.Ball;
using SquashClash.Runtime.Rally;
using UnityEngine;
namespace SquashClash.Runtime.Visual
{
    public sealed class RivalServeRitualView : MonoBehaviour
    {
        public bool Showing {get;private set;}
        public Transform Ball {get;private set;}
        public void Initialize(AppBootstrap app,RallyDirector rally,BallView ballView,LegibilityAids aids){}
        public void Refresh(){}
    }
}
