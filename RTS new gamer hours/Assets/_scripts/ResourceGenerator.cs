using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class ResourceGenerator : MonoBehaviour
{
    [SerializeField] private ResourceAmount amountPerSurroundingHit;
    [SerializeField] private float amountMultiplier = 1f;
    [SerializeField] private float timeToGive = 10f;
    [SerializeField] private ParticleSystem resourceVisualEffect;
    [SerializeField] [TagField] private string[] tagsCounted;
    [SerializeField] private LayerMask hitLayerMask;
    [SerializeField] private float radius = 5f;
    [SerializeField] private Projector projector;


    private int surroundingsHit = 0;
    private float physicsUpdateCounter = 0f;
    private float physicsUpdateTime = 1f;
    private float counter = 0f;
    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        resourceVisualEffect.Stop();
        if (tagsCounted.Length <= 0) surroundingsHit = 1;
    }

    private void Update()
    {
        if (counter < 0)
        {
            resourceVisualEffect.Play();
            //PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].AddResources(amountToGive);

            int food = (int)((float)amountPerSurroundingHit.GetFood * (float)surroundingsHit * amountMultiplier);
            int wood = (int)((float)amountPerSurroundingHit.GetWood * (float)surroundingsHit * amountMultiplier);
            int stone = (int)((float)amountPerSurroundingHit.GetStone * (float)surroundingsHit * amountMultiplier);
            ResourceAmount amtToGive = new ResourceAmount(food, wood, stone);

            PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, amtToGive, transform.position);

            counter = timeToGive;
        }
        counter -= Time.deltaTime;


        physicsUpdateCounter += Time.deltaTime;
        if (physicsUpdateCounter > physicsUpdateTime)
        {
            physicsUpdateCounter = 0f;

            // check for things around
            CheckSurroundings();
        }
    }

    private void CheckSurroundings ()
    {
        if (tagsCounted.Length <= 0)
            return;

        Collider[] collisions = Physics.OverlapSphere(transform.position, radius, hitLayerMask);
        surroundingsHit = collisions.Count(collision => tagsCounted.Contains(collision.tag));

        if (projector)
            projector.orthographicSize = radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

        if (projector && !Application.isPlaying)
            projector.orthographicSize = radius;
    }
}
