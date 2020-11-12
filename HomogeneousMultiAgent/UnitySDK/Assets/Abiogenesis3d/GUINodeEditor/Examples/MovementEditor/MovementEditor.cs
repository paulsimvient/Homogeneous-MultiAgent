// in runtime UnityEditor does not exist so we wrap it in #if
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using GUINodeEditor;

// EditorWindowNodeEditor inherits from EditorWindow
public class MovementEditor : EditorWindowNodeEditor {
    // static reference to EditorWindow
    static MovementEditor editor;

    // name of the menu item (NOTE: has to be hard coded string)
    [MenuItem("Window/Node Editor Demos/Movement Editor")]
    // called once after opening the window
    static void Init() {
        // setting the window reference
        editor = (MovementEditor) EditorWindow.GetWindow (typeof (MovementEditor));
        // title of the window/tab (NOTE: has to be hard coded string)
        editor.titleContent = new GUIContent ("Movement Editor");
    }

    #region NODE_EDITOR
    // node editor name to be used to get nodeEditor gameObject
    public override string GetNodeEditorName () { return "MovementEditor"; }
    // Node menu type as string
    public override Type GetMenuNodeType () { return typeof(Node_Menu_Movement); }
    #endregion

}
#endif
