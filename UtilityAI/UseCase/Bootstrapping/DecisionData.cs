using System;
using Schnozzle.UseCase.Stats;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace Schnozzle.UseCase.Bootstrapping
{
    public struct DecisionData
    {
        public DecisionRunner Runner;
    }

    public struct DecisionRunner
    {
        public enum DecisionType : byte
        {
            MeleeAttack,
            RangedAttack,
            Retreat,
            Heal,
            Strafe
        }

        public DecisionType Type;

        public float MoveSpeed;

        public void Execute(
            in Entity entity,
            ref Random rand,
            ref CombatData data,
            in Entity? target,
            ref ComponentLookup<LocalTransform> localTransformLookup,
            ref EntityCommandBuffer.ParallelWriter writer,
            int chunkIndexInQuery,
            float deltaTime,
            float3 halfSizeClamp
        )
        {
            switch (Type)
            {
                case DecisionType.MeleeAttack:
                    var pos = localTransformLookup.GetRefRW(entity);
                    var targetPos = localTransformLookup[target.Value].Position;
                    if (math.distancesq(pos.ValueRO.Position, targetPos) < 0.01f)
                    {
                        data.ReuseDelay = rand.NextFloat(1, 7);
                        writer.AppendToBuffer(chunkIndexInQuery, target.Value, new DamageBuffer { Value = 10 });
                    }
                    else
                    {
                        var delta = math.normalize(targetPos - pos.ValueRO.Position);
                        pos.ValueRW.Position = pos.ValueRO.Position + delta * MoveSpeed * deltaTime;
                        pos.ValueRW.Position = math.clamp(pos.ValueRO.Position, -halfSizeClamp, halfSizeClamp);
                    }
                    break;
                case DecisionType.RangedAttack:
                    pos = localTransformLookup.GetRefRW(entity);
                    targetPos = localTransformLookup[target.Value].Position;
                    if (math.distancesq(pos.ValueRO.Position, targetPos) < 100)
                    {
                        data.ReuseDelay = rand.NextFloat(2, 12);
                        data.Ammo -= rand.NextInt(1, 5);
                        writer.AppendToBuffer(chunkIndexInQuery, target.Value, new DamageBuffer { Value = 6 });
                    }
                    else
                    {
                        var delta = math.normalize(targetPos - pos.ValueRO.Position);
                        pos.ValueRW.Position = pos.ValueRO.Position + delta * MoveSpeed * deltaTime;
                        pos.ValueRW.Position = math.clamp(pos.ValueRO.Position, -halfSizeClamp, halfSizeClamp);
                    }
                    break;
                case DecisionType.Retreat:
                    pos = localTransformLookup.GetRefRW(entity);
                    targetPos = localTransformLookup[target.Value].Position;
                    var targetDelta = math.normalize(targetPos - pos.ValueRO.Position);
                    pos.ValueRW.Position = pos.ValueRO.Position - targetDelta * MoveSpeed * deltaTime;
                    pos.ValueRW.Position = math.clamp(pos.ValueRO.Position, -halfSizeClamp, halfSizeClamp);
                    break;
                case DecisionType.Heal:
                    data.ReuseDelay = rand.NextFloat(8, 24);
                    writer.AppendToBuffer(chunkIndexInQuery, entity, new DamageBuffer { Value = -15 });
                    break;
                case DecisionType.Strafe:
                    pos = localTransformLookup.GetRefRW(entity);
                    targetPos = localTransformLookup[target.Value].Position;
                    targetDelta = math.rotate(
                        quaternion.Euler(0, 90, 0),
                        math.normalize(targetPos - pos.ValueRO.Position)
                    );
                    pos.ValueRW.Position = pos.ValueRO.Position + targetDelta * MoveSpeed * deltaTime;
                    pos.ValueRW.Position = math.clamp(pos.ValueRO.Position, -halfSizeClamp, halfSizeClamp);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
