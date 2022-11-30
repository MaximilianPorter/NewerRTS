using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceAmount
{
    [SerializeField] private int food = 0;
    [SerializeField] private int wood = 0;
    [SerializeField] private int stone = 0;

    public void SetFood(int amt) => food = amt;
    public void SetWood(int amt) => wood = amt;
    public void SetStone(int amt) => stone = amt;

    public int GetFood => food;
    public int GetWood => wood;
    public int GetStone => stone;

    public ResourceAmount(int food = 0, int wood = 0, int stone = 0)
    {
        this.food = food;
        this.wood = wood;
        this.stone = stone;
    }

    public bool HasResources (ResourceAmount compareResources)
    {
        if (food >= compareResources.GetFood &&
            wood >= compareResources.GetWood &&
            stone >= compareResources.GetStone)
            return true;

        return false;
    }

    public static ResourceAmount operator +(ResourceAmount amt0, ResourceAmount amt1)
    {
        return new ResourceAmount(amt0.food + amt1.food, amt0.wood + amt1.wood, amt0.stone + amt1.stone);
    }
    public static ResourceAmount operator *(ResourceAmount amt0, int value)
    {
        return new ResourceAmount(amt0.food * value, amt0.wood * value, amt0.stone * value);
    }

    public static ResourceAmount operator -(ResourceAmount amt0, ResourceAmount amt1)
    {
        int food = (int)Mathf.Clamp(amt0.food - amt1.food, 0, Mathf.Infinity);
        int wood = (int)Mathf.Clamp(amt0.wood - amt1.wood, 0, Mathf.Infinity);
        int stone = (int)Mathf.Clamp(amt0.stone - amt1.stone, 0, Mathf.Infinity);

        return new ResourceAmount(food, wood, stone);
    }


    /// <summary>
    /// returns true if the resources have the same values
    /// </summary>
    public bool CompareResources(ResourceAmount compareResources)
    {
        if (food != compareResources.GetFood)
            return false;
        if (wood != compareResources.GetWood)
            return false;
        if (stone != compareResources.GetStone)
            return false;

        return true;
    }

    public void SetResources (ResourceAmount resources)
    {
        food = resources.GetFood;
        wood = resources.GetWood;
        stone = resources.GetStone;
    }
}