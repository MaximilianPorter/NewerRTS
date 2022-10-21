using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Wall : MonoBehaviour
{
    [SerializeField] private bool buildNavMeshOnSpawn = false;
    [SerializeField] private bool buildNavMeshOnDestroy = true;
    private NavMeshSurface navSurface;

    [SerializeField] private GameObject[] connectedTower;
    [SerializeField] private GameObject wallPrefab;

    [SerializeField] private float minOverlapWallSpacing = 1.25f;
    [SerializeField] private float idealWallSpacing = 2.5f;

    [SerializeField] private bool placeWallsBetween;


    private void Start()
    {
        navSurface = FindObjectOfType<NavMeshSurface>();
        if (buildNavMeshOnSpawn)
        {
            if (navSurface)
                navSurface.BuildNavMesh();
        }
    }

    private void Update()
    {
        if (placeWallsBetween)
        {
            placeWallsBetween = false;
            PlaceWalls();
        }
    }

    private void PlaceWalls ()
    {
        for (int i = 0; i < connectedTower.Length; i++)
        {
            GameObject otherTower = connectedTower[i];

            Vector3 otherTowerDir = otherTower.transform.position - transform.position;
            float wallsFromMiddle = otherTowerDir.magnitude / idealWallSpacing / 2f;

            Vector3 middleBetweenWalls = (otherTower.transform.position + transform.position) / 2f;

            //Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(middleBetweenWalls, 1f);
            GameObject middleWallInstance = Instantiate(wallPrefab, middleBetweenWalls, Quaternion.identity);
            middleWallInstance.transform.LookAt(otherTower.transform);

            for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
            {
                float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
                if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
                {
                    //Gizmos.color = Color.blue;
                    //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);

                    GameObject wallInstance = Instantiate(wallPrefab, middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), Quaternion.identity);
                    wallInstance.transform.LookAt(otherTower.transform);
                }
                else
                {
                    //Gizmos.color = Color.white;
                    //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);

                    GameObject wallInstance = Instantiate(wallPrefab, middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing, Quaternion.identity);
                    wallInstance.transform.LookAt(otherTower.transform);
                }
            }

            for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
            {
                float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
                if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
                {
                    //Gizmos.color = Color.blue;
                    //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);

                    GameObject wallInstance = Instantiate(wallPrefab, middleBetweenWalls - otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), Quaternion.identity);
                    wallInstance.transform.LookAt(otherTower.transform);
                }
                else
                {
                    //Gizmos.color = Color.white;
                    //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);

                    GameObject wallInstance = Instantiate(wallPrefab, middleBetweenWalls - otherTowerDir.normalized * (j + 1) * idealWallSpacing, Quaternion.identity);
                    wallInstance.transform.LookAt(otherTower.transform);
                }
            }

            //Gizmos.color = Color.blue;
            //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (wallsFromMiddle), 1f);
            //Gizmos.DrawWireSphere(middleBetweenWalls - otherTowerDir.normalized * (wallsFromMiddle), 1f);
        }
    }

    private void OnDisable()
    {
        if (buildNavMeshOnDestroy)
            navSurface.BuildNavMesh();
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < connectedTower.Length; i++)
        {
            GameObject otherTower = connectedTower[i];

            Vector3 otherTowerDir = otherTower.transform.position - transform.position;
            float wallsFromMiddle = (otherTowerDir.magnitude / idealWallSpacing) / 2f;

            Vector3 middleBetweenWalls = (otherTower.transform.position + transform.position) / 2f;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(middleBetweenWalls, 1f);

            for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
            {
                float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
                if (Mathf.Abs (distFromEnd) < idealWallSpacing / 2f)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);
                }
            }

            for (int j = 0; j < Mathf.FloorToInt(wallsFromMiddle); j++)
            {
                float distFromEnd = (wallsFromMiddle * idealWallSpacing) - ((j + 1) * idealWallSpacing);
                if (Mathf.Abs(distFromEnd) < idealWallSpacing / 2f)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(middleBetweenWalls - otherTowerDir.normalized * (j * idealWallSpacing + minOverlapWallSpacing), 1f);
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(middleBetweenWalls - otherTowerDir.normalized * (j + 1) * idealWallSpacing, 1f);
                }
            }

            //Gizmos.color = Color.blue;
            //Gizmos.DrawWireSphere(middleBetweenWalls + otherTowerDir.normalized * (wallsFromMiddle), 1f);
            //Gizmos.DrawWireSphere(middleBetweenWalls - otherTowerDir.normalized * (wallsFromMiddle), 1f);
        }
    }
}
