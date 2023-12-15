using Schnozzle.UseCase.Stats;
using Unity.Entities;
using Unity.Transforms;

namespace Schnozzle.UseCase
{
    public readonly partial struct AIAspect : IAspect
    {
        public readonly Entity Entity;
        public readonly RefRO<Health> Health;
        public readonly RefRO<MaxHealth> MaxHealth;
        public readonly RefRO<LocalToWorld> LocalToWorld;
        public readonly RefRO<CombatData> CombatData;
    }
}
