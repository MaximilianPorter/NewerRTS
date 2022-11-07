using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    //[SerializeField] private Camera playerCam;
    //[SerializeField] private RectTransform playerCanvas;
    [SerializeField] private GameObject buttonMenu;
    [SerializeField] private Vector2 buttonMenuOffset;
    [SerializeField] private Vector2 costMenuOffset;
    [SerializeField] private Transform buttonUiLayoutGroup;
    [SerializeField] private float menuCycleSpeed = 10f;
    [SerializeField] private Animator selectAnim;
    [SerializeField] private Transform queuedUnitsLayoutGroup;
    [SerializeField] private Image costAreaLayout;
    [SerializeField] private Transform requiredBuildingLayout;
    [SerializeField] private LayerMask groundLayermask;

    private Image[] requiredBuildingImageContainers;

    [Header("Building")]
    [SerializeField] private BuyIconUI[] initialIcons;
    [SerializeField] private GameObject placingUiVisualObject;
    [SerializeField] private Image placingUiVisual;
    [SerializeField] private Vector3 placingUiVisualOffset = new Vector3(0, 10f, 0f);
    [SerializeField] private GameObject rallyPointPlaceEffect;
    [SerializeField] private GameObject wallBuildingVisual;
    [SerializeField] private MeshRenderer wallVisualToChangeColor;
    [SerializeField] private GameObject placingBuildingVisual;
    [SerializeField] private MeshRenderer buildingVisualToChangeColor;
    [SerializeField] private Material canPlaceMat;
    [SerializeField] private Material canNOTPlaceMat;
    [SerializeField] private GameObject grid;

    [Header("Units")]
    [SerializeField] private BuyIconUI cancelUnits;

    #region private variables    

    private bool placingRallyPoint = false;
    private bool placingAllRallyPoints = false;
    private Building rallyBuilding = null;

    private Identifier identifier;
    private UnitSelection unitSelection;
    private Building lastHoveringBuilding;
    private Building hoveringBuilding; // building that is being stood on
    private Building aboutToPlaceBuilding;
    private ResourceAmount tempPlaceBuildingCost;
    private Tower fromTower;

    private bool isPlacingBuilding = false;
    private bool clickedOnBuilding = false;
    private bool anyMenuOpen = false;
    private bool buildingMenuIsOpen = false;

    private bool unitMenuIsOpen = false;

    private Transform[] costResourceTransforms;
    private TMP_Text[] costTexts;

    private QueuedUpUnitUi[] allQueuedUnits;
    private BuyIconUI[] allIcons;
    private int selectedIconIndex = 0;
    private List<Queue<BuyIcons>> typesOfUnitsToSpawn = new List<Queue<BuyIcons>>();
    private Dictionary<Queue<BuyIcons>, float> spawnUnitCounter = new Dictionary<Queue<BuyIcons>, float>();

    #endregion

    #region setters and getters
    public BuyIconUI GetSelectedUiButton => allIcons[selectedIconIndex];
    public bool GetHasMenuOpen => anyMenuOpen;

    [Serializable]
    private struct UiBuilding
    {
        public Building building;
        public BuyIconUI icon;
    }
    #endregion

    private void Awake()
    {
        identifier = GetComponent<Identifier> ();
        unitSelection = GetComponent<UnitSelection>();

        // find ui icons in the layout group
        allIcons = buttonUiLayoutGroup.GetComponentsInChildren<BuyIconUI>();
        allQueuedUnits = queuedUnitsLayoutGroup.GetComponentsInChildren<QueuedUpUnitUi>();

        costResourceTransforms = new Transform[costAreaLayout.transform.childCount];
        for (int i = 0; i < costAreaLayout.transform.childCount; i++)
        {
            costResourceTransforms[i] = costAreaLayout.transform.GetChild(i);
        }
        costTexts = costAreaLayout.GetComponentsInChildren<TMP_Text>();
        requiredBuildingImageContainers = requiredBuildingLayout.GetComponentsInChildren<Image>().Where(image => image.transform != requiredBuildingLayout).ToArray();
    }

    private void Start()
    {
        DisableAllIcons();
        buttonMenu.SetActive(false);
    }

    private void Update()
    {
        if (PauseGameManager.GetIsPaused)
            return;

        anyMenuOpen = buildingMenuIsOpen || unitMenuIsOpen;

        if (anyMenuOpen)
            ManageSelectedIcon();

        ManageCostArea();

        HandlePlacementUiVisual();

        HandleOpeningCycleMenu();
        StartCoroutine(HandleCycleMenuOpen());

        CheckForHoveringBuilding();

        SpawnQueuedUnits();

        HandleBuildingRallyPoint();

        HandlePlacingBuilding();

        HandlePlacingWall();
    }

    private void ManageSelectedIcon ()
    {
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight) ||
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadRight))
        {
            IncreaseIconIndex();
        }
        else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft) ||
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadLeft))
        {
            DecreaseIconIndex();
        }
        buttonUiLayoutGroup.transform.localPosition = Vector3.Lerp(buttonUiLayoutGroup.transform.localPosition, -allIcons[selectedIconIndex].transform.localPosition, Time.deltaTime * menuCycleSpeed);


        // adjust the required building layout images
        if (allIcons[selectedIconIndex].GetRequiredBuildings() == null 
            || (allIcons[selectedIconIndex].GetRequiredBuildings() != null && allIcons[selectedIconIndex].GetRequiredBuildings().Length <= 0))
        {
            requiredBuildingLayout.gameObject.SetActive(false);
            return;
        }
        else
        {
            requiredBuildingLayout.gameObject.SetActive(true);
        }

        BuyIcons[] requiredBuildings = allIcons[selectedIconIndex].GetRequiredBuildings();
        for (int i = 0; i < requiredBuildingImageContainers.Length; i++)
        {
            if (i >= requiredBuildings.Length)
            {
                requiredBuildingImageContainers[i].gameObject.SetActive(false);
                continue;
            }
            requiredBuildingImageContainers[i].gameObject.SetActive(true);
            requiredBuildingImageContainers[i].sprite = BuyIconSpriteManager.GetTypeOfIcon(requiredBuildings[i]);
            requiredBuildingImageContainers[i].color = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => building.GetStats.buildingType == requiredBuildings[i]) || 
                PlayerHolder.GetCompletedResearch (identifier.GetPlayerID).Contains(requiredBuildings[i])
                ? allIcons[selectedIconIndex].GetNormalColor
                : allIcons[selectedIconIndex].GetDisabledColor;
        }
    }

    private void ManageCostArea ()
    {
        ResourceAmount cost = allIcons[selectedIconIndex].GetCost;

        if (fromTower)
        {
            cost = tempPlaceBuildingCost;

            costAreaLayout.color = new Color(costAreaLayout.color.r, costAreaLayout.color.g, costAreaLayout.color.b,
                PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources(tempPlaceBuildingCost) ? 1f : 0.5f);
        }
        else
        {
            // fade cost ui if the button isn't affordable
            costAreaLayout.color = new Color(costAreaLayout.color.r, costAreaLayout.color.g, costAreaLayout.color.b,
                allIcons[selectedIconIndex].GetIsAffordable ? 1f : 0.5f);
        }


        // ui display for cost
        costResourceTransforms[0].gameObject.SetActive(cost.GetFood > 0);
        costResourceTransforms[1].gameObject.SetActive(cost.GetWood > 0);
        costResourceTransforms[2].gameObject.SetActive(cost.GetStone > 0);

        costTexts[0].text = cost.GetFood.ToString();
        costTexts[1].text = cost.GetWood.ToString();
        costTexts[2].text = cost.GetStone.ToString();

        costAreaLayout.gameObject.SetActive((anyMenuOpen && (cost.GetFood > 0 || cost.GetWood > 0 || cost.GetStone > 0)) || fromTower);

    }

    private void IncreaseIconIndex ()
    {
        // if the gameobject is active and we can increase


        // DOESN'T LOOP
        int nextAvailableIndex = selectedIconIndex;
        if (selectedIconIndex + 1 < allIcons.Length)
        {
            for (int i = selectedIconIndex; i < allIcons.Length; i++)
            {
                if (i + 1 < allIcons.Length && allIcons[i + 1].gameObject.activeSelf)
                {
                    nextAvailableIndex = i + 1;
                    break;
                }

            }
        }
        selectedIconIndex = nextAvailableIndex;




        //// LOOPS
        //if (selectedIconIndex + 1 < allIcons.Length)
        //{
        //    for (int i = 0; i < allIcons.Length; i++)
        //    {
        //        if (selectedIconIndex + 1 < allIcons.Length)
        //            selectedIconIndex++;
        //        else
        //            selectedIconIndex = 0;

        //        if (allIcons[selectedIconIndex].gameObject.activeSelf)
        //        {
        //            break;
        //        }

        //    }
        //}
        //else
        //{
        //    selectedIconIndex = 0;
        //    for (int i = 0; i < allIcons.Length; i++)
        //    {
        //        if (allIcons[selectedIconIndex].gameObject.activeSelf)
        //        {
        //            break;
        //        }

        //        selectedIconIndex++;
        //    }
        //}
    }
    private void DecreaseIconIndex()
    {
        // if the gameobject is active and we can decrease

        // DOESN'T LOOP
        int nextAvailableIndex = selectedIconIndex;
        if (selectedIconIndex - 1 >= 0)
        {
            for (int i = selectedIconIndex; i > 0; i--)
            {
                if (i - 1 >= 0 && allIcons[i - 1].gameObject.activeSelf)
                {
                    nextAvailableIndex = i - 1;
                    break;
                }

            }
        }
        selectedIconIndex = nextAvailableIndex;


        //// LOOPS
        //if (selectedIconIndex - 1 >= 0)
        //{
        //    for (int i = 0; i < allIcons.Length; i++)
        //    {
        //        if (selectedIconIndex - 1 >= 0)
        //            selectedIconIndex--;
        //        else
        //            selectedIconIndex = allIcons.Length - 1;

        //        if (allIcons[selectedIconIndex].gameObject.activeSelf)
        //        {
        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    selectedIconIndex = allIcons.Length - 1;

        //    for (int i = 0; i < allIcons.Length; i++)
        //    {
        //        if (allIcons[selectedIconIndex].gameObject.activeSelf)
        //        {
        //            break;
        //        }

        //        selectedIconIndex--;
        //    }

        //}
    }

    private void HandleOpeningCycleMenu ()
    {
        // don't open menu if we're cycleing through formations
        if (!PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputRallyTroops) && !unitSelection.GetHasTroopsSelected)
        {
            buttonMenu.SetActive(anyMenuOpen);
            PlayerInput.SetPlayerIsInMenu(identifier.GetPlayerID, buildingMenuIsOpen);

            HandleBuildMenu();
            HandleUnitMenu();
        }
        buttonMenu.transform.localPosition = PlayerHolder.WorldToCanvasLocalPoint(transform.position + (Vector3)buttonMenuOffset, identifier.GetPlayerID).GetValueOrDefault(Vector2.zero);
    }

    private IEnumerator HandleCycleMenuOpen ()
    {
        // BUILDING MENU OPEN
        if (anyMenuOpen)
        {
            unitSelection.DeselectUnits();

            if (unitMenuIsOpen)
                cancelUnits.gameObject.SetActive(typesOfUnitsToSpawn.Count > 0);
            else
                cancelUnits.gameObject.SetActive(false);


            // click the back button
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
                CloseMenu();

            if (buildingMenuIsOpen)
            {
                // close build menu if we're ever too far away || standing on building
                bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);

                if ((/*!inRangeOfBuilding || */hoveringBuilding) && !clickedOnBuilding)
                {
                    CloseMenu();
                }

                if (clickedOnBuilding && !hoveringBuilding)
                    CloseMenu();
            }


            //Vector2 screenPoint = playerCam.WorldToScreenPoint(transform.position + (Vector3)buttonMenuOffset);
            //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(playerCanvas, screenPoint, playerCam, out Vector2 localPoint))
            //{
            //    buttonMenu.transform.localPosition = localPoint;
            //}

            // CLICK ICON
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
            {
                yield return new WaitForSeconds(0.05f);

                selectAnim.SetTrigger("Select");

                if (unitMenuIsOpen)
                {
                    // if we have the resources, click button and close menu
                    if (allIcons[selectedIconIndex].TryClickButton())
                    {
                        // spend them
                    }
                    else
                    {
                        // TODO make some sort of red flash indicating that we don't have the resources
                    }
                }else if (buildingMenuIsOpen)
                {
                    // upgrading an existing building
                    if (clickedOnBuilding)
                    {
                        if (allIcons[selectedIconIndex].TryClickButton())
                        {
                            // spend and close menu
                            CloseMenu();
                        }
                    }
                    else
                    {
                        // placing a new building
                        tempPlaceBuildingCost = allIcons[selectedIconIndex].TryClickButtonReturnCost();
                        if (tempPlaceBuildingCost == null)
                        {
                            // we don't have the resources to buy this, don't close the menu
                            // TODO make some sort of red flash indicating that we don't have the resources
                        }
                        else
                        {
                            CloseMenu();
                        }
                    }
                }

            }
        }
    }

    private void HandleUnitMenu ()
    {
        if (!anyMenuOpen && fromTower == null)
        {
            // open unit menu
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenUnitMenu))
            {
                // if we have no buildings, or all the buildings have no unit attached, return
                if (PlayerHolder.GetBuildings(identifier.GetPlayerID).Count <= 0 ||
                    PlayerHolder.GetBuildings (identifier.GetPlayerID).All (building => building.GetStats.unit == null))
                {
                    return;
                }

                bool runOnce = false;
                for (int i = 0; i < allIcons.Length; i++)
                {
                    Building[] buildings = PlayerHolder.GetBuildings(identifier.GetPlayerID).ToArray();
                    for (int j = 0; j < buildings.Length; j++)
                    {
                        if (buildings[j].GetStats.unit == null) continue;

                        // find buildings with units available
                        if (buildings[j].GetStats.unitType == allIcons[i].GetButtonType)
                        {
                            if (!runOnce)
                            {
                                selectedIconIndex = i;
                                runOnce = true;
                            }
                            allIcons[i].gameObject.SetActive(true);
                            unitMenuIsOpen = true;
                        }
                    }
                }

            }
        }
    }

    private void HandleBuildMenu ()
    {
        if (!anyMenuOpen && fromTower == null)
        {
            // build new building
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu) && !hoveringBuilding)
            {
                // open build menu
                buildingMenuIsOpen = true;

                selectedIconIndex = allIcons.ToList().IndexOf(initialIcons[0]);
                // turn on correct menu buttons
                for (int i = 0; i < initialIcons.Length; i++)
                {
                    initialIcons[i].gameObject.SetActive(true);
                }

                //return;
                //// if any buildings are close enough to player
                //bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);
                //if (inRangeOfBuilding)
                //{
                //    // if we're in range of a building
                //}
            }


            // click on hover building
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu) && hoveringBuilding 
                && hoveringBuilding.GetStats.subsequentUpgrades.Length > 0)
            {
                clickedOnBuilding = true;
                buildingMenuIsOpen = true;


                bool runOnce = false;
                for (int i = 0; i < allIcons.Length; i++)
                {
                    // if any icon type is the same as the hovering building subsequent upgrades
                    if (hoveringBuilding.GetStats.subsequentUpgrades.Any(upgrade => upgrade == allIcons[i].GetButtonType))
                    {
                        if (!runOnce)
                        {
                            selectedIconIndex = i;
                            runOnce = true;
                        }

                        // turn it on, but not if it's in completed research or current research
                        bool notResearchingThisIcon = !PlayerHolder.GetCompletedResearch(identifier.GetPlayerID).Contains(allIcons[i].GetButtonType);

                        // turn off the door icon if there's already a door on the wall
                        if (hoveringBuilding.transform.parent && hoveringBuilding.transform.parent.TryGetComponent (out Wall parentWall) && parentWall.GetDoorSpawned && allIcons[i].GetButtonType == BuyIcons.AddDoorToWall)
                        {
                            allIcons[i].gameObject.SetActive(false);
                            continue;
                        }

                        allIcons[i].gameObject.SetActive(notResearchingThisIcon);

                    }
                }

                if (allIcons[selectedIconIndex].GetButtonType == BuyIcons.SellBuilding)
                    IncreaseIconIndex();
            }
        }
    }
    private void CloseMenu ()
    {
        buildingMenuIsOpen = false;
        unitMenuIsOpen = false;
        clickedOnBuilding = false;
        DisableAllIcons();
    }

    private void SpawnQueuedUnits ()
    {
        // activate queued unit gameobjects
        for (int i = 0; i < allQueuedUnits.Length; i++)
        {
            allQueuedUnits[i].gameObject.SetActive(allQueuedUnits[i].GetUnitAmt > 0);
        }


        for (int i = 0; i < typesOfUnitsToSpawn.Count; i++)
        {
            

            Queue<BuyIcons> activeQueue = typesOfUnitsToSpawn[i];

            if (activeQueue.Count > 0)
            {
                // use main spawn building for stats
                Building activeBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).
                    LastOrDefault(building => building.GetStats.unitType == activeQueue.Peek() && building.GetIsMainSpawnBuilding);

                // if we couldn't find a main spawn building use more recently placed building
                if (activeBuilding == null)
                {
                    activeBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).
                    LastOrDefault(building => building.GetStats.unitType == activeQueue.Peek());
                }

                // get total buildings (ie. total # of archer buildings)
                int buildingsOfThisType = PlayerHolder.GetBuildings(identifier.GetPlayerID).
                    Count(building => building.GetStats.unitType == activeQueue.Peek());

                // we probably upgraded the only building of this type, and now there's 0
                if (buildingsOfThisType <= 0)
                {
                    CancelUnits(typesOfUnitsToSpawn[i].Peek());
                    typesOfUnitsToSpawn[i].Clear();
                    typesOfUnitsToSpawn.RemoveAt(i);
                    break;
                }

                // counting
                spawnUnitCounter[activeQueue] += Time.deltaTime;

                // for each building, the time is 0.7 times less
                float timeUntilSpawn = activeBuilding.GetStats.initialUnitSpawnTime *
                    Mathf.Pow(activeBuilding.GetStats.spawnTimeMultiPerBuilding, buildingsOfThisType - 1);

                // set visual details
                QueuedUpUnitUi queuedUnitUi = allQueuedUnits.FirstOrDefault(queuedUnit => queuedUnit.GetUnitType == activeBuilding.GetStats.unitType);
                queuedUnitUi.SetDetails (activeQueue.Count, spawnUnitCounter[activeQueue] / timeUntilSpawn);


                // dont spawn yet if we don't have space
                if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count + 1 > PlayerResourceManager.PopulationCap[identifier.GetPlayerID])
                    continue;

                // spawn unit and reset timer
                if (spawnUnitCounter[activeQueue] > timeUntilSpawn)
                {
                    activeQueue.Dequeue();
                    activeBuilding.SpawnUnit();
                    spawnUnitCounter[activeQueue] = 0f;

                    // last object, remove it
                    if (activeQueue.Count <= 0)
                    {
                        queuedUnitUi.SetDetails(0, 0);

                        spawnUnitCounter.Remove(activeQueue);
                        typesOfUnitsToSpawn.Remove(activeQueue);
                    }
                }

            }
        }
    }


    private void CancelUnits (BuyIcons unitType)
    {
        ResourceAmount cost = allIcons.FirstOrDefault(icon => icon.GetButtonType == unitType).GetCost;
        int unitCount = allQueuedUnits.FirstOrDefault(queuedUnit => queuedUnit.GetUnitType == unitType).GetUnitAmt;
        // refunt cost of unit
        PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].AddResources(
            cost.GetFood * unitCount,
            cost.GetWood * unitCount,
            cost.GetStone * unitCount
            );

        allQueuedUnits.FirstOrDefault(queuedUnit => queuedUnit.GetUnitType == unitType).SetDetails(0, 0);
    }
    public void CancelQueuedUnits ()
    {
        for (int i = 0; i < typesOfUnitsToSpawn.Count; i++)
        {
            CancelUnits(typesOfUnitsToSpawn[i].Peek());

            typesOfUnitsToSpawn[i].Clear();
        }
        typesOfUnitsToSpawn.RemoveRange(0, typesOfUnitsToSpawn.Count);
        for (int i = 0; i < allQueuedUnits.Length; i++)
        {
            allQueuedUnits[i].SetDetails(0, 0);
        }
        typesOfUnitsToSpawn.Clear();

        IncreaseIconIndex();
    }
    // used by UI buttons
    public void QueueUpUnitFromBuilding ()
    {
        BuyIcons unitType = allIcons[selectedIconIndex].GetButtonType;
        //Building[] buildingsToSpawnFrom = PlayerHolder.GetBuildings(identifier.GetPlayerID).Where(building => building.GetStats.unitType == unitType).ToArray();

        // find the queue to add to
        Queue<BuyIcons> newQueue = typesOfUnitsToSpawn.FirstOrDefault(queue => queue.Peek() == unitType);

        // if there's no queue, make one and add it
        if (newQueue == null)
        {
            newQueue = new Queue<BuyIcons>();
            typesOfUnitsToSpawn.Add(newQueue);
            spawnUnitCounter.Add(newQueue, 0f);
        }

        // queue up building
        newQueue.Enqueue(unitType);
    }
    // used by UI buttons
    public void ActivateBuildAreaWith (Building building)
    {
        isPlacingBuilding = true;
        aboutToPlaceBuilding = building;
    }
    // used by UI buttons

    // used by UI buttons
    public void UpgradeBuilding (Building building)
    {
        if (hoveringBuilding.TryGetComponent (out Tower hoverTower))
        {
            // if we're upgradeing a tower, they might have walls connecting them
            hoverTower.UpgradeTower(BuildBuilding(building).GetComponent<Tower>());
            hoveringBuilding.DeleteBuilding();
            return;
        }

        hoveringBuilding.DeleteBuilding();

        BuildBuilding(building);
    }
    // used by UI buttons
    public void SellBuilding ()
    {
        Wall wallParent = hoveringBuilding.GetComponentInParent<Wall>();
        if (wallParent)
        {
            wallParent.SellWall();
            return;
        }

        hoveringBuilding.SellBuilding();
    }
    // used by UI buttons
    public void BuildWall ()
    {
        Tower tower = hoveringBuilding.GetComponent<Tower>();

        if (fromTower == null) // we are starting the wall
        {
            fromTower = tower;
        }
    }
    // used by UI buttons
    public void PlaceDoorOnWall()
    {
        Wall wall = hoveringBuilding.GetComponentInParent<Wall>();
        wall.PlaceDoor();
    }

    private Identifier BuildBuilding (Building building)
    {
        // place building
        Vector3 buildingPos = hoveringBuilding ? hoveringBuilding.transform.position : placingBuildingVisual.transform.position;
        Identifier placedBuildingIdentity = Instantiate(building.gameObject, buildingPos, Quaternion.identity).GetComponent<Identifier>();

        // set team and player ID of building
        placedBuildingIdentity.UpdateInfo(identifier.GetPlayerID, identifier.GetTeamID);

        return placedBuildingIdentity;
    }
    private void HandlePlacingBuilding ()
    {
        placingBuildingVisual.SetActive(isPlacingBuilding);
        grid.SetActive(isPlacingBuilding);

        if (isPlacingBuilding)
        {
            Vector3 groundPos = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 3f, groundLayermask) ? hitInfo.point : Vector3.zero;
            placingBuildingVisual.transform.position = new Vector3(Mathf.Round (transform.position.x * 2f) / 2f, groundPos.y, Mathf.Round (transform.position.z * 2f) / 2f);

            bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID)
                .Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);

            bool canPlace = inRangeOfBuilding && !hoveringBuilding;
            buildingVisualToChangeColor.material = canPlace ? canPlaceMat : canNOTPlaceMat;


            // PLACE BUILDING
            if (PlayerInput.GetPlayers [identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect) && canPlace)
            {
                PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].SubtractResoruces(tempPlaceBuildingCost);

                // place building
                BuildBuilding(aboutToPlaceBuilding);
                isPlacingBuilding = false;
                aboutToPlaceBuilding = null;
            }
            // CANCEL
            else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack) 
                || PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu)
                || PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenUnitMenu))
            {
                isPlacingBuilding = false;
                aboutToPlaceBuilding = null;
            }
        }
    }

    private void HandlePlacingWall ()
    {
        if (tempPlaceBuildingCost == null)
            tempPlaceBuildingCost = new ResourceAmount();

        wallBuildingVisual.SetActive(fromTower != null);

        if (fromTower == null)
            return;

        bool isNextToCorrectBuilding = hoveringBuilding && hoveringBuilding.TryGetComponent (out Tower hoverTower) && hoverTower != fromTower &&
            !fromTower.GetActiveConnectedTowers.Contains(hoverTower);
        bool canPlaceWall = isNextToCorrectBuilding &&
            PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].HasResources (tempPlaceBuildingCost);

        // change visual colors
        wallVisualToChangeColor.material = canPlaceWall ? canPlaceMat : canNOTPlaceMat;

        // snap to tower
        Vector3 wallEndPos = isNextToCorrectBuilding ? hoveringBuilding.transform.position : transform.position;


        // manage cost
        tempPlaceBuildingCost.SetFood((int)(fromTower.GetWallPrefab.GetStats.cost.GetFood * Vector3.Distance(wallEndPos, fromTower.transform.position)));
        tempPlaceBuildingCost.SetWood((int)(fromTower.GetWallPrefab.GetStats.cost.GetWood * Vector3.Distance(wallEndPos, fromTower.transform.position)));
        tempPlaceBuildingCost.SetStone((int)(fromTower.GetWallPrefab.GetStats.cost.GetStone * Vector3.Distance(wallEndPos, fromTower.transform.position)));

        // manage wall visual look
        Vector3 wallLookDir = wallEndPos - fromTower.transform.position;
        wallLookDir.y = 0f;

        wallBuildingVisual.transform.rotation = Quaternion.LookRotation(wallLookDir, Vector3.up);
        wallBuildingVisual.transform.localScale = new Vector3(0.5f, 1f, (wallEndPos - fromTower.transform.position).magnitude);
        wallBuildingVisual.transform.position = (wallEndPos + fromTower.transform.position + new Vector3 (0f, 0.5f, 0f)) / 2f;

        
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
        {
            if (canPlaceWall)
            {
                // place wall
                PlayerResourceManager.PlayerResourceAmounts[identifier.GetPlayerID].SubtractResoruces(tempPlaceBuildingCost);

                fromTower.PlaceWalls(hoveringBuilding.GetComponent<Tower>());
                fromTower = null;
            }
            else
            {
                fromTower = null;
            }
        }

        // cancel wall placement
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
        {
            fromTower = null;
        }
    }

    private void DisableAllIcons()
    {
        for (int i = 0; i < allIcons.Length; i++)
        {
            allIcons[i].gameObject.SetActive(false);
        }
    }

    private void CheckForHoveringBuilding ()
    {
        Building[] closestBuildings = PlayerHolder.GetBuildings(identifier.GetPlayerID).
            Where(building => building != null &&  // building isn't null
            building.GetStats.buildingType != BuyIcons.Building_CASTLE && // building isn't castle
            (new Vector3 (building.transform.position.x, 0f, building.transform.position.z) - new Vector3(transform.position.x, 0f, transform.position.z)).sqrMagnitude < building.GetStats.interactionRadius * building.GetStats.interactionRadius) // building is in range
            .ToArray();

        if (closestBuildings.Length > 0)
        {
            hoveringBuilding = closestBuildings.OrderBy(building => (building.transform.position - transform.position).sqrMagnitude).ToArray()[0];

            // step away from last building
            if (lastHoveringBuilding != hoveringBuilding)
            {
                if (lastHoveringBuilding)
                    lastHoveringBuilding.PlayerHover(false);
                lastHoveringBuilding = hoveringBuilding;
            }

            hoveringBuilding.PlayerHover(true);
        }
        else
        {
            // step away from building
            if (lastHoveringBuilding)
                lastHoveringBuilding.PlayerHover(false);

            hoveringBuilding = null;
        }

            
    }

    private void HandleBuildingRallyPoint()
    {
        if (placingRallyPoint || placingAllRallyPoints)
        {

            // place rally point or cancel
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
                PlaceRallyPoint();
            else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown (PlayerInput.GetInputBack) ||
                PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelectUnits))
                CancelRallyPoint();
        }

        // place all rally points
        if (PlayerInput.GetPlayers [identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadDown))
        {
            placingAllRallyPoints = true;
        }
    }

    public void ActivateBuildingRallyPoint ()
    {
        rallyBuilding = hoveringBuilding;
        placingRallyPoint = true;
    }
    private void CancelRallyPoint ()
    {
        placingRallyPoint = false;
        placingAllRallyPoints = false;
    }
    private void PlaceRallyPoint ()
    {
        if (placingAllRallyPoints)
        {
            // visual for rally point placing
            GameObject rallyPointEffectInstance = Instantiate(rallyPointPlaceEffect, transform.position + new Vector3(0f, -0.5f, 0f), Quaternion.identity);
            Destroy(rallyPointEffectInstance, 3f);

            // new list for already set building types
            List<BuyIcons> alreadySetTypes = new List<BuyIcons>(0);
            foreach (Building building in PlayerHolder.GetBuildings(identifier.GetPlayerID))
            {
                // skip the building if there's no unit to rally
                if (building.GetStats.unit == null)
                    continue;

                // skip the building if we already checked it
                if (alreadySetTypes.Contains(building.GetStats.buildingType))
                {
                    building.SetMainSpawnBuilding(false);
                    continue;
                }


                alreadySetTypes.Add(building.GetStats.buildingType);
                building.SetRallyPoint(transform.position + new Vector3 (0f, -0.75f, 0f));
                building.SetMainSpawnBuilding(true);
            }

            placingAllRallyPoints = false;
            return;
        }


        // visual for rally point placing
        GameObject rallyPointPlaceInstance = Instantiate(rallyPointPlaceEffect, transform.position + new Vector3 (0f, -0.5f, 0f), Quaternion.identity);
        Destroy(rallyPointPlaceInstance, 3f);

        rallyBuilding.SetMainSpawnBuilding(true);

        Building[] buildingsWithType = PlayerHolder.GetBuildings(identifier.GetPlayerID).Where(building => building.GetStats.buildingType == rallyBuilding.GetStats.buildingType).ToArray();
        for (int i = 0; i < buildingsWithType.Length; i++)
        {
            buildingsWithType[i].SetRallyPoint(transform.position);
            if (buildingsWithType[i] != rallyBuilding)
                buildingsWithType[i].SetMainSpawnBuilding(false);
        }

        rallyBuilding = null;
        placingRallyPoint = false;
    }

    private void HandlePlacementUiVisual ()
    {
        placingUiVisualObject.SetActive(placingRallyPoint || placingAllRallyPoints || isPlacingBuilding || fromTower != null);
        //placingUiVisualObject.transform.localPosition = PlayerHolder.WorldToCanvasLocalPoint(transform.position + placingUiVisualOffset, identifier.GetPlayerID).GetValueOrDefault(Vector2.zero);

        if (placingRallyPoint || placingAllRallyPoints)
        {
            placingUiVisual.sprite = BuyIconSpriteManager.GetTypeOfIcon(BuyIcons.BuildingRallyPoint);
        }else if (isPlacingBuilding)
        {
            placingUiVisual.sprite = BuyIconSpriteManager.GetTypeOfIcon(aboutToPlaceBuilding.GetStats.buildingType);
        }else if (fromTower != null)
        {
            placingUiVisual.sprite = BuyIconSpriteManager.GetTypeOfIcon(BuyIcons.BuildWallNoDoor);
        }
    }
}
