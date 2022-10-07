using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    [SerializeField] private int debugStartResources = 0;

    private int[] debugFood = new int[4] { 0, 0, 0, 0 };
    private int[] debugWood = new int[4] { 0, 0, 0, 0 };
    private int[] debugStone = new int[4] { 0, 0, 0, 0 };

    public static int[] PopulationCap = new int[4] { 0, 0, 0, 0 };
    public static int[] Food = new int[4] { 0, 0, 0, 0 };
    public static int[] Wood = new int[4] { 0, 0, 0, 0 };
    public static int[] Stone = new int[4] { 0, 0, 0, 0 };



    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            Food[i] = debugStartResources;
            Wood[i] = debugStartResources;
            Stone[i] = debugStartResources;
        }
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            debugFood[i] = Food[i];
            debugWood[i] = Wood[i];
            debugStone[i] = Stone[i];
        }
    }




    /// <summary>
    /// returns true if you can actually spend the amt, not go negative
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="amt"></param>
    /// <returns></returns>
    public static void SubtractResource (int playerID, ref int[] GetResource, int amt)
    {
        if (GetResource[playerID] - amt >= 0)
        {
            GetResource[playerID] -= amt;
        }
    }
}
