using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class MapGenerationManager : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeReference] private NavMeshSurface navMeshSurface;

    [Header("Trees")]
    [SerializeField] private GameObject[] trees;
    [SerializeField] private bool randomRotation = true;
    [Tooltip("smaller the number, the more dense")]
    [SerializeField][Range(0.1f, 5f)] private float density = 1f;
    [SerializeField] private float randomTreeOffset = 0.5f;
    //[SerializeField] private int treeAmt = 100;
    [SerializeField][Range(0f, 1f)] private float spawnThreshold = 0.5f;
    [SerializeField] private Vector2 treeArea;

    private List<GameObject> spawnedTrees = new List<GameObject>();
    private GameObject treeParent;

    [Header("Tree Noise")]
    public int mapWidth;
    public int mapHeight;
    public bool randomSeed = true;
    public int seed;
    public float noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public Vector2 offset;
    private float[,] treeNoiseMap;

    [Header("Land Tiles")]
    [SerializeField] [Range(0f, 100)] private float percentageToContinueLedge = 20f;
    [SerializeField] private GameObject flatTile;
    [SerializeField] private GameObject slopeTile;

    private List<GameObject> spawnedTiles = new List<GameObject>();
    private int mapSize = 10;
    private Tile[,] groundHeights;
    private int highPoints = 3;
    private readonly Vector3 TileSize = new Vector3(30f, 6f, 30f);

    [Header("Rocks")]
    [SerializeField] private GameObject[] rocks;
    [SerializeField] private float rockSpacingBuffer = 5f;
    [SerializeField] private int rocksToPlace = 10;
    [SerializeField] private bool showRockGrid = false;
    [SerializeField] private Vector2Int rockGridSize;
    [SerializeField] private float rockGridSquareWidth = 10f;

    private GameObject rockParent;
    private List<GameObject> spawnedRocks = new List<GameObject>();

    private void Awake()
    {
        if (randomSeed)
            seed = Random.Range(0, 100000000);

        Random.InitState(seed);

        rockParent = new GameObject("Rock Parent");
        SpawnRocks();

        treeParent = new GameObject("Tree Parent");
        treeNoiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
        SpawnTrees();
        AdjustTreeNoise();

        navMeshSurface.BuildNavMesh();
    }

    private void Start()
    {

        //InitializeGroundHeights();
        //PickGroundHeights();
        //SpawnTiles();
    }

    private void Update()
    {
        AdjustTreeNoise();
    }

    private void SpawnTrees ()
    {
        for (float x = -treeArea.x; x < treeArea.x; x += density)
        {
            for (float y = -treeArea.y; y < treeArea.y; y += density)
            {

                // if you put the random offset in the raycast, the trees don't overlap, i don't know why
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x, 20f, y), Vector3.down, out hit, 10000f))
                {
                    // check if raycast hit a layer in groundmask
                    if (groundMask == (groundMask | (1 << hit.transform.gameObject.layer)))
                    {
                        Vector3 randomOffset = new Vector3(Random.Range(-randomTreeOffset, randomTreeOffset), 0f, Random.Range(-randomTreeOffset, randomTreeOffset));
                        Quaternion randRot = Quaternion.Lerp(Quaternion.LookRotation(Vector3.forward, Vector3.up), Quaternion.LookRotation(-Vector3.forward, Vector3.up), Random.Range(0f, 1f));

                        GameObject treeInstance = Instantiate(trees[Random.Range(0, trees.Length)], hit.point + randomOffset,
                            randomRotation ? randRot : Quaternion.identity,
                            treeParent.transform);
                        spawnedTrees.Add(treeInstance);
                    }
                }

            }
        }
    }
    private void AdjustTreeNoise ()
    {
        for (int i = 0; i < spawnedTrees.Count; i++)
        {
            if (spawnedTrees[i] == null)
            {
                spawnedTrees.RemoveAt(i);
                break;
            }

            if (treeNoiseMap[(int)(spawnedTrees[i].transform.position.x + treeArea.x), (int)(spawnedTrees[i].transform.position.z + treeArea.y)] < spawnThreshold)
                spawnedTrees[i].SetActive(false);
            else
                spawnedTrees[i].SetActive(true);
        }
    }

    private void SpawnRocks ()
    {
        Bounds[] gridCubes = new Bounds[rockGridSize.x * rockGridSize.y];

        for (int i = 0, y = 0; y < rockGridSize.y; y++)
        {
            for (int x = 0; x < rockGridSize.x; x++)
            {
                // make grid and assign grid cube
                Bounds gridCube = new Bounds(new Vector3(
                    x * rockGridSquareWidth + rockGridSquareWidth / 2f - rockGridSize.x * rockGridSquareWidth / 2f,
                    50f,
                    y * rockGridSquareWidth + rockGridSquareWidth / 2f - rockGridSize.y * rockGridSquareWidth / 2f), Vector3.one * rockGridSquareWidth);

                gridCubes[i] = gridCube;
                i++;
            }
        }

        for (int i = 0; i < rocksToPlace; i++)
        {
            // do 1 rock for each grid square, and then add them randomly
            Bounds gridCube;
            if (i >= gridCubes.Length)
                gridCube = gridCubes[Random.Range(0, gridCubes.Length)];
            else
                gridCube = gridCubes[i];

            // cast down from that point and place a rock
            for (int j = 0; j < 1000; j++)
            {
                if (j > 990)
                {
                    Debug.Log("wtf, we reached the end");
                    break;
                }

                // choose point in cube to cast from
                Vector3 castPoint = gridCube.center + new Vector3(Random.Range(-gridCube.size.x / 2f, gridCube.size.x / 2f),
                    0f,
                    Random.Range(-gridCube.size.z / 2f, gridCube.size.z / 2f));

                RaycastHit hit;
                if (Physics.Raycast(castPoint, Vector3.down, out hit, Mathf.Infinity, groundMask))
                {
                    if (spawnedRocks.Any (rock => (rock.transform.position - hit.point).sqrMagnitude < rockSpacingBuffer * rockSpacingBuffer * 2f * 2f))
                    {
                        Debug.Log("placing rock too close to another rock");
                        continue;
                    }

                    Quaternion randomLookRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0f);
                    GameObject rockInstance = Instantiate(
                        rocks[Random.Range(0, rocks.Length)],
                        hit.point,
                        randomLookRot,
                        rockParent.transform);

                    spawnedRocks.Add(rockInstance);
                    break;
                }
            }
        }
    }

    #region Tile Spawning
    private void InitializeGroundHeights ()
    {
        groundHeights = new Tile[mapSize, mapSize];

        // set tiles to each space
        for (int x = 0; x < groundHeights.GetLength (0); x++)
        {
            for (int y = 0; y < groundHeights.GetLength (1); y++)
            {
                groundHeights[x, y] = new Tile(-1, new Vector2Int (x, y));
            }
        }

        // connect tile references
        for (int x = 0; x < groundHeights.GetLength(0); x++)
        {
            for (int y = 0; y < groundHeights.GetLength(1); y++)
            {
                if (x - 1 >= 0)
                    groundHeights[x, y].left = groundHeights[x - 1, y];

                if (y - 1 >= 0)
                    groundHeights[x, y].back = groundHeights[x, y - 1];

                if (x + 1 < groundHeights.GetLength(0))
                    groundHeights[x, y].right = groundHeights[x + 1, y];

                if (y + 1 < groundHeights.GetLength(1))
                    groundHeights[x, y].front = groundHeights[x, y + 1];
            }
        }
    }
    private void PickGroundHeights ()
    {
        // 30% chance to pick high points
        for (int i = 0; i < highPoints; i++)
        {
            // pick random tile to elevate either to 2 or 3
            Vector2Int highCoords = new Vector2Int(Random.Range(0, mapSize), Random.Range(0, mapSize));

            while (groundHeights[highCoords.x, highCoords.y].value != -1)
                highCoords = new Vector2Int(Random.Range(0, mapSize), Random.Range(0, mapSize));

            // set the map height
            groundHeights[highCoords.x, highCoords.y].value = Random.Range(2, 3);

            //RecurssionPickHeights(groundHeights[highCoords.x, highCoords.y]);
        }


        for (int x = 0; x < groundHeights.GetLength(0); x++)
        {
            for (int y = 0; y < groundHeights.GetLength(1); y++)
            {
                if (groundHeights[x, y].value == 3)
                {
                    DecreaseTilesNextTo(x, y, 3, 2);
                }
                else if (groundHeights[x, y].value == 2)
                {
                    DecreaseTilesNextTo(x, y, 2, 1);
                }
                else if (groundHeights[x, y].value == 1)
                {
                    DecreaseTilesNextTo(x, y, 1, 0);
                }
                else if (groundHeights[x, y].value == -1)
                {
                    groundHeights[x, y].value = 0;
                }
            }
        }


        string debugVisualize = "";
        for (int y = 0; y < groundHeights.GetLength(1); y++)
        {
            for (int x = 0; x < groundHeights.GetLength(0); x++)
            {
                debugVisualize += " {" + groundHeights[x, y].value + "} ";
                if (x == groundHeights.GetLength(0) - 1)
                    debugVisualize += "\n";
            }
            Debug.Log("row" + y + ": " + debugVisualize);
            debugVisualize = "";
        }
    }

    private void RecurssionPickHeights(Tile highPointTile)
    {
        if (highPointTile.value == 0)
            return;


        if (highPointTile.left != null && highPointTile.left.value == -1)
        {
            bool randomlyKeepPathGoing = Random.Range(0f, 100f) < 50f; // 50% chance
            highPointTile.left.value = randomlyKeepPathGoing ? highPointTile.value : highPointTile.value - 1;
            RecurssionPickHeights(highPointTile.left);
        }

        if (highPointTile.right != null && highPointTile.right.value == -1)
        {
            bool randomlyKeepPathGoing = Random.Range(0f, 100f) < 50f; // 50% chance
            highPointTile.right.value = randomlyKeepPathGoing ? highPointTile.value : highPointTile.value - 1;
            RecurssionPickHeights(highPointTile.right);
        }

        if (highPointTile.front != null && highPointTile.front.value == -1)
        {
            bool randomlyKeepPathGoing = Random.Range(0f, 100f) < 50f; // 50% chance
            highPointTile.front.value = randomlyKeepPathGoing ? highPointTile.value : highPointTile.value - 1;
            RecurssionPickHeights(highPointTile.front);
        }

        if (highPointTile.back != null && highPointTile.back.value == -1)
        {
            bool randomlyKeepPathGoing = Random.Range(0f, 100f) < 50f; // 50% chance
            highPointTile.back.value = randomlyKeepPathGoing ? highPointTile.value : highPointTile.value - 1;
            RecurssionPickHeights(highPointTile.back);
        }
    }
    private void SpawnTiles ()
    {
        for (int x = 0; x < groundHeights.GetLength (0); x++)
        {
            for (int y = 0; y < groundHeights.GetLength (1); y++)
            {
                Tile tile = groundHeights[x, y];
                Tile lookAtTileForSlope = SlopeLookAtTile(tile);
                GameObject tilePrefab = flatTile;

                // decide if it's a slope tile
                if (lookAtTileForSlope != null)
                    tilePrefab = slopeTile;

                GameObject tileInstance = Instantiate(tilePrefab, new Vector3(x * TileSize.x, tile.value * TileSize.y, y * TileSize.z), Quaternion.identity);
                spawnedTiles.Add(tileInstance);

                // adjust rotation of slope
                if (lookAtTileForSlope != null)
                {
                    tileInstance.transform.LookAt(new Vector3(lookAtTileForSlope.coords.x * TileSize.x, tile.value * TileSize.y, lookAtTileForSlope.coords.y * TileSize.z));
                }
            }
        }
    }

    private Tile SlopeLookAtTile (Tile thisTile) // returns v3.zero if there's not supposed to be a slop
    {
        List<Tile> directionsToChoose = new List<Tile>();
        if (thisTile.front != null && thisTile.front.value == thisTile.value + 1)
            directionsToChoose.Add(thisTile.front);
        if (thisTile.back != null && thisTile.back.value == thisTile.value + 1)
            directionsToChoose.Add(thisTile.back);
        if (thisTile.right != null && thisTile.right.value == thisTile.value + 1)
            directionsToChoose.Add(thisTile.back);
        if (thisTile.left != null && thisTile.left.value == thisTile.value + 1)
            directionsToChoose.Add(thisTile.left);

        if (directionsToChoose.Count > 0)
        {
            return directionsToChoose[Random.Range(0, directionsToChoose.Count)];
        }

        return null;
    }

    private void DecreaseTilesNextTo (int x, int y, int from, int to)
    {
        // if we're inside the bounds of the map, and the tile surrounding is 2 or more spaces less

        bool randomlyKeepPathGoing = Random.Range(0f, 100f) < percentageToContinueLedge;

        if (from <= 0)
            return;

        Tile left = groundHeights[x, y].left;
        Tile right = groundHeights[x, y].right;
        Tile front = groundHeights[x, y].front;
        Tile back = groundHeights[x, y].back;

        if (left != null && left.value < to)
            left.value = randomlyKeepPathGoing ? from : to;

        if (right != null && right.value < to)
            right.value = randomlyKeepPathGoing ? from : to;

        if (front != null && front.value < to)
            front.value = randomlyKeepPathGoing ? from : to;

        if (back != null && back.value < to)
            back.value = randomlyKeepPathGoing ? from : to;

        groundHeights[x, y].left = left;
        groundHeights[x, y].right = right;
        groundHeights[x, y].front = front;
        groundHeights[x, y].back = back;


        //if (x + 1 < groundHeights.GetLength(0)) // right
        //{
        //    if (groundHeights[x + 1, y].value < to)
        //        groundHeights[x + 1, y].value = randomlyKeepPathGoing ? from : to;
        //}

        //if (y - 1 >= 0) // backward
        //{
        //    if (groundHeights[x, y - 1].value < to)
        //        groundHeights[x, y - 1].value = randomlyKeepPathGoing ? from : to;
        //}

        //if (y + 1 < groundHeights.GetLength(1)) // forward
        //{
        //    if (groundHeights[x, y + 1].value < to)
        //        groundHeights[x, y + 1].value = randomlyKeepPathGoing ? from : to;
        //}
    }
    #endregion

    public void DeleteWorld ()
    {
        for (int i = 0; i < spawnedRocks.Count; i++)
        {
            Destroy(spawnedRocks[i]);
        }
        spawnedRocks.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(new Vector3(0f, 20f, 0f), new Vector3(treeArea.x * 2f, 5f, treeArea.y * 2f));

        if (showRockGrid)
        {
            for (int y = 0; y < rockGridSize.y; y++)
            {
                for (int x = 0; x < rockGridSize.x; x++)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(new Vector3(
                        x * rockGridSquareWidth + rockGridSquareWidth/2f - rockGridSize.x * rockGridSquareWidth/2f,
                        50f,
                        y * rockGridSquareWidth + rockGridSquareWidth / 2f - rockGridSize.y * rockGridSquareWidth / 2f), Vector3.one * rockGridSquareWidth);
                }
            }

            if (Application.isPlaying)
            {
                for (int i = 0; i < spawnedRocks.Count; i++)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(spawnedRocks[i].transform.position, rockSpacingBuffer);
                }
            }
        }
    }
}

public class Tile
{
    public int value;
    public Vector2Int coords;

    public Tile front;
    public Tile back;
    public Tile left;
    public Tile right;

    public Tile (int value, Vector2Int coords, Tile front = null, Tile back = null, Tile left = null, Tile right = null)
    {
        this.value = value;
        this.coords = coords;
        this.front = front;
        this.back = back;
        this.left = left;
        this.right = right;
    }
}
