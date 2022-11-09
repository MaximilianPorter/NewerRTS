using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class HomeBase : MonoBehaviour
{
    [SerializeField] private Building destroyedHomeBaseBuilding;

    private Identifier identifier;
    private Identifier attachedPlayerIdentifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        
    }

    private void Start()
    {
        attachedPlayerIdentifier = PlayerHolder.GetPlayerIdentifiers[identifier.GetPlayerID];

        MatchPlayersIdentity();
    }

    private void Update()
    {
        MatchPlayersIdentity();

        if (attachedPlayerIdentifier && GameWinManager.instance != null)
        {
            if (!attachedPlayerIdentifier.gameObject.activeInHierarchy)
            {
                PlayerHolder.GetBuildings(attachedPlayerIdentifier.GetPlayerID).Remove(GetComponent<Building>());
                gameObject.SetActive(false);
            }
        }
    }

    public void Die ()
    {
        // place building
        Identifier placedBuildingIdentity = Instantiate(destroyedHomeBaseBuilding.gameObject, transform.position, Quaternion.identity).GetComponent<Identifier>();

        // set team and player ID of building
        placedBuildingIdentity.UpdateInfo(identifier.GetPlayerID, identifier.GetTeamID);
    }

    private void MatchPlayersIdentity ()
    {
        // stay the same as the player
        if (attachedPlayerIdentifier.GetPlayerID != identifier.GetPlayerID ||
            attachedPlayerIdentifier.GetTeamID != identifier.GetTeamID)
        {
            identifier.UpdateInfo(
                attachedPlayerIdentifier.GetPlayerID,
                attachedPlayerIdentifier.GetTeamID);
        }
    }
}
