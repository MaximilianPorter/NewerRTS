using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    //private static List<Identifier> players = new List<Identifier>();

    private GameObject[] playerGameObjects;

    // THESE NEED TO BE A CHILD OF 1 GAMEOBJECT OR ELSE IT WILL REORDER THEM
    private static Camera[] playerCams = new Camera[4];
    private static RectTransform[] playerCanvasRects = new RectTransform[4];

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
            if (checkBuilding == null)
                continue;

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
        playerGameObjects = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < 4; i++)
        {
            playerUnits.Add(new List<UnitActions>());
            playerBuildings.Add(new List<Building>());

            playerCams[i] = playerGameObjects.FirstOrDefault(player => player.transform.parent.GetComponent<Identifier>().GetPlayerID == i)
                .transform.parent.GetComponentInChildren<Camera>();

            playerCanvasRects[i] = playerGameObjects.FirstOrDefault(player => player.transform.parent.GetComponent<Identifier>().GetPlayerID == i)
                .transform.parent.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();
        }
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            playerBuildings[i].RemoveAll(building => building == null);
            playerUnits[i].RemoveAll(unit => unit == null);
        }
    }


    /// <summary>
    /// returns Vector3.zero if the point is outside of the bounds of the canvas, else, returns the local point
    /// </summary>
    public static Vector2 WorldToCanvasLocalPoint(Vector3 worldPos, int playerID)
    {
        if (playerCams[playerID] == null)
            Debug.Log("cam is null");
        if (playerCanvasRects[playerID] == null)
            Debug.Log("canvas is null");

        Vector2 screenPoint = playerCams[playerID].WorldToScreenPoint(worldPos);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playerCanvasRects[playerID], screenPoint, playerCams[playerID], out Vector2 localPoint))
        {
            return localPoint;
        }

        return Vector2.zero;
    }
}
