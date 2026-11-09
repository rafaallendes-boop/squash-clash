using System;
using UnityEngine;
namespace SquashClash.AutoQA
{
    [Serializable] internal sealed class Attempt
    {
        public string schema="autoqa-attempt/1",runId,verdict,failure,repro;
        public ScenarioRef scenario;public BuildMeta build;public ConfigMeta config;public EnvironmentMeta environment;
        public InputMeta input;public IdMeta ids;public Expected expected;public Observed observed;
        public Invariant[] invariants;public MetricsMeta metrics=new MetricsMeta();public ArtifactRef artifacts;
    }
    [Serializable] internal sealed class ScenarioRef {public string id,sha256;public int repetition;public bool canary;}
    [Serializable] internal sealed class BuildMeta {public string gitHash,profile="editor",unity,driver="editor-deterministic";public bool dirty;}
    [Serializable] internal sealed class ConfigMeta {public string effectiveHash,simulationKey,gestureHash,preset,expectedHash,backend;public int pending;public bool hashMatch;}
    [Serializable] internal sealed class EnvironmentMeta {public int[] screen;public float[] safeArea;public string dpiSource;public float mmPerPx;public bool uiBlockedAtAnchor,anchorInSafeArea;}
    [Serializable] internal sealed class InputMeta {public Scenario intent;public TouchPoint[] samples;}
    [Serializable] internal sealed class IdMeta {public long launchId;public string attemptId;}
    [Serializable] internal sealed class Observed
    {
        public string closure,result,fail,presentationCategory,clip;
        public bool applied,scheduled,attemptConsumed,predictionBitwise;
        public int presentations,discrepancies;public long contactStep;
        public double deltaMs,quantizationErrorMs;public float frontError;
        public string firstSurface;public Vector3 firstPoint;public PocObservation normalized;
    }
    [Serializable] internal sealed class Invariant {public string id;public bool pass;}
    [Serializable] internal sealed class MetricsMeta {public double solverCostMs;}
    [Serializable] internal sealed class ArtifactRef {public string gameMetrics,sha256;}
    [Serializable] internal sealed class EvidenceRef {public string path,sha256;}
    [Serializable] internal sealed class Counts {public int PASS,FAIL,INCONCLUSIVE,NOT_EVALUATED;}
    [Serializable] internal sealed class Integrity {public bool hashesVerified=true,presetsUnchanged=true,guardPassed=true,evidenceSha256Ok=true;public bool All=>hashesVerified&&presetsUnchanged&&guardPassed&&evidenceSha256Ok;}
}
