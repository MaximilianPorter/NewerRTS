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

    public ResourceAmount(int food, int wood, int stone)
    {
        this.food = food;
        this.wood = wood;
        this.stone = stone;
    }
}
