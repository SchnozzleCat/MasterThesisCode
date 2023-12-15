using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using UnityEngine;

public class PerformanceTests 
{
    [BurstCompile(CompileSynchronously = true)]
    private struct TestFalseSharingWriteJob : IJobParallelFor
    {
        [NativeSetThreadIndex]
        private int _threadIndex;

        [NativeDisableParallelForRestriction]
        public NativeArray<byte> Array;
        
        public void Execute(int index)
        {
            Array[_threadIndex] = 1;
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    private struct TestNoFalseSharingWriteJob : IJobParallelFor
    {
        [NativeSetThreadIndex]
        private int _threadIndex;

        [NativeDisableParallelForRestriction]
        public NativeArray<byte> Array;
        
        public void Execute(int index)
        {
            Array[_threadIndex * 64] = 1;
        }
    }
    
    [Performance]
    [Test]
    public void TestFalseSharingPerformance()
    {
            const int Count = 1000000;
            var array1 = new NativeArray<byte>(Environment.ProcessorCount,Allocator.Persistent);
        Measure.Method(() =>
        {

            
            new TestFalseSharingWriteJob
            {
                Array = array1
            }.Schedule(Count, 1).Complete();

        }).WarmupCount(100).MeasurementCount(1000).SampleGroup("False Sharing").Run();
        array1.Dispose();
        
 //            var array2 = new NativeArray<byte>(Environment.ProcessorCount * 64,Allocator.Persistent);
 // Measure.Method(() =>
 //        {
 //
 //            
 //            new TestNoFalseSharingWriteJob
 //            {
 //                Array = array2
 //            }.Schedule(Count, 1).Complete();
 //
 //        }).WarmupCount(100).MeasurementCount(100).SampleGroup("No False Sharing").Run();
 //        array2.Dispose();
    }
}
