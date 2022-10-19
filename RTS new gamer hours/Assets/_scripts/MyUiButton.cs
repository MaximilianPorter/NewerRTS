using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyUiButton : MonoBehaviour
{
    [SerializeField] private ButtonType buttonType = ButtonType.Click;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color clickedColor;

    [SerializeField] private Image buttonImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private float timeToFill = 3f;

    [SerializeField] private Button.ButtonClickedEvent onComplete = new Button.ButtonClickedEvent();

    private float fillTimer = 0f;
    private bool isSelected = false;
    private bool isClicking = false;
    public void SelectButton() => isSelected = true;
    public void DeSelectButton() => isSelected = false;
    public void SetIsClicking(bool isClicking) => this.isClicking = isClicking;

    private void Update()
    {
        if (buttonType == ButtonType.Hold)
        {
            if (isClicking)
                fillTimer += Time.unscaledDeltaTime;
            else
                fillTimer = 0f;

            fillImage.fillAmount = fillTimer / timeToFill;
        }

        buttonImage.color = isClicking ? clickedColor : (isSelected ? selectedColor : normalColor);

        if (buttonType == ButtonType.Click)
        {
            if (isClicking)
                onComplete.Invoke();
        }
        else if (buttonType == ButtonType.Hold)
        {
            if (isClicking && fillTimer >= timeToFill)
            {
                fillTimer = 0f;
                onComplete.Invoke();
            }
        }
    }
}

[Serializable]
public enum ButtonType
{
    Click,
    Hold
}
