using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (Identifier))]
public class PlayerBuilding : MonoBehaviour
{
    [SerializeField] private GameObject cycleMenu;
    [SerializeField] private Vector2 cycleMenuOffset;
    [SerializeField] private Transform uiLayoutGroup;
    [SerializeField] private float menuCycleSpeed = 5f;

    private UnitSelection unitSelection;
    private Identifier identifier;
    private Building hoveringBuilding;
    private Camera mainCam;

    private bool cycleMenuIsOpen = false;
    private Transform[] uiIcons;
    private int selectedIconIndex = 0;

    public bool GetHasMenuOpen => cycleMenuIsOpen;

    private void Awake()
    {
        identifier = GetComponent<Identifier> ();
        unitSelection = GetComponent<UnitSelection>();

        uiIcons = new Transform[uiLayoutGroup.childCount];
        for (int i = 0; i < uiLayoutGroup.childCount; i++)
        {
            uiIcons[i] = uiLayoutGroup.GetChild(i).transform;
        }

        mainCam = Camera.main;
    }

    private void Update()
    {
        // TODO if (building menu open / building troop buy menu open)

        HandleOpeningCycleMenu();

        ManageSelectedIcon();
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
        uiLayoutGroup.transform.localPosition = Vector3.Lerp(uiLayoutGroup.transform.localPosition, -uiIcons[selectedIconIndex].localPosition, Time.deltaTime * menuCycleSpeed);
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
        cycleMenu.SetActive(cycleMenuIsOpen);
        PlayerInput.SetPlayerIsInMenu(identifier.GetPlayerID, cycleMenuIsOpen);

        // toggle cycle menu
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputToggleCycleMenu) && !hoveringBuilding)
        {
            // close cycle menu
            if (cycleMenuIsOpen)
            {
                cycleMenuIsOpen = false;
                return;
            }

            // if we're standing close enough to a building to build
            Building closestBuilding = PlayerHolder.GetBuildings(identifier.GetPlayerID).OrderBy(building => (building.transform.position - transform.position).sqrMagnitude).ToArray()[0];

            if ((closestBuilding.transform.position - transform.position).sqrMagnitude < closestBuilding.GetBuildRadius * closestBuilding.GetBuildRadius)
            {
                // if we're in range of a building
                // open build menu
                cycleMenu.transform.position = mainCam.WorldToScreenPoint(transform.position + (Vector3)cycleMenuOffset);
                cycleMenuIsOpen = true;
                unitSelection.DeselectUnits();
            }

        }

        if (cycleMenuIsOpen)
        {
            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
                cycleMenuIsOpen = false;

            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect))
            {
                // TODO build building
                cycleMenuIsOpen = false;
            }
        }
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
