using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Building Name", menuName = "New Building Stats")]
public class BuildingStats : ScriptableObject
{
    public BuyIcons buildingType;
    public BuyIcons unitType;
    public BuyIcons[] subsequentUpgrades;
    public float health = 100f;
    public float interactionRadius = 2f;
    public float buildRadius = 5;
    public int population = 5;
    public UnitActions unit;
    public float initialUnitSpawnTime = 5f;
    public float spawnTimeMultiPerBuilding = 0.7f;
}
