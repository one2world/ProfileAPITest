using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public static class ProfileAPI
{
    public static AutoScope CountAllocAuto(Action<long, int> allocCount, int id)
    {
        return new AutoScope(allocCount,id);
    }

    public struct AutoScope : IDisposable
    {
        ProfilerRecorder gcAllocRecorder;
        Action<long, int> allocCount;
        int id;
        internal AutoScope(Action<long, int> allocCount, int id)
        {
            this.id = id;
            this.allocCount = allocCount;
            this.gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, 
                "GC.Alloc", 
                1, 
                ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.CollectOnlyOnCurrentThread
                );

        }

        public void Dispose()
        {
            gcAllocRecorder.Stop();
            var count = gcAllocRecorder.Count == 0 ? 0 : gcAllocRecorder.GetSample(0).Count;
            gcAllocRecorder.Dispose();
            allocCount?.Invoke(count, id);
            allocCount = null;
        }
    }
}
