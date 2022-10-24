using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceAmount amount;
    [SerializeField] private float resetTime = 0f;
    [SerializeField] private GameObject resourceSpawnEffect;
    [SerializeField] private Vector3 resourceSpawnOffset;
    [SerializeField] private bool canBeDestroyed = false;
    [SerializeField] private int destroyAfterHits = 3;

    private int currentHits = 0;

    private float resetCounter = 0f;

    public bool GetHasResources => resetCounter < 0;

    /// <summary>
    /// doensn't add resources by itself, and returns false if the resource counter is not ready
    /// </summary>
    public bool TryCollectResources(out ResourceAmount returnedAmount, Vector3? effectSpawnPos = null)
    {
        if (effectSpawnPos == null)
            effectSpawnPos = transform.position;

        if (resetCounter < 0)
        {
            resetCounter = resetTime;
            if (resourceSpawnEffect != null)
            {
                GameObject resourceSpawnInstance = Instantiate(resourceSpawnEffect, effectSpawnPos.GetValueOrDefault() + resourceSpawnOffset, Quaternion.identity);
                Destroy(resourceSpawnInstance, 5f);
            }
            returnedAmount = amount;
            currentHits++;
            return true;
        }

        returnedAmount = null;
        return false;
    }


    private void Update()
    {
        resetCounter -= Time.deltaTime;

        if (currentHits >= destroyAfterHits && canBeDestroyed)
        {
            if (TryGetComponent(out TreeShake tree))
            {
                tree.KillTree();
                return;
            }
            Destroy(gameObject);
        }
    }
}
