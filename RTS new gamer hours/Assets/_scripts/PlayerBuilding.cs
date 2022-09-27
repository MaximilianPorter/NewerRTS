using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    private Identifier identifier;
    private Building hoveringBuilding;

    private void Awake()
    {
        identifier = GetComponent<Identifier> ();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hoveringBuilding == null && other.TryGetComponent (out Building building) && other.GetComponent<Identifier>().GetPlayerID == identifier.GetPlayerID)
        {
            building.PlayerHover(true);
            hoveringBuilding = building;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hoveringBuilding == other.GetComponent<Building>())
        {
            hoveringBuilding.PlayerHover(false);
            hoveringBuilding = null;
        }
    }
}
