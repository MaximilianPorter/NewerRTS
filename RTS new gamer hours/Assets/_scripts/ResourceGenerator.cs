using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class ResourceGenerator : MonoBehaviour
{
    [SerializeField] private ResourceAmount flatAmtToGive;
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

    public float GetRadius => radius;

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
            if (tagsCounted.Length > 0)
                CheckSurroundings();
            else
            {
                PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, AmountWithResearch(flatAmtToGive), transform.position);
            }

            // THIS WAS FOR GETTING RESOURCES BASED ON NUMBER OF SURROUNDINGS
            //float hasMoreResourcesResearch = PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_MoreResources) ? 1.15f : 1f;
            //int food = Mathf.CeilToInt((float)amountPerSurroundingHit.GetFood * (float)surroundingsHit * amountMultiplier * hasMoreResourcesResearch);
            //int wood = Mathf.CeilToInt((float)amountPerSurroundingHit.GetWood * (float)surroundingsHit * amountMultiplier * hasMoreResourcesResearch);
            //int stone = Mathf.CeilToInt((float)amountPerSurroundingHit.GetStone * (float)surroundingsHit * amountMultiplier * hasMoreResourcesResearch);


            //ResourceAmount amtToGive = new ResourceAmount(food, wood, stone);

            //PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, amtToGive, transform.position);

            counter = timeToGive;
        }
        counter -= Time.deltaTime;


        physicsUpdateCounter += Time.deltaTime;
        if (physicsUpdateCounter > physicsUpdateTime)
        {
            physicsUpdateCounter = 0f;

            // check for things around
            
        }
    }

    private void CheckSurroundings ()
    {
        if (tagsCounted.Length <= 0)
            return;

        Collider[] collisions = Physics.OverlapSphere(transform.position, radius, hitLayerMask).Where (collision => collision != null && tagsCounted.Contains (collision.tag)).ToArray();
        surroundingsHit = collisions.Count();


        if (surroundingsHit <= 0)
            return;


        // hit node
        ResourceNode node = collisions.OrderBy (collision => (collision.transform.position - transform.position).sqrMagnitude).FirstOrDefault(collision => collision.TryGetComponent(out ResourceNode node)).GetComponent<ResourceNode>();
        if (node && node.TryCollectResources(out ResourceAmount returnedAmount))
        {
            PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, AmountWithResearch(returnedAmount), transform.position);
        }

        // shake tree if that's what we're doing
        if (node && node.TryGetComponent (out TreeShake tree))
        {
            Vector3 dir = Vector3.Cross(tree.transform.position - transform.position, Vector3.up).normalized;
            tree.ShakeOnce(-dir, 0.2f);
        }

        if (projector)
            projector.orthographicSize = radius;
    }

    private ResourceAmount AmountWithResearch (ResourceAmount amt, float multiplier = 1)
    {
        float hasMoreResourcesResearch = PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(BuyIcons.Research_MoreResources) ? 1.5f : 1f;

        int food = Mathf.CeilToInt((float)amt.GetFood * amountMultiplier * multiplier * hasMoreResourcesResearch);
        int wood = Mathf.CeilToInt((float)amt.GetWood * amountMultiplier * multiplier * hasMoreResourcesResearch);
        int stone = Mathf.CeilToInt((float)amt.GetStone * amountMultiplier * multiplier * hasMoreResourcesResearch);

        return new ResourceAmount(food, wood, stone);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);

        if (projector && !Application.isPlaying)
            projector.orthographicSize = radius;
    }
}
