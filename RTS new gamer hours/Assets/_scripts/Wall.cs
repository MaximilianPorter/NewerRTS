using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class Wall : MonoBehaviour
{
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private int wallLevel = 0;

    private bool doorSpawned = false;

    public bool GetDoorSpawned => doorSpawned;
    public void SetDoorExists(bool doorSpawned) => this.doorSpawned = doorSpawned;
    public int GetWallLevel => wallLevel;

    // connected towers
    private Tower tower0;
    private Tower tower1;

    private Tower doorTower0;
    private Tower doorTower1;

    private Identifier identifier;

    public void InitializeWall(Tower tower0, Tower tower1, bool doorSpawned = false)
    {
        this.tower0 = tower0;
        this.tower1 = tower1;
    }

    private void Start()
    {
        identifier = GetComponent<Identifier>();
        
    }

    private void Update()
    {
        // either tower has been destroyed
        if (tower0 == null && tower1 != null)
        {
            tower1.RemoveConnectedTower(tower0);
            tower1.RemoveConnectedTower(doorTower1);

            Destroy(gameObject);
        }
        else if (tower1 == null && tower0 != null)
        {
            tower0.RemoveConnectedTower(tower1);
            tower0.RemoveConnectedTower(doorTower0);

            Destroy(gameObject);
        }
        else if (tower0 == null && tower1 == null)
        {
            Destroy(gameObject);
        }


        
    }

    private void LateUpdate()
    {
        // if the towers are the same, but the wall isn't, change the wall to match the towers
        if (tower0.GetWallParentPrefab.wallLevel == tower1.GetWallParentPrefab.wallLevel && wallLevel != tower0.GetWallParentPrefab.wallLevel)
        {
            // remove tower connections
            tower0.RemoveConnectedTower(tower1);
            tower0.RemoveConnectedTower(doorTower0);

            tower1.RemoveConnectedTower(tower0);
            tower1.RemoveConnectedTower(doorTower1);

            tower0.GetWallParents.Remove(this);
            tower1.GetWallParents.Remove(this);

            // build new connections
            Wall newWall = tower0.PlaceWalls(tower1);
            if (doorSpawned)
                newWall.PlaceDoor();


            // destroy current walls
            Destroy(gameObject);
        }
    }

    public void ChangeTower (Tower oldTower, Tower newTower)
    {
        if (tower0 == oldTower)
            tower0 = newTower;

        if (tower1 == oldTower)
            tower1 = newTower;
    }

    public void SellWall ()
    {
        Building[] childBuildings = GetComponentsInChildren<Building>();
        for (int i = 0; i < childBuildings.Length; i++)
        {
            childBuildings[i].SellBuilding();
        }

        tower1.RemoveConnectedTower(tower0);
        tower1.RemoveConnectedTower(doorTower1);

        tower0.RemoveConnectedTower(tower1);
        tower0.RemoveConnectedTower(doorTower0);

        Destroy(gameObject);
    }

    public void PlaceDoor ()
    {
        if (doorSpawned)
            return;

        doorSpawned = true;

        // rotation of door
        Vector3 lookDir = (tower0.transform.position - tower1.transform.position);
        lookDir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(lookDir, Vector3.up);

        // pos of door
        Vector3 pos = transform.GetChild(0).position;
        GameObject doorInstance = Instantiate(doorPrefab, pos, lookRot, transform);

        // find towers on door
        doorTower0 = doorInstance.GetComponentsInChildren<Tower>()[0];
        //doorTower0.GetComponent<Identifier>().UpdateInfo(identifier.GetPlayerID, identifier.GetTeamID);
        doorTower1 = doorInstance.GetComponentsInChildren<Tower>()[1];
        //doorTower1.GetComponent<Identifier>().UpdateInfo(identifier.GetPlayerID, identifier.GetTeamID);

        // delete existing wall
        for (int i = 0; i < transform.childCount - 1; i++) // -1 is so it doesn't destroy the door
        {
            Destroy (transform.GetChild(i).gameObject);
        }

        doorTower0.PlaceWalls(tower0, true, this);
        doorTower1.PlaceWalls(tower1, true, this);
    }
}
