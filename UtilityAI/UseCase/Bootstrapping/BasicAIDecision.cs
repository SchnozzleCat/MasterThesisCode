using Schnozzle.AI.Data;
using UnityEngine;

namespace Schnozzle.UseCase.Bootstrapping
{
    [CreateAssetMenu(menuName = "BasicAI/Decision")]
    public class BasicAIDecision : DecisionSettings<GameContext, BasicAIInputType, DecisionData>
    {
        [SerializeField]
        private DecisionRunner _runner;

        public override void PopulateData(ref DecisionData data)
        {
            data.Runner = _runner;
        }
    }
}
