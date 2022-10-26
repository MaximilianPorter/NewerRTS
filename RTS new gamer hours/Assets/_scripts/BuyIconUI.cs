using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Identifier))]
public class BuyIconUI : MonoBehaviour
{
    [SerializeField] private string buttonName = "Name thing";
    [SerializeField] [TextArea] private string buttonDescription = "Description of that same thing";

    [SerializeField] private UnitStats overrideUnitCost;
    [SerializeField] private BuildingStats overrideBuildingCost;

    [Header("Overridden Values")]
    [SerializeField] private BuyIcons buttonType;
    [SerializeField] private ResourceAmount cost;

    [Space(10)]

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Button.ButtonClickedEvent buttonAction = new Button.ButtonClickedEvent();

    private bool isAffordable = true;
    private Image[] images;
    private Identifier identifier;

    public BuildingStats GetOverrideBuilding => overrideBuildingCost;
    public bool GetIsAffordable => isAffordable;
    public ResourceAmount GetCost => cost;
    public string GetButtonName => buttonName;
    public string GetButtonDescription => buttonDescription;
    //public Resources GetCost => cost;
    public BuyIcons GetButtonType => buttonType;

    public Color GetNormalColor => normalColor;
    public Color GetDisabledColor => disabledColor;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();

        BuyIconSpriteManager.AddTypeOfSprite(buttonType, GetComponent<Image>().sprite);
    }

    private void Start()
    {
        images = GetComponentsInChildren<Image>();

        if (overrideUnitCost)
        {
            cost = overrideUnitCost.cost;
        }else if (overrideBuildingCost)
        {
            cost = overrideBuildingCost.cost;
        }
    }

    private void Update()
    {
        bool hasRequiredBuildings = overrideBuildingCost == null ? true : HasRequiredBuildings();


        isAffordable = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources(cost) && hasRequiredBuildings;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = isAffordable ? normalColor : disabledColor;
        }
    }

    private bool HasRequiredBuildings ()
    {
        int hasBuildingAmount = 0;
        for (int i = 0; i < overrideBuildingCost.requiredBuildings.Length; i++)
        {
            if (PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => building.GetStats.buildingType == overrideBuildingCost.requiredBuildings[i]))
            {
                hasBuildingAmount++;
            }
        }

        if (hasBuildingAmount == overrideBuildingCost.requiredBuildings.Length)
            return true;

        return false;
    }

    /// <summary>
    /// returns true if the player has enough resources, and it subtracts the resources from the player
    /// </summary>
    /// <returns></returns>
    public bool TryClickButton ()
    {
        if (!isAffordable)
            return false;

        if (PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources (cost))
        {
            PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].SubtractResoruces(cost);

            buttonAction.Invoke();
            return true;
        }

        return false;
    }

    /// <summary>
    /// returns the cost if we have enough resources to click, returns null if not
    /// </summary>
    /// <returns></returns>
    public ResourceAmount TryClickButtonReturnCost()
    {
        if (!isAffordable)
            return null;

        if (PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources(cost))
        {
            buttonAction.Invoke();
            return cost;
        }

        return null;
    }
}

public enum BuyIcons
{
    // numbers are assigned in case i ever need to add a new enum in between
    // DONT CHANGE THE NUMBERS, JUST ADD A NEW NUMBER

    // DEFAULT
    NONE = 0,
    PLAYER = 27,

    // BUILDINGS
    Building_CASTLE = 22,
    Building_House = 20,
    Building_VillageHouse = 1,
    Building_Archers = 2,
    Building_ArchersPlus = 3,
    Building_Mage = 4,
    Building_Swordsman = 5,
    Building_SwordsmanPlus = 6,
    Building_Spearman = 7,

    Building_Workshop = 8,

    Building_TowerWood = 9,
    Building_TowerStone = 10,

    Building_StorageYard = 26,
    Building_Farm = 11,
    Building_FarmPlus = 12,
    Building_LoggingCamp = 25,
    Building_Mine = 24,


    // OTHER UI BUTTONS
    BuildingRallyPoint = 13,
    SellBuilding = 28,
    BuildWallNoDoor = 23,

    // UNITS
    CancelProduction = 21,
    Archer = 14,
    ArcherPlus = 15,
    Swordsman = 16,
    SwordsmanPlus = 17,
    Mage = 18,
    Spearman = 19,

    // highest number: 28
    // last changed on 10/25/22 - 4:35pm
}
