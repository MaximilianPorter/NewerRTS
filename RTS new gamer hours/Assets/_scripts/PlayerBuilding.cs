using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private GameObject buttonMenu;
    [SerializeField] private Vector2 buttonMenuOffset;
    [SerializeField] private Transform buttonUiLayoutGroup;
    [SerializeField] private float menuCycleSpeed = 10f;
    [SerializeField] private Animator selectAnim;
    [SerializeField] private Transform queuedUnitsLayoutGroup;
    [SerializeField] private Image costAreaLayout;

    [Header("Building")]
    [SerializeField] private BuyIconUI[] initialIcons;
    [SerializeField] private Transform placeBuildingRallyVisual;
    [SerializeField] private GameObject rallyPointPlaceEffect;
    [SerializeField] private Vector3 rallyVisualOffset = new Vector3(0, 10f, 0f);

    [Header("Units")]
    [SerializeField] private BuyIconUI cancelUnits;

    #region private variables    

    private bool placingRallyPoint = false;
    private BuyIcons rallyBuildingType = BuyIcons.NONE;

    private Identifier identifier;
    private UnitSelection unitSelection;
    private Building lastHoveringBuilding;
    private Building hoveringBuilding; // building that is being stood on

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
        DisableAllIcons();
        allQueuedUnits = queuedUnitsLayoutGroup.GetComponentsInChildren<QueuedUpUnitUi>();

        costResourceTransforms = new Transform[costAreaLayout.transform.childCount];
        for (int i = 0; i < costAreaLayout.transform.childCount; i++)
        {
            costResourceTransforms[i] = costAreaLayout.transform.GetChild(i);
        }
        costTexts = costAreaLayout.GetComponentsInChildren<TMP_Text>();
    }

    private void Update()
    {
        anyMenuOpen = buildingMenuIsOpen || unitMenuIsOpen;

        if (anyMenuOpen)
            ManageSelectedIcon();

        HandleOpeningCycleMenu();
        StartCoroutine(HandleCycleMenuOpen());

        CheckForHoveringBuilding();

        SpawnQueuedUnits();

        HandleBuildingRallyPoint();
    }

    private void ManageSelectedIcon ()
    {
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight))
        {
            IncreaseIconIndex();
        }
        else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft))
        {
            DecreaseIconIndex();
        }
        buttonUiLayoutGroup.transform.localPosition = Vector3.Lerp(buttonUiLayoutGroup.transform.localPosition, -allIcons[selectedIconIndex].transform.localPosition, Time.deltaTime * menuCycleSpeed);

        // fade cost ui if the button isn't affordable
        costAreaLayout.color = new Color(costAreaLayout.color.r, costAreaLayout.color.g, costAreaLayout.color.b,
            allIcons[selectedIconIndex].GetIsAffordable ? 1f : 0.5f);

        // adjust cost ui display of selected button
        ResourceAmount cost = allIcons[selectedIconIndex].GetCost;
        costResourceTransforms[0].gameObject.SetActive(cost.GetFood > 0);
        costResourceTransforms[1].gameObject.SetActive(cost.GetWood > 0);
        costResourceTransforms[2].gameObject.SetActive(cost.GetStone > 0);
        costAreaLayout.gameObject.SetActive(cost.GetFood > 0 || cost.GetWood > 0 || cost.GetStone > 0);
        costTexts[0].text = cost.GetFood.ToString();
        costTexts[1].text = cost.GetWood.ToString();
        costTexts[2].text = cost.GetStone.ToString();
    }

    private void IncreaseIconIndex ()
    {
        // if the gameobject is active and we can increase
        if (selectedIconIndex + 1 < allIcons.Length)
        {
            for (int i = 0; i < allIcons.Length; i++)
            {
                if (selectedIconIndex + 1 < allIcons.Length)
                    selectedIconIndex++;
                else
                    selectedIconIndex = 0;

                if (allIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedIconIndex = 0;
            for (int i = 0; i < allIcons.Length; i++)
            {
                if (allIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }

                selectedIconIndex++;
            }
        }
    }
    private void DecreaseIconIndex()
    {
        // if the gameobject is active and we can decrease
        if (selectedIconIndex - 1 >= 0)
        {
            for (int i = 0; i < allIcons.Length; i++)
            {
                if (selectedIconIndex - 1 >= 0)
                    selectedIconIndex--;
                else
                    selectedIconIndex = allIcons.Length - 1;

                if (allIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedIconIndex = allIcons.Length - 1;

            for (int i = 0; i < allIcons.Length; i++)
            {
                if (allIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }

                selectedIconIndex--;
            }

        }
    }

    private void HandleOpeningCycleMenu ()
    {
        // don't open menu if we're cycleing through formations
        if (!PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputRallyTroops))
        {
            buttonMenu.SetActive(anyMenuOpen);
            PlayerInput.SetPlayerIsInMenu(identifier.GetPlayerID, buildingMenuIsOpen);

            HandleBuildMenu();
            HandleUnitMenu();
        }
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

                if ((!inRangeOfBuilding || hoveringBuilding) && !clickedOnBuilding)
                {
                    CloseMenu();
                }

                if (clickedOnBuilding && !hoveringBuilding)
                    CloseMenu();
            }



            buttonMenu.transform.position = playerCam.WorldToScreenPoint(transform.position + (Vector3)buttonMenuOffset);

            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
            {
                yield return new WaitForSeconds(0.05f);

                // if we have the resources, click button and close menu
                if (allIcons[selectedIconIndex].TryClickButton())
                {
                    // don't close menu for units, we want to select it a bunch of times maybe
                    if (!unitMenuIsOpen)
                        CloseMenu();

                    selectAnim.SetTrigger("Select");
                }
                else
                {
                    // TODO make some sort of red flash indicating that we don't have the resources
                }
            }
        }
    }

    private void HandleUnitMenu ()
    {
        if (!anyMenuOpen)
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
        if (!anyMenuOpen)
        {
            // build new building
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu) && !hoveringBuilding)
            {
                // if any buildings are close enough to player
                bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);
                if (inRangeOfBuilding)
                {
                    // if we're in range of a building
                    // open build menu
                    buildingMenuIsOpen = true;

                    selectedIconIndex = allIcons.ToList().IndexOf(initialIcons[0]);
                    // turn on correct menu buttons
                    for (int i = 0; i < initialIcons.Length; i++)
                    {
                        initialIcons[i].gameObject.SetActive(true);
                    }

                    return;
                }
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
                        allIcons[i].gameObject.SetActive(true);
                    }
                }
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
                // use last building for stats and spawning
                Building activeBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).
                    LastOrDefault(building => building.GetStats.unitType == activeQueue.Peek());

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
            //ResourceAmount cost = allIcons.FirstOrDefault(icon => icon.GetButtonType == typesOfUnitsToSpawn[i].Peek().GetStats.unitType).GetCost;

            //// refunt cost of unit
            //PlayerResourceManager.Food[identifier.GetPlayerID] += cost.GetFood * typesOfUnitsToSpawn[i].Count;
            //PlayerResourceManager.Wood[identifier.GetPlayerID] += cost.GetWood * typesOfUnitsToSpawn[i].Count;
            //PlayerResourceManager.Stone[identifier.GetPlayerID] += cost.GetStone * typesOfUnitsToSpawn[i].Count;
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
    public void BuildBuilding (Building building)
    {
        // place building
        Vector3 buildingPos = hoveringBuilding ? hoveringBuilding.transform.position : new Vector3(transform.position.x, 0f, transform.position.z);
        Identifier placedBuildingIdentity = Instantiate(building.gameObject, buildingPos, Quaternion.identity).GetComponent<Identifier>();

        // set team and player ID of building
        placedBuildingIdentity.SetPlayerID(identifier.GetPlayerID);
        placedBuildingIdentity.SetTeamID(identifier.GetTeamID);
    }

    // used by UI buttons
    public void UpgradeBuilding (Building building)
    {
        hoveringBuilding.DeleteBuilding();

        BuildBuilding(building);
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
            Where(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.interactionRadius * building.GetStats.interactionRadius).ToArray();
            //OrderBy(building => (building.transform.position - transform.position).sqrMagnitude).ToArray();

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
        placeBuildingRallyVisual.gameObject.SetActive(placingRallyPoint);
        if (placingRallyPoint)
        {
            placeBuildingRallyVisual.position = playerCam.WorldToScreenPoint(transform.position + rallyVisualOffset);

            // place rally point or cancel
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
                PlaceRallyPoint();
            else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown (PlayerInput.GetInputBack) ||
                PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelectUnits))
                CancelRallyPoint();
        }
    }

    public void ActivateBuildingRallyPoint ()
    {
        rallyBuildingType = hoveringBuilding.GetStats.buildingType;
        placingRallyPoint = true;
    }
    private void CancelRallyPoint ()
    {
        placingRallyPoint = false;
    }
    private void PlaceRallyPoint ()
    {
        // visual for rally point placing
        GameObject rallyPointPlaceInstance = Instantiate(rallyPointPlaceEffect, transform.position + new Vector3 (0f, -0.5f, 0f), Quaternion.identity);
        Destroy(rallyPointPlaceInstance, 3f);

        Building[] buildingsWithType = PlayerHolder.GetBuildings(identifier.GetPlayerID).Where(building => building.GetStats.buildingType == rallyBuildingType).ToArray();
        for (int i = 0; i < buildingsWithType.Length; i++)
        {
            buildingsWithType[i].SetRallyPoint(transform.position);
        }
        placingRallyPoint = false;
    }
}
