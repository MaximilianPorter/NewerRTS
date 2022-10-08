using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

public class Identifier : MonoBehaviour
{
    [SerializeField] private bool isParent = false;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private int playerID;
    [SerializeField] private int teamID;

    public bool GetIsParent => isParent;
    public bool GetIsPlayer => isPlayer;
    public int GetPlayerID => playerID;
    public int GetTeamID => teamID;

    public void SetPlayerID (int newPlayerID)
    {
        playerID = newPlayerID;
    }
    public void SetTeamID (int newTeamID)
    {
        teamID = newTeamID;
    }

    private void Awake()
    {
        if (!isParent)
        {
            if (transform.parent == null)
            {
                Debug.LogError("If this gameobject is a parent, check isParent");
                return;
            }


            // find an identifier in parents
            Transform currentCheckedTransform = transform.parent;

            while (currentCheckedTransform)
            {
                if (currentCheckedTransform.TryGetComponent(out Identifier identifier))
                {
                    if (identifier.GetIsParent)
                    {
                        teamID = identifier.GetTeamID;
                        playerID = identifier.GetPlayerID;
                        break;

                    }
                }

                if (currentCheckedTransform.parent == null)
                {
                    Debug.LogError("Never found a Identifier with bool=isParent set to true");
                    break;
                }

                currentCheckedTransform = currentCheckedTransform.parent;
            }
        }
    }
}