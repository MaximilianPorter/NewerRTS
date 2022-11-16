using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    public static PlayerColorManager instance;

    [SerializeField] private Color[] playerColors;
    private static Color[] staticPlayerColors = new Color[4];

    [SerializeField] private Material nonPlayerMaterial;
    [SerializeField] private Material[] playerMaterials;
    private static Material staticNonPlayerMaterial;
    private static Material[] staticPlayerMaterials = new Material[4];

    [SerializeField] private Material nonPlayerProjectorMaterial;
    [SerializeField] private Material[] playerProjectorMaterials;
    private static Material staticNonPlayerProjectorMaterial;
    private static Material[] staticPlayerProjectorMaterials = new Material[4];

    [SerializeField] private Material nonPlayerUnitMaterial;
    [SerializeField] private Material[] unitMaterials;
    private static Material staticNonPlayerUnitMaterial;
    private static Material[] staticUnitMaterials = new Material[4];

    public static Material GetNonPlayerProjectorMaterial => staticNonPlayerProjectorMaterial;
    public static Material GetNonPlayerMaterial => staticNonPlayerMaterial;
    public static Material GetNonPlayerUnitMaterial => staticNonPlayerUnitMaterial;

    public static Material GetUnitMaterial(int playerID) => staticUnitMaterials[playerID];
    public static Material GetPlayerMaterial(int playerID)
    {
        if (playerID < 0)
            return GetNonPlayerMaterial;
        return staticPlayerMaterials[playerID];
    }
    public static Material GetPlayerProjectorMaterial(int playerID)
    {
        if (playerID < 0)
            return GetNonPlayerProjectorMaterial;

        return staticPlayerProjectorMaterials[playerID];
    }
    public static Color GetPlayerColor(int playerID)
    {
        if (playerID < 0)
            return Color.white;

        return staticPlayerColors[playerID];
    }
    public static Color GetPlayerColorIgnoreAlpha(int playerID, float alpha)
    {
        if (playerID < 0)
            return new Color(Color.white.r, Color.white.g, Color.white.b, alpha);
        return new Color(staticPlayerColors[playerID].r, staticPlayerColors[playerID].g, staticPlayerColors[playerID].b, alpha);
    }
    public static void SetPlayerColor(int playerID, Color newColor)
    {
        staticPlayerColors[playerID] = newColor;
        instance.playerColors[playerID] = newColor;
    }

    private void Awake()
    {
    }

    private void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(transform.gameObject);


        // initialize arrays
        for (int i = 0; i < 4; i++)
        {
            staticPlayerMaterials[i] = playerMaterials[i];
            staticPlayerProjectorMaterials[i] = playerProjectorMaterials[i];
            staticUnitMaterials[i] = unitMaterials[i];
        }

        staticNonPlayerProjectorMaterial = nonPlayerProjectorMaterial;
        staticNonPlayerMaterial = nonPlayerMaterial;
        staticNonPlayerUnitMaterial = nonPlayerUnitMaterial;


        // if there is already an instance of this, take it's colors
        if (instance != null)
        {
            for (int i = 0; i < playerColors.Length; i++)
            {
                this.playerColors[i] = instance.playerColors[i];
            }
            Destroy(instance.gameObject);
            instance = this;
        }
        else
            instance = this;

        for (int i = 0; i < playerColors.Length; i++)
        {
            staticPlayerColors[i] = playerColors[i];
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < staticPlayerMaterials.Length; i++)
        {
            staticPlayerMaterials[i].color = staticPlayerColors[i];
        }
        for (int i = 0; i < staticPlayerProjectorMaterials.Length; i++)
        {
            staticPlayerProjectorMaterials[i].color = staticPlayerColors[i];
        }
        for (int i = 0; i < staticUnitMaterials.Length; i++)
        {
            staticUnitMaterials[i].SetColor("_TintColorA", staticPlayerColors[i]);
            staticUnitMaterials[i].SetColor("_TintColorB", staticPlayerColors[i]);
        }
    }
}