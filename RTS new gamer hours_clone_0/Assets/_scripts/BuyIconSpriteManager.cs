using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuyIconSpriteManager : MonoBehaviour
{
    private static List<TypeAndSprite> typesOfIconsStatic = new List<TypeAndSprite>();
    public static void AddTypeOfSprite(BuyIcons typeOfIcon, Sprite icon)
    {
        TypeAndSprite newOne = new TypeAndSprite();
        newOne.typeOfIcon = typeOfIcon;
        newOne.icon = icon;
        typesOfIconsStatic.Add(newOne);
    }
    public static Sprite GetTypeOfIcon(BuyIcons typeOfIcon)
    {
        return typesOfIconsStatic.FirstOrDefault(icon => icon.typeOfIcon == typeOfIcon).icon;
    }

    [Serializable]
    private struct TypeAndSprite
    {
        public BuyIcons typeOfIcon;
        public Sprite icon;
    }
}
