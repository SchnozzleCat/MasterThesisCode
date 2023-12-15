using Schnozzle.AI.Data;
using Schnozzle.UseCase.Inputs;
using UnityEngine;

namespace Schnozzle.UseCase.Bootstrapping
{
    [CreateAssetMenu(menuName = "BasicAI/Decision Set")]
    public class BasicAIDecisionSet : DecisionSetSettings<GameContext, BasicAIInputType, DecisionData> { }
}
