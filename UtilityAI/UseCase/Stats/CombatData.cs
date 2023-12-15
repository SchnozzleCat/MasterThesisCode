using Unity.Entities;

namespace Schnozzle.UseCase.Stats
{
    public struct CombatData : IComponentData
    {
        public int Ammo;
        public float ReuseDelay;
    }
}
