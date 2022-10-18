using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPhysical : MonoBehaviour
{
    [SerializeField] private int requiredID = -1;
    [SerializeField] private GameObject buttonObject;
    [SerializeField] private float heightPressed = 0.1f;
    [SerializeField] private float buttonMoveSpeed = 10f;
    [SerializeField] private bool pressOnce = false;
    [SerializeField] private ParticleSystem buttonCompleteEffect;

    [SerializeField] private Button.ButtonClickedEvent onButtonComplete = new Button.ButtonClickedEvent();
    //[SerializeField] private Button.ButtonClickedEvent onButtonUp = new Button.ButtonClickedEvent();

    private float startHeight;
    private bool pressingButton = false;
    private bool hasCorrectID = false;



    private void Start()
    {
        startHeight = buttonObject.transform.localPosition.y;
        buttonCompleteEffect.Stop();
    }

    private void Update()
    {
        if (pressingButton)
        {
            buttonObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(buttonObject.transform.localPosition.y, heightPressed, Time.deltaTime * buttonMoveSpeed), 0f);
        }
        else
        {
            buttonObject.transform.localPosition = new Vector3(0f, Mathf.Lerp(buttonObject.transform.localPosition.y, startHeight, Time.deltaTime * buttonMoveSpeed), 0f);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent (out Identifier playerPressing))
        {
            pressingButton = true;
            if (playerPressing.GetPlayerID == requiredID)
            {
                hasCorrectID = true;
                buttonCompleteEffect.Stop();
                buttonCompleteEffect.Play();
                onButtonComplete.Invoke();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.TryGetComponent(out Identifier playerPressing))
        {
            if (hasCorrectID)
            {
                if (pressOnce && pressingButton)
                    return;
                hasCorrectID = false;
            }
            pressingButton = false;
        }
    }
}
