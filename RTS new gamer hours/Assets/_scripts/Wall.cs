using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class Wall : MonoBehaviour
{
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private int wallLevel = 0;

    private bool doorSpawned = false;

    private List<Health> wallHealths = new List<Health>();
    public void AddWall (Health newWallHealth) => wallHealths.Add (newWallHealth);

    public bool GetDoorSpawned => doorSpawned;
    public void SetDoorExists(bool doorSpawned) => this.doorSpawned = doorSpawned;
    public int GetWallLevel => wallLevel;

    // connected towers
    private Tower tower0;
    private Tower tower1;

    private Tower doorTower0;
    private Tower doorTower1;

    private Identifier identifier;
    private Health health;

    private int lastWallSize = 0;

    public void InitializeWall(Tower tower0, Tower tower1, bool doorSpawned = false)
    {
        this.tower0 = tower0;
        this.tower1 = tower1;
    }

    private void Start()
    {
        identifier = GetComponent<Identifier>();
        health = GetComponent<Health>();
        
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

        if (lastWallSize != wallHealths.Count)
        {
            health.SetValues(wallHealths.Sum(wall => wall.GetMaxHealth), 0);
            lastWallSize = wallHealths.Count;
        }
    }


    private void LateUpdate()
    {
        wallHealths.RemoveAll(wall => wall == null);

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

        // if a wall takes damage, transfer the damage to 'this.health' and heal the wall
        for (int i = 0; i < wallHealths.Count; i++)
        {
            if (wallHealths[i].GetCurrentHealth < wallHealths[i].GetMaxHealth)
            {
                float diff = wallHealths[i].GetMaxHealth - wallHealths[i].GetCurrentHealth;
                health.TakeDamage(diff, wallHealths[i].GetLastHitByPlayer, wallHealths[i].GetLastHitFromPos);
                wallHealths[i].Heal(1000000);
            }
        }

        if (health.GetCurrentHealth <= 0)
            DestroyWall();
    }

    public void DestroyWall ()
    {
        tower1.RemoveConnectedTower(tower0);
        tower1.RemoveConnectedTower(doorTower1);

        tower0.RemoveConnectedTower(tower1);
        tower0.RemoveConnectedTower(doorTower0);

        Destroy(gameObject);
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

        DestroyWall();
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
