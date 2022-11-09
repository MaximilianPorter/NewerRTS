using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIdentifier : MonoBehaviour
{
    private Identifier identifier;
    public Identifier GetIdentifier => identifier;

    private Collider col;
    public Collider GetCollider => col;

    private void Start()
    {
        identifier = GetComponent<Identifier>();
        col = GetComponent<Collider>();
    }
}
