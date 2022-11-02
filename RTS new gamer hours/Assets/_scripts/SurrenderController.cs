using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurrenderController : MonoBehaviour
{
    [SerializeField] private bool[] debugSurrender = new bool[4] { false, false, false, false };


    private float[] surrenderCounter = new float[4] { 0f, 0f, 0f, 0f };
    private readonly float timeToSurrender = 10f;

    private bool[] playerSurrendered = new bool[4] { false, false, false, false };

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (playerSurrendered[i])
                continue;


            if (PlayerInput.GetPlayers[i].GetButton(PlayerInput.GetInputBack))
            {
                surrenderCounter[i] += Time.deltaTime;
            }
            else
                surrenderCounter[i] = 0f;


            if (debugSurrender[i])
                surrenderCounter[i] = 100000f;

            if (surrenderCounter[i] > timeToSurrender && playerSurrendered[i] == false)
            {
                playerSurrendered[i] = true;
            }
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
