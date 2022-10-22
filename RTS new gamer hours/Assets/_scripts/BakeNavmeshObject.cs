using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class BakeNavmeshObject : MonoBehaviour
{
    [SerializeField] private bool buildNavMeshOnSpawn = false;
    [SerializeField] private bool buildNavMeshOnDisable = false;
    [SerializeField] private bool buildNavMeshOnDestroy = false;

    private NavMeshSurface navSurface;

    private void Start()
    {
        navSurface = FindObjectOfType<NavMeshSurface>();
        if (buildNavMeshOnSpawn)
        {
            if (navSurface)
            {
                navSurface.BuildNavMesh();
            }
        }
    }

    private void OnDisable()
    {
        if (buildNavMeshOnDisable && navSurface != null)
        {
            navSurface.BuildNavMesh();
        }
    }

    private void OnDestroy()
    {
        if (buildNavMeshOnDestroy && navSurface != null)
        {
            navSurface.BuildNavMesh();
        }
    }
}
