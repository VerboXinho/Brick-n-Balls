using UnityEngine;

public class GameObjectCleanupHelper : MonoBehaviour
{
    private void LateUpdate()
    {
        VisualCleanupSystem.ProcessDestroyQueue();
    }
}
