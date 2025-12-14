using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[UpdateBefore(typeof(VisualCleanupSystem))]
public partial class BrickDamageSystem : SystemBase
{
    private ComponentLookup<BrickComponent> brickComponentLookup;
    private ComponentLookup<BrickVisualLink> brickVisualLookup;

    protected override void OnCreate()
    {
        brickComponentLookup = GetComponentLookup<BrickComponent>(false);
        brickVisualLookup = GetComponentLookup<BrickVisualLink>(true);
    }

    protected override void OnUpdate()
    {
        brickComponentLookup.Update(ref this.CheckedStateRef);
        brickVisualLookup.Update(ref this.CheckedStateRef);

        while (BrickHitQueue.TryDequeue(out Entity brickEntity))
        {
            if (!EntityManager.Exists(brickEntity))
                continue;

            if (!brickComponentLookup.TryGetComponent(brickEntity, out BrickComponent brick))
                continue;

            brick.Health--;
            
            GameManager.Instance?.AddScore(brick.PointsValue);
            Debug.Log($"[BrickDamageSystem] Brick health: {brick.Health}, Score: +{brick.PointsValue}");

            if (brick.Health <= 0)
            {
                if (brickVisualLookup.TryGetComponent(brickEntity, out BrickVisualLink visualLink))
                {
                    if (visualLink.VisualGameObject.Value != null)
                    {
                        GameObject.Destroy(visualLink.VisualGameObject.Value);
                    }
                }

                Debug.Log($"[BrickDamageSystem] Destroying brick entity");
                EntityManager.DestroyEntity(brickEntity);
            }
            else
            {
                brickComponentLookup[brickEntity] = brick;

                if (brickVisualLookup.TryGetComponent(brickEntity, out BrickVisualLink visualLink))
                {
                    if (visualLink.VisualGameObject.Value != null)
                    {
                        BrickController controller = visualLink.VisualGameObject.Value.GetComponent<BrickController>();
                        if (controller != null)
                        {
                            controller.UpdateVisual(brick.Health);
                        }
                    }
                }
            }
        }
    }
}
