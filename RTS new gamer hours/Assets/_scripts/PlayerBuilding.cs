using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private GameObject buttonMenu;
    [SerializeField] private Vector2 buttonMenuOffset;
    [SerializeField] private Transform buttonUiLayoutGroup;
    [SerializeField] private float menuCycleSpeed = 10f;
    [SerializeField] private BuyIconUI[] initialIcons;



    private Identifier identifier;
    private UnitSelection unitSelection;
    private Building lastHoveringBuilding;
    private Building hoveringBuilding; // building that is being stood on

    private bool clickedOnBuilding = false;
    private bool buildingMenuIsOpen = false;
    private bool unitMenuIsOpen = false;
    private BuyIconUI[] allIcons;
    private int selectedIconIndex = 0;

    public BuyIconUI GetSelectedUiButton => allIcons[selectedIconIndex];
    public bool GetHasMenuOpen => buildingMenuIsOpen;

    [Serializable]
    private struct UiBuilding
    {
        public Building building;
        public BuyIconUI icon;
    }


    private void Awake()
    {
        identifier = GetComponent<Identifier> ();
        unitSelection = GetComponent<UnitSelection>();

        // find ui icons in the layout group
        allIcons = buttonUiLayoutGroup.GetComponentsInChildren<BuyIconUI>();
        DisableAllIcons();
    }

    private void Update()
    {
        // TODO unit buy menu

        if (buildingMenuIsOpen)
            ManageSelectedIcon();

        HandleOpeningCycleMenu();

        CheckForHoveringBuilding();
    }

    private void ManageSelectedIcon ()
    {
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight))
        {
            IncreaseIconIndex();
        }
        else if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft))
        {
            DecreaseIconIndex();
        }
        buttonUiLayoutGroup.transform.localPosition = Vector3.Lerp(buttonUiLayoutGroup.transform.localPosition, -allIcons[selectedIconIndex].transform.localPosition, Time.deltaTime * menuCycleSpeed);
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
        buttonMenu.SetActive(buildingMenuIsOpen);
        PlayerInput.SetPlayerIsInMenu(identifier.GetPlayerID, buildingMenuIsOpen);

        // turn on building menu on button down
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu) && !hoveringBuilding && !buildingMenuIsOpen)
        {
            // if any buildings are close enough to player
            bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);
            if (inRangeOfBuilding)
            {
                // if we're in range of a building
                // open build menu
                buildingMenuIsOpen = true;
                unitSelection.DeselectUnits();

                selectedIconIndex = allIcons.ToList().IndexOf(initialIcons[0]);
                // turn on correct menu buttons
                for (int i = 0; i < initialIcons.Length; i++)
                {
                    initialIcons[i].gameObject.SetActive(true);
                }

                return;
            }
        }


        // turn on building menu for selected building
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputOpenBuildMenu) && hoveringBuilding && !buildingMenuIsOpen
            && hoveringBuilding.GetStats.subsequentUpgrades.Length > 0)
        {
            clickedOnBuilding = true;
            buildingMenuIsOpen = true;
            unitSelection.DeselectUnits();

            bool runOnce = false;
            for (int i = 0; i < allIcons.Length; i++)
            {
                // if any icon type is the same as the hovering building subsequent upgrades
                if (hoveringBuilding.GetStats.subsequentUpgrades.Any (upgrade => upgrade == allIcons[i].GetButtonType))
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


        // BUILDING MENU OPEN
        if (buildingMenuIsOpen)
        {
            // click the back button
            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
                CloseBuildMenu();

            // close build menu if we're ever too far away || standing on building
            bool inRangeOfBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).Any(building => (building.transform.position - transform.position).sqrMagnitude < building.GetStats.buildRadius * building.GetStats.buildRadius);
            
            if ((!inRangeOfBuilding || hoveringBuilding) && !clickedOnBuilding)
            {
                CloseBuildMenu();
            }



            buttonMenu.transform.position = playerCam.WorldToScreenPoint(transform.position + (Vector3)buttonMenuOffset);

            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
            {
                // if we have the resources, click button and close menu
                if (allIcons[selectedIconIndex].TryClickButton())
                {
                    CloseBuildMenu();
                }
                else
                {
                    // TODO make some sort of red flash indicating that we don't have the resources
                }
            }
        }
    }

    private void CloseBuildMenu ()
    {
        buildingMenuIsOpen = false;
        clickedOnBuilding = false;
        DisableAllIcons();
    }



    public void BuildBuilding (Building building)
    {
        // place building
        Vector3 buildingPos = hoveringBuilding ? hoveringBuilding.transform.position : new Vector3(transform.position.x, 0f, transform.position.z);
        Identifier placedBuildingIdentity = Instantiate(building.gameObject, buildingPos, Quaternion.identity).GetComponent<Identifier>();

        // set team and player ID of building
        placedBuildingIdentity.SetPlayerID(identifier.GetPlayerID);
        placedBuildingIdentity.SetTeamID(identifier.GetTeamID);
    }

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
}
