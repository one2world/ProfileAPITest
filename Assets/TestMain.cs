using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    //static readonly ProfilerMarker s_PreparePerfMarker = new ProfilerMarker("MySystem.Prepare");

    string statsText;

    (ProfilerCategory, string, int, ProfilerRecorder, ProfilerRecorderOptions)[] timeRecorders = new (ProfilerCategory, string, int, ProfilerRecorder, ProfilerRecorderOptions)[]
        {
            (ProfilerCategory.Internal, "Main Thread", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "PlayerLoop", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "GC.Collect", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "GC.Alloc", 15, default, ProfilerRecorderOptions.SumAllSamplesInFrame| ProfilerRecorderOptions.CollectOnlyOnCurrentThread),
            (ProfilerCategory.Internal, "Update.ScriptRunBehaviourUpdate", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "CoroutinesDelayedCalls", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "PreLateUpdate.ScriptRunBehaviourLateUpdate", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "FixedBehaviourUpdate", 15, default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Internal, "Camera.Render", 15, default, ProfilerRecorderOptions.Default),

            //user
        };

    (ProfilerCategory, string, ProfilerRecorder, ProfilerRecorderOptions)[] memoryRecorders = new (ProfilerCategory, string, ProfilerRecorder, ProfilerRecorderOptions)[]
       {
            (ProfilerCategory.Memory, "System Used Memory", default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Memory, "GC Reserved Memory", default, ProfilerRecorderOptions.Default),
            (ProfilerCategory.Memory, "GC.Alloc", default, ProfilerRecorderOptions.Default),

            //user
       };

    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }

        return r;
    }

    void OnEnable()
    {
        for (int i = 0; i < timeRecorders.Length; i++)
        {
            var timeRecorder = timeRecorders[i];
            timeRecorder.Item4 = ProfilerRecorder.StartNew(timeRecorder.Item1, timeRecorder.Item2, timeRecorder.Item3, timeRecorder.Item5);
            timeRecorders[i] = timeRecorder;
        }

        for (int i = 0; i < memoryRecorders.Length; i++)
        {
            var memoryRecorder = memoryRecorders[i];
            memoryRecorder.Item3 = ProfilerRecorder.StartNew(memoryRecorder.Item1, memoryRecorder.Item2, 1, memoryRecorder.Item4);
            memoryRecorders[i] = memoryRecorder;
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < timeRecorders.Length; i++)
        {
            var timeRecorder = timeRecorders[i];
            timeRecorder.Item4.Dispose();
        }
        for (int i = 0; i < memoryRecorders.Length; i++)
        {
            var memoryRecorder = memoryRecorders[i];
            memoryRecorder.Item3.Dispose();
        }
    }

    private static void debugAlloc(long count, int id)
    {
        Debug.Log($"[{id}]GC allocs count: {GetUnitMemory(count)}");
    }

    void Update()
    {
        int n1 = 1, n2 = 2;
        string s1 = "long", s2 = "yg";

        string tempName;
        using (ProfileAPI.CountAllocAuto(debugAlloc, 1))
        {
            tempName = n1 + s1 + s2 + n2;
        }

        using (ProfileAPI.CountAllocAuto(debugAlloc, 2))
        {
            tempName = string.Format("{0}{1}{2}{3}", n1, s1, s2, n2);
        }

        using (ProfileAPI.CountAllocAuto(debugAlloc, 3))
        {
            tempName = $"{n1}{s1}{s2}{n2}";
        }
        Debug.Log(tempName);

        var sb = new StringBuilder(500);

        for (int i = 0; i < timeRecorders.Length; i++)
        {
            var timeRecorder = timeRecorders[i];
            sb.AppendLine($"{timeRecorder.Item2}: {GetRecorderFrameAverage(timeRecorder.Item4) * (1e-6f):F1} ms");
        }
        sb.AppendLine($"==========================");
        for (int i = 0; i < memoryRecorders.Length; i++)
        {
            var memoryRecorder = memoryRecorders[i];
            sb.AppendLine($"{memoryRecorder.Item2}: {GetUnitMemory(memoryRecorder.Item3.LastValue)}");
        }
        statsText = sb.ToString();

        GameStats.ParticleSystemNum(UnityEngine.Random.Range(1, 20));
    }

    static string GetUnitMemory(long memory)
    {
        if (memory < 1024L)
        {
            return $"{memory:N3} B";
        }

        memory = memory / 1024L;
        if (memory < 1024L)
        {
            return $"{memory:N3} KB"; 
        }

        memory = memory / 1024L;
        //if (memory < 1024L)
        {
            return $"{memory:N3} MB";
        }

        memory = memory / 1024L;
        return $"{memory:N3} GB"; 
    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(10, 30, 250, 240), statsText);
    }
}
