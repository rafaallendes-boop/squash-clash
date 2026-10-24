using SquashClash.Core.Config;
using SquashClash.Core.Court;
using UnityEngine;

namespace SquashClash.Runtime.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class CourtCameraRig : MonoBehaviour
    {
        private Vector3 basePosition, baseTarget;
        private float offset, velocity, damping, maximum;
        private bool follow;
        public void Apply(ParamSet p, CourtDimensions d)
        {
            var camera = GetComponent<UnityEngine.Camera>();
            camera.orthographic = false;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100;
            camera.fieldOfView = (float)p.Get("camera.fovDeg");
            basePosition = new Vector3(d.Width / 2, (float)p.Get("camera.height"), d.Length + (float)p.Get("camera.backOffset"));
            baseTarget = new Vector3(d.Width / 2, (float)p.Get("camera.lookAtHeight"), 0);
            follow = p.Get("camera.mode") == 1;
            damping = (float)p.Get("camera.followDamping"); maximum = (float)p.Get("camera.followMaxOffset");
            offset = follow ? Mathf.Clamp(offset, -maximum, maximum) : 0;
            velocity = 0;
            Place();
        }

        public void Tick(Vector3 ballRenderPos, float dt)
        {
            // SmoothDamp with zero delta can poison its velocity with NaN.
            if (!follow || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            float target = Mathf.Clamp(ballRenderPos.x - basePosition.x, -maximum, maximum);
            offset = damping == 0 ? target : Mathf.SmoothDamp(offset, target, ref velocity, damping, Mathf.Infinity, dt);
            offset = Mathf.Clamp(offset, -maximum, maximum);
            Place();
        }

        private void Place()
        {
            var translation = Vector3.right * offset;
            transform.position = basePosition + translation;
            transform.LookAt(baseTarget + translation);
        }
    }
}
