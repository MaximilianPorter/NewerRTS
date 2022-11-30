using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    [SerializeField] private ResourceAmount amount;
    [SerializeField] private float resetTime = 0f;
    [SerializeField] private ParticleSystem resourceSpawnEffect;
    [SerializeField] private bool playEffectOnCollect = true;
    [SerializeField] private bool playEffectOnChildOff = false;
    [SerializeField] private Vector3 resourceSpawnOffset;
    [SerializeField] private bool canBeDestroyed = false;
    [SerializeField] private int destroyAfterHits = 3;
    [SerializeField] private int turnOffChildrenAfterHits = 0;
    [SerializeField] private bool isReward = false;

    private int currentHits = 0;
    private int childrenOff = 0;

    private float resetCounter = 0f;

    public bool GetHasResources => resetCounter < 0;
    public bool GetIsReward => isReward;

    private void Start()
    {
        if (resourceSpawnEffect)
            resourceSpawnEffect.Stop();
    }

    /// <summary>
    /// doensn't add resources by itself, and returns false if the resource counter is not ready
    /// </summary>
    public bool TryCollectResources(out ResourceAmount returnedAmount)
    {
        if (resetCounter < 0)
        {
            resetCounter = resetTime;

            if (resourceSpawnEffect != null && playEffectOnCollect)
            {
                resourceSpawnEffect.transform.position = transform.position + resourceSpawnOffset;
                resourceSpawnEffect.Play();

                if (currentHits + 1 >= destroyAfterHits && canBeDestroyed)
                {
                    resourceSpawnEffect.transform.SetParent(null);
                    Destroy(resourceSpawnEffect.gameObject, 5f);
                }
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

        if (turnOffChildrenAfterHits > 0 && currentHits >= turnOffChildrenAfterHits)
        {
            if (resourceSpawnEffect != null && playEffectOnChildOff)
            {
                resourceSpawnEffect.transform.position = transform.GetChild(childrenOff).position + resourceSpawnOffset;
                resourceSpawnEffect.Play();
            }

            transform.GetChild(childrenOff).gameObject.SetActive (false);


            if (childrenOff + 1 < transform.childCount)
                childrenOff++;

            if (childrenOff >= transform.childCount - 1)
                Destroy(gameObject);

            currentHits = 0;
        }
    }
}
