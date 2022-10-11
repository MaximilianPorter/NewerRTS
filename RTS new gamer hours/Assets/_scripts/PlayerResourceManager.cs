using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour
{
    [SerializeField] private int debugStartResources = 0;

    private int[] debugFood = new int[4] { 0, 0, 0, 0 };
    private int[] debugWood = new int[4] { 0, 0, 0, 0 };
    private int[] debugStone = new int[4] { 0, 0, 0, 0 };

    public static int[] PopulationCap = new int[4] { 0, 0, 0, 0 };
    public static ResourceAmount[] PlayerResourceAmounts = new ResourceAmount[4]
    {
        new ResourceAmount (),
        new ResourceAmount (),
        new ResourceAmount (),
        new ResourceAmount ()
    };



    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            PlayerResourceAmounts[i].SetFood(debugStartResources);
            PlayerResourceAmounts[i].SetWood(debugStartResources);
            PlayerResourceAmounts[i].SetStone(debugStartResources);
        }
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            debugFood[i] = PlayerResourceAmounts[i].GetFood;
            debugWood[i] = PlayerResourceAmounts[i].GetWood;
            debugStone[i] = PlayerResourceAmounts[i].GetStone;

            PopulationCap[i] = PlayerHolder.GetBuildings(i).Sum(building => building.GetStats.population);
        }
    }
}
