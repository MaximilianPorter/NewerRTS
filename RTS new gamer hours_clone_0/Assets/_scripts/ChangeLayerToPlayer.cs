using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeLayerToPlayer : MonoBehaviour
{
    private Identifier identifier;

    private void Start()
    {
        identifier = GetComponentInParent<Identifier>();
        SetUnitSpecificPlayerLayers(identifier.GetPlayerID);
    }
    public void SetUnitSpecificPlayerLayers(int playerID)
    {
        // change layer of self
        transform.gameObject.layer = RuntimeLayerController.GetLayer(playerID);

        // change layer of children
        Transform[] children = GetComponentsInChildren<Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            children[i].gameObject.layer = RuntimeLayerController.GetLayer(playerID);
        }
    }
}
