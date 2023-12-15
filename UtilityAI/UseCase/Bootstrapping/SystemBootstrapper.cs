using Schnozzle.AI.Components;
using Schnozzle.AI.Systems;
using Unity.Entities;
using Unity.Jobs;
using UnityEditor;
#if UNITY_EDITOR
using Schnozzle.AI.Editor;
#endif

// ----------------
// Set types here:

using Context = Schnozzle.UseCase.GameContext;
using InputType = Schnozzle.UseCase.BasicAIInputType;
using DecisionData = Schnozzle.UseCase.Bootstrapping.DecisionData;

// -----------------

[assembly: RegisterGenericSystemType(
    typeof(AIExecutionSystem<
        Context,
        DecisionSetBuffer<Context, InputType, DecisionData>,
        ActiveDecision<Context, InputType, DecisionData>,
        InputType,
        DecisionData
    >)
)]
[assembly: RegisterGenericJobType(
    typeof(AIExecutorJob<
        Context,
        DecisionSetBuffer<Context, InputType, DecisionData>,
        ActiveDecision<Context, InputType, DecisionData>,
        InputType,
        DecisionData
    >)
)]
[assembly: RegisterGenericComponentType(typeof(ActiveDecision<Context, InputType, DecisionData>))]
[assembly: RegisterGenericComponentType(typeof(DecisionSetBuffer<Context, InputType, DecisionData>))]

#if UNITY_EDITOR
[assembly: RegisterGenericSystemType(typeof(AIDebuggingSystem<Context, InputType, DecisionData>))]
[assembly: RegisterGenericComponentType(typeof(RuntimeDebuggingBuffer<Context, InputType, DecisionData>))]

public static class BootstrapDebugging
{
    [MenuItem("Window/" + nameof(InputType) + " Debugging")]
    public static void ShowBasicAIDebugging() => AIDebuggingEditorWindow.ShowWindow<Context, InputType, DecisionData>();
}
#endif
