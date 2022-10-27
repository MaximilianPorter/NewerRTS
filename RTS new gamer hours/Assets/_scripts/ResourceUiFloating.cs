using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml;

public class ResourceUiFloating : MonoBehaviour
{
    [SerializeField] private float fadeTime = 3f;
    [SerializeField] private float moveUpSpeed = 3f;

    private float fadeCounter = 0f;

    private TMP_Text[] amountTexts;
    private Image[] images;

    private List<Color> textStartColors = new List<Color>();
    private List<Color> imageStartColors = new List<Color>();

    private Vector3 startWorldPos;
    private float moveUpCounter = 0f;
    private int playerCanvasID = -1;

    public bool isDoneFading => fadeCounter <= 0f;

    public void RestartFade ()
    {
        fadeCounter = fadeTime;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = imageStartColors[i];
        }

        for (int i = 0; i < amountTexts.Length; i++)
        {
            amountTexts[i].color = textStartColors[i];
        }

        transform.position = startWorldPos;
    }

    private void Start()
    {
        

        for (int i = 0; i < amountTexts.Length; i++)
        {
            textStartColors.Add(amountTexts[i].color);
        }

        for (int i = 0; i < images.Length; i++)
        {
            imageStartColors.Add(images[i].color);
        }
    }

    private void Update()
    {
        moveUpCounter += Time.deltaTime * moveUpSpeed;
        if (playerCanvasID != -1)
        {
            Vector2? localPos = PlayerHolder.WorldToCanvasLocalPoint(startWorldPos + transform.up * moveUpCounter, playerCanvasID);
            if (localPos == null)
            {
                Destroy(gameObject);
            }
            transform.localPosition = localPos.GetValueOrDefault();
            transform.localScale = Vector3.one * PlayerHolder.ScaleWithScreenOrthoSizeMultiplier(playerCanvasID);
        }

        //transform.position += transform.up * Time.deltaTime * moveUpSpeed;

        fadeCounter -= Time.deltaTime;

        for (int i = 0; i < images.Length; i++)
        {
            images[i].color = new Color(images[i].color.r, images[i].color.g, images[i].color.b, imageStartColors[i].a * (fadeCounter / fadeTime));
        }

        for (int i = 0; i < amountTexts.Length; i++)
        {
            amountTexts[i].color = new Color(amountTexts[i].color.r, amountTexts[i].color.g, amountTexts[i].color.b, textStartColors[i].a * (fadeCounter / fadeTime));
        }

    }
    public void SetDetails(ResourceAmount amount, Vector3 startWorldPos, int playerCanvasID = -1)
    {
        images = GetComponentsInChildren<Image>();
        amountTexts = GetComponentsInChildren<TMP_Text>();


        this.startWorldPos = startWorldPos;
        this.playerCanvasID = playerCanvasID;

        if (amount.GetFood <= 0)
            amountTexts[0].transform.parent.gameObject.SetActive(false);
        if (amount.GetWood <= 0)
            amountTexts[1].transform.parent.gameObject.SetActive(false);
        if (amount.GetStone <= 0)
            amountTexts[2].transform.parent.gameObject.SetActive(false);


        amountTexts[0].text = "+" + amount.GetFood.ToString();
        amountTexts[1].text = "+" + amount.GetWood.ToString();
        amountTexts[2].text = "+" + amount.GetStone.ToString();

        fadeCounter = fadeTime;
    }

}
