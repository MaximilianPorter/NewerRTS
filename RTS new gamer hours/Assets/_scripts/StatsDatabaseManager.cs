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
    [SerializeField] private BuildingStats[] buildingStats;

    private DatabaseValue[][] tempUnitDatabaseValues;
    private DatabaseValue[][] tempBuildingDatabaseValues;

    private DatabaseReference reference;

    private void Start()
    {
        InitializeTempArrays();

        reference = FirebaseDatabase.DefaultInstance.RootReference;

        for (int i = 0; i < unitStats.Length; i++)
        {
            StartCoroutine(SetUnitDetails(i));
        }

        for (int i = 0; i < buildingStats.Length; i++)
        {
            StartCoroutine(SetBuildingDetails(i));
        }
    }

    private void InitializeTempArrays ()
    {
        // initialize unit temp arrays
        tempUnitDatabaseValues = new DatabaseValue[unitStats.Length][];
        for (int i = 0; i < tempUnitDatabaseValues.GetLength(0); i++)
        {
            tempUnitDatabaseValues[i] = new DatabaseValue[5]
            {
                new DatabaseValue ("Health", unitStats[i].health),
                new DatabaseValue ("Damage", unitStats[i].damage),
                new DatabaseValue ("Move Speed", unitStats[i].maxMoveSpeed),
                new DatabaseValue ("Time Between Attacks", unitStats[i].timeBetweenAttacks),
                new DatabaseValue ("Cost", new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone)),
            };
        }

        // initialize building temp arrays
        tempBuildingDatabaseValues = new DatabaseValue[buildingStats.Length][];
        for (int i = 0; i < tempBuildingDatabaseValues.GetLength(0); i++)
        {
            tempBuildingDatabaseValues[i] = new DatabaseValue[2]
            {
                new DatabaseValue ("Health", buildingStats[i].health),
                new DatabaseValue ("Cost", new Vector3(buildingStats[i].cost.GetFood, buildingStats[i].cost.GetWood, buildingStats[i].cost.GetStone)),
            };
        }
    }

    private IEnumerator SetUnitDetails(int i)
    {
        // health
        yield return StartCoroutine(UpdateUnitFloatDetails("Units", "Health", unitStats[i].health, i, unitStats[i].unitType, tempUnitDatabaseValues));
        unitStats[i].health = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health").floatValue;

        // damage
        yield return StartCoroutine(UpdateUnitFloatDetails("Units", "Damage", unitStats[i].damage, i, unitStats[i].unitType, tempUnitDatabaseValues));
        unitStats[i].damage = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Damage").floatValue;

        // move speed
        yield return StartCoroutine(UpdateUnitFloatDetails("Units", "Move Speed", unitStats[i].maxMoveSpeed, i, unitStats[i].unitType, tempUnitDatabaseValues));
        unitStats[i].maxMoveSpeed = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Move Speed").floatValue;

        // time between attacks
        yield return StartCoroutine(UpdateUnitFloatDetails("Units", "Time Between Attacks", unitStats[i].timeBetweenAttacks, i, unitStats[i].unitType, tempUnitDatabaseValues));
        unitStats[i].timeBetweenAttacks = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Time Between Attacks").floatValue;

        // cost
        Vector3 costToVector3 = new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone);
        yield return StartCoroutine(UpdateUnitVector3Details("Units", "Cost", costToVector3, i, unitStats[i].unitType, tempUnitDatabaseValues));
        Vector3 costVector = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Cost").vectorValue;
        unitStats[i].cost = new ResourceAmount((int)costVector.x, (int)costVector.y, (int)costVector.z);
    }

    private IEnumerator SetBuildingDetails(int i)
    {
        // health
        yield return StartCoroutine(UpdateUnitFloatDetails("Buildings", "Health", buildingStats[i].health, i, buildingStats[i].buildingType, tempBuildingDatabaseValues));
        buildingStats[i].health = tempBuildingDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health").floatValue;

        // cost
        Vector3 costToVector3 = new Vector3(buildingStats[i].cost.GetFood, buildingStats[i].cost.GetWood, buildingStats[i].cost.GetStone);
        yield return StartCoroutine(UpdateUnitVector3Details("Buildings", "Cost", costToVector3, i, buildingStats[i].buildingType, tempBuildingDatabaseValues));
        Vector3 costVector = tempBuildingDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Cost").vectorValue;
        buildingStats[i].cost = new ResourceAmount((int)costVector.x, (int)costVector.y, (int)costVector.z);
    }

    public IEnumerator UpdateUnitFloatDetails(string categoryName, string variableName, float defaultValue, int index, BuyIcons statsType, DatabaseValue[][] tempDatabase)
    {
        // try to get the value from the database
        var GetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).GetValueAsync();

        // wait for a response
        yield return new WaitUntil(predicate: () => GetDBTask.IsCompleted);
        
        if (GetDBTask.Result.Exists)
        {
            // if we find a current existing value (there's no exceptions), set it from the database
            Debug.Log("found a variable for " + variableName + " on " + statsType.ToString() + " getting it's value...");
            DataSnapshot snapshot = GetDBTask.Result;
            float value = float.Parse(Convert.ToString (snapshot.Value));

            // set the value to the tempDatabase to retrieve from later
            tempDatabase[index].FirstOrDefault (databaseValue => databaseValue.statName == variableName).floatValue = value;
        }
        else
        {
            // if there wasn't a variable in the database, make the variable (THIS WILL ONLY RUN IF 'WRITE'=TRUE IN THE DATABASE)
            Debug.LogError("no value for " + variableName + ", making one now...");

            // try to set the value from stats to the database for the first time
            var SetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).SetValueAsync(defaultValue);

            // wait for the setting action to be completed
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

    public IEnumerator UpdateUnitVector3Details(string categoryName, string variableName, Vector3 defaultValue, int index, BuyIcons statsType, DatabaseValue[][] tempDatabase)
    {
        // try to get the value from the database
        var GetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).GetValueAsync();

        // wait for a response
        yield return new WaitUntil(predicate: () => GetDBTask.IsCompleted);

        if (GetDBTask.Result.Exists)
        {
            // if we find a current existing value (there's no exceptions), set it FROM the database
            Debug.Log("found a variable for " + variableName + " on " + statsType.ToString() + " getting it's value...");
            DataSnapshot snapshot = GetDBTask.Result;

            // remove things that I might add to vector3's when entering new values into the database
            char[] charactersToTrim = { '{', '}', '(', ')', '[', ']' };
            string snapshotString = Convert.ToString(snapshot.Value).Trim(charactersToTrim);
            snapshotString = snapshotString.Replace(" ", "");

            // split the string by commas (there needs to be at least 2 for there not to be an error. Please manage that max, I ask for very little)
            float[] value = new float[3] {
                float.Parse (snapshotString.Split(',')[0]),
                float.Parse (snapshotString.Split(',')[1]),
                float.Parse (snapshotString.Split(',')[2]),
            };

            // set the value to the tempDatabase to retrieve from later
            tempDatabase[index].FirstOrDefault(databaseValue => databaseValue.statName == variableName).vectorValue = new Vector3(value[0], value[1], value[2]);
        }
        else
        {
            // if there wasn't a variable in the database, make the variable (THIS WILL ONLY RUN IF 'WRITE'=TRUE IN THE DATABASE)
            Debug.LogError("no value for " + variableName + ", making one now...");

            // convert the default vector given into a string with commas in between
            string vectorToString = defaultValue.x + "," + defaultValue.y + "," + defaultValue.z;

            // try to set the value from stats to the database for the first time
            var SetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).SetValueAsync(vectorToString);

            // wait for the setting action to be completed
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
public class DatabaseValue
{
    public string statName = "Stat Name";
    public float floatValue = 0;
    public Vector3 vectorValue;

    public DatabaseValue(string statName, float value)
    {
        this.statName = statName;
        this.floatValue = value;
    }

    public DatabaseValue(string statName, Vector3 value)
    {
        this.statName = statName;
        this.vectorValue = value;
    }
}
