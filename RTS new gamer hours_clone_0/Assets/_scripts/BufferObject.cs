using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferObject : MonoBehaviour
{
    [SerializeField] private SpawnObject spawnObject;
    [SerializeField] private bool showBuffer = false;
    public SpawnObject GetSpawnObject => spawnObject;

    private void Awake()
    {
        if (spawnObject.prefab == null)
            spawnObject.prefab = transform.gameObject;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showBuffer)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnObject.prefab == null ? transform.position : spawnObject.prefab.transform.position, spawnObject.spacingBuffer);
    }
}
