using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;
using UnityEngine.Analytics;
using System.Runtime.CompilerServices;

public class PlayerInput : MonoBehaviour
{
    private static readonly string inputMoveHorizontal = "Move Horizontal";
    private static readonly string inputMoveVertical = "Move Vertical";
    private static readonly string inputLookHorizontal = "Look Horizontal";
    private static readonly string inputLookVertical = "Look Vertical";
    private static readonly string inputJump = "Jump";
    private static readonly string inputAttack = "Attack";
    private static readonly string inputBlock = "Block";
    private static readonly string inputSelectUnits = "SelectUnits";
    private static readonly string inputDeselectUnits = "DeselectUnits";
    private static readonly string inputRallyTroops = "RallyTroops";
    private static readonly string inputOpenBuildMenu = "OpenBuildMenu";
    private static readonly string inputOpenUnitMenu = "OpenUnitMenu";
    private static readonly string inputCycleRight = "Cycle Right";
    private static readonly string inputCycleLeft = "Cycle Left";
    private static readonly string inputBack = "Back";
    private static readonly string inputSelect = "Select";
    private static readonly string inputDpadUp = "DPAD UP";
    private static readonly string inputDpadDown = "DPAD DOWN";
    private static readonly string inputDpadLeft = "DPAD LEFT";
    private static readonly string inputDpadRight = "DPAD RIGHT";
    private static readonly string inputInteract = "Interact";
    private static readonly string inputPause = "Pause";
    private static readonly string inputSprint = "Sprint";

    public static string GetInputMoveHorizontal => inputMoveHorizontal;
    public static string GetInputMoveVertical => inputMoveVertical;
    public static string GetInputLookHorizontal => inputLookHorizontal;
    public static string GetInputLookVertical => inputLookVertical;
    public static string GetInputJump => inputJump;
    public static string GetInputAttack => inputAttack;
    public static string GetInputBlock => inputBlock;
    public static string GetInputSelectUnits => inputSelectUnits;
    public static string GetInputDeselectUnits => inputDeselectUnits;
    public static string GetInputRallyTroops => inputRallyTroops;
    public static string GetInputOpenBuildMenu => inputOpenBuildMenu;
    public static string GetInputOpenUnitMenu => inputOpenUnitMenu;
    public static string GetCycleRight => inputCycleRight;
    public static string GetCycleLeft => inputCycleLeft;
    public static string GetInputBack => inputBack;
    public static string GetInputSelect => inputSelect;
    public static string GetInputDpadUp => inputDpadUp;
    public static string GetInputDpadDown => inputDpadDown;
    public static string GetInputDpadLeft => inputDpadLeft;
    public static string GetInputDpadRight => inputDpadRight;
    public static string GetInputInteract => inputInteract;
    public static string GetInputPause => inputPause;
    public static string GetInputSprint => inputSprint;

    private static List<Player> players;
    public static List<Player> GetPlayers => players;



    private static bool[] playerIsInMenu = new bool[4] { false, false, false, false };
    public static bool GetPlayerIsInMenu(int playerID) => playerIsInMenu[playerID];
    public static void SetPlayerIsInMenu(int playerID, bool inMenu) => playerIsInMenu[playerID] = inMenu;

    private static float[] vibrateControllerCounter = new float[4];
    private static float[] vibrateDurationMultiplier = new float[4];
    public static void VibrateController(int playerID, float intensity, float duration)
    {
        intensity = Mathf.Clamp01(intensity);

        vibrateControllerCounter[playerID] = intensity;
        vibrateDurationMultiplier[playerID] = duration;
    }

    private void Update()
    {
        ManageControllerVibration();
        
    }

    private void ManageControllerVibration ()
    {
        for (int i = 0; i < vibrateControllerCounter.Length; i++)
        {
            if (i >= ReInput.controllers.joystickCount)
                continue;

            if (vibrateControllerCounter[i] <= 0)
            {
                vibrateControllerCounter[i] = 0f;
            }

            ReInput.controllers.Joysticks[i].SetVibration(vibrateControllerCounter[i], vibrateControllerCounter[i]);

            vibrateControllerCounter[i] -= Time.deltaTime / vibrateDurationMultiplier[i];
        }
    }

    private void FixedUpdate()
    {
    }

    private void Awake()
    {
        players = ReInput.players.GetPlayers().ToList();
    }
}
