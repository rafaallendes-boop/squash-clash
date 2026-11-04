using SquashClash.Core.Shot;
using UnityEngine;
namespace SquashClash.Runtime.Shot
{
    public sealed partial class ShotController
    {
        public ReturnResolution? LastPreviewReturn {get;private set;}
        public bool TryPreviewReturn(Vector2 aim,float power,double releaseAt,out ReturnResolution resolution)
        {resolution=default;return false;}
    }
}
