using Rewired;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHolder : MonoBehaviour
{
    public static PlayerHolder instance;

    private GameObject[] playerGameObjects;

    // THESE NEED TO BE A CHILD OF 1 GAMEOBJECT OR ELSE IT WILL REORDER THEM
    private static Camera[] playerCams = new Camera[4];
    private static float[] playerCamStartOrthoSize = new float[4];
    private static RectTransform[] playerCanvasRects = new RectTransform[4];
    public static RectTransform[] GetPlayerCanvasRects => playerCanvasRects;


    // BUILDINGS
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


    // UNITS
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


    // RESEARCH
    private static List<List<BuyIcons>> completedResearch = new List<List<BuyIcons>>();
    private static ResearchUi[] currentResearch = new ResearchUi[4];
    public static ResearchUi GetCurrentResearch(int playerID) => currentResearch[playerID];
    public static void SetCurrentResearch(int playerID, ResearchUi newCurrentResearch) => currentResearch[playerID] = newCurrentResearch;
    public static List<BuyIcons> GetCompletedResearch(int playerID) => completedResearch[playerID];
    public static void AddCompletedResearch(int playerID, BuyIcons researchType) => completedResearch[playerID].Add(researchType);

    private static Identifier[] playerIdentifiers = new Identifier[4];
    public static Identifier[] GetPlayerIdentifiers => playerIdentifiers;

    private void Awake()
    {
        playerGameObjects = GameObject.FindGameObjectsWithTag("Player").OrderBy (player => player.GetComponent <Identifier>().GetPlayerID).ToArray();

        for (int i = 0; i < playerIdentifiers.Length; i++)
        {
            playerIdentifiers[i] = playerGameObjects[i].GetComponent<Identifier>();
        }


        for (int i = 0; i < 4; i++)
        {
            playerUnits.Add(new List<UnitActions>());
            playerBuildings.Add(new List<Building>());
            completedResearch.Add(new List<BuyIcons>());

            playerCams[i] = playerGameObjects.FirstOrDefault(player => player.transform.parent.GetComponent<Identifier>().GetPlayerID == i)
                .transform.parent.GetComponentInChildren<Camera>();

            playerCanvasRects[i] = playerGameObjects.FirstOrDefault(player => player.transform.parent.GetComponent<Identifier>().GetPlayerID == i)
                .transform.parent.GetComponentInChildren<Canvas>().GetComponent<RectTransform>();

            playerCamStartOrthoSize[i] = playerCams[i].orthographicSize;
        }
        instance = this;
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
    public static Vector2? WorldToCanvasLocalPoint(Vector3 worldPos, int playerID)
    {
        Vector2 screenPoint = playerCams[playerID].WorldToScreenPoint(worldPos);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playerCanvasRects[playerID], screenPoint, playerCams[playerID], out Vector2 localPoint))
        {
            return localPoint / (playerCams[playerID].orthographicSize / playerCamStartOrthoSize[playerID]);
        }

        return null;
    }

    public static float ScaleWithScreenOrthoSizeMultiplier (int playerID)
    {
        return (playerCamStartOrthoSize[playerID] / playerCams[playerID].orthographicSize);
    }
}
