using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{

    // THIS SCRIPT IS APPLIED TO ANYTHING THAT WOULD CHANGE COLORS WITH TEAMS

    [Tooltip("Doesn't matter if there's a Identifier.cs in the parent or self")]
    [SerializeField] private int teamID;
    [SerializeField] private int[] meshRendMaterialChange;

    private void Start()
    {
        // find an identifier in self or parents
        Transform currentCheckedTransform = transform;
        while (currentCheckedTransform)
        {
            if (currentCheckedTransform.TryGetComponent (out Identifier identifier))
            {
                teamID = identifier.GetTeamID;
                break;
            }

            if (currentCheckedTransform.parent == null)
                break;

            currentCheckedTransform = currentCheckedTransform.parent;
        }



        // SPRITE REND COLOR
        if (TryGetComponent(out SpriteRenderer sRend))
        {
            Color colorNoAlpha = new Color(PlayerColorManager.GetPlayerColor(teamID).r,
            PlayerColorManager.GetPlayerColor(teamID).g,
            PlayerColorManager.GetPlayerColor(teamID).b,
            sRend.color.a);

            sRend.color = colorNoAlpha;
        }



        // IMAGE COLOR
        if (TryGetComponent(out Image iRend))
        {
            Color colorNoAlpha = new Color(PlayerColorManager.GetPlayerColor(teamID).r,
            PlayerColorManager.GetPlayerColor(teamID).g,
            PlayerColorManager.GetPlayerColor(teamID).b,
            iRend.color.a);

            iRend.color = colorNoAlpha;
        }



        // PARTICLE COLOR
        if (TryGetComponent (out ParticleSystem pRend))
        {
            Color colorNoAlpha = new Color(PlayerColorManager.GetPlayerColor(teamID).r,
            PlayerColorManager.GetPlayerColor(teamID).g,
            PlayerColorManager.GetPlayerColor(teamID).b,
            pRend.main.startColor.color.a);

            ParticleSystem.MainModule main = pRend.main;
            main.startColor = colorNoAlpha;
        }



        // MATERIAL
        if (TryGetComponent(out MeshRenderer mRend))
        {
            Material[] mats = mRend.materials;
            if (meshRendMaterialChange.Length <= 0)
                mats[0] = PlayerColorManager.GetPlayerMaterial(teamID);
            for (int i = 0; i < meshRendMaterialChange.Length; i++)
            {
                mats[meshRendMaterialChange[i]] = PlayerColorManager.GetPlayerMaterial(teamID);
            }
            mRend.materials = mats;
        }

        if (TryGetComponent (out SkinnedMeshRenderer smRend))
        {
            Material[] mats = smRend.materials;
            if (meshRendMaterialChange.Length <= 0)
                mats[0] = PlayerColorManager.GetPlayerMaterial(teamID);
            for (int i = 0; i < meshRendMaterialChange.Length; i++)
            {
                mats[meshRendMaterialChange[i]] = PlayerColorManager.GetPlayerMaterial(teamID);
            }
            smRend.materials = mats;
        }

        // SPRITE REND COLOR
        if (TryGetComponent(out TMP_Text textRend))
        {
            Color colorNoAlpha = new Color(PlayerColorManager.GetPlayerColor(teamID).r,
            PlayerColorManager.GetPlayerColor(teamID).g,
            PlayerColorManager.GetPlayerColor(teamID).b,
            textRend.color.a);

            textRend.color = colorNoAlpha;
        }


        // PROJECTOR
        if (TryGetComponent (out Projector projRend))
        {
            projRend.material = PlayerColorManager.GetPlayerProjectorMaterial (teamID);
        }
    }
}
