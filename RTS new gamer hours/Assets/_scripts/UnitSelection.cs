using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof (Identifier))]
public class UnitSelection : MonoBehaviour
{
    [SerializeField] private Camera playerCam;
    [SerializeField] private LayerMask unitMask;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private DecalProjector unitSelectionVisual;
    [SerializeField] private ParticleSystem rallyTroopsEffect;
    [SerializeField] private float maxSelectionRadius = 10f;
    [SerializeField] private float radiusIncreaseSpeed = 4f;
    [SerializeField] private float patternSpacingMulti = 2f;
    [SerializeField] private Transform selectedUnitsUiLayout;
    [SerializeField] private TMP_Text patternNameText;
    [SerializeField] private UnitMovementType unitMovementType = UnitMovementType.LookNearDestination;
    [SerializeField] private TMP_Text movementTypeText;
    [SerializeField] private TMP_Text movementTypeDescText;

    [Header("Micro")]
    [SerializeField] private Transform microGroupUiLayout;
    private Animator[] microUiObjects;

    private List<UnitActions> selectedUnits = new List<UnitActions>(0);
    private float currentSelectionRadius = 0f;
    private int patternIndex = -1;
    private readonly int totalPatterns = 4;
    private Identifier identifier;

    private int selectedUnitIndex = -1;
    private QueuedUpUnitUi[] selectedUnitsUi;
    private List<UnitActions> tempSelectedUnits = new List<UnitActions>(0);
    private UnitActions[][] microGroups = new UnitActions[4][];
    private Vector3 lastRallyPointWorldPos;
    private float movementTypeTextCounter = 0f;
    private bool isRallyingTroopsOnPattern = false;

    public bool GetHasTroopsSelected => selectedUnits.Count > 0;
    public bool GetIsRallyingTroopsOnPattern => isRallyingTroopsOnPattern;

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

        for (int i = 0; i < microGroups.GetLength(0); i++)
        {
            microGroups[i] = new UnitActions[0];
        }

        microUiObjects = new Animator[microGroupUiLayout.childCount];
        for (int i = 0; i < microGroupUiLayout.childCount; i++)
        {
            microUiObjects[i] = microGroupUiLayout.GetChild(i).GetComponent<Animator>();
        }
        patternNameText.text = "";
        rallyTroopsEffect.Stop();

