using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{

    private static List<List<Building>> playerBuildings = new List<List<Building>>();

    public static List<Building> GetBuildings (int playerID)
    {
        return playerBuildings[playerID];
    }
    public static void AddBuilding (int playerID, Building building)
    {
        playerBuildings[playerID].Add(building);
    }
    public static void RemoveBuilding(int playerID, Building building)
    {
        playerBuildings[playerID].Remove(building);
    }



    private static List<List<UnitActions>> playerUnits = new List<List<UnitActions>>();
    public static List<UnitActions> GetUnits (int playerID)
    {
        return playerUnits[playerID];
    }
    public static void AddUnit (int playerID, UnitActions unit)
    {
        playerUnits[playerID].Add(unit);
    }
    public static void RemoveUnit (int playerID, UnitActions unit)
    {
        playerUnits[playerID].Remove(unit);
    }




    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            playerUnits.Add(new List<UnitActions>());
            playerBuildings.Add(new List<Building>());
        }
    }
}
