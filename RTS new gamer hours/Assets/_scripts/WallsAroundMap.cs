using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallsAroundMap : MonoBehaviour
{
    [SerializeField] private BoxCollider map;
    [SerializeField] private Transform[] walls;

    private const float height = 20f;
    private const float depth = 5f;
    private const float overextend = 1f;

    private void Start()
    {
        float xSize = map.transform.localScale.x * map.size.x + overextend + depth / 2f;
        float zSize = map.transform.localScale.z * map.size.z + overextend + depth / 2f;

        walls[0].localScale = new Vector3(xSize, height, depth);
        walls[0].transform.position = map.transform.position + new Vector3(0f, 0f, zSize / 2f);

        walls[1].localScale = new Vector3(xSize, height, depth);
        walls[1].transform.position = map.transform.position - new Vector3(0f, 0f, zSize / 2f);

        walls[2].localScale = new Vector3(depth, height, zSize);
        walls[2].transform.position = map.transform.position + new Vector3(xSize / 2f, 0f, 0f);

        walls[3].localScale = new Vector3(depth, height, zSize);
        walls[3].transform.position = map.transform.position - new Vector3(xSize / 2f, 0f, 0f);
    }
}
