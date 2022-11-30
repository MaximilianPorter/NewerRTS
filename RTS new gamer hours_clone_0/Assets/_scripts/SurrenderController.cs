using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SurrenderController : MonoBehaviour
{
    public static SurrenderController Instance { get; private set; }

    [SerializeField] private bool[] debugSurrender = new bool[4] { false, false, false, false };


    private bool[] playerSurrendered = new bool[4] { false, false, false, false };

    public bool GetPlayerSurrendered(int playerID) => playerSurrendered[playerID];
    public void SetPlayerSurrendered(int playerID) => playerSurrendered[playerID] = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            
        }

        for (int i = 0; i < playerSurrendered.Length; i++)
        {
            if (playerSurrendered[i])
            {
                if (PlayerHolder.GetBuildings (i).Count > 0)
                {
                    if (PlayerHolder.GetBuildings(i)[0] != null)
                        PlayerHolder.GetBuildings(i)[0].Die();
                }

                if (PlayerHolder.GetUnits (i).Count > 0)
                {
                    if (PlayerHolder.GetUnits(i)[0] != null)
                        PlayerHolder.GetUnits (i)[0].Die();
                }
            }
        }
    }
}
