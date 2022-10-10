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

    private UnitActions[] selectedUnits = new UnitActions[0];
    private float currentSelectionRadius = 0f;
    private int patternIndex = -1;
    private readonly int totalPatterns = 2;
    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
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


        RallyTroopsOnPattern();
        
    }

    private void RallyTroopsOnPattern ()
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
                int boxWidth = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt((float)selectedUnits.Length)), 0, 20);
                for (int i = 0; i < selectedUnits.Length; i++)
                {
                    if (selectedUnits[i] == null)
                        continue;

                    int row = Mathf.FloorToInt(i / boxWidth);
                    int column = i % boxWidth;
                    Vector3 boxPos = transform.forward * (row + 1f) + transform.right * (column - boxWidth / 2f);
                    selectedUnits[i].GetOrderingObject.SetActive(true);
                    selectedUnits[i].GetOrderingObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    selectedUnits[i].GetOrderingObject.transform.position = transform.position + boxPos;
                }
            }else if (patternIndex == 1)
            {
                // pattern long line
                int boxWidth = Mathf.Clamp (selectedUnits.Length, 0, 20);
                for (int i = 0; i < selectedUnits.Length; i++)
                {
                    if (selectedUnits[i] == null)
                        continue;

                    int row = Mathf.FloorToInt(i / boxWidth);
                    int column = i % boxWidth;
                    Vector3 boxPos = transform.forward * (row + 1f) + transform.right * (column - boxWidth / 2f);
                    selectedUnits[i].GetOrderingObject.SetActive(true);
                    selectedUnits[i].GetOrderingObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    selectedUnits[i].GetOrderingObject.transform.position = transform.position + boxPos;
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

            for (int i = 0; i < selectedUnits.Length; i++)
            {
                if (selectedUnits[i] == null)
                    continue;

                selectedUnits[i].GetOrderingObject.SetActive(false);

                // if we never chose a pattern, just put the unit near the player
                if (patternIndex == -1)
                {
                    float maxDistAway = Mathf.Lerp(0.5f, 3f, selectedUnits.Length / 10);
                    selectedUnits[i].GetOrderingObject.transform.position = transform.position + new Vector3(Random.Range(-maxDistAway, maxDistAway), 0f, Random.Range(-maxDistAway, maxDistAway));
                }


                selectedUnits[i].GetMovement.SetMoveTarget(selectedUnits[i].GetOrderingObject.transform.position);
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

    private void SelectNearbyUnits()
    {
        // overlap units that are owned by the same player
        //Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, currentSelectionRadius, unitMask).Where(unit => unit.GetComponent<Identifier>().GetPlayerID == identifier.GetPlayerID).ToArray();
        ////selectedUnits = new UnitActions[nearbyUnits.Length];
        //for (int i = 0; i < nearbyUnits.Length; i++)
        //{
        //    selectedUnits[i] = nearbyUnits[i].GetComponent<UnitActions>();
        //    selectedUnits[i].SetIsSelected(true);
        //}

        selectedUnits = PlayerHolder.GetUnits(identifier.GetPlayerID).Where(unit => (unit.transform.position - transform.position).sqrMagnitude < currentSelectionRadius * currentSelectionRadius).ToArray();
        foreach (UnitActions unit in PlayerHolder.GetUnits(identifier.GetPlayerID))
        {
            if (selectedUnits.Contains(unit))
                unit.SetIsSelected(true);
            else
                unit.SetIsSelected(false);
        }
    }

    public void DeselectUnits ()
    {
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
