using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class UnitSelection : MonoBehaviour
{
    [SerializeField] private LayerMask unitMask;
    [SerializeField] private Transform unitSelectionVisual;
    [SerializeField] private float maxSelectionRadius = 10f;
    [SerializeField] private float radiusIncreaseSpeed = 4f;

    private Movement[] selectedUnits = new Movement[0];
    private float currentSelectionRadius = 0f;
    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        // deselect units when you're selecting new ones
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonShortPressDown(PlayerInput.GetInputSelectUnits))
        {
            DeselectUnits(); 
        }

        // select some units
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonShortPress(PlayerInput.GetInputSelectUnits))
        {
            // increase radius and select units within
            unitSelectionVisual.gameObject.SetActive(true);
            unitSelectionVisual.localScale = new Vector3(currentSelectionRadius * 2f, currentSelectionRadius * 2f, 5f);
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
            selectedUnits = PlayerUnitHolder.GetUnits(identifier.GetPlayerID).ToArray();
            for (int i = 0; i < PlayerUnitHolder.GetUnits (identifier.GetPlayerID).Count; i++)
            {
                PlayerUnitHolder.GetUnits(identifier.GetPlayerID)[i].SetIsSelected(true);

            }
        }


        // deselect all units
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDeselectUnits))
        {
            DeselectUnits();
        }

        // rally troops
        if (PlayerInput.players[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputRallyTroops))
        {
            for (int i = 0; i < selectedUnits.Length; i++)
            {
                selectedUnits[i].SetMoveTarget(transform.position + new Vector3(Random.Range(0f, 1f), 0f, Random.Range(0f, 1f)));
            }
        }
    }

    private void SelectNearbyUnits()
    {
        // overlap units that are owned by the same player
        Collider[] nearbyUnits = Physics.OverlapSphere(transform.position, currentSelectionRadius, unitMask).Where(unit => unit.GetComponent<Identifier>().GetPlayerID == identifier.GetPlayerID).ToArray();
        selectedUnits = new Movement[nearbyUnits.Length];

        for (int i = 0; i < nearbyUnits.Length; i++)
        {
            selectedUnits[i] = nearbyUnits[i].GetComponent<Movement>();
            selectedUnits[i].SetIsSelected(true);
        }
    }

    private void DeselectUnits ()
    {
        for (int i = 0; i < selectedUnits.Length; i++)
        {
            selectedUnits[i].SetIsSelected(false);
        }
        selectedUnits = new Movement[0];
    }
}
