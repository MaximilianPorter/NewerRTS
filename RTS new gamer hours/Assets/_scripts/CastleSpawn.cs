using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleSpawn : MonoBehaviour
{
    [SerializeField] private int teamID = -1;
    [SerializeField] private float castleSpacing = 10f;
    [SerializeField] private Identifier castle;
    [SerializeField] private float teamInwardAmount = 1.15f;
    [SerializeField] private int debugPlayerCount = 3;
    [SerializeField] private bool showSpawnPoints = true;

    private List<Identifier> playersOnTeam = new List<Identifier>();

    public void AddPlayerToTeam (Identifier player)
    {
        playersOnTeam.Add(player);
    }
    public int GetTeamID => teamID;

    private void Start()
    {
        if (playersOnTeam.Count > 0)
        {
            for (int i = 0; i < playersOnTeam.Count; i++)
            {
                BuildCastle(playersOnTeam[i].GetPlayerID, i);
            }
        }
    }

    private void BuildCastle (int playerID, int i)
    {
        Vector3 dir = Vector3.Cross(transform.position, Vector3.up).normalized * (i - (playersOnTeam.Count - 1) / 2f) * castleSpacing;
        Vector3 pos = transform.position + dir;
        pos -= transform.position * teamInwardAmount * (pos - transform.position).magnitude / 100f;

        Identifier castleIdentity = Instantiate(castle, pos, Quaternion.identity);

        castleIdentity.UpdateInfo(playerID, teamID);
    }

    private void OnDrawGizmos()
    {
        if (!showSpawnPoints)
            return;

        for (int i = 0; i < debugPlayerCount; i++)
        {
            Vector3 dir = Vector3.Cross(transform.position, Vector3.up).normalized * (i - (debugPlayerCount - 1)/2f) * castleSpacing;
            Vector3 pos = transform.position + dir;
            pos -= transform.position * teamInwardAmount * (pos - transform.position).magnitude / 100f;

            Gizmos.DrawSphere(pos, 2f);
        }
    }
}
