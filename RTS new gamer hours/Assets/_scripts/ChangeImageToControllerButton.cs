using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Identifier))]
public class ChangeImageToControllerButton : MonoBehaviour
{
    [SerializeField] private InputElement input;
    private Image image;
    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponent <Identifier>();
        image = GetComponent<Image>();

        ControllerType controller = JoystickButtonSpriteController.instance.GetPlayerControllerType(identifier.GetPlayerID);
        image.sprite = JoystickButtonSpriteController.instance.GetIcon(controller, input);
    }

    private void Update()
    {
        
    }
}
