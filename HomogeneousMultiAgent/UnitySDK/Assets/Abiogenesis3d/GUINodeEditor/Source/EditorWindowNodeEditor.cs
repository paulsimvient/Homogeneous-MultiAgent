#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.Linq;
using System.IO;
using Type = System.Type;

namespace GUINodeEditor {
    /// Derrive from this to create a node editor EditorWindow
    public class EditorWindowNodeEditor: EditorWindow {
        public NodeEditor nodeEditor;

        /// Has to be virtual because EditorWindow Init is called only once upon creation.
        public virtual string GetNodeEditorName () {return "";}
        /// Specify the node menu type.
        public virtual Type GetMenuNodeType() {return default(Type);}

        SaveLoadGUI saveLoadGUI = new SaveLoadGUI ();

        void Update () {
            // out of playmode nodeEditor.Update has to be called manually
            if (nodeEditor != null) {
                if (! Application.isPlaying && nodeEditor.config.runUpdateInEditMode)
                    nodeEditor.Update ();

                if (nodeEditor.ShouldRepaint ())
                    Repaint ();
            }
        }

        void OnGUI () {
            // initialize node editor
            if (nodeEditor == null)
                nodeEditor = NodeEditor.GetOrCreateNodeEditor (GetNodeEditorName(), GetMenuNodeType());

            nodeEditor.DrawBackground ();
            nodeEditor.DrawGrid ();
            nodeEditor.DrawDebug ();

            // editor requires BeginWindows EndWindows to render GUI.Window
            BeginWindows ();
            nodeEditor.DrawNodeWindows ();
            EndWindows ();

            nodeEditor.DrawMinimap ();

            // draw load/save GUI
            GUILayout.BeginArea(new Rect(Screen.width - 220, 10, 200, Screen.height - 50));

            string path = Serialization.GetFullResourcesPath(nodeEditor.saveLoadResourcesFolderName);
            string fileExtension = ".txt";
            DirectoryInfo dir = new DirectoryInfo (path);
            List<string> fileNames = dir.GetFiles ("*" + fileExtension)
                .OrderByDescending (f => f.LastWriteTime)
                .Select (f => f.Name.Substring (0, f.Name.Length - fileExtension.Length))
                .ToList();

            saveLoadGUI.currentName = nodeEditor.saveLoadName;
            saveLoadGUI.fileNames = fileNames;
            saveLoadGUI.onLoad = (string name) => {
                nodeEditor.Load (name);
                nodeEditor.saveLoadName = name;
                saveLoadGUI.saveLoadName = name;
            };

            saveLoadGUI.onSave = (string name) => {
                // isShouldSaveDialogue = File.Exists (filePath) != false;
                nodeEditor.Save (name);
                nodeEditor.saveLoadName = name;
            };

            saveLoadGUI.OnGUI ();
            GUILayout.EndArea ();
        }

        // refresh screen
        void OnInspectorUpdate() {
            wantsMouseMove = true;
            Repaint();
        }

    }
}
#endif