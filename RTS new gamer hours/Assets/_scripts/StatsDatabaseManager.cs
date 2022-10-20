using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

public class StatsDatabaseManager : MonoBehaviour
{
    [SerializeField] private UnitStats[] unitStats;

    public UnitDatabaseValue[][] tempDatabaseValues;

    private DatabaseReference reference;

    private void Start()
    {
        tempDatabaseValues = new UnitDatabaseValue[unitStats.Length][];
        for (int i = 0; i < tempDatabaseValues.GetLength(0); i++)
        {
            tempDatabaseValues[i] = new UnitDatabaseValue[3]
            {
                new UnitDatabaseValue ("Health", unitStats[i].health),
                new UnitDatabaseValue ("Damage", unitStats[i].damage),
                new UnitDatabaseValue ("Time Between Attacks", unitStats[i].timeBetweenAttacks)
            };
        }

        reference = FirebaseDatabase.DefaultInstance.RootReference;

        for (int i = 0; i < unitStats.Length; i++)
        {
            StartCoroutine(SetUnitDetails(i));
        }
    }

    private IEnumerator SetUnitDetails(int i)
    {
        // health
        yield return StartCoroutine(UpdateUnitFloatDetails(i, unitStats[i], "Health", unitStats[i].health));
        unitStats[i].health = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health").value;

        // damage
        yield return StartCoroutine(UpdateUnitFloatDetails(i, unitStats[i], "Damage", unitStats[i].damage));
        unitStats[i].damage = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Damage").value;

        // time between attacks
        yield return StartCoroutine(UpdateUnitFloatDetails(i, unitStats[i], "Time Between Attacks", unitStats[i].timeBetweenAttacks));
        unitStats[i].timeBetweenAttacks = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Time Between Attacks").value;
    }

    public IEnumerator UpdateUnitFloatDetails(int index, UnitStats stats, string variableName, float defaultValue)
    {
        // try to get the value from the database
        var GetDBTask = reference.Child("Units").Child(stats.unitType.ToString()).Child(variableName).GetValueAsync();

        // wait for a response
        yield return new WaitUntil(predicate: () => GetDBTask.IsCompleted);
        
        if (GetDBTask.Result.Exists)
        {
            // if we find a current existing value (there's no exceptions), set it from the database
            Debug.Log("found a variable for " + variableName + " getting it's value...");
            DataSnapshot snapshot = GetDBTask.Result;
            float value = float.Parse(Convert.ToString (snapshot.Value));
            //stats.health = value;
            tempDatabaseValues[index].FirstOrDefault (databaseValue => databaseValue.statName == variableName).value = value;
        }
        else
        {
            // if there wasn't a variable in the database, make the variable
            Debug.LogError("no value for " + variableName + ", making one now...");
            var SetDBTask = reference.Child("Units").Child(stats.unitType.ToString()).Child(variableName).SetValueAsync(defaultValue);

            yield return new WaitUntil(predicate: () => SetDBTask.IsCompleted);

            if (SetDBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {SetDBTask.Exception}");
            }
            else
            {
                // database is now updated
            }
        }
    }
}

[Serializable]
public class UnitDatabaseValue
{
    public string statName = "Stat Name";
    public float value = 0;

    public UnitDatabaseValue(string statName, float value)
    {
        this.statName = statName;
        this.value = value;
    }
}
