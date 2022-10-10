using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QueuedUpUnitUi : MonoBehaviour
{
    [SerializeField] private BuyIcons unitType;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image fillImage;

    private int unitAmt = 0;

    public int GetUnitAmt => unitAmt;
    public void SetDetails(int amount, float radialFill)
    {
        fillImage.fillAmount = radialFill;
        unitAmt = amount;
    }
    public BuyIcons GetUnitType => unitType;

    //public void RefundResources (int playerID)
    //{
    //    PlayerResourceManager.Food[playerID] += cost.GetFood;
    //    PlayerResourceManager.Wood[playerID] += cost.GetWood;
    //    PlayerResourceManager.Stone[playerID] += cost.GetStone;
    //}

    private void Update()
    {
        amountText.text = unitAmt.ToString();
    }
}
