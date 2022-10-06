using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Identifier))]
public class Building : MonoBehaviour
{
    [SerializeField] private GameObject playerHoverEffect;
    [SerializeField] private bool debugSpawnUnit = false;
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private GameObject unit;
    [SerializeField] private float buildRadius = 5f;
    
    private Identifier identifier;
    private bool playerIsHovering = false;

    public float GetBuildRadius => buildRadius;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        PlayerHolder.AddBuilding(identifier.GetPlayerID, this);
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

    private void SpawnUnit ()
    {
        // spawn unit
        UnitActions unitInstance = Instantiate(unit, transform.position, Quaternion.identity).GetComponent <UnitActions>();
        unitInstance.gameObject.SetActive(true); // i think when i spawn them as UnitActions, they spawn disabled

        // set team / ownership stuff
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);

        // first rally point
        unitInstance.GetMovement.SetMoveTarget(rallyPoint.position + new Vector3(Random.Range(-.5f, 0.5f), 0f, Random.Range(-.5f, 0.5f)));

        // add unit to list of all units for player
        PlayerHolder.AddUnit(identifier.GetPlayerID, unitInstance);
    }

    public void PlayerHover (bool isHovering)
    {
        playerIsHovering = isHovering;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, buildRadius);
    }
}
