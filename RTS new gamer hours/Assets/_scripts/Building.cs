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

    private bool rallyPointMoved = false;
    private Identifier identifier;
    private bool playerIsHovering = false;

    public bool GetRallyPointMoved => rallyPointMoved;
    public BuildingStats GetStats => stats;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        PlayerHolder.AddBuilding(identifier.GetPlayerID, this);

        DestroySurroundings();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stats.buildRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.interactionRadius);
    }
}
