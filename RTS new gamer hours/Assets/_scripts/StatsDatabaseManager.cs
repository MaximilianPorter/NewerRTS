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

    private void Awake()
    {
        tempDatabaseValues = new UnitDatabaseValue[unitStats.Length][];
        for (int i = 0; i < tempDatabaseValues.GetLength(0); i++)
        {
            tempDatabaseValues[i] = new UnitDatabaseValue[4]
            {
                new UnitDatabaseValue ("Health", unitStats[i].health),
                new UnitDatabaseValue ("Damage", unitStats[i].damage),
                new UnitDatabaseValue ("Time Between Attacks", unitStats[i].timeBetweenAttacks),
                new UnitDatabaseValue ("Cost", new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone)),
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
        unitStats[i].health = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health").floatValue;

        // damage
        yield return StartCoroutine(UpdateUnitFloatDetails(i, unitStats[i], "Damage", unitStats[i].damage));
        unitStats[i].damage = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Damage").floatValue;

        // time between attacks
        yield return StartCoroutine(UpdateUnitFloatDetails(i, unitStats[i], "Time Between Attacks", unitStats[i].timeBetweenAttacks));
        unitStats[i].timeBetweenAttacks = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Time Between Attacks").floatValue;

        // cost
        yield return StartCoroutine(UpdateUnitVector3Details(i, unitStats[i], "Cost", new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone)));
        Vector3 costVector = tempDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Cost").vectorValue;
        unitStats[i].cost = new ResourceAmount((int)costVector.x, (int)costVector.y, (int)costVector.z);
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
            tempDatabaseValues[index].FirstOrDefault (databaseValue => databaseValue.statName == variableName).floatValue = value;
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

    public IEnumerator UpdateUnitVector3Details(int index, UnitStats stats, string variableName, Vector3 defaultValue)
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

            // remove things that I might add when entering new values into the database
            char[] charactersToTrim = { '{', '}', '(', ')', '[', ']' };
            string snapshotString = Convert.ToString(snapshot.Value).Trim(charactersToTrim);
            snapshotString = snapshotString.Replace(" ", "");

            // split the string by commas (there needs to be at least 2 for there not to be an error. Please manage that max, I ask for very little)
            float[] value = new float[3] {
                float.Parse (snapshotString.Split(',')[0]),
                float.Parse (snapshotString.Split(',')[1]),
                float.Parse (snapshotString.Split(',')[2]),
            };
            tempDatabaseValues[index].FirstOrDefault(databaseValue => databaseValue.statName == variableName).vectorValue = new Vector3(value[0], value[1], value[2]);
        }
        else
        {
            // if there wasn't a variable in the database, make the variable (THIS WILL ONLY RUN WITH 'WRITE'=TRUE IN THE DATABASE)
            Debug.LogError("no value for " + variableName + ", making one now...");
            string vectorToString = defaultValue.x + "," + defaultValue.y + "," + defaultValue.z;
            var SetDBTask = reference.Child("Units").Child(stats.unitType.ToString()).Child(variableName).SetValueAsync(vectorToString);

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
    public float floatValue = 0;
    public Vector3 vectorValue;

    public UnitDatabaseValue(string statName, float value)
    {
        this.statName = statName;
        this.floatValue = value;
    }

    public UnitDatabaseValue(string statName, Vector3 value)
    {
        this.statName = statName;
        this.vectorValue = value;
    }
}
