using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinManager : MonoBehaviour
{
    [SerializeField] private bool ModeDestroyMainBuilding = false;
    [SerializeField] private bool ModeDestroyAllBuildings = false;

    [SerializeField] private GameObject[] playerCastles;

    private bool[] debugPlayerDefeated = new bool[4];
    private static bool[] playerDefeated = new bool[4];
    public static bool[] GetPlayerDefeated => playerDefeated;

    private void Update()
    {
        debugPlayerDefeated = playerDefeated;

        if (ModeDestroyAllBuildings)
        {
            for (int i = 0; i < 4; i++)
            {
                playerDefeated[i] = PlayerHolder.GetBuildings(i).Count <= 0;
            }
        }
        
        else if (ModeDestroyMainBuilding)
        {
            for (int i = 0; i < 4; i++)
            {
                playerDefeated[i] = playerCastles[i] == null;
            }
        }
    }

}
