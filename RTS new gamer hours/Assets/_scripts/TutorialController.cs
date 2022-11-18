using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    public static TutorialController Instance { get; private set; }

    [SerializeField] private Identifier tutorialIdentifier;
    [SerializeField] private TMP_Text tutorialAreaTitle;
    [SerializeField] private Transform spawnedObjectsHolder;
    [SerializeField] private GameObject startingResources;
    [SerializeField] private TMP_Text stepTimer;
    [SerializeField] private Image skipStepFill;

    public Transform GetSpawnedObjectsHolder => spawnedObjectsHolder;

    [SerializeField] private Transform startBuildingSpawnPoint;
    [SerializeField] private Building startBuilding;

    [SerializeField] private Transform towerSpawnPoint0;
    [SerializeField] private Transform towerSpawnPoint1;
    [SerializeField] private Building tower;

    private TutorialStep[] steps;
    private Identifier activePlayer = null;

    private int currentStep = -1;
    private float currentStepTimer = 0f;
    private float skipStepTimer = 0f;
    private bool startSkip = false;

    private void Awake()
    {
        Instance = this;

    }

    private void Start()
    {
        steps = tutorialIdentifier.GetComponentsInChildren<TutorialStep>(true);
    }

    private void Update()
    {
        tutorialIdentifier.gameObject.SetActive(activePlayer);

        if (!activePlayer)
        {
            tutorialAreaTitle.text = "TUTORIAL\nAREA";
            currentStep = -1;
        }
        else
            tutorialAreaTitle.text = "PLAYER ACTIVE";


        if (currentStep == -1)
            return;

        for (int i = 0; i < steps.Length; i++)
        {
            if (i == currentStep)
                steps[i].gameObject.SetActive(true);
            else
                steps[i].gameObject.SetActive(false);
        }

        // skip step
        if (PlayerInput.GetPlayers[activePlayer.GetPlayerID].GetButtonDown(PlayerInput.GetInputInteract))
            startSkip = true;
        if (startSkip && PlayerInput.GetPlayers[activePlayer.GetPlayerID].GetButton(PlayerInput.GetInputInteract))
        {
            skipStepTimer += Time.deltaTime;
            skipStepFill.fillAmount = skipStepTimer / 1f;

            if (skipStepTimer > 1f)
            {
                GoNextStep();
                skipStepTimer = 0f;
            }
        }
        if (PlayerInput.GetPlayers[activePlayer.GetPlayerID].GetButtonUp(PlayerInput.GetInputInteract))
            startSkip = false;
        else
        {
            skipStepFill.fillAmount = 1f;
            skipStepTimer = 0f;
        }
            

        // wait for timer
        if (steps[currentStep].stepTimer > -1)
        {
            currentStepTimer += Time.deltaTime;
            if (currentStepTimer >= steps[currentStep].stepTimer)
                GoNextStep();

            stepTimer.text = "STEP TIMER: " + (steps[currentStep].stepTimer - currentStepTimer).ToString("F1");
        }
        else
            stepTimer.text = "";

        // wait for placed building
        if (steps[currentStep].waitForPlacedBuilding != BuyIcons.NONE)
        {
            if (PlayerHolder.GetBuildings(activePlayer.GetPlayerID).Any(building => building.GetStats.buildingType == steps[currentStep].waitForPlacedBuilding))
                GoNextStep();
        }

        // collect the needed amount of resources
        if (!steps[currentStep].neededResources.CompareResources (new ResourceAmount (0, 0, 0)))
        {
            ResourceAmount myResources = PlayerResourceManager.PlayerResourceAmounts[activePlayer.GetPlayerID];
            if (myResources.HasResources(steps[currentStep].neededResources))
                GoNextStep();
        }

        // wait for button action
        if (steps[currentStep].waitForAction != "")
        {
            if (PlayerInput.GetPlayers[activePlayer.GetPlayerID].GetButtonDown(steps[currentStep].waitForAction))
                GoNextStep();
        }

        // get required units
        if (steps[currentStep].requiredUnitType != BuyIcons.NONE)
        {
            if (PlayerHolder.GetUnits(activePlayer.GetPlayerID).Count(unit => unit.GetStats.unitType == steps[currentStep].requiredUnitType)
                >= steps[currentStep].requiredUnitAmount)
                GoNextStep();
        }

        // need units selected
        if (steps[currentStep].requireUnitsSelected)
        {
            if (PlayerHolder.GetPlayerIdentifiers[activePlayer.GetPlayerID].GetComponent<UnitSelection>().GetHasTroopsSelected)
                GoNextStep();
        }

    }

    private void GoNextStep ()
    {
        steps[currentStep].onFinishStep.Invoke();

        skipStepTimer = 0f;
        startSkip = true;
        currentStepTimer = 0f;
        if (currentStep + 1 < steps.Length)
            currentStep++;

        steps[currentStep].onStartStep.Invoke();
    }

    public void BuildFirstBuilding()
    {
        Identifier buildingIdentity = Instantiate(startBuilding, startBuildingSpawnPoint.position, Quaternion.identity, spawnedObjectsHolder).GetComponent<Identifier>();
        buildingIdentity.UpdateInfo(activePlayer.GetPlayerID, activePlayer.GetTeamID);
    }
    public void BuildTowers ()
    {
        Identifier tower0 = Instantiate(tower, towerSpawnPoint0.position, Quaternion.identity, spawnedObjectsHolder).GetComponent<Identifier>();
        tower0.UpdateInfo(activePlayer.GetPlayerID, activePlayer.GetTeamID);

        Identifier tower1 = Instantiate(tower, towerSpawnPoint1.position, Quaternion.identity, spawnedObjectsHolder).GetComponent<Identifier>();
        tower1.UpdateInfo(activePlayer.GetPlayerID, activePlayer.GetTeamID);
    }
    public void GiveResources()
    {
        PlayerResourceManager.PlayerResourceAmounts[activePlayer.GetPlayerID] += new ResourceAmount(10000, 10000, 10000);
    }

    private void DestroySpawnedItems ()
    {
        // destroy all children of spawnedOBjectsHolder
        for (int i = 0; i < spawnedObjectsHolder.childCount; i++)
        {
            Destroy(spawnedObjectsHolder.GetChild(i).gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && activePlayer == null)
        {
            activePlayer = other.GetComponent<Identifier>();
            activePlayer.GetComponent<PlayerBuilding>().SetTutorial(true);
            currentStep = 0;
            tutorialIdentifier.UpdateInfo(activePlayer.GetPlayerID, activePlayer.GetTeamID);

            PlayerHolder.DestroyAllBuildingsExceptFirst(activePlayer.GetPlayerID);
            PlayerHolder.DestroyAllUnits(activePlayer.GetPlayerID);
            PlayerResourceManager.PlayerResourceAmounts[activePlayer.GetPlayerID].SetResources(new ResourceAmount(0, 0, 0));

            GameObject resourcesInstance = Instantiate(startingResources, startingResources.transform.position, Quaternion.identity, spawnedObjectsHolder);
            resourcesInstance.SetActive(true);
        }

        // units on another team that try to enter, die
        if (other.TryGetComponent(out UnitActions unit))
        {
            if (unit.GetComponent<Identifier>().GetPlayerID != activePlayer.GetPlayerID && unit.GetComponent <Identifier>().GetPlayerID != -1)
                unit.Die();
        }

        if (other.TryGetComponent(out Building building))
            if (building.GetComponent<Identifier>().GetPlayerID != activePlayer.GetPlayerID && building.GetComponent <Identifier>().GetPlayerID != -1)
                building.Die();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<Identifier>() == activePlayer)
        {
            tutorialIdentifier.UpdateInfo(-1, -1);
            activePlayer.GetComponent<PlayerBuilding>().SetTutorial(false);
            activePlayer = null;
            DestroySpawnedItems();
        }
    }
        
}
