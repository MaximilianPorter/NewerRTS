using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private Vector2 buildingMenuOffset;
    [SerializeField] private Transform buildingUiLayoutGroup;
    [SerializeField] private float menuCycleSpeed = 5f;
    [SerializeField] private Building[] buildings;


    private Identifier identifier;
    private UnitSelection unitSelection;
    private Building hoveringBuilding; // building that is being stood on

    private bool buildingMenuIsOpen = false;
    private BuildingIconUi[] uiIcons;
    private int selectedIconIndex = 0;

    public bool GetHasMenuOpen => buildingMenuIsOpen;

    private void Awake()
    {
        identifier = GetComponent<Identifier> ();
        unitSelection = GetComponent<UnitSelection>();

        // find ui icons in the layout group
        uiIcons = buildingUiLayoutGroup.GetComponentsInChildren<BuildingIconUi>();
    }

    private void Update()
    {
        // TODO if (building menu open / building troop buy menu open)

        HandleOpeningCycleMenu();

        ManageSelectedIcon();

        ChangeColorOfBuildingIcon();
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
        buildingUiLayoutGroup.transform.localPosition = Vector3.Lerp(buildingUiLayoutGroup.transform.localPosition, -uiIcons[selectedIconIndex].transform.localPosition, Time.deltaTime * menuCycleSpeed);
    }

    private void ChangeColorOfBuildingIcon ()
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            bool hasWood = PlayerResourceManager.Wood[identifier.GetPlayerID] >= buildings[i].GetStats.woodCost;
            bool hasStone = PlayerResourceManager.Stone[identifier.GetPlayerID] >= buildings[i].GetStats.stoneCost;

            uiIcons[i].SetAffordable(hasWood && hasStone);
        }
    }

    private void IncreaseIconIndex ()
    {
        // if the gameobject is active and we can increase
        if (selectedIconIndex + 1 < uiIcons.Length)
        {
            for (int i = 0; i < uiIcons.Length; i++)
            {
                if (selectedIconIndex + 1 < uiIcons.Length)
                    selectedIconIndex++;
                else
                    selectedIconIndex = 0;

                if (uiIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedIconIndex = 0;
            for (int i = 0; i < uiIcons.Length; i++)
            {
                if (uiIcons[selectedIconIndex].gameObject.activeSelf)
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
            for (int i = 0; i < uiIcons.Length; i++)
            {
                if (selectedIconIndex - 1 >= 0)
                    selectedIconIndex--;
                else
                    selectedIconIndex = uiIcons.Length - 1;

                if (uiIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedIconIndex = uiIcons.Length - 1;

            for (int i = 0; i < uiIcons.Length; i++)
            {
                if (uiIcons[selectedIconIndex].gameObject.activeSelf)
                {
                    break;
                }

                selectedIconIndex--;
            }

        }
    }

    private void HandleOpeningCycleMenu ()
    {
        buildingMenu.SetActive(buildingMenuIsOpen);
        PlayerInput.SetPlayerIsInMenu(identifier.GetPlayerID, buildingMenuIsOpen);

        // toggle building menu
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputToggleCycleMenu) && !hoveringBuilding)
        {
            // close building menu
            if (buildingMenuIsOpen)
            {
                CloseBuildMenu();
                return;
            }

            // if we're standing close enough to a building to build another building
            Building closestBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).OrderBy(building => (building.transform.position - transform.position).sqrMagnitude).ToArray()[0];

            if ((closestBuilding.transform.position - transform.position).sqrMagnitude < closestBuilding.GetStats.buildRadius * closestBuilding.GetStats.buildRadius)
            {
                // if we're in range of a building
                // open build menu
                buildingMenuIsOpen = true;
                unitSelection.DeselectUnits();
            }

        }


        // BUILDING MENU OPEN
        if (buildingMenuIsOpen)
        {
            // close build menu if we're ever too far away || standing on building
            Building closestBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).OrderBy(building => (building.transform.position - transform.position).sqrMagnitude).ToArray()[0];
            if ((closestBuilding.transform.position - transform.position).sqrMagnitude >= closestBuilding.GetStats.buildRadius * closestBuilding.GetStats.buildRadius
                || hoveringBuilding)
            {
                CloseBuildMenu();
            }


            buildingMenu.transform.position = playerCam.WorldToScreenPoint(transform.position + (Vector3)buildingMenuOffset);

            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
                CloseBuildMenu();

            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
            {
                bool hasWood = PlayerResourceManager.Wood[identifier.GetPlayerID] >= buildings[selectedIconIndex].GetStats.woodCost;
                bool hasStone = PlayerResourceManager.Stone[identifier.GetPlayerID] >= buildings[selectedIconIndex].GetStats.stoneCost;

                // if we have the resources
                if (hasWood && hasStone)
                {
                    BuildBuilding();
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
        selectedIconIndex = 0;
    }
    private void BuildBuilding ()
    {
        // spend resources
        PlayerResourceManager.SubtractResource(identifier.GetPlayerID, ref PlayerResourceManager.Wood, buildings[selectedIconIndex].GetStats.woodCost);
        PlayerResourceManager.SubtractResource(identifier.GetPlayerID, ref PlayerResourceManager.Stone, buildings[selectedIconIndex].GetStats.stoneCost);

        // place building
        Vector3 buildingPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Identifier placedBuildingIdentity = Instantiate(buildings[selectedIconIndex].gameObject, buildingPos, Quaternion.identity).GetComponent<Identifier>();

        // set team and player ID of building
        placedBuildingIdentity.SetPlayerID(identifier.GetPlayerID);
        placedBuildingIdentity.SetTeamID(identifier.GetTeamID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hoveringBuilding == null && other.TryGetComponent (out Building building) && other.GetComponent<Identifier>().GetPlayerID == identifier.GetPlayerID)
        {
            building.PlayerHover(true);
            hoveringBuilding = building;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (hoveringBuilding && hoveringBuilding == other.GetComponent<Building>())
        {
            hoveringBuilding.PlayerHover(false);
            hoveringBuilding = null;
        }
    }
}
