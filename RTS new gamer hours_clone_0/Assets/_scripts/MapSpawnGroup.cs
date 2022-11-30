using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Map Group Name", menuName = "Create New Map Group")]
public class MapSpawnGroup : ScriptableObject
{
    public string parentName = "Spawn Group Parent";
    [Tooltip("Decides whether or not we use the trees as spawn buffers")]
    public bool spawnBeforeTrees = true;
    public SpawnObject[] spawnObjects;
    public int numberToPlace = 10;
    public bool showGrid = false;
    public Vector2Int gridSize = new Vector2Int (3, 3);
    public Vector2 gridOffset = Vector2.zero;
    public Vector2 squareSpacing = Vector2.zero;
    public Vector2 gridSquareSize = new Vector2 (10f, 10f);
}

[Serializable]
public class SpawnObject
{
    public GameObject prefab;
    public float spacingBuffer;
    public bool randomRotation = true;
    [Tooltip("Higher the value, the more likely it is to spawn")]
    public int spawnWeight = 1;

    public SpawnObject(GameObject prefab, float spacingBuffer, bool randomRotation = true, int spawnWeight = 1)
    {
        this.prefab = prefab;
        this.spacingBuffer = spacingBuffer;
        this.randomRotation = randomRotation;
        this.spawnWeight = spawnWeight;
    }
}
