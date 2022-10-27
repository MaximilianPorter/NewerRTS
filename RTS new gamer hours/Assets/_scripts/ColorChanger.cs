using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{

    // THIS SCRIPT IS APPLIED TO ANYTHING THAT WOULD CHANGE COLORS WITH TEAMS

    //[Tooltip("Doesn't matter if there's a Identifier.cs in the parent or self")]
    //[SerializeField] private int colorID;
    //[SerializeField] private int playerID;
    [SerializeField] private int[] meshRendMaterialChange;

    private void Start()
    {
        // find an identifier in self or parents
        Transform currentCheckedTransform = transform;
        while (currentCheckedTransform)
        {
            if (currentCheckedTransform.TryGetComponent (out Identifier identifier))
            {
                ChangeColor(identifier.GetColorID, identifier.GetPlayerID);
                break;
            }

            if (currentCheckedTransform.parent == null)
                break;

            currentCheckedTransform = currentCheckedTransform.parent;
        }


        
    }

    public void ChangeColor (int colorID, int playerID)
    {
        // SPRITE REND COLOR
        if (TryGetComponent(out SpriteRenderer sRend))
        {
            sRend.color = ColorIgnoreAlpha(sRend.color, colorID);
        }



        // IMAGE COLOR
        if (TryGetComponent(out Image iRend))
        {
            iRend.color = ColorIgnoreAlpha(iRend.color, colorID);
        }

        // LINE RENDERER COLOR
        if (TryGetComponent(out LineRenderer lineRend))
        {
            Color colorNoAlpha = ColorIgnoreAlpha(lineRend.startColor, colorID);

            lineRend.startColor = colorNoAlpha;
            lineRend.endColor = colorNoAlpha;
        }



        // PARTICLE COLOR
        if (TryGetComponent(out ParticleSystem pRend))
        {
            ParticleSystem.MainModule main = pRend.main;
            main.startColor = ColorIgnoreAlpha(pRend.main.startColor.color, colorID);
        }

        // SPRITE REND COLOR
        if (TryGetComponent(out TMP_Text textRend))
        {
            textRend.color = ColorIgnoreAlpha(textRend.color, colorID);
        }


        // MATERIAL
        if (TryGetComponent(out MeshRenderer mRend))
        {
            Material[] mats = mRend.materials;
            if (meshRendMaterialChange.Length <= 0)
                mats[0] = PlayerColorManager.GetPlayerMaterial(playerID);
            for (int i = 0; i < meshRendMaterialChange.Length; i++)
            {
                mats[meshRendMaterialChange[i]] = PlayerColorManager.GetPlayerMaterial(playerID);
            }
            mRend.materials = mats;
        }

        if (TryGetComponent(out SkinnedMeshRenderer smRend))
        {
            Material[] mats = smRend.materials;
            if (meshRendMaterialChange.Length <= 0)
                mats[0] = PlayerColorManager.GetPlayerMaterial(playerID);
            for (int i = 0; i < meshRendMaterialChange.Length; i++)
            {
                mats[meshRendMaterialChange[i]] = PlayerColorManager.GetPlayerMaterial(playerID);
            }
            smRend.materials = mats;
        }



        // PROJECTOR
        if (TryGetComponent(out Projector projRend))
        {
            projRend.material = PlayerColorManager.GetPlayerProjectorMaterial(playerID);
        }
    }

    private Color ColorIgnoreAlpha (Color color, int colorID)
    {
        return new Color(PlayerColorManager.GetPlayerColor(colorID).r,
            PlayerColorManager.GetPlayerColor(colorID).g,
            PlayerColorManager.GetPlayerColor(colorID).b,
            color.a);
    }
}
