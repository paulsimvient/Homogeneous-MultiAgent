using UnityEngine;
using Type = System.Type;
using System.IO;
using FullSerializer;

public static class Serialization {
    public static void Save <T> (string path, T obj) {
        string fullPath = GetFullResourcesPath (path);
        // create serialized data
        string serialized = StringSerializationAPI.Serialize (typeof(T), obj);

        #if UNITY_EDITOR
        // create resources folder
        string pathParentName = Directory.GetParent(fullPath).FullName;
        if (! Directory.Exists(pathParentName))
            Directory.CreateDirectory(pathParentName);

        // write to file
        File.WriteAllText (GetFullResourcesPath(path) +".txt", serialized);

        // refresh project window
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    /// Returns a generic object that is serialized to a Resources folder path.
    public static T Load <T> (string resourcesLocalPath) {
        // get serialized data from file
        TextAsset serialized = (TextAsset)Resources.Load (resourcesLocalPath, typeof(TextAsset));

        if (serialized == null) {
            Debug.LogWarning ("No save file found to load: " + GetFullResourcesPath (resourcesLocalPath));
            return default(T);
        }

        // return deserialized data
        return (T) StringSerializationAPI.Deserialize (typeof(T), serialized.text);
    }

    /// Returns the Application.dataPath + Resources folder.
    public static string GetFullResourcesPath (string path) {
        string p = Application.dataPath;
        p = Path.Combine (p, "Resources");
        return Path.Combine (p, path);
    }
}

public static class StringSerializationAPI {
    private static readonly fsSerializer _serializer = new fsSerializer();

    public static string Serialize(Type type, object value) {
        // serialize the data
        fsData data;
        _serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

        // emit the data via JSON
        return fsJsonPrinter.CompressedJson(data);
    }

    public static object Deserialize(Type type, string serializedState) {
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(serializedState);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }
}