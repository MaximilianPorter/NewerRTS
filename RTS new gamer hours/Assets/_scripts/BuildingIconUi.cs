using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingIconUi : MonoBehaviour
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private bool isAffordable = true;

    private Image[] images;

    public void SetAffordable (bool enabled) => isAffordable = enabled;

    private void Start()
    {
        images = GetComponentsInChildren<Image>();
    }

    private void Update()
    {
        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = isAffordable ? normalColor : disabledColor;
        }
    }
}
