using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JoystickButtonSpriteController : MonoBehaviour
{
    public static JoystickButtonSpriteController instance;

    private ControllerType[] playerControllerTypes = new ControllerType[]
    {
        ControllerType.Xbox,
        ControllerType.Xbox,
        ControllerType.Xbox,
        ControllerType.Xbox,
    };
    public ControllerType GetPlayerControllerType(int playerID) => playerControllerTypes[playerID];
    public Sprite GetIcon (ControllerType controller, InputElement input)
    {
        if (controller == ControllerType.PS4)
            return controllerIcons.FirstOrDefault(settings => settings.input == input).ps4Sprite;
        else if (controller == ControllerType.Xbox)
            return controllerIcons.FirstOrDefault(settings => settings.input == input).xboxSprite;

        return null;
    }

    [SerializeField] private InputSprite[] controllerIcons;

    [Serializable]
    public struct InputSprite
    {
        public InputElement input;
        public ButtonState buttonState;
        public Sprite ps4Sprite;
        public Sprite xboxSprite;
    }

    private void Awake()
    {
        instance = this;
    }
}

public enum ControllerType
{
    PS4,
    Xbox
}

public enum ButtonState
{
    Up,
    Down
}

public enum InputElement
{
    LeftStickX = 0,
    LeftStickY = 1,
    RightStickX = 2,
    RightStickY = 3,
    ActionBottomRow1 = 4,
    ActionBottomRow2 = 5,
    ActionBottomRow3 = 6,
    ActionTopRow1 = 7,
    ActionTopRow2 = 8,
    ActionTopRow3 = 9,
    LeftShoulder1 = 10,
    LeftShoulder2 = 11,
    RightShoulder1 = 12,
    RightShoulder2 = 13,
    Center1 = 14,
    Center2 = 15,
    Center3 = 16,
    LeftStickbutton = 17,
    RightStickbutton = 18,
    DpadUp = 19,
    DpadDown = 20,
    DpadLeft = 21,
    DpadRight = 22,
}
