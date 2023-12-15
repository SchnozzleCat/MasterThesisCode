// using System;
// using System.Collections.Generic;
// using Schnozzle.AI.Data;
// using Unity.Entities;
//
// namespace Schnozzle.AI.Authoring
// {
//     public interface IDecisionSetBakingComponent<TExecutionContext, TEnum> : IComponentData
//         where TExecutionContext : unmanaged, IExecutionContext<TEnum>
//         where TEnum : unmanaged, Enum
//     {
//         public List<DecisionSetSettings<TExecutionContext, TEnum>> DecisionSets { get; set; }
//     }
//
//     [TemporaryBakingType]
//     public class DecisionSetBakingComponent<TExecutionContext, TEnum>
//         : IDecisionSetBakingComponent<TExecutionContext, TEnum>
//         where TExecutionContext : unmanaged, IExecutionContext<TEnum>
//         where TEnum : unmanaged, Enum
//     {
//         public List<DecisionSetSettings<TExecutionContext, TEnum>> DecisionSets { get; set; }
//     }
// }
