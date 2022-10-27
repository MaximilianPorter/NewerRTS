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
    [SerializeField] private int colorID;

    private int lastPlayerID = -1;
    private int lastTeamID = -1;
    private int lastColorID = -1;

    public void SetIsParent(bool isParent) => this.isParent = isParent;
    public bool GetIsParent => isParent;
    public bool GetIsPlayer => isPlayer;
    public int GetPlayerID => playerID;
    public int GetTeamID => teamID;
    public int GetColorID => colorID;

    private bool isTargetable = true;
    public bool GetIsTargetable => isTargetable;

    public void SetPlayerID (int newPlayerID)
    {
        playerID = newPlayerID;
        lastPlayerID = newPlayerID;
    }
    public void SetTeamID (int newTeamID)
    {
        teamID = newTeamID;
        lastTeamID = newTeamID;
    }
    public void SetColorID (int newColorID)
    {
        colorID = newColorID;
        lastColorID = newColorID;
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
            UpdateInfo(playerID, teamID, colorID);

        }



        if (TryGetComponent(out UnitActions attachedUnit))
            isTargetable = attachedUnit.GetIsTargetable;
        else if (TryGetComponent(out Building attachedBuilding))
            isTargetable = attachedBuilding.GetIsTargetable;
        
    }

    private void Update()
    {
        if (lastPlayerID != playerID || lastTeamID != teamID || lastColorID != colorID)
        {
            if (isParent)
                UpdateInfo(playerID, teamID, colorID);
        }
    }

    public void UpdateInfo (int playerID, int teamID, int colorID)
    {
        Debug.Log($"Something changed in the identifier for {gameObject.name}, updating values...");
        Identifier[] childrenIdentifiers = GetComponentsInChildren<Identifier>(true);
        for (int i = 0; i < childrenIdentifiers.Length; i++)
        {
            childrenIdentifiers[i].SetPlayerID(playerID);
            childrenIdentifiers[i].SetTeamID(teamID);
            childrenIdentifiers[i].SetColorID(colorID);
        }
    }
}