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

        // Art pass: a short, decaying nudge for strong strokes. It moves the camera and its target together (no rotation)
        // and returns exactly to the placed pose; with no shake running the rig behaves as before.
        private float shakeAmplitude, shakeLeft, shakeDuration;
        private Vector3 shakeOffset;
        public bool Shaking => shakeLeft > 0;
        public void Shake(float amplitudeMeters, float seconds)
        {
            if (amplitudeMeters <= 0 || seconds <= 0) return;
            shakeAmplitude = amplitudeMeters; shakeLeft = shakeDuration = seconds;
        }
        public void TickShake(float dt)
        {
            if (shakeLeft <= 0 || dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            shakeLeft -= dt;
            if (shakeLeft <= 0) { shakeLeft = 0; shakeOffset = Vector3.zero; }
            else
            {
                float k = shakeLeft / shakeDuration, phase = (shakeDuration - shakeLeft) * 70;
                shakeOffset = new Vector3(Mathf.Sin(phase) * shakeAmplitude * k, Mathf.Cos(phase * 1.3f) * shakeAmplitude * k * .6f, 0);
            }
            Place();
        }
        private void LateUpdate() => TickShake(Time.unscaledDeltaTime);

        private void Place()
        {
            var translation = Vector3.right * offset + shakeOffset;
            transform.position = basePosition + translation;
            transform.LookAt(baseTarget + translation);
        }
    }
}
