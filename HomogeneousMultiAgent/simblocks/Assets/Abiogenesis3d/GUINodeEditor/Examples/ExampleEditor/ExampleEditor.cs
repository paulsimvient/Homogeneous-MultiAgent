// in runtime UnityEditor does not exist so we wrap it in #if
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using GUINodeEditor;

// EditorWindowNodeEditor inherits from EditorWindow
public class ExampleEditorWindow : EditorWindowNodeEditor {
    // static reference to EditorWindow
    static ExampleEditorWindow editor;

    // name of the menu item (NOTE: has to be hard coded string)
    [MenuItem("Window/Node Editor Demos/Example Node Editor")]
    // called once after opening the window
    static void Init() {
        // setting the window reference
        editor = (ExampleEditorWindow) EditorWindow.GetWindow (typeof (ExampleEditorWindow));
        // title of the window/tab (NOTE: has to be hard coded string)
        editor.titleContent = new GUIContent ("Example");
    }

    #region NODE_EDITOR
    // node editor name to be used to get nodeEditor gameObject
    public override string GetNodeEditorName () { return "ExampleEditor"; }
    // Node menu type as string
    public override Type GetMenuNodeType () { return typeof(Node_Menu_Example); }
    #endregion

}
#endif
