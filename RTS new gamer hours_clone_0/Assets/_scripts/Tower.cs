using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof (Identifier))]
public class Tower : MonoBehaviour
{
    [SerializeField] private Tower debugConnectionTower;
    [SerializeField] private List<Tower> activeConnectedTowers;
    public List<Tower> GetActiveConnectedTowers => activeConnectedTowers;
    public void RemoveConnectedTower(Tower tower)
    {
        if (activeConnectedTowers.Contains (tower))
            activeConnectedTowers.Remove(tower);
    }
    

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private Wall wallParentPrefab;

    [Header("Spaced Prefabs")]
    [SerializeField] private bool showWallSectionGizmos = false;
    [SerializeField] private bool placeWallsBetween = false;
    [SerializeField] private Building wallSectionPrefab;
    [SerializeField] private float minOverlapWallSpacing = 1.25f;
    [SerializeField] private float idealWallSpacing = 2.5f;

    [Header("Beeg ole wall")]
    [SerializeField] private bool placeBigWall = false;
    [SerializeField] private GameObject cubeForWall;
    [SerializeField] private GameObject cubeForWalkingPlatform;
    [SerializeField] private GameObject cubeForNotch;
    [SerializeField] private float wallWidth = 1f;
    [SerializeField] private float wallHeight = 2f;
    [SerializeField] private float stoneSpacing = 0.5f;

    private List<Wall> wallParents = new List<Wall>();
    public List<Wall> GetWallParents => wallParents;

    private Identifier identifier;

    public Building GetWallPrefab => wallSectionPrefab;
    public Wall GetWallParentPrefab => wallParentPrefab;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        if (placeWallsBetween)
        {
            placeWallsBetween = false;
            PlaceWalls(debugConnectionTower);
        }

