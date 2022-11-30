using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Building Name", menuName = "New Building Stats")]
public class BuildingStats : JsonSerialisableScriptableObject <BuildingStats>
{
    public BuyIcons buildingType;
    public BuyIcons unitType;
    public BuyIcons[] subsequentUpgrades;
    [Tooltip("These buildings need to be built in order to buy this building, MAX OF 4")]
    [SerializeField] private BuyIcons[] requiredBuildings;
    public BuyIcons[] GetRequiredBuildings => requiredBuildings;

    // change these variables in database https://console.firebase.google.com/u/3/project/rts-castles/database/rts-castles-default-rtdb/data
    // this is for the json file
    public readonly string instructions = "-------------------vvv JUST CHANGE THESE THINGS vvv-------------------";
    [Header("Changed in Database")]
    public float health = 100f;
    public ResourceAmount cost;
    public readonly string instructions0 = "-------------------vvv DONT CHANGE THESE vvv-------------------";

    [Space(10)]

    public float costMultiPerBuilding = 1.3f;
    public float interactionRadius = 2f;
    public float buildRadius = 5;
    public int population = 5;
    //public UnitActions unit;
    public float initialUnitSpawnTime = 5f;
    public float spawnTimeMultiPerBuilding = 0.7f;
}