        unitSelectionVisual.material = PlayerColorManager.GetPlayerProjectorMaterial(identifier.GetPlayerID);
    }

    private void Update()
    {
        // don't do anything if we're paused
        if (PauseGameManager.GetIsPaused)
            return;

        ChangeUnitMovementType();

        // if the player is in a menu, we don't do unit selection updates
        if (PlayerInput.GetPlayerIsInMenu(identifier.GetPlayerID))
            return;

        HandleMircroGroups();

        // handle unit visuals for units selected (little dot over the unit)
        if (selectedUnits.Count > 0)
        {
            // this loop is just for the visuals of units being selected
            foreach (UnitActions unit in PlayerHolder.GetUnits(identifier.GetPlayerID))
            {
                if (selectedUnits.Contains(unit) && tempSelectedUnits.Count <= 0)
                    unit.SetIsSelected(true);
                else if (tempSelectedUnits.Count > 0 && tempSelectedUnits.Contains(unit))
                    unit.SetIsSelected(true);
                else
                    unit.SetIsSelected(false);
            }
        }

        // if the player has no troops, we don't do unit selection updates
        if (PlayerHolder.GetUnits(identifier.GetPlayerID).Count <= 0)
        {
            tempSelectedUnits = new List<UnitActions>(0);
            selectedUnits = new List<UnitActions>(0);
            unitSelectionVisual.gameObject.SetActive(false);
            HandleSelectedUnitTypesUI();
            return;
        }

        // if you're assigning a selection group, don't do anything
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock))
            return;

        // deselect units when you're selecting new ones
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonSinglePressDown(PlayerInput.GetInputSelectUnits))
        {
            DeselectUnits(); 
        }

        // select some units
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonSinglePressHold(PlayerInput.GetInputSelectUnits))
        {
            // increase radius and select units within
            unitSelectionVisual.gameObject.SetActive(true);
            //unitSelectionVisual.localScale = new Vector3(currentSelectionRadius, currentSelectionRadius, 5f);
            currentSelectionRadius = Mathf.Clamp(currentSelectionRadius + Time.deltaTime * radiusIncreaseSpeed, 0f, maxSelectionRadius);
            unitSelectionVisual.size = new Vector3(currentSelectionRadius * 2f, currentSelectionRadius * 2f, unitSelectionVisual.size.z);
            SelectNearbyUnits();
        }
        else
        {
            // reset visuals and counter for radius size
            currentSelectionRadius = 0f;
            unitSelectionVisual.gameObject.SetActive(false);
        }
        
        // select all units
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDoublePressDown(PlayerInput.GetInputSelectUnits))
        {
            DeselectUnits();
            selectedUnits = PlayerHolder.GetUnits(identifier.GetPlayerID).Where (unit => unit.GetIsSelectable).ToList();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].SetIsSelected(true);

            }
        }

        


        // deselect all units
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDeselectUnits))
        {
            DeselectUnits();
        }

        // stop unit movement
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputStopUnitMovement))
        {
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                selectedUnits[i].GetMovement.ResetDestination();
            }
        }



        tempSelectedUnits.RemoveAll(unit => unit == null);
        selectedUnits.RemoveAll(unit => unit == null);

        RallyTroopsOnPattern(tempSelectedUnits.Count > 0 ? tempSelectedUnits : selectedUnits);
        rallyTroopsEffect.transform.position = lastRallyPointWorldPos;

        HandleSelectedUnitTypesUI();
        
    }

    private void RallyTroopsOnPattern(List<UnitActions> unitsToRally)
    {
        if (selectedUnits.Count <= 0)
            return;


        // if you hold for a short time
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonShortPressDown(PlayerInput.GetInputRallyTroops))
            patternIndex = 0;
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonShortPress(PlayerInput.GetInputRallyTroops))
        {
            patternNameText.transform.position = playerCam.WorldToScreenPoint(transform.position + new Vector3(0f, 4f, 0f));
            isRallyingTroopsOnPattern = true;

            // change pattern number
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight))
                IncreasePatternIndex();
            else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft))
                DecreasePatternIndex();

            float avgMoveSpeed = unitsToRally.Sum(unit => unit.GetStats.maxMoveSpeed) / unitsToRally.Count;

            float boxWidth = Mathf.Clamp(unitsToRally.Count, 0, 20);

            if (patternIndex == 1)
                boxWidth = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float)unitsToRally.Count)), 0, 20);

            float totalRows = Mathf.Ceil((float)unitsToRally.Count / (float)boxWidth);
            float lastRowStartIndex = totalRows * boxWidth - boxWidth;
            float remainderInLastRow = totalRows * boxWidth - unitsToRally.Count;

            for (int i = 0; i < unitsToRally.Count; i++)
            {
                if (patternIndex == 0)
                {
                    // pattern offset from current location
                    patternNameText.text = "CURRENT FORMATION";
                    Vector3 avgGroupPos = new Vector3(unitsToRally.Sum(unit => unit.transform.position.x) / unitsToRally.Count,
                        transform.position.y,
                        unitsToRally.Sum(unit => unit.transform.position.z) / unitsToRally.Count);

                    Vector3 dir = unitsToRally[i].transform.position - avgGroupPos;
                    dir.y = 0f;

                    dir = MaxPosWithHit(dir);

                    SetUnitPos(unitsToRally[i], dir, avgMoveSpeed);
                }
                else if (patternIndex == 1)
                {
                    // pattern box
                    patternNameText.text = "BOX";
                    if (unitsToRally[i] == null)
                        continue;

                    float row = Mathf.FloorToInt(i / boxWidth);
                    float column = i % boxWidth;

                    // only apply if unit is in the last row (centering)
                    if (i >= lastRowStartIndex)
                        column += remainderInLastRow / 2f;

                    Vector3 dir =
                        transform.forward * (row + 1f) * patternSpacingMulti +
                        transform.right * (column - boxWidth / 2f) * patternSpacingMulti;

                    dir = MaxPosWithHit(dir);

                    SetUnitPos(unitsToRally[i], dir, unitsToRally[i].GetStats.maxMoveSpeed);
                }
                else if (patternIndex == 2)
                {
                    // pattern long line
                    patternNameText.text = "LINE";
                    if (unitsToRally[i] == null)
                        continue;
                    float row = Mathf.FloorToInt(i / boxWidth);
                    float column = i % boxWidth;

                    // only apply if unit is in the last row (centering)
                    if (i >= lastRowStartIndex)
                        column += remainderInLastRow / 2f;

                    Vector3 dir =
                        transform.forward * (row + 1f) * patternSpacingMulti +
                        transform.right * (column - boxWidth / 2f) * patternSpacingMulti;

                    dir = MaxPosWithHit(dir);

                    SetUnitPos(unitsToRally[i], dir, unitsToRally[i].GetStats.maxMoveSpeed);
                }
                else if (patternIndex == 3)
                {
                    // pattern arrow (basically just a line that's offset by column)
                    patternNameText.text = "ARROW";
                    if (unitsToRally[i] == null)
                        continue;

                    float row = Mathf.FloorToInt(i / boxWidth);
                    float column = i % boxWidth;// + row * Mathf.Sign(i % (boxWidth + row) - boxWidth / 2f);

                    // only apply if unit is in the last row (centering)
                    if (i >= lastRowStartIndex)
                        column += remainderInLastRow / 2f;

                    //column += ;
                    float arrowForwardOffset = Mathf.Abs(column - boxWidth / 2f) * 0.5f;

                    Vector3 dir =
                        transform.forward * (row + 1f + arrowForwardOffset) * patternSpacingMulti +
                        transform.right * (column - boxWidth / 2f) * patternSpacingMulti;

                    dir = MaxPosWithHit(dir);

                    SetUnitPos(unitsToRally[i], dir, unitsToRally[i].GetStats.maxMoveSpeed);
                }
                else
                {
                    patternNameText.text = "";
                }
            }

            //if (patternIndex == 0)
            //{
            //    // pattern offset from current location
            //    patternNameText.text = "CURRENT FORMATION";
            //    Vector3 avgGroupPos = new Vector3(unitsToRally.Sum(unit => unit.transform.position.x)/unitsToRally.Count,
            //        transform.position.y,
            //        unitsToRally.Sum(unit => unit.transform.position.z) / unitsToRally.Count);

            //    for (int i = 0; i < unitsToRally.Count; i++)
            //    {
            //        Vector3 pos = unitsToRally[i].transform.position - avgGroupPos;

            //        SetUnitPos(unitsToRally[i], pos, avgMoveSpeed);
            //    }
            //}
            //else if (patternIndex == 1)
            //{
            //    // pattern box
            //    patternNameText.text = "BOX";
            //    for (int i = 0; i < unitsToRally.Count; i++)
            //    {
            //        if (unitsToRally[i] == null)
            //            continue;

            //        float row = Mathf.FloorToInt(i / boxWidth);
            //        float column = i % boxWidth;

            //        // only apply if unit is in the last row (centering)
            //        if (i >= lastRowStartIndex)
            //            column += remainderInLastRow / 2f;

            //        Vector3 pos =
            //            transform.forward * (row + 1f) * patternSpacingMulti +
            //            transform.right * (column - boxWidth / 2f) * patternSpacingMulti;

            //        bool hitSomethingInBetween = Physics.Raycast(transform.position, pos, out RaycastHit hitSomethingBetweenInfo, pos.magnitude, obstacleMask);
            //        float minMagnitude = hitSomethingInBetween ? hitSomethingBetweenInfo.distance : pos.magnitude;
            //        pos = Vector3.ClampMagnitude(pos, minMagnitude);

            //        SetUnitPos(unitsToRally[i], pos, unitsToRally[i].GetStats.maxMoveSpeed);
            //    }
            //}
            //else if (patternIndex == 2)
            //{
            //    // pattern long line
            //    patternNameText.text = "LINE";
            //    for (int i = 0; i < unitsToRally.Count; i++)
            //    {
            //        if (unitsToRally[i] == null)
            //            continue;
            //        float row = Mathf.FloorToInt(i / boxWidth);
            //        float column = i % boxWidth;

            //        // only apply if unit is in the last row (centering)
            //        if (i >= lastRowStartIndex)
            //            column += remainderInLastRow / 2f;

            //        Vector3 pos =
            //            transform.forward * (row + 1f) * patternSpacingMulti + 
            //            transform.right * (column - boxWidth / 2f) * patternSpacingMulti;
            //        SetUnitPos(unitsToRally[i], pos, unitsToRally[i].GetStats.maxMoveSpeed);
            //    }
            //}
            //else if (patternIndex == 3)
            //{
            //    // pattern arrow (basically just a line that's offset by column)
            //    patternNameText.text = "ARROW";
            //    for (int i = 0; i < unitsToRally.Count; i++)
            //    {
            //        if (unitsToRally[i] == null)
            //            continue;

            //        float row = Mathf.FloorToInt(i / boxWidth);
            //        float column = i % boxWidth;// + row * Mathf.Sign(i % (boxWidth + row) - boxWidth / 2f);

            //        // only apply if unit is in the last row (centering)
            //        if (i >= lastRowStartIndex)
            //            column += remainderInLastRow / 2f;

            //        //column += ;
            //        float arrowForwardOffset = Mathf.Abs (column - boxWidth / 2f) * 0.5f;

            //        Vector3 pos =
            //            transform.forward * (row + 1f + arrowForwardOffset) * patternSpacingMulti + 
            //            transform.right * (column - boxWidth / 2f) * patternSpacingMulti;

            //        SetUnitPos(unitsToRally[i], pos, unitsToRally[i].GetStats.maxMoveSpeed);
            //    }
            //}
            //else
            //{
            //    patternNameText.text = "";
            //}
            

            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
            {
                DeselectUnits();
            }
        }


        // set the move target of each unit to whatever the final ordering object pos was based on the pattern
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonUp(PlayerInput.GetInputRallyTroops))
        {
            patternNameText.text = "";
            isRallyingTroopsOnPattern = false;

            rallyTroopsEffect.Play();
            lastRallyPointWorldPos = transform.position + new Vector3 (0f, -0.75f, 0f);

            for (int i = 0; i < unitsToRally.Count; i++)
            {
                if (unitsToRally[i] == null)
                    continue;

                unitsToRally[i].GetOrderingObject.SetActive(false);

                // if we never chose a pattern, just put the unit near the player
                if (patternIndex == -1)
                {
                    //float maxDistAway = Mathf.Lerp(0.5f, 3f, (float)unitsToRally.Count / 15f);
                    //Vector3 pos = new Vector3(Random.Range(-maxDistAway, maxDistAway), 0f, Random.Range(-maxDistAway, maxDistAway));
                    SetUnitPos(unitsToRally[i], Vector3.zero, unitsToRally[i].GetStats.maxMoveSpeed);
                    unitsToRally[i].GetOrderingObject.SetActive(false);
                }

                unitsToRally[i].SetDestinationWithType(unitsToRally[i].GetOrderingObject.transform.position, unitMovementType);
            }
            // reset pattern index
            patternIndex = -1;
        }
    }

    private void HandleMircroGroups ()
    {
        for (int i = 0; i < microUiObjects.Length; i++)
        {
            microUiObjects[i].gameObject.SetActive(microGroups[i].Length >= 1);
        }
        // THESE WORK, I JUST HAVE THEM TURNED OFF UNTIL I FEEL LIKE USING MICRO GROUPS
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputBlock))
        {
            CreateMicroGroup(PlayerInput.GetInputSelect, 0);
            CreateMicroGroup(PlayerInput.GetInputInteract, 1);
            //CreateMicroGroup(PlayerInput.GetInputSelectUnits, 2);
            CreateMicroGroup(PlayerInput.GetInputBack, 3);
        }
    }

    private void CreateMicroGroup (string buttonName, int microGroupIndex)
    {
        // CREATING MICRO GROUPS
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonLongPressDown(buttonName))
        {
            microGroups[microGroupIndex] = new UnitActions[0];
            microUiObjects[microGroupIndex].SetTrigger("Select");

            if (selectedUnits.Count > 0 || tempSelectedUnits.Count > 0)
            {
                if (tempSelectedUnits.Count > 0)
                    microGroups[microGroupIndex] = tempSelectedUnits.ToArray();
                else
                    microGroups[microGroupIndex] = selectedUnits.ToArray();
            }
        }
        else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonUp(buttonName) && microGroups[microGroupIndex].Length > 0)
        {
            tempSelectedUnits.Clear();
            selectedUnits = microGroups[microGroupIndex].ToList();
        }
    }

    private void ChangeUnitMovementType ()
    {
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadUp))
        {
            movementTypeText.transform.parent.gameObject.SetActive(true);

            int lengthOfEnum = System.Enum.GetNames(typeof(UnitMovementType)).GetLength(0) - 1; // the -1 is so we don't switch to patrol mode, I don't want it
            if ((int)unitMovementType + 1 >= lengthOfEnum)
            {
                unitMovementType = 0;
            }
            else
            {
                unitMovementType++;
            }

            movementTypeTextCounter = 3f;

            if (unitMovementType == UnitMovementType.IgnoreEnemies)
            {
                movementTypeText.text = "MOVEMENT: <b><i>POINT</i></b>";
                movementTypeDescText.text = "Units move directly to their destination, no interruptions.";
            }
            else if(unitMovementType == UnitMovementType.LookNearDestination)
            {
                movementTypeText.text = "MOVEMENT: <b><i>CASUAL</i></b><b> [DEFAULT]</b>";
                movementTypeDescText.text = "Units will auto attack nearby enemies when close to their destination.";

            }
            //else if(unitMovementType == UnitMovementType.Patrol)
            //{
            //    movementTypeText.text = "UNIT MOVEMENT: <i>PATROL</i>";
            //    movementTypeDescText.text = "Units will constantly look for enemies while they move and will stop when they see one.";
            //}

        }

        movementTypeTextCounter -= Time.deltaTime;

        if (movementTypeTextCounter <= 0)
        {
            movementTypeText.transform.parent.gameObject.SetActive(false);
        }
    }

    private Vector3 MaxPosWithHit (Vector3 pos)
    {
        bool hitSomethingInBetween = Physics.Raycast(transform.position, pos, out RaycastHit hitSomethingBetweenInfo, pos.magnitude, obstacleMask);
        float minMagnitude = hitSomethingInBetween ? hitSomethingBetweenInfo.distance : pos.magnitude;
        return Vector3.ClampMagnitude(pos, minMagnitude);
    }

    private void SetUnitPos(UnitActions unit, Vector3 dir, float moveSpeed)
    {
        unit.GetOrderingObject.SetActive(true);
        unit.GetOrderingObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

        bool hitDown = Physics.Raycast(transform.position + new Vector3(0f, 0.5f, 0f) + dir, Vector3.down, out RaycastHit hitInfo, 100f, obstacleMask);
        unit.GetOrderingObject.transform.position = hitDown ? hitInfo.point + new Vector3(0f, 0.5f, 0f) : transform.position + dir;
        unit.SetGroupMoveSpeed(moveSpeed);
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
        // if we're holding the 'A' button, we don't want to mess with selected unit types/numbers
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputRallyTroops) || selectedUnitsUi.Length <= 0)
            return;

        for (int i = 0; i < selectedUnitsUi.Length; i++)
        {
            selectedUnitsUi[i].SetDetails(selectedUnits.Count(unit => unit.GetStats.unitType == selectedUnitsUi[i].GetUnitType), 0);
            selectedUnitsUi[i].gameObject.SetActive(selectedUnits.Any(unit => unit.GetStats.unitType == selectedUnitsUi[i].GetUnitType) && selectedUnits.Count > 0);
        }

        if (selectedUnits.Count <= 0)
            return;

        // change selected unit type
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleRight))
            IncreaseSelectedUnitIndex();
        else if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetCycleLeft))
            DecreaseSelectedUnitIndex();

        // if the selected unit index was actually changed
        if (selectedUnitIndex >= 0)
        {
            tempSelectedUnits = selectedUnits.Where(unit => unit.GetStats.unitType == selectedUnitsUi[selectedUnitIndex].GetUnitType).ToList();

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
        selectedUnits = PlayerHolder.GetUnits(identifier.GetPlayerID)
            .Where(unit => (unit.transform.position - transform.position).sqrMagnitude < currentSelectionRadius * currentSelectionRadius &&
        unit.GetIsSelectable).ToList();
    }

    public void DeselectUnits ()
    {
        patternNameText.text = "";
        patternIndex = -1;

        selectedUnitIndex = -1;
        tempSelectedUnits = new List<UnitActions>(0);
        isRallyingTroopsOnPattern = false;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            if (selectedUnits[i] == null)
                continue;

            selectedUnits[i].GetOrderingObject.SetActive(false);
            selectedUnits[i].SetIsSelected(false);
        }
        selectedUnits = new List<UnitActions>(0);
    }
}

public enum UnitMovementType
{
    IgnoreEnemies = 0, // go to destination first, then look for enemies
    LookNearDestination = 1, // stop when we're close enough to destination and there's an enemy in sight
    Patrol = 2 // stop at the sight of any enemies
}
