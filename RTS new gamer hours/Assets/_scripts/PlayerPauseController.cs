using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPauseController : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private MyUiButton[] pauseMenuButtons;

    [SerializeField] private MyUiButton[] buttonsNotAvailableInGame;

    private Identifier identifier;

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

    private bool movedIndex = false;

    public bool GetIsPaused => isPaused;

    private void Awake()
    {
        identifier = GetComponent<Identifier>();
    }

    private void Start()
    {
        //pauseMenuButtons = GetComponentsInChildren<MyUiButton>();
    }

    private void Update()
    {
        if (isPaused)
        {
            HandlePauseMenuOpen();
        }
        else
        {
            if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputPause))
            {
                DeSelectAllButtons();
                selectedButtonIndex = 0;
                pauseMenuButtons[selectedButtonIndex].SelectButton();
                Pause();
            }
        }

        

        if (pauseMenu)
            pauseMenu.SetActive(isPaused);
    }

    private void HandlePauseMenuOpen ()
    {
        // unpause
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputBack))
        {
            UnPause();
        }


        ButtonType buttonType = pauseMenuButtons[selectedButtonIndex].GetButtonType;
        pauseMenuButtons[selectedButtonIndex].SetIsClicking(buttonType == ButtonType.Hold ? 
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButton(PlayerInput.GetInputSelect) :
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputSelect));

        // go up
        if ((!movedIndex && PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical) > 0.5f) ||
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadUp))
        {
            if (selectedButtonIndex - 1 >= 0)
            {
                ChangeButtonIndex(-1);
            }
        }

        // go down
        if ((!movedIndex && PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical) < -0.5f) ||
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetButtonDown(PlayerInput.GetInputDpadDown))
        {
            if (selectedButtonIndex + 1 < pauseMenuButtons.Length)
            {
                ChangeButtonIndex(1);
            }
        }

        // reset the movement (stick is in the middle)
        if (PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical) < 0.5 && 
            PlayerInput.GetPlayers[identifier.GetPlayerID].GetAxis(PlayerInput.GetInputMoveVertical) > -0.5)
            movedIndex = false;

        // turn off buttons that aren't available in game
        for (int i = 0; i < buttonsNotAvailableInGame.Length; i++)
        {
            buttonsNotAvailableInGame[i].gameObject.SetActive(GameWinManager.instance == null);
        }
    }

    private void ChangeButtonIndex (int amount)
    {
        movedIndex = true;
        pauseMenuButtons[selectedButtonIndex].DeSelectButton();
        pauseMenuButtons[selectedButtonIndex].SetIsClicking(false);

        // if the button isn't active, skip over it
        // capped to 10 times cause I don't like while loops
        for (int i = 0; i < 10; i++)
        {
            if (pauseMenuButtons[selectedButtonIndex + amount].gameObject.activeInHierarchy)
                break;

            selectedButtonIndex += amount;
        }
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

    public void LeaveGame ()
    {
        UnPause();
        SplitscreenAutoCamera.instance.RemoveJoinedPlayer(identifier.GetPlayerID);
    }

    public void MainMenuButton ()
    {
        SceneController.ChangeScene(0);
    }

    public void ToggleHealthBars (Toggle toggle)
    {
        HealthBarManager.SetHealthBarsOn(identifier.GetPlayerID, !HealthBarManager.GetHealthBarsOn[identifier.GetPlayerID]);
        toggle.isOn = HealthBarManager.GetHealthBarsOn[identifier.GetPlayerID];
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
