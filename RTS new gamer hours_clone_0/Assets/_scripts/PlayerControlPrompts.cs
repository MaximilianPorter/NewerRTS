using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControlPrompts : MonoBehaviour
{
    [SerializeField] private PlayerBuilding playerBuilding;
    [SerializeField] private UnitSelection unitSelection;

    [Header("Bumpers")]
    [SerializeField] private GameObject leftBumperGO;
    [SerializeField] private GameObject rightBumperGO;
    [SerializeField] private TMP_Text leftBumperText;
    [SerializeField] private TMP_Text rightBumperText;

    [Header("AB")]
    [SerializeField] private GameObject unitSelectionPrompts;
    [SerializeField] private GameObject preUnitsSelectedPrompts;

    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        bool hasTroops = PlayerHolder.GetUnits(identifier.GetPlayerID).Count > 0;
        unitSelectionPrompts.SetActive(unitSelection.GetHasTroopsSelected && hasTroops);
        preUnitsSelectedPrompts.SetActive(!unitSelection.GetHasTroopsSelected && hasTroops);

        if (unitSelection.GetIsRallyingTroopsOnPattern)
        {
            SetBumperText("CHANGE FORMATION", "CHANGE FORMATION");
        }
        else if (unitSelection.GetHasTroopsSelected)
        {
            SetBumperText("CYCLE UNIT TYPE", "CYCLE UNIT TYPE");
        }
        else if (!playerBuilding.GetHasMenuOpen && playerBuilding.GetHoverBuilding != null)
        {
            SetBumperText("UNIT MENU", "UPGRADE BUILDING");
        }
        else if(!playerBuilding.GetHasMenuOpen)
        {
            SetBumperText("UNIT MENU", "BUILD MENU");
        }
        else if (playerBuilding.GetHasMenuOpen)
        {
            SetBumperText("CYCLE LEFT", "CYCLE RIGHT");
        }
    }

    private void SetBumperText (string left, string right)
    {
        leftBumperText.text = left;
        rightBumperText.text = right;
    }
}
