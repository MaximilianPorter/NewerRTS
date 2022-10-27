using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    public static PlayerColorManager instance;

    [SerializeField] private Color[] playerColors;
    private static Color[] staticPlayerColors;

    [SerializeField] private Material nonPlayerMaterial;
    [SerializeField] private Material[] playerMaterials;
    private static Material staticNonPlayerMaterial;
    private static Material[] staticPlayerMaterials = new Material[4];

    [SerializeField] private Material nonPlayerProjectorMaterial;
    [SerializeField] private Material[] playerProjectorMaterials;
    private static Material staticNonPlayerProjectorMaterial;
    private static Material[] staticPlayerProjectorMaterials = new Material[4];

    [SerializeField] private Material[] unitMaterials;
    private static Material[] staticUnitMaterials = new Material[4];

    public static Material GetNonPlayerProjectorMaterial => staticNonPlayerProjectorMaterial;
    public static Material GetNonPlayerMaterial => staticNonPlayerMaterial;


    public static Material GetUnitMaterial(int playerID) => staticUnitMaterials[playerID];
    public static Material GetPlayerMaterial (int playerID) => staticPlayerMaterials[playerID];
    public static Material GetPlayerProjectorMaterial (int playerID) => staticPlayerProjectorMaterials[playerID];
    public static Color GetPlayerColor(int colorID) => staticPlayerColors[colorID];
    public static void SetPlayerColor(int colorID, Color newColor)
    {
        staticPlayerColors[colorID] = newColor;
        instance.playerColors[colorID] = newColor;
    }

    private void Awake()
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

        staticPlayerColors = new Color[playerColors.Length];
    }

    private void Start()
    {
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