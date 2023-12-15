using Schnozzle.AI.Data;
using Schnozzle.UseCase.Inputs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Schnozzle.UseCase.Bootstrapping
{
    [CreateAssetMenu(menuName = "BasicAI/Consideration")]
    public class BasicAIConsideration : ConsiderationSettings<GameContext, BasicAIInputType>
    {
        [ShowInInspector, PropertyOrder(-1)]
        public string Name => name;
    }
}
