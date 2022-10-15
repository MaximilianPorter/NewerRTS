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

    public void SubtractResoruces (ResourceAmount subtractResources)
    {
        food = (int)Mathf.Clamp(food - subtractResources.GetFood, 0, Mathf.Infinity);
        wood = (int)Mathf.Clamp(wood - subtractResources.GetWood, 0, Mathf.Infinity);
        stone = (int)Mathf.Clamp(stone - subtractResources.GetStone, 0, Mathf.Infinity);
    }

    public void AddResources (ResourceAmount addResources)
    {
        food += addResources.GetFood;
        wood += addResources.GetWood;
        stone += addResources.GetStone;
    }

    public void AddResources (int food = 0, int wood = 0, int stone = 0)
    {
        this.food += food;
        this.wood += wood;
        this.stone += stone;
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