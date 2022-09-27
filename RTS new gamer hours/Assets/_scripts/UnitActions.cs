using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Attacking))]
public class UnitActions : MonoBehaviour
{
    private Movement movement;
    private Attacking attacking;

    private void Awake()
    {
        movement = GetComponent<Movement>();
        attacking = GetComponent<Attacking>();
    }




}
