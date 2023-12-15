// using System.Runtime.CompilerServices;
// using Schnozzle.GameSpace.Components;
// using Unity.Entities;
// using Unity.Jobs;
// using UnityEngine;
//
// namespace Schnozzle.GameSpace.Systems
// {
//     public struct TestComponentCollapse : IComponentData { }
//
//     public partial struct TestJobCollapse : IJobEntity
//     {
//         public void Execute(in SimulatedAgent agent) { }
//     }
//
//     public partial struct TestSystemCollapse : ICollapseSystem<TestComponentCollapse, TestJobCollapse, TestJobCollapse>
//     {
//         public TestJobCollapse CollapseJob(ref SystemState state)
//         {
//             return default;
//         }
//
//         public TestJobCollapse UncollapseJob(ref SystemState state)
//         {
//             return default;
//         }
//     }
//
//     public interface ICollapseSystem<T, TCollapse, TUncollapse> : ISystem
//         where T : unmanaged, IComponentData
//         where TCollapse : unmanaged, IJobEntity
//         where TUncollapse : unmanaged, IJobEntity
//     {
//         void ISystem.OnCreate(ref SystemState state) { }
//
//         void ISystem.OnUpdate(ref SystemState state)
//         {
//             state.Dependency = ScheduleUpdate(ref state);
//         }
//
//         /// <summary>
//         /// Schedule the collapse and uncollapse jobs for this system.
//         /// Declared here as protected to more easily override the <see cref="OnUpdate"/> method
//         /// but still schedule the required jobs.
//         /// </summary>
//         /// <param name="state"></param>
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         protected JobHandle ScheduleUpdate(ref SystemState state)
//         {
//             Debug.Log("Hello World");
//             return default;
//         }
//
//         TCollapse CollapseJob(ref SystemState state);
//         TUncollapse UncollapseJob(ref SystemState state);
//     }
// }
