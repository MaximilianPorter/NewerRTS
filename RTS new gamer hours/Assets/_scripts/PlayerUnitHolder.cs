using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitHolder : MonoBehaviour
{
    private static List<List<Movement>> playerUnits = new List<List<Movement>>();

    public static List<Movement> GetUnits (int playerID)
    {
        return playerUnits[playerID];
    }
    public static void AddUnit (int playerID, Movement unit)
    {
        playerUnits[playerID].Add(unit);
    }

    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            playerUnits.Add(new List<Movement>());
        }
    }
}
