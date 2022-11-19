using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using System;
using System.Diagnostics;

public static class GameStats
{
    private static readonly ProfilerCategory ParticleSystemCategory = ProfilerCategory.Internal;

    public const string ParticleSystemCountName = "ParticleSystem Count";
    private static readonly ProfilerCounterValue<int> ParticleSystemCount =
        new ProfilerCounterValue<int>(ParticleSystemCategory, ParticleSystemCountName, ProfilerMarkerDataUnit.Count,
            ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

    [Conditional("ENABLE_PROFILER")]
    public static void ParticleSystemNum(int v)
    {
        ParticleSystemCount.Value = v;
    }
}
