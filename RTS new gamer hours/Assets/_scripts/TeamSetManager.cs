using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSetManager : MonoBehaviour
{
    public static TeamSetManager instance;
    // ex. index 0 (first player) has a value of 3, so he's on team 3
    private int[] playerTeams = new int[4] { 0, 0, 0, 0 };

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Start()
    {
        // if there is already an instance of this, take it's colors
        if (instance != null)
        {
            // take something from old instance
            for (int i = 0; i < playerTeams.Length; i++)
            {
                this.playerTeams[i] = instance.playerTeams[i];
                PlayerHolder.GetPlayerIdentifiers[i].transform.parent.GetComponent<Identifier>().UpdateTeamAndEverythingElse(playerTeams[i]);
            }
            Destroy(instance.gameObject);
            instance = this;
        }
        else
            instance = this;
    }

    public void SetTeamOfPlayer (int playerID, int teamID)
    {
        playerTeams[playerID] = teamID;
    }
}
