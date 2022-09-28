using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

[RequireComponent(typeof(Identifier))]
public class Building : MonoBehaviour
{
    [SerializeField] private GameObject playerHoverEffect;
    [SerializeField] private bool debugSpawnUnit = false;
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
        UnitActions unitInstance = Instantiate(unit, transform.position, Quaternion.identity).GetComponent <UnitActions>();
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);

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
