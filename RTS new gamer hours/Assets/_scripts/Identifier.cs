using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class Identifier : MonoBehaviour
{
    [SerializeField] private bool isParent = false;
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private int playerID;
    [SerializeField] private int teamID;

    public Button.ButtonClickedEvent onChange = new Button.ButtonClickedEvent();

    private int lastPlayerID = -1;
    private int lastTeamID = -1;

    public void SetIsParent(bool isParent) => this.isParent = isParent;
    public bool GetIsParent => isParent;
    public bool GetIsPlayer => isPlayer;
    public int GetPlayerID => playerID;
    public int GetTeamID => teamID;

    private bool isTargetable = true;
    public bool GetIsTargetable => isTargetable;

    private void SetPlayerID (int newPlayerID)
    {
        playerID = newPlayerID;
        lastPlayerID = newPlayerID;
    }
    private void SetTeamID (int newTeamID)
    {
        teamID = newTeamID;
        lastTeamID = newTeamID;
    }

    private void Awake()
    {
        
    }

    private void Start()
    {

        if (!isParent)
        {

            if (transform.parent == null)
            {
                Debug.LogError("If " + gameObject.name + " is a parent, check isParent");
                return;
            }

            Identifier parentId = transform.parent.GetComponentInParent<Identifier>(true);
            this.SetPlayerID(parentId.playerID);
            this.SetTeamID(parentId.teamID);


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
            UpdateInfo(playerID, teamID);

        }



        if (TryGetComponent(out UnitActions attachedUnit))
            isTargetable = attachedUnit.GetIsTargetable;
        else if (TryGetComponent(out Building attachedBuilding))
            isTargetable = attachedBuilding.GetIsTargetable;
        
    }

    private void Update()
    {
        if (lastPlayerID != playerID || lastTeamID != teamID)
        {
            if (isParent)
                UpdateInfo(playerID, teamID);
        }
    }

    public void UpdateInfo (int playerID, int teamID)
    {
        Identifier[] childrenIdentifiers = GetComponentsInChildren<Identifier>(true);
        for (int i = 0; i < childrenIdentifiers.Length; i++)
        {
            childrenIdentifiers[i].SetPlayerID(playerID);
            childrenIdentifiers[i].SetTeamID(teamID);
            childrenIdentifiers[i].onChange.Invoke();
        }
    }

    public void UpdateTeamAndEverythingElse (int teamID)
    {
        //Debug.Log($"Something changed in the identifier for {gameObject.name}, updating values...");
        Identifier[] childrenIdentifiers = GetComponentsInChildren<Identifier>(true);
        for (int i = 0; i < childrenIdentifiers.Length; i++)
        {
            childrenIdentifiers[i].SetPlayerID(this.playerID);
            childrenIdentifiers[i].SetTeamID(teamID);
        }
    }
}