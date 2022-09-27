using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identifier : MonoBehaviour
{
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private int playerID;
    [SerializeField] private int teamID;

    public bool GetIsPlayer => isPlayer;
    public int GetPlayerID => playerID;
    public int GetTeamID => teamID;

    public void SetPlayerID (int newPlayerID)
    {
        playerID = newPlayerID;
    }
    public void SetTeamID (int newTeamID)
    {
        teamID = newTeamID;
    }
}
