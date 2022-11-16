using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.RemoteConfig;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public struct userAttributes { };
    public struct appAttributes { };

    [SerializeField] private UnitStats[] savedUnitStats;

    private void Awake()
    {
        //ConfigManager.FetchCompleted += UpdateUnitStats;
        //ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
    }

    private void OnDestroy()
    {
        //ConfigManager.FetchCompleted -= UpdateUnitStats;
    }

    public static void SaveUnitStatsToJson(UnitStats stats)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + stats.unitType + ".json";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize (stream, JsonUtility.ToJson(stats));
        stream.Close();
    }

    public static void LoadUnitStatsFromRemote (UnitStats statsToUpdate)
    {


        string path = Application.persistentDataPath + "/" + statsToUpdate.unitType + ".gamer";

        if (File.Exists(path))
        {
            var jsonUnitString = ConfigManager.appConfig.GetJson(statsToUpdate.unitType.ToString());
            JsonUtility.FromJsonOverwrite(jsonUnitString, statsToUpdate);

            SaveUnitStatsToJson(statsToUpdate);
        }
        else
        {
            // change data from statsToUpdate to data from config
            var jsonUnitString = ConfigManager.appConfig.GetJson(statsToUpdate.unitType.ToString());
            JsonUtility.FromJsonOverwrite(jsonUnitString, statsToUpdate);

            SaveUnitStatsToJson(statsToUpdate);
            Debug.LogError("Save file not found in " + path);
        }


    }

    public static void LoadUnitStatsFromJson (UnitStats stats)
    {
        string path = Application.persistentDataPath + "/" + stats.unitType + ".json";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            //UnitStats data = formatter.Deserialize(stream) as UnitStats;

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(formatter.Deserialize(stream)), stats);

            stream.Close();
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
        }
    }

    private void UpdateUnitStats (ConfigResponse response)
    {
        for (int i = 0; i < savedUnitStats.Length; i++)
        {
            LoadUnitStatsFromRemote(savedUnitStats[i]);
        }
    }
}
