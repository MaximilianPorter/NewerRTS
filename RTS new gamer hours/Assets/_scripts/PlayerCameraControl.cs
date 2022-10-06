using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class PlayerCameraControl : MonoBehaviour
{
    [SerializeField] private Transform lookAheadDot;
    [SerializeField] private float lookAheadDist = 10f;

    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Update()
    {
        Vector3 lookDir = new Vector3(PlayerInput.players[identifier.GetPlayerID].GetAxisRaw(PlayerInput.GetInputLookHorizontal),
            0f,
            PlayerInput.players[identifier.GetPlayerID].GetAxisRaw(PlayerInput.GetInputLookVertical)).normalized;

        lookAheadDot.transform.position = transform.position + lookDir * lookAheadDist;
    }
}
