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

    [SerializeField] private UnitStats overrideUnit;
    [SerializeField] private BuildingStats overrideBuilding;
    [SerializeField] private ResearchStats overrideResearch;

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

    public bool GetIsAffordable => isAffordable;
    public ResourceAmount GetCost => cost;
    public string GetButtonName => buttonName;
    public string GetButtonDescription => buttonDescription;
    //public Resources GetCost => cost;
    public BuyIcons GetButtonType => buttonType;
    public UnitStats GetUnitStats => overrideUnit;
    public BuildingStats GetBuildingStats => overrideBuilding;
    public ResearchStats GetResearchStats => overrideResearch;

    public Color GetNormalColor => normalColor;
    public Color GetDisabledColor => disabledColor;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        images = GetComponentsInChildren<Image>();

        BuyIconSpriteManager.AddTypeOfSprite(buttonType, GetComponent<Image>().sprite);
    }

    private void Start()
    {

    }
    

    private void Update()
    {
        UpdateCost();
        isAffordable = PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources(cost) && HasRequiredBuildings() && NotYetResearched();

        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = isAffordable ? normalColor : disabledColor;
        }
    }

    public BuyIcons[] GetRequiredBuildings ()
    {
        if (overrideBuilding)
            return overrideBuilding.GetRequiredBuildings;
        else if (overrideResearch)
            return overrideResearch.GetRequiredBuildings;

        return null;
    }

    private bool HasRequiredBuildings ()
    {
        // if you don't have your castle and this is not the 'build castle' button
        if ((overrideBuilding == null || (overrideBuilding && overrideBuilding.buildingType != BuyIcons.Building_CASTLE)) && 
            !PlayerHolder.GetBuildings (identifier.GetPlayerID).Any (building => building.GetStats.buildingType == BuyIcons.Building_CASTLE))
        {
            return false;
        }

        if (overrideBuilding == null && overrideResearch == null)
            return true;

        BuyIcons[] requiredBuildings = GetRequiredBuildings();

        int hasBuildingAmount = 0;
        for (int i = 0; i < requiredBuildings.Length; i++)
        {
            if (PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => building.GetStats.buildingType == requiredBuildings[i]))
            {
                hasBuildingAmount++;
            }

            // also works for research
            if (PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(requiredBuildings[i]))
                hasBuildingAmount++;
        }

        if (hasBuildingAmount == requiredBuildings.Length)
            return true;

        return false;
    }

    private bool NotYetResearched ()
    {
        if (overrideResearch == null)
            return true;

        // if we've already researched it
        if (PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Count > 0 && PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(overrideResearch.researchType))
            return false;

        // if we're currently researching anything
        if (PlayerHolder.GetCurrentResearch(identifier.GetPlayerID) != null)
            return false;

        return true;
    }

    private void UpdateCost ()
    {
        if (overrideUnit)
        {
            cost = overrideUnit.cost;
            buttonType = overrideUnit.unitType;
        }
        else if (overrideBuilding)
        {
            int placedBuildingCount = PlayerHolder.GetBuildings(identifier.GetPlayerID).Count <= 0 ? 0 : PlayerHolder.GetBuildings(identifier.GetPlayerID).Count(building => building.GetStats.buildingType == overrideBuilding.buildingType);
            int food = Mathf.CeilToInt((float)overrideBuilding.cost.GetFood * Mathf.Pow (overrideBuilding.costMultiPerBuilding, placedBuildingCount));
            int wood = Mathf.CeilToInt((float)overrideBuilding.cost.GetWood * Mathf.Pow(overrideBuilding.costMultiPerBuilding, placedBuildingCount));
            int stone = Mathf.CeilToInt((float)overrideBuilding.cost.GetStone * Mathf.Pow(overrideBuilding.costMultiPerBuilding, placedBuildingCount));

            cost = new ResourceAmount (food, wood, stone);
            buttonType = overrideBuilding.buildingType;
        }
        else if (overrideResearch)
        {
            cost = overrideResearch.cost;
            buttonType = overrideResearch.researchType;
        }
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
            PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID] -= cost;

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
    Building_CASTLE_DESTROYED = 46,
    Building_House = 20,
    Building_VillageHouse = 1,
    Building_Archers = 2,
    Building_ArchersPlus = 3,
    Building_Mage = 4,
    Building_Swordsman = 5,
    Building_SwordsmanPlus = 6,
    Building_Spearman = 7,
    Building_Sheildman = 31,

    Building_Workshop = 8,
    Building_WorkshopPlus = 50,
    Building_ResearchLab = 29,

    Building_TowerWood = 9,
    Building_TowerStone = 10,

    Building_LandPlot = 35,
    Building_StorageYard = 26,
    Building_Farm = 11,
    Building_FarmPlus = 12,
    Building_LoggingCamp = 25,
    Building_Mine = 24,

    // WALLS
    Wall_Wood = 40,
    Wall_Stone = 43,


    // OTHER UI BUTTONS
    BuildingRallyPoint = 13,
    SellBuilding = 28,
    BuildWallNoDoor = 23,
    AddUnitToWall = 41,
    AddDoorToWall = 42,

    // UNITS
    CancelProduction = 21,
    Unit_Archer = 14,
    Unit_ArcherPlus = 15,
    Unit_Swordsman = 16,
    Unit_SwordsmanPlus = 17,
    Unit_Mage = 18,
    Unit_Spearman = 19,
    Unit_Shieldman = 32,
    Unit_BatteringRam = 47,
    Unit_Catapult = 48,
    Unit_Balista = 49,

    // RESEARCH
    Research_SharpArrows = 30,
    Research_FlamingArrows = 33,
    Research_MoreResources = 34,
    Research_DualWieldingPick = 36,
    Research_FasterChopping = 37,
    Research_EnhancedFood = 38,
    Research_HotterFire = 39,
    Research_Magic = 44,
    Research_LargerMageAttacks = 45,
    Research_SiegeTechnology = 51,

    Creature_Bear = 52,
    Creature_Cow = 53,

    // highest number: 53
    // last changed on 11-20-22 : 12:01pm
}
