using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System.Linq;
using UnityEngine.Analytics;

public class PlayerInput : MonoBehaviour
{
    private static readonly string inputMoveHorizontal = "Move Horizontal";
    private static readonly string inputMoveVertical = "Move Vertical";
    private static readonly string inputJump = "Jump";
    private static readonly string inputAttack = "Attack";
    private static readonly string inputSelectUnits = "SelectUnits";
    private static readonly string inputDeselectUnits = "DeselectUnits";
    private static readonly string inputRallyTroops = "RallyTroops";

    public static string GetInputMoveHorizontal => inputMoveHorizontal;
    public static string GetInputMoveVertical => inputMoveVertical;
    public static string GetInputJump => inputJump;
    public static string GetInputAttack => inputAttack;
    public static string GetInputSelectUnits => inputSelectUnits;
    public static string GetInputDeselectUnits => inputDeselectUnits;
    public static string GetInputRallyTroops => inputRallyTroops;

    public static List<Player> players;

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
