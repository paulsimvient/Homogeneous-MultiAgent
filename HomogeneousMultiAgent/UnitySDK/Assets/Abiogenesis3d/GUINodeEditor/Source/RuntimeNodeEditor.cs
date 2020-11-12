using UnityEngine;

namespace GUINodeEditor {
    /// It will render the nodes on screen if attached to the gameObject that has NodeEditor
    public class RuntimeNodeEditor: MonoBehaviour {
        public NodeEditor nodeEditor;

        bool showDebug = true;

        void OnGUI () {
            // initialize node editor
            if (nodeEditor == null)
                nodeEditor = NodeEditor.GetOrCreateNodeEditor (name);

            nodeEditor.DrawBackground ();
            nodeEditor.DrawGrid ();

            if (GUILayout.Button ("Reload", GUILayout.Width (100)))
                nodeEditor.Load ();

            showDebug = GUILayout.Toggle (showDebug, "Show debug info");
            if (showDebug)
                nodeEditor.DrawDebug ();

            // draw nodes
            nodeEditor.DrawNodeWindows ();

            nodeEditor.DrawMinimap ();
        }
    }
}
