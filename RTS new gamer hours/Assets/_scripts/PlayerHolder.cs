using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (playerID < 0)
            return;

        playerBuildings[playerID].Add(building);

        // if other buildings of this type have a rallypoint set, set the new building too
        Building[] buildingsWithType = playerBuildings[playerID].Where(check => check.GetStats.buildingType == building.GetStats.buildingType).ToArray();
        for (int i = 0; i < buildingsWithType.Length; i++)
        {
            Building checkBuilding = buildingsWithType[i];
            if (checkBuilding.GetRallyPointMoved)
            {
                building.SetRallyPoint(checkBuilding.GetRallyPointPos);
                break;
            }
        }
    }
    public static void RemoveBuilding(int playerID, Building building)
    {
        if (playerID < 0)
            return;

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

    private void Update()
    {
        // not as fast as PhysicsOverlapSphere
        //FindNearestEnemy();
    }

    private void FindNearestEnemy ()
    {
        for (int i = 0; i < playerUnits.Count; i++)
        {
            for (int j = 0; j < playerUnits[i].Count; j++)
            {
                UnitActions unit = playerUnits[i][j];

                if (unit == null)
                    continue;


                // find nearest enemy
                for (int x = 0; x < playerUnits.Count; x++)
                {
                    for (int y = 0; y < playerUnits[x].Count; y++)
                    {
                        UnitActions enemy = playerUnits[x][y];

                        if (enemy == unit)
                            continue;

                        bool isEnemy = enemy.GetIdentifier().GetTeamID != unit.GetIdentifier().GetTeamID;
                        if (!isEnemy)
                            continue;

                        bool closeEnough = (enemy.transform.position - unit.transform.position).sqrMagnitude < unit.GetStats.lookRange * unit.GetStats.lookRange;
                        bool closerThanNearestEnemy = unit.GetAttacking().GetNearestEnemy != null && (enemy.transform.position - unit.transform.position).sqrMagnitude <
                            (unit.GetAttacking().GetNearestEnemy.transform.position - unit.transform.position).sqrMagnitude;


                        bool nearestEnemyNull = unit.GetAttacking().GetNearestEnemy == null;

                        if (isEnemy && closeEnough && (nearestEnemyNull || closerThanNearestEnemy))
                            unit.GetAttacking().SetNearestEnemy(enemy.transform);
                        else
                            unit.GetAttacking().SetNearestEnemy(null);
                    }
                }
            }
        }
    }
}
