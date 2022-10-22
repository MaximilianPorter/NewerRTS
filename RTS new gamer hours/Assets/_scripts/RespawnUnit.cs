using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class RespawnUnit : MonoBehaviour
{
    [SerializeField] private int maxNumberOfUnits = 1;
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject unitPrefab;


    private List<UnitActions> spawnUnitInstances = new List<UnitActions>();
    private float respawnTimer = 0f;
    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        if (spawnUnitInstances.Count < maxNumberOfUnits)
        {
            // he is dead or just hasn't spawned yet, so start respawn timer
            respawnTimer -= Time.deltaTime;

            if (respawnTimer < 0)
            {
                respawnTimer = respawnTime;
                SpawnUnit();
            }
        }

        spawnUnitInstances.RemoveAll(unit => unit == null);
    }

    public void SpawnUnit ()
    {
        // spawn unit

        UnitActions unitInstance = Instantiate(unitPrefab, spawnPoint.position, Quaternion.identity).GetComponent<UnitActions>();
        spawnUnitInstances.Add(unitInstance);

        unitInstance.gameObject.SetActive(true); // i think when i spawn them as UnitActions, they spawn disabled

        // set team / ownership stuff
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);
        unitInstance.SetIsSelectable(false);

        // first rally point
        unitInstance.GetMovement.SetDestination(spawnPoint.position + new Vector3(Random.Range(-0.01f, 0.01f), 0f, Random.Range(-0.01f, 0.01f)));

        // unit is added to player list in UnitActions.Start()
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPoint.position, 0.2f);
    }
}
