using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamSetManager : MonoBehaviour
{
    public static TeamSetManager instance;
    // ex. index 0 (first player) has a value of 3, so he's on team 3
    private int[] playerTeams = new int[4] { 0, 1, 2, 3 };

    private CastleSpawn[] castleSpawns;

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);

        castleSpawns = FindObjectsOfType<CastleSpawn>().OrderBy(castle => castle.GetTeamID).ToArray();
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
                Identifier playerIdentity = PlayerHolder.GetPlayerIdentifiers[i].transform.parent.GetComponent<Identifier>();

                playerIdentity.UpdateTeamAndEverythingElse(playerTeams[i]);

                // if there's castle spawns available and the player has joined
                if (castleSpawns.Length > 0 && SplitscreenAutoCamera.instance.GetPlayerJoinOrder.Contains (playerIdentity.GetPlayerID))
                    castleSpawns.FirstOrDefault(castle => castle.GetTeamID == playerTeams[i]).AddPlayerToTeam(playerIdentity);
            }
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;

            for (int i = 0; i < playerTeams.Length; i++)
            {
                Identifier playerIdentity = PlayerHolder.GetPlayerIdentifiers[i].transform.parent.GetComponent<Identifier>();

                // if there's castle spawns available and the player has joined
                if (castleSpawns.Length > 0 && SplitscreenAutoCamera.instance && SplitscreenAutoCamera.instance.GetPlayerJoinOrder.Contains(playerIdentity.GetPlayerID))
                    castleSpawns.FirstOrDefault(castle => castle.GetTeamID == playerTeams[i]).AddPlayerToTeam(playerIdentity);
            }
        }
    }


    public void SetTeamOfPlayer (int playerID, int teamID)
    {
        playerTeams[playerID] = teamID;
    }
}
