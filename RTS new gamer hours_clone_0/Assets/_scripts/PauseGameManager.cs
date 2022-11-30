using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PauseGameManager : MonoBehaviour
{
    private PlayerPauseController[] playerPauseControllers;

    public static bool ForcePause = false;
    public static bool GetIsPaused => isPaused;
    private static bool isPaused = false;

    private void Start()
    {
        playerPauseControllers = FindObjectsOfType<PlayerPauseController>().OrderBy(player => player.GetComponent<Identifier>().GetPlayerID).ToArray();

        if (playerPauseControllers.Length > 4)
            Debug.LogError("There are more than 4 PlayerPauseControllers.cs in the scene, that shouldn't happen");
    }

    private void Update()
    {
        // pause if any player is paused
        if (playerPauseControllers.Any(player => player.GetIsPaused) || ForcePause)
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            isPaused = true;
        }

        // unpause if all players aren't paused
        if (playerPauseControllers.All (player => !player.GetIsPaused) && !ForcePause)
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
            isPaused = false;
        }
    }
}
