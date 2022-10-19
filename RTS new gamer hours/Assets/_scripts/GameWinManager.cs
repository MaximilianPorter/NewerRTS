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

    private static Dictionary<int, bool> playerDefeated = new Dictionary<int, bool>()
    {
        { 0, false },
        { 1, false },
        { 2, false },
        { 3, false }
    };


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
    /// returns -1 if there's no winner yet, else, returns winner playerID
    /// </summary>
    /// <returns></returns>
    public int GetWinnerID()
    {
        // 3 players defeated
        if (playerDefeated.Count(player => player.Value == true) == playerDefeated.Count - 1)
        {
            return playerDefeated.FirstOrDefault(player => player.Value == false).Key;
        }

        return -1;
    }

}
