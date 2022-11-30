using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PracticeAreaTeamBox : MonoBehaviour
{
    [SerializeField] private int teamBox = -1;
    [SerializeField] private LayerMask hitMask;

    private Identifier[] touchingPlayers = new Identifier[4];
    public Identifier[] GetTouchingPlayers => touchingPlayers;

    private void FixedUpdate()
    {
        touchingPlayers = Physics.OverlapBox(transform.position, transform.localScale / 2f, Quaternion.identity, hitMask)
            .Where(collider => collider != null && collider.TryGetComponent(out Identifier playerID) && playerID.GetIsPlayer)
            .Select(collider => collider.GetComponent<Identifier>()).ToArray();


        if (touchingPlayers.Length > 0)
        {
            for (int i = 0; i < touchingPlayers.Length; i++)
            {
                TeamSetManager.instance.SetTeamOfPlayer(touchingPlayers[i].GetPlayerID, teamBox);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
