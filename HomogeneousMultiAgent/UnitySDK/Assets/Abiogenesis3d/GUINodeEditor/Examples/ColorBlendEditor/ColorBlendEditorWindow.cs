// in runtime UnityEditor does not exist so we wrap it in #if
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using GUINodeEditor;

// EditorWindowNodeEditor inherits from EditorWindow
public class ColorBlendEditorWindow : EditorWindowNodeEditor {
    // static reference to EditorWindow
    static ColorBlendEditorWindow editor;

    // name of the menu item (NOTE: has to be hard coded string)
    [MenuItem("Window/Node Editor Demos/Color Blend Editor")]
    // called once after opening the window
    static void Init() {
        // setting the window reference
        editor = (ColorBlendEditorWindow) EditorWindow.GetWindow (typeof (ColorBlendEditorWindow));
        // title of the window/tab (NOTE: has to be hard coded string)
        editor.titleContent = new GUIContent ("Color Blend Example");
    }

    #region NODE_EDITOR
    // node editor name to be used to get nodeEditor gameObject
    public override string GetNodeEditorName () { return "ColorBlendEditor"; }
    // Node menu type as string
    public override Type GetMenuNodeType () { return typeof(Node_Menu_ColorBlend); }
    #endregion

}
#endif
