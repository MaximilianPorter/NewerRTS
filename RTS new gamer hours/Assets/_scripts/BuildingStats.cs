using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Building Name", menuName = "New Building Stats")]
public class BuildingStats : ScriptableObject
{
    [Header("Cost")]
    public int foodCost = 0;
    public int woodCost = 100;
    public int stoneCost = 100;

    [Header("Other")]
    public float buildRadius = 5;
    public GameObject unit;
}
