using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Attacking))]
public class UnitActions : MonoBehaviour
{
    [SerializeField] private GameObject selectedGO;
    [SerializeField] private GameObject orderingObject;
    [SerializeField] private UnitStats unitStats;

    private Identifier identifier;
    private Movement movement;
    private Attacking attacking;
    private bool isSelected = false;


    public Movement GetMovement => movement;
    public Attacking GetAttacking() => attacking;
    public GameObject GetOrderingObject => orderingObject;
    public bool GetIsSelected => isSelected;
    public void SetIsSelected(bool select) => isSelected = select;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
        movement = GetComponent<Movement>();
        attacking = GetComponent<Attacking>();

        orderingObject.SetActive(false);
    }

    private void Start()
    {
        // add unit to list of all units for player
        PlayerHolder.AddUnit(identifier.GetPlayerID, this);
    }

    private void Update()
    {
        selectedGO.SetActive(isSelected);
    }




}