        if (placeBigWall)
        {
            placeBigWall = false;
            PlaceBigWall(debugConnectionTower);
        }
    }

    private void PlaceBigWall (Tower towerToConnectTo)
    {
        if (activeConnectedTowers.Contains(towerToConnectTo))
            return;

        Tower otherTower = towerToConnectTo;

        Vector3 otherTowerDir = otherTower.transform.position - transform.position;
        Vector3 middleBetweenTowers = (otherTower.transform.position + transform.position) / 2f;

        Wall wallParent = Instantiate(wallParentPrefab, (transform.position + towerToConnectTo.transform.position) / 2f, Quaternion.identity);
        wallParent.InitializeWall(this, towerToConnectTo);
        wallParents.Add(wallParent);

        GameObject bigWall = Instantiate(cubeForWall, middleBetweenTowers, Quaternion.identity, wallParent.transform);
        bigWall.transform.rotation = Quaternion.LookRotation(otherTowerDir, Vector3.up);
        bigWall.transform.localScale = new Vector3(wallWidth, wallHeight, otherTowerDir.magnitude);

        GameObject walkingPlatformInstance = Instantiate(cubeForWalkingPlatform, bigWall.transform.position + new Vector3 (0f, wallHeight, 0f), bigWall.transform.rotation, wallParent.transform);
        walkingPlatformInstance.transform.localScale = new Vector3(wallWidth * 0.8f, 0.02f, otherTowerDir.magnitude);

        PlaceStonesOnBigWall(wallParent.transform, towerToConnectTo);
    }

    private void PlaceStonesOnBigWall(Transform parent, Tower towerToConnectTo)
    {
        Tower otherTower = debugConnectionTower;

        Vector3 otherTowerDir = otherTower.transform.position - transform.position;

        for (float i = 0; i < otherTowerDir.magnitude; i += stoneSpacing)
        {
            Vector3 pos = transform.position + new Vector3 (0f, wallHeight, 0f) + otherTowerDir.normalized * i
                + Vector3.Cross (otherTowerDir, Vector3.up).normalized * wallWidth/2f;
            GameObject stoneInstance = Instantiate(cubeForNotch, pos, Quaternion.identity, parent);
            stoneInstance.transform.rotation = Quaternion.LookRotation(otherTowerDir, Vector3.up);
        }
    }

    /// <summary>
    /// returns wall parent instance
    /// </summary>
    public Wall PlaceWalls(Tower towerToConnectTo, bool doorExists = false, Wall changeParent = null)
    {
        if (activeConnectedTowers.Contains(towerToConnectTo))
            return null;

        activeConnectedTowers.Add(towerToConnectTo);
        towerToConnectTo.activeConnectedTowers.Add(this);

        Wall wallParent;
        if (changeParent)
        {
            wallParent = changeParent;
        }
        else
        {
            wallParent = Instantiate(wallParentPrefab, (transform.position + towerToConnectTo.transform.position) / 2f, Quaternion.identity);
            wallParent.InitializeWall(this, towerToConnectTo);
            if (wallParent.TryGetComponent (out Identifier wallID))
            {
                if (identifier == null)
                    identifier = GetComponent<Identifier>();
                wallID.UpdateInfo(identifier.GetPlayerID, identifier.GetTeamID);
            }
        }


        wallParent.SetDoorExists(doorExists);
        wallParents.Add(wallParent);
        towerToConnectTo.wallParents.Add(wallParent);

        Vector3 lookDir = towerToConnectTo.transform.position - transform.position;
        lookDir.y = 0f;
        Vector3 otherTowerDir = towerToConnectTo.transform.position - transform.position;
        float wallsFromMiddle = otherTowerDir.magnitude / idealWallSpacing / 2f;

        Vector3 middleBetweenWalls = (towerToConnectTo.transform.position + transform.position) / 2f;

        bool middleHitDown = Physics.Raycast(middleBetweenWalls + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hitDownInfo, 10f, groundMask);
        if (middleHitDown)
            middleBetweenWalls = hitDownInfo.point;
        GameObject middleWallInstance = Instantiate(wallSectionPrefab.gameObject, middleBetweenWalls, Quaternion.identity, wallParent.transform);
        wallParent.AddWall(middleWallInstance.GetComponent<Health>());

        middleWallInstance.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);

        for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
        {
            float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
            if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
            {
                Vector3 pos = middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing);

                bool hitDown = Physics.Raycast(pos + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hitInfo, 10f, groundMask);
                if (hitDown)
                    pos = hitInfo.point;

                GameObject wallInstance = Instantiate(wallSectionPrefab.gameObject, pos, Quaternion.identity,
                    wallParent.transform);

                wallParent.AddWall(wallInstance.GetComponent<Health>());

                wallInstance.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
            else
            {
                Vector3 pos = middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing;

                bool hitDown = Physics.Raycast(pos + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hitInfo, 10f, groundMask);
                if (hitDown)
                    pos = hitInfo.point;

                GameObject wallInstance = Instantiate(wallSectionPrefab.gameObject, pos, Quaternion.identity,
                    wallParent.transform);
                wallParent.AddWall(wallInstance.GetComponent<Health>());

                wallInstance.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }

        for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
        {
            float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
            if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
            {
                //Gizmos.color = Color.blue;
                //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);

                Vector3 pos = middleBetweenWalls - otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing);

                bool hitDown = Physics.Raycast(pos + new Vector3(0f, 5f, 0f), Vector3.down, out RaycastHit hitInfo, 10f, groundMask);
                if (hitDown)
                    pos = hitInfo.point;

                GameObject wallInstance = Instantiate(wallSectionPrefab.gameObject, pos, Quaternion.identity,
                    wallParent.transform);
                wallParent.AddWall(wallInstance.GetComponent<Health>());

                wallInstance.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
            else
            {
                //Gizmos.color = Color.white;
                //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);

                Vector3 pos = middleBetweenWalls - otherTowerDir.normalized * (j + 1) * idealWallSpacing;

                bool hitDown = Physics.Raycast(pos + new Vector3 (0f, 5f, 0f), Vector3.down, out RaycastHit hitInfo, 10f, groundMask);
                if (hitDown)
                    pos = hitInfo.point;

                GameObject wallInstance = Instantiate(wallSectionPrefab.gameObject, pos, Quaternion.identity,
                    wallParent.transform);
                wallParent.AddWall(wallInstance.GetComponent<Health>());

                wallInstance.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }

        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (wallsFromMiddle), 1f);
        //Gizmos.DrawWireSphere(middleBetweenWalls - otherTowerDir.normalized * (wallsFromMiddle), 1f);

        return wallParent;
    }


    public void UpgradeTower(Tower newTower)
    {
        for (int i = 0; i < wallParents.Count; i++)
        {
            wallParents[i].ChangeTower(this, newTower);
        }


        // change out active connected towers to the new tower
        for (int i = 0; i < activeConnectedTowers.Count; i++)
        {
            Tower connectedTower = activeConnectedTowers[i];
            if (connectedTower.activeConnectedTowers.Contains(this))
            {
                connectedTower.activeConnectedTowers.Remove(this);
                connectedTower.activeConnectedTowers.Add(newTower);
            }

            newTower.activeConnectedTowers = activeConnectedTowers;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showWallSectionGizmos || debugConnectionTower == null)
            return;

        Tower otherTower = debugConnectionTower;

        Vector3 otherTowerDir = otherTower.transform.position - transform.position;
        float wallsFromMiddle = (otherTowerDir.magnitude / idealWallSpacing) / 2f;

        Vector3 middleBetweenTowers = (otherTower.transform.position + transform.position) / 2f;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(middleBetweenTowers, 1f);

        for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
        {
            float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
            if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(middleBetweenTowers + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(middleBetweenTowers + otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);
            }
        }

        for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
        {
            float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
            if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(middleBetweenTowers - otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);
            }
            else
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(middleBetweenTowers - otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);
            }
        }
    }
}
