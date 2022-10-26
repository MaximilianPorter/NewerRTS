using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Identifier))]
[RequireComponent(typeof(Health))]
public class Building : MonoBehaviour
{
    [SerializeField] private bool isTargetable = true;
    [SerializeField] private BuildingStats stats;
    [SerializeField] private bool debugSpawnUnit = false;
    [SerializeField] private Transform rallyPoint;
    [SerializeField] private GameObject playerHoverEffect;
    [SerializeField] private GameObject smokeExplosion;
    [SerializeField] private GameObject sellBuildingEffect;


    private float scaleUpCounter = 0f;
    private bool rallyPointMoved = false;
    private Identifier identifier;
    private bool playerIsHovering = false;
    private bool isMainSpawnBuilding = false;

    private Cell lastCell;
    private Cell activeCell;

    private Health health;


    public Health GetHealth => health;
    public void SetIsTargetable(bool isTargetable) => this.isTargetable = isTargetable;
    public bool GetIsTargetable => isTargetable;
    public void SetMainSpawnBuilding(bool isMainSpawnBuilding) => this.isMainSpawnBuilding = isMainSpawnBuilding;
    public bool GetIsMainSpawnBuilding => isMainSpawnBuilding;
    public bool GetRallyPointMoved => rallyPointMoved;
    public Vector3 GetRallyPointPos => rallyPoint.position;
    
    public BuildingStats GetStats => stats;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        health = GetComponent<Health>();

        health.SetValues(stats.health);
    }

    private void Start()
    {
        PlayerHolder.AddBuilding(identifier.GetPlayerID, this);

        DestroySurroundings();

        if (playerHoverEffect)
            playerHoverEffect.SetActive(false);

        AssignActiveCell();
    }

    private void Update()
    {
        if (debugSpawnUnit)
        {
            SpawnUnit();
            debugSpawnUnit = false;
        }

        if (playerHoverEffect)
            playerHoverEffect.SetActive(playerIsHovering);

        if (health.GetCurrentHealth < 0)
        {
            Die();
        }

    }

    private void AssignActiveCell()
    {
        activeCell = UnitCellManager.GetCell(transform.position);
        if (lastCell == null || lastCell != activeCell)
        {
            if (lastCell != null)
                lastCell.unitsInCell.Remove(identifier);

            activeCell.unitsInCell.Add(identifier);
            lastCell = activeCell;
        }
    }

    public void SpawnUnit ()
    {
        // spawn unit
        UnitActions unitInstance = Instantiate(stats.unit.gameObject, transform.position, Quaternion.identity).GetComponent <UnitActions>();
        unitInstance.gameObject.SetActive(true); // i think when i spawn them as UnitActions, they spawn disabled

        // set team / ownership stuff
        unitInstance.GetComponent<Identifier>().SetTeamID(identifier.GetTeamID);
        unitInstance.GetComponent<Identifier>().SetPlayerID(identifier.GetPlayerID);

        // first rally point
        unitInstance.GetMovement.SetDestination(rallyPoint.position + new Vector3(Random.Range(-.5f, 0.5f), 0f, Random.Range(-.5f, 0.5f)));

        // unit is added to player list in UnitActions.Start()
    }

    public void PlayerHover (bool isHovering)
    {
        playerIsHovering = isHovering;
    }


    public void Die ()
    {
        GameObject smokeInstance = Instantiate(smokeExplosion, transform.position, Quaternion.identity);
        Destroy(smokeInstance, 5f);

        if(stats.buildingType == BuyIcons.Building_CASTLE && GameWinManager.instance && GameWinManager.instance.ModeDestroyMainBuilding)
        {
            health.Heal(1000000f);

            SwitchAllBuildingTeams();
            SwitchAllUnitTeams();

            SwitchTeams(health.GetLastHitByPlayer.GetPlayerID, health.GetLastHitByPlayer.GetTeamID);
            return;
        }


        DeleteBuilding();
    }

    public void SwitchTeams (int newPlayerID, int newTeamID)
    {
        Identifier newBuildingInstance = Instantiate(this.gameObject, transform.position, transform.rotation).GetComponent<Identifier>();
        newBuildingInstance.SetPlayerID(newPlayerID);
        newBuildingInstance.SetTeamID(newTeamID);

        DeleteBuilding();
    }
    public void DeleteBuilding ()
    {
        lastCell.unitsInCell.Remove(identifier);
        PlayerHolder.RemoveBuilding(identifier.GetPlayerID, this);

        Destroy(gameObject);
    }

    public void SellBuilding ()
    {
        // give back resources worth half the cost of the building
        ResourceAmount sellPrice = new ResourceAmount(GetStats.cost.GetFood / 2, GetStats.cost.GetWood / 2, GetStats.cost.GetStone / 2);
        PlayerResourceManager.instance.AddResourcesWithUI(identifier.GetPlayerID, sellPrice, transform.position);

        // play sell building effect
        GameObject sellEffectInstance = Instantiate(sellBuildingEffect, transform.position, Quaternion.identity);
        Destroy(sellEffectInstance, 5f);

        DeleteBuilding();
    }


    private void SwitchAllBuildingTeams ()
    {
        int breakIndex = 0;
        int firstIndex = 0;
        while (PlayerHolder.GetBuildings(identifier.GetPlayerID).Count > 1)
        {
            breakIndex++;
            if (breakIndex > 10000)
                break;

            Building building = PlayerHolder.GetBuildings(identifier.GetPlayerID)[firstIndex];

            if (building != this)
                building.SwitchTeams(health.GetLastHitByPlayer.GetPlayerID, health.GetLastHitByPlayer.GetTeamID);
            else
                firstIndex++;

        }
    }
    private void SwitchAllUnitTeams()
    {
        int breakIndex = 0;
        int firstIndex = 0;
        while (PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 0)
        {
            breakIndex++;
            if (breakIndex > 10000)
                break;

            UnitActions unit = PlayerHolder.GetUnits(identifier.GetPlayerID)[firstIndex];

            if (unit != null)
                unit.SwitchTeams(health.GetLastHitByPlayer.GetPlayerID, health.GetLastHitByPlayer.GetTeamID);
            else
                firstIndex++;

        }
    }
    

    private void DestroySurroundings()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.interactionRadius);
        string[] tagsToHit = new string[] { "Tree", "Field" };
        for (int i = 0; i < hits.Length; i++)
        {
            if (tagsToHit.Contains(hits[i].tag))
            {
                if (hits[i].TryGetComponent(out TreeShake tree))
                {
                    tree.KillTree();
                }

                if (hits[i].TryGetComponent (out Field field))
                {
                    field.KillField();
                }
            }

        }
    }

    public void SetRallyPoint(Vector3 newRallyPoint)
    {
        rallyPoint.position = newRallyPoint;
        rallyPointMoved = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (!stats)
        {
            Debug.LogError("Please assign stats to " + gameObject.name);
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stats.buildRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stats.interactionRadius);
    }
}
