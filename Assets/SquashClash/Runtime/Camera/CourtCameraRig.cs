using SquashClash.Core.Config;
using SquashClash.Core.Court;
using UnityEngine;

namespace SquashClash.Runtime.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CourtCameraRig : MonoBehaviour
    {
        public void Apply(ParamSet p, CourtDimensions d)
        {
            var camera = GetComponent<UnityEngine.Camera>();
            camera.orthographic = false;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100;
            camera.fieldOfView = (float)p.Get("camera.fovDeg");
            transform.position = new Vector3(d.Width / 2, (float)p.Get("camera.height"), d.Length + (float)p.Get("camera.backOffset"));
            transform.LookAt(new Vector3(d.Width / 2, (float)p.Get("camera.lookAtHeight"), 0));
        }

        public void Tick(Vector3 ballRenderPos, float dt) { }
    }
}
