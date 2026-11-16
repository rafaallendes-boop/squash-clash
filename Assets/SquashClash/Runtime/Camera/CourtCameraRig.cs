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

        // Presentation layer (art pass, reworked in the feel pass): a short decaying nudge and a brief field-of-view punch
        // for strong strokes. Both exist only while this camera is being rendered: the transform and the projection that
        // gameplay reads between frames (the M2-4 reticle snapshots them when the finger lands) always hold the placed pose.
        // Both wait while the simulation clock is paused.
        private float shakeAmplitude, shakeLeft, shakeDuration, punchDegrees, punchLeft, punchDuration, placedFov;
        private Vector3 shakeOffset, placedPosition;
        private bool drawing;
        private UnityEngine.Camera view;
        public bool Shaking => shakeLeft > 0;
        public bool Punching => punchLeft > 0;
        // What the next rendered frame adds to the placed pose: metres of translation and degrees of field of view.
        public Vector3 RenderOffset => shakeOffset;
        public float RenderFovOffset => punchLeft > 0 ? punchDegrees * (punchLeft / punchDuration) * (punchLeft / punchDuration) : 0;
        public void Shake(float amplitudeMeters, float seconds)
        {
            if (amplitudeMeters <= 0 || seconds <= 0) return;
            shakeAmplitude = amplitudeMeters; shakeLeft = shakeDuration = seconds;
        }
        public void Punch(float degrees, float seconds)
        {
            if (degrees == 0 || seconds <= 0) return;
            punchDegrees = degrees; punchLeft = punchDuration = seconds;
        }
        public void TickShake(float dt)
        {
            if (dt <= 0 || float.IsNaN(dt) || float.IsInfinity(dt)) return;
            if (punchLeft > 0) punchLeft = Mathf.Max(0, punchLeft - dt);
            if (shakeLeft <= 0) return;
            shakeLeft -= dt;
            if (shakeLeft <= 0) { shakeLeft = 0; shakeOffset = Vector3.zero; }
            else
            {
                float k = shakeLeft / shakeDuration, phase = (shakeDuration - shakeLeft) * 70;
                shakeOffset = new Vector3(Mathf.Sin(phase) * shakeAmplitude * k, Mathf.Cos(phase * 1.3f) * shakeAmplitude * k * .6f, 0);
            }
        }
        private void LateUpdate()
        {
            var app = SquashClash.Runtime.App.AppBootstrap.Instance;
            if (app != null && app.Clock != null && app.Clock.Paused) return; // frozen with the game
            TickShake(Time.unscaledDeltaTime);
        }
        private void OnEnable()
        {
            view = GetComponent<UnityEngine.Camera>();
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += BeginDraw;
            UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += EndDraw;
        }
        private void OnDisable()
        {
            UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= BeginDraw;
            UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= EndDraw;
            Restore();
        }
        private void BeginDraw(UnityEngine.Rendering.ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera != view || drawing) return;
            float fov = RenderFovOffset;
            if (shakeOffset == Vector3.zero && fov == 0) return;
            drawing = true; placedPosition = transform.position; placedFov = view.fieldOfView;
            // Camera and look-at target move together, so the rotation is untouched.
            transform.position = placedPosition + shakeOffset; view.fieldOfView = placedFov + fov;
        }
        private void EndDraw(UnityEngine.Rendering.ScriptableRenderContext context, UnityEngine.Camera camera)
        {
            if (camera == view) Restore();
        }
        private void Restore()
        {
            if (!drawing) return;
            drawing = false; transform.position = placedPosition; view.fieldOfView = placedFov;
        }

        private void Place()
        {
            var translation = Vector3.right * offset;
            transform.position = basePosition + translation;
            transform.LookAt(baseTarget + translation);
        }
    }
}
