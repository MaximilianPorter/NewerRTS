using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPauseController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private MyUiButton[] pauseMenuButtons;

    public void Pause()
    {
        isPaused = true;
    }
    public void UnPause ()
    {
        isPaused = false;
    }

    private int selectedButtonIndex = 0;
    private bool isPaused = false;
    private int playerThatPaused = -1;

    private bool movedIndex = false;

    private void Update()
    {
        

        if (isPaused)
        {
            HandlePauseMenuOpen();
        }
        else
        {
            for (int i = 0; i < PlayerInput.GetPlayers.Count; i++)
            {
                if (PlayerInput.GetPlayers[i].GetButtonDown(PlayerInput.GetInputPause))
                {
                    DeSelectAllButtons();
                    playerThatPaused = i;
                    selectedButtonIndex = 0;
                    pauseMenuButtons[selectedButtonIndex].SelectButton();
                    Pause();
                }
            }
        }

        

        if (pauseMenu)
            pauseMenu.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
        else
        {
            playerThatPaused = -1;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = Time.timeScale * 0.02f;
        }
    }

    private void HandlePauseMenuOpen ()
    {
        if (playerThatPaused != -1 &&
            (PlayerInput.GetPlayers[playerThatPaused].GetButtonDown(PlayerInput.GetInputBack)))
        {
            UnPause();
        }

        pauseMenuButtons[selectedButtonIndex].SetIsClicking(PlayerInput.GetPlayers[playerThatPaused].GetButton(PlayerInput.GetInputSelect));

        if ((!movedIndex && PlayerInput.GetPlayers[playerThatPaused].GetAxis(PlayerInput.GetInputMoveVertical) > 0.5f) ||
            PlayerInput.GetPlayers[playerThatPaused].GetButtonDown(PlayerInput.GetInputDpadUp))
        {
            if (selectedButtonIndex - 1 >= 0)
            {
                ChangeButtonIndex(-1);
            }
        }
        if ((!movedIndex && PlayerInput.GetPlayers[playerThatPaused].GetAxis(PlayerInput.GetInputMoveVertical) < -0.5f) ||
            PlayerInput.GetPlayers[playerThatPaused].GetButtonDown(PlayerInput.GetInputDpadDown))
        {
            if (selectedButtonIndex + 1 < pauseMenuButtons.Length)
            {
                ChangeButtonIndex(1);
            }
        }

        // reset the movement
        if (PlayerInput.GetPlayers[playerThatPaused].GetAxis(PlayerInput.GetInputMoveVertical) < 0.5 && PlayerInput.GetPlayers[playerThatPaused].GetAxis(PlayerInput.GetInputMoveVertical) > -0.5)
            movedIndex = false;
    }

    private void ChangeButtonIndex (int amount)
    {
        movedIndex = true;
        pauseMenuButtons[selectedButtonIndex].DeSelectButton();
        pauseMenuButtons[selectedButtonIndex].SetIsClicking(false);
        selectedButtonIndex += amount;
        pauseMenuButtons[selectedButtonIndex].SelectButton();
    }

    private void DeSelectAllButtons ()
    {
        for (int i = 0; i < pauseMenuButtons.Length; i++)
        {
            pauseMenuButtons[i].DeSelectButton();
        }
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
