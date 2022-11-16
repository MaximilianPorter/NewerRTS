using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialStep : MonoBehaviour
{
    public string waitForAction;
    public BuyIcons waitForPlacedBuilding = BuyIcons.NONE;
    public ResourceAmount neededResources = new ResourceAmount();
    public BuyIcons requiredUnitType = BuyIcons.NONE;
    public int requiredUnitAmount = 0;
    public bool requireUnitsSelected = false;
    public float stepTimer = -1;
    public Button.ButtonClickedEvent onStartStep;
    public Button.ButtonClickedEvent onFinishStep;
}
