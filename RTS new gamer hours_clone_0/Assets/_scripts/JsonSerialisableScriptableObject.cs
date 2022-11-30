using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

// from github: https://gist.github.com/sebtoun/205d455bbb9fedc5d6b976c4e588c03f
public class JsonSerialisableScriptableObject<T> : ScriptableObject where T : JsonSerialisableScriptableObject<T>
{
    public void LoadJsonFromFile(string filename)
    {
        Debug.Log(string.Format("[{0}] Read file {1}", typeof(T).Name, filename));
        LoadJsonFromText(File.ReadAllText(filename));
    }

    public void LoadJsonFromText(string json)
    {
        var settings = new JsonSerializerSettings()
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };
        JsonConvert.PopulateObject(json, this, settings);
        OnPostJsonDeserialization();
    }

    public string DumpJson()
    {
        var settings = new JsonSerializerSettings()
        {
            ContractResolver = new DerivedTypeOnlyContractResolver<T>(),
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };
        return JsonConvert.SerializeObject(this, settings);
    }

    public void DumpJsonToFile(string filename)
    {
        Debug.Log(string.Format("[{0}] Write file {1}", typeof(T).Name, filename));
        File.WriteAllText(filename, DumpJson());
    }

    protected virtual void OnPostJsonDeserialization()
    {
    }

    private class DerivedTypeOnlyContractResolver<T> : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            if (!typeof(T).IsAssignableFrom(type))
            {
                return properties;
            }
            else
            {
                return properties.Where(property => property.DeclaringType == typeof(T)).ToList();
            }
        }
    }
}