using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Town : MonoBehaviour
{
    [SerializeField] private LayerMask hitLayerMask;
    [SerializeField] private float timeToGive = 5f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private ResourceAmount amountToGive = new ResourceAmount(10, 10, 10);
    [SerializeField] private Projector projector;
    [SerializeField] private MeshRenderer flagRend;


    private bool allSameTeam = false;
    private int teamWithin = -1;
    private int mainPlayerID = -1;

    private float giveCounter = 0f;
    private float checkCounter = 0f;
    private float timeBetweenChecks = 1f;

    private void Update()
    {
        checkCounter += Time.deltaTime;

        if (checkCounter > timeBetweenChecks)
        {
            CheckSurroundings();
            checkCounter = 0f;
        }


        giveCounter += Time.deltaTime;

        projector.material = teamWithin == -1 ? PlayerColorManager.GetNonPlayerProjectorMaterial : PlayerColorManager.GetPlayerProjectorMaterial(mainPlayerID);
        flagRend.material = teamWithin == -1 ? PlayerColorManager.GetNonPlayerMaterial : PlayerColorManager.GetPlayerMaterial(mainPlayerID);

        if (giveCounter > timeToGive && teamWithin != -1 && allSameTeam)
        {
            for (int i = 0; i < PlayerHolder.GetPlayerIdentifiers.Length; i++)
            {
                if (PlayerHolder.GetPlayerIdentifiers[i].GetTeamID == teamWithin)
                    PlayerResourceManager.instance.AddResourcesWithUI(PlayerHolder.GetPlayerIdentifiers[i].GetPlayerID, amountToGive, transform.position);
            }

            // TODO ui display
            giveCounter = 0f;
        }
    }

    private void CheckSurroundings()
    {
        Collider[] collisions = Physics.OverlapSphere(transform.position, radius, hitLayerMask);
        

        if (collisions.Length > 0)
        {
            Identifier firstUnitIdentity = collisions[0].GetComponent<Identifier>();
            if (collisions.All(unit => unit.GetComponent<Identifier>().GetTeamID == firstUnitIdentity.GetTeamID))
            {
                allSameTeam = true;
                teamWithin = firstUnitIdentity.GetTeamID;
                mainPlayerID = firstUnitIdentity.GetPlayerID;
            }
            else
                allSameTeam = false;
        }
        else
        {
            teamWithin = -1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
