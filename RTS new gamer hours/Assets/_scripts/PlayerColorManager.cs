using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    [SerializeField] private Material[] playerMaterials;

    private static Material[] staticPlayerMaterials = new Material[4];

    public static Material GetPlayerMaterial (int teamID) => staticPlayerMaterials[teamID];
    public static Color GetPlayerColor (int teamID) => staticPlayerMaterials[teamID].color;
    public static void SetPlayerColor(int teamID, Color newColor) => staticPlayerMaterials[teamID].color = newColor;

    private void Awake()
    {
        // initialize arrays
        for (int i = 0; i < 4; i++)
        {
            staticPlayerMaterials[i] = playerMaterials[i];
        }
    }
}