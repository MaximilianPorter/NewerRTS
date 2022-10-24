using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class ResourceGenerator : MonoBehaviour
{
    [SerializeField] private ResourceAmount amountToGive;
    [SerializeField] private float timeToGive = 10f;
    [SerializeField] private ParticleSystem resourceVisualEffect;


    private float counter = 0f;

    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        resourceVisualEffect.Stop();
    }

    private void Update()
    {
        if (counter < 0)
        {
            resourceVisualEffect.Play();
            //PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].AddResources(amountToGive);
            PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, amountToGive, transform.position);

            counter = timeToGive;
        }
        counter -= Time.deltaTime;

    }
}
