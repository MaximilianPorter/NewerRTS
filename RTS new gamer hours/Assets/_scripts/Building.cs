using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Identifier))]
public class Building : MonoBehaviour
{
    [SerializeField] private BuildingStats stats;
    [SerializeField] private bool debugSpawnUnit = false;
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private GameObject playerHoverEffect;
    [SerializeField] private GameObject smokeExplosion;


    private float scaleUpCounter = 0f;
    private bool rallyPointMoved = false;
    private Identifier identifier;
    private bool playerIsHovering = false;

    private Health health;

    public bool GetRallyPointMoved => rallyPointMoved;
    public Vector3 GetRallyPointPos => rallyPoint.position;
    
    public BuildingStats GetStats => stats;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        PlayerHolder.AddBuilding(identifier.GetPlayerID, this);

        DestroySurroundings();

        if (playerHoverEffect)
            playerHoverEffect.SetActive(false);
    }

    private void Update()
    {
        if (debugSpawnUnit)
        {
            SpawnUnit();
            debugSpawnUnit = false;
        }

        if (playerHoverEffect)
            playerHoverEffect.SetActive(playerIsHovering);

        if (health.GetCurrentHealth < 0)
        {
            Die();
        }

        
    }

    public void SpawnUnit ()
    {
        // spawn unit
        UnitActions unitInstance = Instantiate(stats.unit.gameObject, transform.position, Quaternion.identity).GetComponent <UnitActions>();
        unitInstance.gameObject.SetActive(true); // i think when i spawn them as UnitActions, they spawn disabled

        // set team / ownership stuff
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);

        // first rally point
        unitInstance.GetMovement.SetMoveTarget(rallyPoint.position + new Vector3(Random.Range(-.5f, 0.5f), 0f, Random.Range(-.5f, 0.5f)));

        // unit is added to player list in UnitActions.Start()
    }

    public void PlayerHover (bool isHovering)
    {
        playerIsHovering = isHovering;
    }


    public void Die ()
    {
        GameObject smokeInstance = Instantiate(smokeExplosion, transform.position, Quaternion.identity);
        Destroy(smokeInstance, 5f);

        DeleteBuilding();
    }
    public void DeleteBuilding ()
    {
        PlayerHolder.RemoveBuilding(identifier.GetPlayerID, this);

        Destroy(gameObject);
    }

    

    private void DestroySurroundings()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.interactionRadius);
        string[] tagsToHit = new string[] { "Tree" };
        for (int i = 0; i < hits.Length; i++)
        {
            if (tagsToHit.Contains(hits[i].tag))
            {
                if (hits[i].TryGetComponent(out TreeShake tree))
                {
                    tree.KillTree();
                }
            }
        }
    }

    public void SetRallyPoint(Vector3 newRallyPoint)
    {
        rallyPoint.position = newRallyPoint;
        rallyPointMoved = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!stats)
        {
            Debug.LogError("Please assign stats to " + gameObject.name);
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stats.buildRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.interactionRadius);
    }
}
