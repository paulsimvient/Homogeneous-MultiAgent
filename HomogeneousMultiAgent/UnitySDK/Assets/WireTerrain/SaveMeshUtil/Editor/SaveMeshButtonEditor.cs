using UnityEngine;
using System.Collections;
using UnityEditor;

namespace WireTerrain
{
    [CustomEditor(typeof(SaveMeshButton))]
    public class SaveMeshButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SaveMeshButton saveButton = (SaveMeshButton)target;
            if (GUILayout.Button("Save Mesh"))
            {
                MeshFilter mf = saveButton.gameObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    Mesh mesh = mf.sharedMesh;
                    if (mesh != null)
                    {
                        var path = EditorUtility.SaveFilePanelInProject("Save Mesh", mesh.name, "asset", "Save Mesh");
                        if (string.IsNullOrEmpty(path))
                        {
                            return;
                        }
                        AssetDatabase.CreateAsset(Object.Instantiate(mesh), path);
                    }
                }
            }
        }
    }
}
