using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetails : MonoBehaviour
{
    [SerializeField] private Identifier playerIdentifier;
    [SerializeField] private Camera playerCam;
    [SerializeField] private RectTransform playerCanvas;

    private void Start()
    {
        //PlayerHolder.SetPlayerDetails(playerIdentifier, playerCam, playerCanvas);
    }
}
