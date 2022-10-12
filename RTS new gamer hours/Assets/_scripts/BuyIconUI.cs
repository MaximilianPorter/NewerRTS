using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Identifier))]
public class BuyIconUI : MonoBehaviour
{
    [SerializeField] private string buttonName = "Name thing";
    [SerializeField] private string buttonDescription = "Description of that same thing";

    [SerializeField] private BuyIcons buttonType;
    [SerializeField] private ResourceAmount cost;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Button.ButtonClickedEvent buttonAction = new Button.ButtonClickedEvent();

    private bool isAffordable = true;
    private Image[] images;
    private Identifier identifier;

    public bool GetIsAffordable => isAffordable;
    public ResourceAmount GetCost => cost;
    public string GetButtonName => buttonName;
    public string GetButtonDescription => buttonDescription;
    //public Resources GetCost => cost;
    public BuyIcons GetButtonType => buttonType;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        images = GetComponentsInChildren<Image>();
    }

    private void Update()
    {
        isAffordable = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources(cost);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = isAffordable ? normalColor : disabledColor;
        }
    }

    /// <summary>
    /// returns true if the player has enough resources
    /// </summary>
    /// <returns></returns>
    public bool TryClickButton ()
    {
        if (PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources (cost))
        {
            PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].SubtractResoruces(cost);

            buttonAction.Invoke();
            return true;
        }

        return false;
    }
}

public enum BuyIcons
{
    // numbers are assigned in case i ever need to add a new enum in between
    // DONT CHANGE THE NUMBERS, JUST ADD A NEW NUMBER

    // DEFAULT
    NONE = 0,

    // BUILDINGS
    Building_House = 20,
    Building_Village = 1,
    Building_Archers = 2,
    Building_ArchersPlus = 3,
    Building_Mage = 4,
    Building_Swordsman = 5,
    Building_SwordsmanPlus = 6,
    Building_Spearman = 7,

    Building_Workshop = 8,

    Building_Tower = 9,
    Building_TowerPlus = 10,

    Building_Farm = 11,
    Building_FarmPlus = 12,


    // OTHER UI BUTTONS
    BuildingRallyPoint = 13,

    // UNITS
    CancelProduction = 21,
    Archer = 14,
    ArcherPlus = 15,
    Swordsman = 16,
    SwordsmanPlus = 17,
    Mage = 18,
    Spearman = 19,

    // highest number: 21
    // last changed on 10/9/22 - 11:05am
}
