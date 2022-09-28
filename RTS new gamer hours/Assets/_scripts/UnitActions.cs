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
        movement = GetComponent<Movement>();
        attacking = GetComponent<Attacking>();

        orderingObject.SetActive(false);
    }

    private void Update()
    {
        selectedGO.SetActive(isSelected);
    }




}
