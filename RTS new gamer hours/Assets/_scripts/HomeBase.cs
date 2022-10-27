using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class HomeBase : MonoBehaviour
{
    private Identifier identifier;
    private Identifier attachedPlayerIdentifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        
    }

    private void Start()
    {
        attachedPlayerIdentifier = PlayerHolder.GetPlayerIdentifiers[identifier.GetPlayerID];
    }

    private void Update()
    {
        // stay the same as the player
        if (attachedPlayerIdentifier.GetPlayerID != identifier.GetPlayerID ||
            attachedPlayerIdentifier.GetTeamID != identifier.GetTeamID ||
            attachedPlayerIdentifier.GetColorID != identifier.GetColorID)
        {
            identifier.UpdateInfo(
                attachedPlayerIdentifier.GetPlayerID,
                attachedPlayerIdentifier.GetTeamID,
                attachedPlayerIdentifier.GetColorID);
        }
    }
}
