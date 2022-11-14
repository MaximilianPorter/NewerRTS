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
        ConfigManager.FetchCompleted -= UpdateUnitStats;
    }

    public static void SaveUnitStats(UnitStats stats)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + stats.unitType + ".gamer";
        FileStream stream = new FileStream(path, FileMode.Create);

        UnitStats data = new UnitStats(stats);

        formatter.Serialize (stream, data);
        stream.Close();
    }

    public static void LoadUnitStatsFromRemote (UnitStats statsToUpdate)
    {


        string path = Application.persistentDataPath + "/" + statsToUpdate.unitType + ".gamer";

        if (File.Exists(path))
        {
            var jsonUnitString = ConfigManager.appConfig.GetJson(statsToUpdate.unitType.ToString());
            JsonUtility.FromJsonOverwrite(jsonUnitString, statsToUpdate);

            SaveUnitStats(statsToUpdate);
            //BinaryFormatter formatter = new BinaryFormatter();
            //FileStream stream = new FileStream(path, FileMode.Open);

            //UnitStats data = formatter.Deserialize(stream) as UnitStats;



            //stream.Close();
        }
        else
        {
            // change data from statsToUpdate to data from config
            var jsonUnitString = ConfigManager.appConfig.GetJson(statsToUpdate.unitType.ToString());
            JsonUtility.FromJsonOverwrite(jsonUnitString, statsToUpdate);

            SaveUnitStats(statsToUpdate);
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
