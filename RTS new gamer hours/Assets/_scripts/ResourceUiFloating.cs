using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUiFloating : MonoBehaviour
{
    [SerializeField] private float fadeTime = 3f;
    [SerializeField] private float moveUpSpeed = 3f;


    private TMP_Text[] amountTexts;
    private Image[] images;

    private void Update()
    {
        transform.position += new Vector3(0f, Time.deltaTime * moveUpSpeed);

        foreach (Image image in images)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - Time.deltaTime * fadeTime);
        }

        foreach (TMP_Text text in amountTexts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - Time.deltaTime * fadeTime);
        }
    }
    public void SetAmount(ResourceAmount amount)
    {
        images = GetComponentsInChildren<Image>();
        amountTexts = GetComponentsInChildren<TMP_Text>();


        if (amount.GetFood <= 0)
            amountTexts[0].transform.parent.gameObject.SetActive(false);
        if (amount.GetWood <= 0)
            amountTexts[1].transform.parent.gameObject.SetActive(false);
        if (amount.GetStone <= 0)
            amountTexts[2].transform.parent.gameObject.SetActive(false);


        amountTexts[0].text = "+" + amount.GetFood.ToString();
        amountTexts[1].text = "+" + amount.GetWood.ToString();
        amountTexts[2].text = "+" + amount.GetStone.ToString();
    }

}
