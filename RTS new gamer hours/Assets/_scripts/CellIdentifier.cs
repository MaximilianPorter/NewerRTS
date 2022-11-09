using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIdentifier : MonoBehaviour
{
    private Identifier identifier;
    public Identifier GetIdentifier => identifier;

    private Collider col;
    public Collider GetCollider => col;

    private Building building;
    public Building GetBuilding => building;

    private UnitActions unit;
    public UnitActions GetUnit => unit;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
        col = GetComponent<Collider>();
        building = GetComponent<Building>();
        unit = GetComponent<UnitActions>();
    }
}
