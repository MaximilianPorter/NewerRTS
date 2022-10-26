using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    [SerializeField] private Color[] playerColors;

    [SerializeField] private Material[] playerMaterials;
    [SerializeField] private Material[] playerProjectorMaterials;
    [SerializeField] private Material[] unitMaterials;

    private static Material[] staticPlayerMaterials = new Material[4];
    private static Material[] staticPlayerProjectorMaterials = new Material[4];
    private static Material[] staticUnitMaterials = new Material[4];

    public static Material GetUnitMaterial(int teamID) => staticUnitMaterials[teamID];

    public static Material GetPlayerMaterial (int teamID) => staticPlayerMaterials[teamID];
    public static Material GetPlayerProjectorMaterial (int teamID) => staticPlayerProjectorMaterials[teamID];
    public static Color GetPlayerColor (int teamID) => staticPlayerMaterials[teamID].color;
    public static void SetPlayerColor(int teamID, Color newColor) => staticPlayerMaterials[teamID].color = newColor;

    private void Awake()
    {
        // initialize arrays
        for (int i = 0; i < 4; i++)
        {
            staticPlayerMaterials[i] = playerMaterials[i];
            staticPlayerProjectorMaterials[i] = playerProjectorMaterials[i];
            staticUnitMaterials[i] = unitMaterials[i];
        }

        
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < staticPlayerMaterials.Length; i++)
        {
            staticPlayerMaterials[i].color = playerColors[i];
        }
        for (int i = 0; i < staticPlayerProjectorMaterials.Length; i++)
        {
            staticPlayerProjectorMaterials[i].color = playerColors[i];
        }
        for (int i = 0; i < staticUnitMaterials.Length; i++)
        {
            staticUnitMaterials[i].SetColor("_TintColorA", playerColors[i]);
        }
    }
}