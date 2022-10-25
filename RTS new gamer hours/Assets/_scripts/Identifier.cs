using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Identifier : MonoBehaviour
{
    [SerializeField] private bool isParent = false;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private int playerID;
    [SerializeField] private int teamID;

    public void SetIsParent(bool isParent) => this.isParent = isParent;
    public bool GetIsParent => isParent;
    public bool GetIsPlayer => isPlayer;
    public int GetPlayerID => playerID;
    public int GetTeamID => teamID;

    private bool isTargetable = true;
    public bool GetIsTargetable => isTargetable;

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
        
    }

    private void Start()
    {

        if (!isParent)
        {
            //if (transform.parent == null)
            //{
            //    Debug.LogError("If " + gameObject.name + " is a parent, check isParent");
            //    return;
            //}


            //// find an identifier in parents
            //Transform currentCheckedTransform = transform.parent;

            //while (currentCheckedTransform)
            //{
            //    if (currentCheckedTransform.TryGetComponent(out Identifier identifier))
            //    {
            //        if (identifier.GetIsParent)
            //        {
            //            teamID = identifier.GetTeamID;
            //            playerID = identifier.GetPlayerID;
            //            break;

            //        }
            //    }

            //    if (currentCheckedTransform.parent == null)
            //    {
            //        Debug.LogError("Never found a Identifier with bool=isParent set to true");
            //        break;
            //    }

            //    currentCheckedTransform = currentCheckedTransform.parent;
            //}
        }
        else
        {
            Identifier[] childrenIdentifiers = GetComponentsInChildren<Identifier>(true);
            for (int i = 0; i < childrenIdentifiers.Length; i++)
            {
                childrenIdentifiers[i].SetPlayerID(playerID);
                childrenIdentifiers[i].SetTeamID(teamID);
            }

        }



        if (TryGetComponent(out UnitActions attachedUnit))
            isTargetable = attachedUnit.GetIsTargetable;
        else if (TryGetComponent(out Building attachedBuilding))
            isTargetable = attachedBuilding.GetIsTargetable;
        
    }
}