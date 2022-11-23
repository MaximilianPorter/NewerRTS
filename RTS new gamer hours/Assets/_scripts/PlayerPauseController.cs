using SCPE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerPauseController : MonoBehaviour
{

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private PauseMenuSection[] pauseMenus;

    [SerializeField] private MyUiButton[] buttonsNotAvailableInGame;
    [SerializeField] private VolumeProfile postProfile;
    private CloudShadows cloudShadows;


    private Identifier identifier;

    public void Pause()
    {
        isPaused = true;
    }
    public void UnPause ()
    {
        isPaused = false;
    }

    [Serializable]
    public struct PauseMenuSection
    {
        public PauseMenus menuType;
        public GameObject menuParent;
        public MyUiButton[] buttonsInMenu;
    }
    public enum PauseMenus
    {
        Main = 0,
        Options = 1,
    }


    private PauseMenuSection currentMenu;
    private PauseMenus currentMenuType = PauseMenus.Main;
    private PauseMenus lastMenuType;
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
        currentMenu = pauseMenus.FirstOrDefault(menu => menu.menuType == PauseMenus.Main);
        if (postProfile.TryGet (out CloudShadows clouds))
            this.cloudShadows = clouds;

        lastMenuType = currentMenu.menuType;
    }

    private void Update()
    {
        currentMenu = pauseMenus.FirstOrDefault(menu => menu.menuType == currentMenuType);
        // update first selected if the menu changes
        if (lastMenuType != currentMenu.menuType)
        {
            lastMenuType = currentMenu.menuType;
            selectedButtonIndex = 0;
            currentMenu.buttonsInMenu[selectedButtonIndex].SelectButton();
        }

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
                currentMenu.buttonsInMenu[selectedButtonIndex].SelectButton();
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
            if (currentMenuType != PauseMenus.Main)
            {
                currentMenuType = PauseMenus.Main;
            }
            else
                UnPause();
        }


        ButtonType buttonType = currentMenu.buttonsInMenu[selectedButtonIndex].GetButtonType;
        currentMenu.buttonsInMenu[selectedButtonIndex].SetIsClicking(buttonType == ButtonType.Hold ? 
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
            if (selectedButtonIndex + 1 < currentMenu.buttonsInMenu.Length)
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

        for (int i = 0; i < pauseMenus.Length; i++)
        {
            pauseMenus[i].menuParent.SetActive(pauseMenus[i].menuType == currentMenuType);
        }
    }

    private void ChangeButtonIndex (int amount)
    {
        movedIndex = true;
        currentMenu.buttonsInMenu[selectedButtonIndex].DeSelectButton();
        currentMenu.buttonsInMenu[selectedButtonIndex].SetIsClicking(false);

        // if the button isn't active, skip over it
        // capped to 10 times cause I don't like while loops
        for (int i = 0; i < 10; i++)
        {
            if (currentMenu.buttonsInMenu[selectedButtonIndex + amount].gameObject.activeInHierarchy)
                break;

            selectedButtonIndex += amount;
        }
        selectedButtonIndex += amount;
        currentMenu.buttonsInMenu[selectedButtonIndex].SelectButton();
    }

    private void DeSelectAllButtons ()
    {
        for (int i = 0; i < currentMenu.buttonsInMenu.Length; i++)
        {
            currentMenu.buttonsInMenu[i].DeSelectButton();
        }
    }

    public void OpenOptionsMenu()
    {
        currentMenuType = PauseMenus.Options;
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

    public void ToggleClouds (Toggle toggle)
    {
        toggle.isOn = !toggle.isOn;
        cloudShadows.density.value = toggle.isOn ? 1f : 0f;
    }

    public void QuitGame ()
    {
        Application.Quit();
    }
}
