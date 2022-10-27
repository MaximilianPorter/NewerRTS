using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class StatsDatabaseManager : MonoBehaviour
{
    [SerializeField] private bool read = true;
    [SerializeField] private bool write = false;

    [SerializeField] private UnitStats[] unitStats;
    [SerializeField] private BuildingStats[] buildingStats;

    private DatabaseValue[][] tempUnitDatabaseValues;
    private DatabaseValue[][] tempBuildingDatabaseValues;

    // the downloaded 'google-services.json' needs to be in Assets/StreamingAssets folder
    // if you don't have the download, you can find it online in your firebase project settings
    private DatabaseReference reference;

    private IEnumerator Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;


        PauseGameManager.ForcePause = true;

        bool isConnected = false;
        yield return StartCoroutine(CheckIsConnected(connected => isConnected = connected));
        if (!isConnected)
        {
            Debug.LogWarning("could not connect, continuing with game...");
            PauseGameManager.ForcePause = false;
            yield break;
        }



        InitializeTempArrays();


        for (int i = 0; i < unitStats.Length; i++)
        {
            yield return StartCoroutine(SetUnitDetails(i));
        }

        for (int i = 0; i < buildingStats.Length; i++)
        {
            yield return StartCoroutine(SetBuildingDetails(i));
        }

        PauseGameManager.ForcePause = false;
    }

    private void InitializeTempArrays ()
    {
        // initialize unit temp arrays with variables
        tempUnitDatabaseValues = new DatabaseValue[unitStats.Length][];
        for (int i = 0; i < tempUnitDatabaseValues.GetLength(0); i++)
        {
            tempUnitDatabaseValues[i] = new DatabaseValue[]
            {
                new DatabaseValue ("Health", unitStats[i].health),
                new DatabaseValue ("Damage", unitStats[i].damage),
                new DatabaseValue ("Armor", unitStats[i].armor),
                new DatabaseValue ("Move Speed", unitStats[i].maxMoveSpeed),
                new DatabaseValue ("Time Between Attacks", unitStats[i].timeBetweenAttacks),
                new DatabaseValue ("Range Look", unitStats[i].lookRange),
                new DatabaseValue ("Range Attack", unitStats[i].attackRange),
                new DatabaseValue ("Cost", new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone)),
            };
        }

        // initialize building temp arrays with variables
        tempBuildingDatabaseValues = new DatabaseValue[buildingStats.Length][];
        for (int i = 0; i < tempBuildingDatabaseValues.GetLength(0); i++)
        {
            tempBuildingDatabaseValues[i] = new DatabaseValue[]
            {
                new DatabaseValue ("Health", buildingStats[i].health),
                new DatabaseValue ("Cost", new Vector3(buildingStats[i].cost.GetFood, buildingStats[i].cost.GetWood, buildingStats[i].cost.GetStone)),
            };
        }
    }

    private IEnumerator SetUnitDetails(int i)
    {
        // health
        yield return StartCoroutine(UpdateFloatDetails("Units", "Health", unitStats[i].health, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue healthData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health");
        unitStats[i].health = healthData == null ? unitStats[i].health : healthData.floatValue;

        // damage
        yield return StartCoroutine(UpdateFloatDetails("Units", "Damage", unitStats[i].damage, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue damageData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Damage");
        unitStats[i].damage = damageData == null ? unitStats[i].damage : damageData.floatValue;

        // armor
        yield return StartCoroutine(UpdateFloatDetails("Units", "Armor", unitStats[i].armor, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue armorData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Armor");
        unitStats[i].armor = armorData == null ? unitStats[i].armor : armorData.floatValue;

        // move speed
        yield return StartCoroutine(UpdateFloatDetails("Units", "Move Speed", unitStats[i].maxMoveSpeed, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue maxMoveSpeedData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Move Speed");
        unitStats[i].maxMoveSpeed = maxMoveSpeedData == null ? unitStats[i].maxMoveSpeed : maxMoveSpeedData.floatValue;

        // time between attacks
        yield return StartCoroutine(UpdateFloatDetails("Units", "Time Between Attacks", unitStats[i].timeBetweenAttacks, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue timeBetweenAttacksData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Time Between Attacks");
        unitStats[i].timeBetweenAttacks = timeBetweenAttacksData == null ? unitStats[i].timeBetweenAttacks : timeBetweenAttacksData.floatValue;

        // look range
        yield return StartCoroutine(UpdateFloatDetails("Units", "Range Look", unitStats[i].lookRange, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue lookRangeData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Range Look");
        unitStats[i].lookRange = lookRangeData == null ? unitStats[i].lookRange : lookRangeData.floatValue;

        // attack range
        yield return StartCoroutine(UpdateFloatDetails("Units", "Range Attack", unitStats[i].attackRange, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue attackRangeData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Range Attack");
        unitStats[i].attackRange = attackRangeData == null ? unitStats[i].attackRange : attackRangeData.floatValue;

        // cost
        Vector3 costToVector3 = new Vector3(unitStats[i].cost.GetFood, unitStats[i].cost.GetWood, unitStats[i].cost.GetStone);
        yield return StartCoroutine(UpdateVector3Details("Units", "Cost", costToVector3, i, unitStats[i].unitType, tempUnitDatabaseValues));
        DatabaseValue costData = tempUnitDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Cost");
        unitStats[i].cost = costData == null ? unitStats[i].cost : new ResourceAmount((int)costData.vectorValue.x, (int)costData.vectorValue.y, (int)costData.vectorValue.z);
    }

    private IEnumerator SetBuildingDetails(int i)
    {
        // health
        yield return StartCoroutine(UpdateFloatDetails("Buildings", "Health", buildingStats[i].health, i, buildingStats[i].buildingType, tempBuildingDatabaseValues));
        DatabaseValue healthData = tempBuildingDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Health");
        buildingStats[i].health = healthData == null ? buildingStats[i].health : healthData.floatValue;

        // cost
        Vector3 costToVector3 = new Vector3(buildingStats[i].cost.GetFood, buildingStats[i].cost.GetWood, buildingStats[i].cost.GetStone);
        yield return StartCoroutine(UpdateVector3Details("Buildings", "Cost", costToVector3, i, buildingStats[i].buildingType, tempBuildingDatabaseValues));
        DatabaseValue costValue = tempBuildingDatabaseValues[i].FirstOrDefault(databaseValue => databaseValue.statName == "Cost");
        buildingStats[i].cost = costValue == null ? buildingStats[i].cost : new ResourceAmount((int)costValue.vectorValue.x, (int)costValue.vectorValue.y, (int)costValue.vectorValue.z);
    }

    public IEnumerator UpdateFloatDetails(string categoryName, string variableName, float defaultValue, int index, BuyIcons statsType, DatabaseValue[][] tempDatabase)
    {
        // try to get the value from the database
        var GetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).GetValueAsync();

        // wait for a response
        yield return new WaitUntil(predicate: () => GetDBTask.IsCompleted);
        
        if (GetDBTask.Result.Exists)
        {
            if (!read)
            {
                Debug.LogError($"READ is disabled, YOU NEED TO ENABLE READ ON {gameObject.name}");
            }

            // if we find a current existing value (there's no exceptions), set it from the database
            DataSnapshot snapshot = GetDBTask.Result;
            float value = float.Parse(Convert.ToString (snapshot.Value));
            Debug.Log("found a variable for " + variableName + " on " + statsType.ToString() + " with value " + value);

            // set the value to the tempDatabase to retrieve from later
            tempDatabase[index].FirstOrDefault (databaseValue => databaseValue.statName == variableName).floatValue = value;
        }
        else
        {
            if (!write)
            {
                Debug.LogError($"WRITE is disabled, no new value will be made for {variableName}");
                yield break;
            }

            // if there wasn't a variable in the database, make the variable (THIS WILL ONLY RUN IF 'WRITE'=TRUE IN THE DATABASE)
            Debug.LogWarning("No value for " + variableName + ", writing to the database now...");

            // try to create the value in the database
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

    public IEnumerator UpdateVector3Details(string categoryName, string variableName, Vector3 defaultValue, int index, BuyIcons statsType, DatabaseValue[][] tempDatabase)
    {
        // try to get the value from the database
        var GetDBTask = reference.Child(categoryName).Child(statsType.ToString()).Child(variableName).GetValueAsync();

        // wait for a response
        yield return new WaitUntil(predicate: () => GetDBTask.IsCompleted);

        if (GetDBTask.Result.Exists)
        {
            if (!read)
            {
                Debug.LogError($"READ is disabled, YOU NEED TO ENABLE READ ON {gameObject.name}");
            }
            // if we find a current existing value (there's no exceptions), set it FROM the database
            DataSnapshot snapshot = GetDBTask.Result;
            Debug.Log("found a variable for " + variableName + " on " + statsType.ToString() + " with value " + Convert.ToString(snapshot.Value));

            // remove things that I might add to vector3's when entering new values into the database
            char[] charactersToTrim = { '{', '}', '(', ')', '[', ']' };
            string snapshotString = Convert.ToString(snapshot.Value).Trim(charactersToTrim);
            snapshotString = snapshotString.Replace(" ", "");

            // split the string by commas (there needs to be at least 2 for there not to be an error. Please manage that, I ask for very little)
            float[] values = new float[3] {
                float.Parse (snapshotString.Split(',')[0]),
                float.Parse (snapshotString.Split(',')[1]),
                float.Parse (snapshotString.Split(',')[2]),
            };

            // set the value to the tempDatabase to retrieve from later
            tempDatabase[index].FirstOrDefault(databaseValue => databaseValue.statName == variableName).vectorValue = new Vector3(values[0], values[1], values[2]);
        }
        else
        {

            if (!write)
            {
                Debug.LogError($"WRITE is disabled, no new value will be made for {variableName}");
                yield break;
            }

            // if there wasn't a variable in the database, make the variable (THIS WILL ONLY RUN IF 'WRITE'=TRUE IN THE DATABASE)
            Debug.LogWarning("no value for " + variableName + ", making one now...");

            // convert the given default vector into a string WITH commas in between
            string vectorToString = defaultValue.x + "," + defaultValue.y + "," + defaultValue.z;

            // try to create the value in the database
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


    private IEnumerator CheckIsConnected(System.Action<bool> callback)
    {
        bool isConnected = false;
        //DatabaseReference connectedRef = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");
        var task = reference.GetValueAsync().ContinueWith (task =>
        {
            isConnected = task.IsCompletedSuccessfully;
        });

        Debug.Log("start wait for task completion...");
        yield return new WaitForSecondsRealtime(3f);// (predicate: () => task.IsCompleted || task.IsFaulted || task.IsCanceled);

        if (isConnected)
            Debug.Log("connected.");
        else
            Debug.Log("NOT connected.");

        yield return null;
        callback(isConnected);

    }
}

[Serializable]
public class DatabaseValue
{
    public string statName = "Stat Name";
    public float floatValue;
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
