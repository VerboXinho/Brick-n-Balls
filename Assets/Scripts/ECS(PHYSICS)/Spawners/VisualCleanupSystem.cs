using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial class VisualCleanupSystem : SystemBase
{
    private static Queue<GameObject> visualsToDestroy = new Queue<GameObject>();
    private ComponentLookup<BallVisualLink> ballVisualLookup;

    protected override void OnCreate()
    {
        ballVisualLookup = GetComponentLookup<BallVisualLink>(true);
    }

    protected override void OnUpdate()
    {
        ballVisualLookup.Update(ref this.CheckedStateRef);

        while (BallDestructionQueue.TryDequeue(out Entity ballEntity))
        {
            if (EntityManager.Exists(ballEntity))
            {
                if (ballVisualLookup.TryGetComponent(ballEntity, out BallVisualLink visualLink))
                {
                    if (visualLink.VisualGameObject.Value != null)
                    {
                        visualsToDestroy.Enqueue(visualLink.VisualGameObject.Value);
                    }
                }

                Debug.Log($"[VisualCleanupSystem] Destroying ball entity");
                EntityManager.DestroyEntity(ballEntity);
            }
        }
    }

    public static void ProcessDestroyQueue()
    {
        while (visualsToDestroy.Count > 0)
        {
            GameObject obj = visualsToDestroy.Dequeue();
            if (obj != null)
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
