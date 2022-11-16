using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (Image))]
public class ChangeImageToBuyIcon : MonoBehaviour
{
    [SerializeField] private BuyIcons spriteType = BuyIcons.NONE;
    private Image image;

    private void Update()
    {
        image.sprite = BuyIconSpriteManager.GetTypeOfIcon(spriteType);
    }
}
