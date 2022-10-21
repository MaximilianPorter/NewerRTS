using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class RespawnUnit : MonoBehaviour
{
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject unitPrefab;

    private UnitActions spawnedUnitInstance;
    private float respawnTimer = 0f;
    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        if (spawnedUnitInstance == null)
        {
            // he is dead or just hasn't spawned yet, so start respawn timer
            respawnTimer -= Time.deltaTime;

            if (respawnTimer < 0)
            {
                respawnTimer = respawnTime;
                SpawnUnit();
            }
        }
    }

    public void SpawnUnit ()
    {
        // spawn unit
        spawnedUnitInstance = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity).GetComponent<UnitActions>();
        spawnedUnitInstance.gameObject.SetActive(true); // i think when i spawn them as UnitActions, they spawn disabled

        // set team / ownership stuff
        spawnedUnitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        spawnedUnitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);
        spawnedUnitInstance.SetIsSelectable(false);

        // first rally point
        spawnedUnitInstance.GetMovement.SetDestination(spawnedUnitInstance.transform.position);

        // unit is added to player list in UnitActions.Start()
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
    }
}
