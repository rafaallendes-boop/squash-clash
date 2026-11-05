using System.Collections.Generic;
using SquashClash.Core.Config;
using SquashClash.Core.Simulation;
using UnityEngine;

namespace SquashClash.Runtime.Ball
{
    public sealed class LegibilityAids : MonoBehaviour
    {
        private BallSimulationHost host;
        private TuningState tuning;
        private BallView view;
        private Mesh disc;
        private MeshRenderer shadow;
        private Material shadowMaterial;
        private Material strokeMaterial, impactMaterial;
        private LineRenderer trail;
        private struct TrailPoint { public Vector3 Position; public float Age; }
        private readonly List<TrailPoint> trailPoints = new List<TrailPoint>();
        private float trailSeconds;
        private LineRenderer prediction;
        private struct WallMark { public MeshRenderer Renderer; public float Age; }
        private readonly List<WallMark> marks = new List<WallMark>(6);
        private MaterialPropertyBlock block;

        public void Initialize(BallSimulationHost host, TuningState tuning, BallView view)
        {
            this.host = host; this.tuning = tuning; this.view = view;
            block = new MaterialPropertyBlock();
            disc = DiscMesh();
            shadowMaterial = NewMaterial("Ball shadow", Color.black);
            shadow = Disc("Ball shadow", shadowMaterial);
            shadow.transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.up);
            strokeMaterial = NewMaterial("Ball trajectory", Color.white);
            impactMaterial = NewMaterial("Wall impact", Color.white);
            var trailObject = new GameObject("Ball trail"); trailObject.transform.SetParent(transform, false);
            trail = trailObject.AddComponent<LineRenderer>(); trail.sharedMaterial = strokeMaterial;
            trail.useWorldSpace = true; trail.numCapVertices = 2;
            trail.startColor = new Color(.05f, .08f, .09f, 0); trail.endColor = new Color(.05f, .08f, .09f, .55f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; trail.receiveShadows = false;
            var lineObject = new GameObject("Ball prediction"); lineObject.transform.SetParent(transform, false);
            prediction = lineObject.AddComponent<LineRenderer>(); prediction.sharedMaterial = strokeMaterial;
            prediction.useWorldSpace = true; prediction.widthMultiplier = .012f;
            prediction.startColor = prediction.endColor = new Color(.03f, .35f, .4f, .6f);
            prediction.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; prediction.receiveShadows = false;
            tuning.Changed += OnChanged;
            host.ContactRaised += RecordContact; host.Launched += OnLaunched; host.PredictionReplaced += OnPredictionReplaced;
            host.ReadVisualBallDiameter=()=>view.transform.lossyScale.x;
            Apply(); Tick(host.RenderPosition, 0);
        }
        private void OnLaunched(EffectiveConfig config)
        {
            ClearTrail(); ClearMarks();
            Apply(); Tick(host.RenderPosition, 0);
        }
        private void OnPredictionReplaced(Trajectory trajectory)
        {
            ClearTrail();ClearMarks();Apply();
            foreach(var contact in host.CanonicalContacts)
            {
                if(!host.ClockMap.TrySimToReal(contact.Step,0,out var when)||host.ObservedRealTime-when>=1.5)continue;
                int before=marks.Count;RecordContact(contact);
                if(marks.Count>before){var mark=marks[marks.Count-1];mark.Age=(float)(host.ObservedRealTime-when);marks[marks.Count-1]=mark;PaintMark(mark.Renderer,mark.Age);}
            }
        }
        private void OnChanged(string key) { if (key.StartsWith("aids.", System.StringComparison.Ordinal)) Apply(); }
        private void Apply()
        {
            shadow.gameObject.SetActive(tuning.Applied.GetBool("aids.shadow"));
            view.ApplyVisualScale((float)tuning.Applied.Get("aids.ballVisualScale"));
            trailSeconds = (float)tuning.Applied.Get("aids.trailSeconds");
            bool showTrail = tuning.Applied.GetBool("aids.trail") && trailSeconds > 0;
            if (!showTrail) ClearTrail();
            trail.enabled = showTrail;
            trail.startWidth = 0; trail.endWidth = view.transform.lossyScale.x * .5f;
            if (!tuning.Applied.GetBool("aids.impactMarks")) ClearMarks();
            prediction.enabled = tuning.Applied.GetBool("aids.prediction");
            prediction.positionCount = prediction.enabled ? host.Prediction.States.Count : 0;
            for (int i = 0; i < prediction.positionCount; i++) prediction.SetPosition(i, host.Prediction.States[i].Position);
        }
        public void Tick(Vector3 ballRenderPosition, float realSeconds)
        {
            shadow.transform.position = new Vector3(ballRenderPosition.x, .001f, ballRenderPosition.z);
            // Experimental visual cue: smaller and lighter at height, retaining a visible minimum.
            float height = Mathf.Max(0, ballRenderPosition.y - host.ActiveConfig.Physics.BallRadius);
            float proximity = 1 / (1 + height * .4f);
            shadow.transform.localScale = Vector3.one * (.07f + .07f * proximity);
            block.SetColor("_BaseColor", new Color(0, 0, 0, .14f + .28f * proximity)); shadow.SetPropertyBlock(block);
            // Own the trail's ages: Unity's automatic TrailRenderer clock cannot pause with SimClock.
            float ageSeconds = host.Clock.Paused ? 0 : Mathf.Max(0, realSeconds);
            for (int i = trailPoints.Count - 1; i >= 0; i--)
            {
                var point = trailPoints[i]; point.Age += ageSeconds;
                if (point.Age >= trailSeconds) trailPoints.RemoveAt(i); else trailPoints[i] = point;
            }
            if (trail.enabled && realSeconds > 0 && (trailPoints.Count == 0 ||
                Vector3.Distance(trailPoints[trailPoints.Count - 1].Position, ballRenderPosition) >= .015f))
                trailPoints.Add(new TrailPoint { Position = ballRenderPosition });
            trail.positionCount = trailPoints.Count;
            for (int i = 0; i < trailPoints.Count; i++) trail.SetPosition(i, trailPoints[i].Position);
            for (int i = marks.Count - 1; i >= 0; i--)
            {
                var mark = marks[i]; mark.Age += ageSeconds;
                if (mark.Age >= 1.5f) { RemoveMark(i); continue; }
                PaintMark(mark.Renderer, mark.Age); marks[i] = mark;
            }
        }
        private void ClearTrail() { trailPoints.Clear(); trail.positionCount = 0; }
        public void RecordContact(ContactEvent contact)
        {
            if (!tuning.Applied.GetBool("aids.impactMarks") || contact.Persistent) return;
            switch (contact.Surface)
            {
                case Core.Court.SurfaceId.FrontWall: case Core.Court.SurfaceId.BackWall:
                case Core.Court.SurfaceId.LeftWall: case Core.Court.SurfaceId.RightWall: break;
                default: return;
            }
            if (marks.Count == 6) RemoveMark(0);
            var renderer = Disc("Wall impact", impactMaterial);
            renderer.transform.position = contact.Point + contact.Normal * .002f;
            renderer.transform.rotation = Quaternion.FromToRotation(Vector3.forward, contact.Normal);
            renderer.transform.localScale = Vector3.one * .06f;
            PaintMark(renderer, 0); marks.Add(new WallMark { Renderer = renderer });
        }
        private void PaintMark(MeshRenderer renderer, float age)
        {
            block.SetColor("_BaseColor", new Color(.03f, .25f, .3f, .55f * (1 - age / 1.5f))); renderer.SetPropertyBlock(block);
        }
        private void RemoveMark(int index)
        {
            var renderer = marks[index].Renderer; renderer.gameObject.SetActive(false); Destroy(renderer.gameObject); marks.RemoveAt(index);
        }
        private void ClearMarks() { while (marks.Count > 0) RemoveMark(marks.Count - 1); }
        private void LateUpdate() { if (host != null) Tick(host.RenderPosition, Time.unscaledDeltaTime); }
        private Material NewMaterial(string name, Color color)
        {
            var material = new Material(Resources.Load<Shader>("Legibility")) { name = name };
            material.SetColor("_BaseColor", color); return material;
        }
        private MeshRenderer Disc(string name, Material material)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = disc;
            var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false; return renderer;
        }
        private static Mesh DiscMesh()
        {
            const int sides = 32;
            var vertices = new Vector3[sides + 1]; var colors = new Color[sides + 1]; var triangles = new int[sides * 3];
            colors[0] = Color.white;
            for (int i = 0; i < sides; i++)
            {
                float angle = 2 * Mathf.PI * i / sides; vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0); colors[i + 1] = Color.white;
                triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % sides + 1;
            }
            var mesh = new Mesh { name = "Legibility disc", vertices = vertices, colors = colors, triangles = triangles }; mesh.RecalculateBounds(); return mesh;
        }
        private void OnDestroy()
        {
            if (tuning != null) tuning.Changed -= OnChanged;
            if (host != null) { host.ContactRaised -= RecordContact; host.Launched -= OnLaunched; host.PredictionReplaced -= OnPredictionReplaced; host.ReadVisualBallDiameter=null; }
            if (shadow != null) Destroy(shadow.gameObject);
            if (trail != null) Destroy(trail.gameObject); if (prediction != null) Destroy(prediction.gameObject);
            ClearMarks();
            if (disc != null) Destroy(disc); if (shadowMaterial != null) Destroy(shadowMaterial);
            if (strokeMaterial != null) Destroy(strokeMaterial); if (impactMaterial != null) Destroy(impactMaterial);
        }
    }
}
