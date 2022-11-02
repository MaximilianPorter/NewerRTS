using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameWinManager : MonoBehaviour
{
    public static GameWinManager instance;

    public bool ModeDestroyMainBuilding = false;
    public bool ModeDestroyAllBuildings = false;

    [SerializeField] private GameObject[] playerCastles;

    private static bool[] playerDefeated = new bool[4] { false, false, false, false };

    public bool GameOver => GetWinningTeamID() != -1;

    public bool GetPlayerDefeated(int id) => playerDefeated[id];

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
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

    /// <summary>
    /// returns -1 if there's no winner yet, else, returns winner teamID
    /// </summary>
    /// <returns></returns>
    public int GetWinningTeamID()
    {
        int lastTeamRemaining = -1;
        for (int i = 0; i < playerDefeated.Length; i++)
        {
            // the player is alive
            if (playerDefeated[i] == false)
            {
                // if we're the only one remaining (lastTeamRemaining == -1) or the other player(s) remaining is the same team as you
                if (lastTeamRemaining == -1 || lastTeamRemaining == PlayerHolder.GetPlayerIdentifiers[i].GetTeamID)
                    lastTeamRemaining = PlayerHolder.GetPlayerIdentifiers[i].GetTeamID;
                else
                    return -1;
            }
        }

        return lastTeamRemaining;
    }

}
