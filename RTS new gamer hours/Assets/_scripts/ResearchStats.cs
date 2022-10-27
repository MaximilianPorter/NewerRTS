using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Research Name", menuName = "Create Research Stats")]
public class ResearchStats : ScriptableObject
{
    public BuyIcons researchType = BuyIcons.NONE;
    public float timeToResearch = 60f;
    public ResourceAmount cost;

    [Tooltip("These buildings need to be built in order to buy this building, MAX OF 4")]
    [SerializeField] private BuyIcons[] requiredBuildings = new BuyIcons[] { BuyIcons.Building_ResearchLab };
    public BuyIcons[] GetRequiredBuildings => requiredBuildings;
}
