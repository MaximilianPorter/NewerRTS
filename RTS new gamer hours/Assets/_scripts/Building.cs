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
    
    private Identifier identifier;
    private bool playerIsHovering = false;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        if (debugSpawnUnit)
        {
            SpawnUnit();
            debugSpawnUnit = false;
        }

        playerHoverEffect.SetActive(playerIsHovering);
    }

    private void SpawnUnit ()
    {
        Movement unitInstance = Instantiate(unit, transform.position, Quaternion.identity).GetComponent <Movement>();
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);

        PlayerUnitHolder.AddUnit(identifier.GetPlayerID, unitInstance);
    }

    public void PlayerHover (bool isHovering)
    {
        playerIsHovering = isHovering;
    }
}
