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
    [SerializeField] private Material buttonMaterial;

    [SerializeField] private Button.ButtonClickedEvent onButtonComplete = new Button.ButtonClickedEvent();
    //[SerializeField] private Button.ButtonClickedEvent onButtonUp = new Button.ButtonClickedEvent();

    private float startHeight;
    private bool pressingButton = false;
    private bool hasCorrectID = false;
    private int buttonLastClickedBy = -1;



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

    public void Give1000Food()
    {
        PlayerResourceManager.PlayerResourceAmounts[buttonLastClickedBy].AddResources(1000, 0, 0);
    }
    public void Give1000Wood()
    {
        PlayerResourceManager.PlayerResourceAmounts[buttonLastClickedBy].AddResources(0, 1000, 0);
    }
    public void Give1000Stone()
    {
        PlayerResourceManager.PlayerResourceAmounts[buttonLastClickedBy].AddResources(0, 0, 1000);
    }

    public void ChangePlayerColor ()
    {
        PlayerColorManager.SetPlayerColor(buttonLastClickedBy, buttonMaterial.color);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent (out Identifier playerPressing))
        {
            pressingButton = true;
            buttonLastClickedBy = playerPressing.GetPlayerID;
            if (playerPressing.GetPlayerID == requiredID)
            {
                hasCorrectID = true;
                buttonCompleteEffect.Stop();
                buttonCompleteEffect.Play();
                onButtonComplete.Invoke();
            }else if (requiredID == -1)
            {
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
            buttonLastClickedBy = -1;
        }
    }
}
