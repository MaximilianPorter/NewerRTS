using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class UnitSelection : MonoBehaviour
{
    [SerializeField] private LayerMask unitMask;
    [SerializeField] private Transform unitSelectionVisual;
    [SerializeField] private GameObject rallyTroopsEffectPrefab;
    [SerializeField] private float maxSelectionRadius = 10f;
    [SerializeField] private float radiusIncreaseSpeed = 4f;
    [SerializeField] private Transform selectedUnitsUiLayout;

    private UnitActions[] selectedUnits = new UnitActions[0];
    private float currentSelectionRadius = 0f;
    private int patternIndex = -1;
    private readonly int totalPatterns = 2;
    private Identifier identifier;

    private int selectedUnitIndex = -1;
    private QueuedUpUnitUi[] selectedUnitsUi;
    private UnitActions[] tempSelectedUnits = new UnitActions[0];

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        selectedUnitsUi = selectedUnitsUiLayout.GetComponentsInChildren <QueuedUpUnitUi>();
        for (int i = 0; i < selectedUnitsUi.Length; i++)
        {
            selectedUnitsUi[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // if the player is in a menu, we don't do unit selection updates
        if (PlayerInput.GetPlayerIsInMenu(identifier.GetPlayerID))
            return;

        // if the player has no troops, we don't do unit selection updates
        if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count <= 0)
            return;

        // deselect units when you're selecting new ones
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonSinglePressDown(PlayerInput.GetInputSelectUnits))
        {
            DeselectUnits(); 
        }

        // select some units
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonSinglePressHold(PlayerInput.GetInputSelectUnits))
        {
            // increase radius and select units within
            unitSelectionVisual.gameObject.SetActive(true);
            unitSelectionVisual.localScale = new Vector3(currentSelectionRadius, currentSelectionRadius, 5f);
            currentSelectionRadius = Mathf.Clamp(currentSelectionRadius + Time.deltaTime * radiusIncreaseSpeed, 0f, maxSelectionRadius);
            SelectNearbyUnits();
        }
        else
        {
            // reset visuals and counter for radius size
            currentSelectionRadius = 0f;
            unitSelectionVisual.gameObject.SetActive(false);
        }

        // handle unit visuals for units selected (little dot over the unit)
        if (selectedUnits.Length > 0)
        {
            // this loop is just for the visuals of units being selected
            foreach (UnitActions unit in PlayerHolder.GetUnits(identifier.GetPlayerID))
            {
                if (selectedUnits.Contains(unit) && tempSelectedUnits.Length <= 0)
                    unit.SetIsSelected(true);
                else if (tempSelectedUnits.Length > 0 && tempSelectedUnits.Contains(unit))
                    unit.SetIsSelected(true);
                else
                    unit.SetIsSelected(false);
            }
        }
        
        // select all units
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDoublePressDown(PlayerInput.GetInputSelectUnits))
        {
            DeselectUnits();
            selectedUnits = PlayerHolder.GetUnits(identifier.GetPlayerID).ToArray();
            for (int i = 0; i < PlayerHolder.GetUnits (identifier.GetPlayerID).Count; i++)
            {
                PlayerHolder.GetUnits(identifier.GetPlayerID)[i].SetIsSelected(true);

            }
        }



        // deselect all units
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDeselectUnits))
        {
            DeselectUnits();
        }


        RallyTroopsOnPattern(tempSelectedUnits.Length > 0 ? tempSelectedUnits : selectedUnits);

        HandleSelectedUnitTypesUI();
        
    }

    private void RallyTroopsOnPattern(UnitActions[] unitsToRally)
    {
        if (selectedUnits.Length <= 0)
            return;


        // if you hold for a short time
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonShortPressDown(PlayerInput.GetInputRallyTroops))
            patternIndex = 0;
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonShortPress(PlayerInput.GetInputRallyTroops))
        {
            // change pattern number
            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight))
                IncreasePatternIndex();
            else if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft))
                DecreasePatternIndex();

            if (patternIndex == 0)
            {
                // pattern box
                int boxWidth = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float)unitsToRally.Length)), 0, 20);
                for (int i = 0; i < unitsToRally.Length; i++)
                {
                    if (unitsToRally[i] == null)
                        continue;

                    int row = Mathf.FloorToInt(i / boxWidth);
                    int column = i % boxWidth;
                    Vector3 boxPos = transform.forward * (row + 1f) + transform.right * (column - boxWidth / 2f);
                    unitsToRally[i].GetOrderingObject.SetActive(true);
                    unitsToRally[i].GetOrderingObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    unitsToRally[i].GetOrderingObject.transform.position = transform.position + boxPos;
                }
            }else if (patternIndex == 1)
            {
                // pattern long line
                int boxWidth = Mathf.Clamp (unitsToRally.Length, 0, 20);
                for (int i = 0; i < unitsToRally.Length; i++)
                {
                    if (unitsToRally[i] == null)
                        continue;

                    int row = Mathf.FloorToInt(i / boxWidth);
                    int column = i % boxWidth;
                    Vector3 boxPos = transform.forward * (row + 1f) + transform.right * (column - boxWidth / 2f);
                    unitsToRally[i].GetOrderingObject.SetActive(true);
                    unitsToRally[i].GetOrderingObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    unitsToRally[i].GetOrderingObject.transform.position = transform.position + boxPos;
                }
            }

            if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
            {
                DeselectUnits();
            }
        }


        // set the move target of each unit to whatever the final ordering object pos was based on the pattern
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonUp(PlayerInput.GetInputRallyTroops))
        {
            GameObject rallyTroopsEffectInstance = Instantiate(rallyTroopsEffectPrefab, transform.position + new Vector3(0f, -1f, 0f), Quaternion.identity);
            Destroy(rallyTroopsEffectInstance, 3f);

            for (int i = 0; i < unitsToRally.Length; i++)
            {
                if (unitsToRally[i] == null)
                    continue;

                unitsToRally[i].GetOrderingObject.SetActive(false);

                // if we never chose a pattern, just put the unit near the player
                if (patternIndex == -1)
                {
                    float maxDistAway = Mathf.Lerp(0.5f, 3f, unitsToRally.Length / 10);
                    unitsToRally[i].GetOrderingObject.transform.position = transform.position + new Vector3(Random.Range(-maxDistAway, maxDistAway), 0f, Random.Range(-maxDistAway, maxDistAway));
                }

                unitsToRally[i].GetMovement.SetMoveTarget(unitsToRally[i].GetOrderingObject.transform.position);
            }
            // reset pattern index
            patternIndex = -1;
        }
    }

    private void IncreasePatternIndex ()
    {
        if (patternIndex + 1 < totalPatterns)
            patternIndex++;
        else
            patternIndex = 0;
    }
    private void DecreasePatternIndex ()
    {
        if (patternIndex - 1 >= 0)
            patternIndex--;
        else
            patternIndex = totalPatterns - 1;
    }



    private void HandleSelectedUnitTypesUI ()
    {
        if (PlayerInput.players[identifier.GetPlayerID].GetButton(PlayerInput.GetInputRallyTroops) || selectedUnitsUi.Length <= 0)
            return;

        for (int i = 0; i < selectedUnitsUi.Length; i++)
        {
            selectedUnitsUi[i].SetDetails(selectedUnits.Count(unit => unit.GetStats.unitType == selectedUnitsUi[i].GetUnitType), 0);
            selectedUnitsUi[i].gameObject.SetActive(selectedUnits.Any(unit => unit.GetStats.unitType == selectedUnitsUi[i].GetUnitType));
        }

        // change pattern number
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadRight))
            IncreaseSelectedUnitIndex();
        else if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadLeft))
            DecreaseSelectedUnitIndex();

        // if the selected unit index was actually changed
        if (selectedUnitIndex >= 0)
        {
            tempSelectedUnits = selectedUnits.Where(unit => unit.GetStats.unitType == selectedUnitsUi[selectedUnitIndex].GetUnitType).ToArray();

            selectedUnitsUi[selectedUnitIndex].SetDetails(selectedUnitsUi[selectedUnitIndex].GetUnitAmt, 1f);
        }
    }

    private void IncreaseSelectedUnitIndex()
    {
        // if the gameobject is active and we can increase
        if (selectedUnitIndex + 1 < selectedUnitsUi.Length)
        {
            for (int i = 0; i < selectedUnitsUi.Length; i++)
            {
                if (selectedUnitIndex + 1 < selectedUnitsUi.Length)
                    selectedUnitIndex++;
                else
                    selectedUnitIndex = 0;

                if (selectedUnitsUi[selectedUnitIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedUnitIndex = 0;
            for (int i = 0; i < selectedUnitsUi.Length; i++)
            {
                if (selectedUnitsUi[selectedUnitIndex].gameObject.activeSelf)
                {
                    break;
                }

                selectedUnitIndex++;
            }
        }
    }
    private void DecreaseSelectedUnitIndex()
    {
        // if the gameobject is active and we can decrease
        if (selectedUnitIndex - 1 >= 0)
        {
            for (int i = 0; i < selectedUnitsUi.Length; i++)
            {
                if (selectedUnitIndex - 1 >= 0)
                    selectedUnitIndex--;
                else
                    selectedUnitIndex = selectedUnitsUi.Length - 1;

                if (selectedUnitsUi[selectedUnitIndex].gameObject.activeSelf)
                {
                    break;
                }
            }
        }
        else
        {
            selectedUnitIndex = selectedUnitsUi.Length - 1;

            for (int i = 0; i < selectedUnitsUi.Length; i++)
            {
                if (selectedUnitsUi[selectedUnitIndex].gameObject.activeSelf)
                {
                    break;
                }

                selectedUnitIndex--;
            }

        }
    }


    private void SelectNearbyUnits()
    {
        selectedUnits = PlayerHolder.GetUnits(identifier.GetPlayerID).Where(unit => (unit.transform.position - transform.position).sqrMagnitude < currentSelectionRadius * currentSelectionRadius).ToArray();
    }

    public void DeselectUnits ()
    {
        selectedUnitIndex = -1;
        tempSelectedUnits = new UnitActions[0];

        for (int i = 0; i < selectedUnits.Length; i++)
        {
            if (selectedUnits[i] == null)
                continue;

            selectedUnits[i].GetOrderingObject.SetActive(false);
            selectedUnits[i].SetIsSelected(false);
        }
        selectedUnits = new UnitActions[0];
    }
}
