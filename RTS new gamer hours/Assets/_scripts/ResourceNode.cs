using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceAmount amount;
    [SerializeField] private float resetTime = 0f;
    [SerializeField] private GameObject resourceSpawnEffect;
    [SerializeField] private Vector3 resourceSpawnOffset;

    private float resetCounter = 0f;

    public bool GetHasResources => resetCounter < 0;
    public void CollectResources (int playerID, Vector3? effectSpawnPos = null)
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
            PlayerResourceManager.PlayerResourceAmounts[playerID].AddResources(amount);
            //PlayerResourceManager.instance.AddResourcesWithUI(playerID, amount, transform.position);
        }
    }

    private void Update()
    {
        resetCounter -= Time.deltaTime;
    }
}
