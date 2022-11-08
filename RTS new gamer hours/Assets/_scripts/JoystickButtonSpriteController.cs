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

    //private void Awake()
    //{
    //    // Subscribe to events
    //    ReInput.ControllerConnectedEvent += OnControllerConnected;
    //    ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
    //    ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
    //}

    //// This function will be called when a controller is connected
    //// You can get information about the controller that was connected via the args parameter
    //void OnControllerConnected(ControllerStatusChangedEventArgs args)
    //{
    //    Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    //}

    //// This function will be called when a controller is fully disconnected
    //// You can get information about the controller that was disconnected via the args parameter
    //void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    //{
    //    Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    //}

    //// This function will be called when a controller is about to be disconnected
    //// You can get information about the controller that is being disconnected via the args parameter
    //// You can use this event to save the controller's maps before it's disconnected
    //void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
    //{
    //    Debug.Log("A controller is being disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    //}

    //void OnDestroy()
    //{
    //    // Unsubscribe from events
    //    ReInput.ControllerConnectedEvent -= OnControllerConnected;
    //    ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
    //    ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
    //}

    private void Update()
    {
        for (int i = 0; i < ReInput.players.playerCount; i++)
        {
            Controller controller = PlayerInput.GetPlayers[i].controllers.GetController(Rewired.ControllerType.Joystick, i);
            if (controller == null)
                continue;

            if (controller.name.Contains("Dualshock") || controller.name.Contains("Sony"))
                playerControllerTypes[i] = ControllerType.PS4;
            else if (controller.name.Contains("XInput"))
                playerControllerTypes[i] = ControllerType.Xbox;

            //Debug.Log("player" + i + ":" + controller.name);
        }
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
