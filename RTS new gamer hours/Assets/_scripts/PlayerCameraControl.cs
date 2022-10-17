using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Identifier))]
public class PlayerCameraControl : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera playerVirtualCam;
    [SerializeField] private float minOrthoSize = 5;
    [SerializeField] private float maxOrthoSize = 20;
    [SerializeField] private float camZoomSpeed = 10f;
    [SerializeField] private Transform lookAheadDot;
    //[SerializeField] private float lookAheadDist = 15f;

    private float startOrthoSize = 12;
    private Identifier identifier;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        startOrthoSize = playerVirtualCam.m_Lens.OrthographicSize;
    }

    private void Update()
    {
        Vector3 lookDir = new Vector3(PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxisRaw(PlayerInput.GetInputLookHorizontal),
            0f,
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxisRaw(PlayerInput.GetInputLookVertical)).normalized;

        //lookAheadDot.transform.position = transform.position + lookDir * lookAheadDist;

        float desiredSize = startOrthoSize;
        if (lookDir.z > 0f)
            desiredSize = minOrthoSize;
        else if (lookDir.z < 0f)
            desiredSize = maxOrthoSize;

        playerVirtualCam.m_Lens.OrthographicSize = Mathf.Lerp(playerVirtualCam.m_Lens.OrthographicSize, desiredSize, camZoomSpeed * Time.deltaTime);

    }
}
