using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Building Name", menuName = "New Building Stats")]
public class BuildingStats : ScriptableObject
{
    public BuyIcons buildingType;
    public BuyIcons[] subsequentUpgrades;
    public float interactionRadius = 2f;
    public float buildRadius = 5;
    public GameObject unit;
}
