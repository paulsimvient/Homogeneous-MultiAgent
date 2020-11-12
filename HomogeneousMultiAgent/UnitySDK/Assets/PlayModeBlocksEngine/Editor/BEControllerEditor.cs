using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(BEController))]
[CanEditMultipleObjects]
public class BEControllerEditor : Editor 
{
    // v1.1 -Bug fix: impossibility of selecting another target object to be create via workshop
    int targetObjectIndex = 0;

    public override void OnInspectorGUI()
    {
        BEController beController = (BEController)target;
        float tempWidth = EditorGUIUtility.currentViewWidth-60;
        DrawDefaultInspector();
        DrawSeparator();

        //v1.1 -"Scene" section added on BEController inspector
        EditorGUILayout.LabelField("Scene UI", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // v1.2 -Custom UI Scale section on the inspector for adjusting the scale based on the screen width
        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Custom UI Scale", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Scale based on Screen width", EditorStyles.label);
        beController.beUIController.enableCustomUIScale = EditorGUILayout.Toggle("Enable", beController.beUIController.enableCustomUIScale);
        EditorGUI.BeginDisabledGroup(beController.beUIController.enableCustomUIScale == false);
        EditorGUILayout.HelpBox("It only takes effect during Play Mode.\nDefault value = 1300", MessageType.Warning);
        beController.beUIController.uiScaleDivisor = EditorGUILayout.FloatField("Scale Divisor", beController.beUIController.uiScaleDivisor);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Target Objects in Scene", EditorStyles.boldLabel);

        beController.singleEnabledProgrammingEnv = EditorGUILayout.Toggle("Single selection", beController.singleEnabledProgrammingEnv);
        
        //header
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Name", GUILayout.Width(2*tempWidth/3));
        EditorGUILayout.LabelField("Enable Env", GUILayout.Width(tempWidth/3));
        GUILayout.EndHorizontal();
        DrawThinSeparator();

        beController.FindTargetObjects();
        
        foreach (BETargetObject targetObject in BEController.beTargetObjectList)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(targetObject.name, GUILayout.Width(2 * tempWidth / 3));
            DrawVerticalSeparator();
            targetObject.EnableProgrammingEnv = EditorGUILayout.Toggle(targetObject.EnableProgrammingEnv, GUILayout.Width(tempWidth / 4));
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        DrawSeparator();
        EditorGUILayout.LabelField("Workshop", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("New Target Object", EditorStyles.boldLabel);
        
        Object[] targetObjects = Resources.LoadAll(beController.targetObjectsPrefabsPath, typeof(GameObject));
        string[] targetObjectOptions = new string[targetObjects.Length];
        for(int i = 0; i < targetObjects.Length; i++)
        {
            targetObjectOptions[i] = targetObjects[i].name;
        }
        targetObjectIndex = EditorGUILayout.Popup("Target Object", targetObjectIndex, targetObjectOptions);

        beController.newTargetObjectPosition = EditorGUILayout.Vector3Field("Position", beController.newTargetObjectPosition);
        if (GUILayout.Button("Build Target Object"))
        {
            beController.BuildTargetObject(targetObjectOptions[targetObjectIndex]);
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        if (Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enabled in Edit Mode Only", MessageType.Warning);
            GUI.enabled = false;
        }

        EditorGUILayout.LabelField("Build Block", EditorStyles.boldLabel);
        beController.newBlockInstructionName = EditorGUILayout.TextField("Name", beController.newBlockInstructionName);
        beController.newBlockType = (BEBlock.BlockTypeItems)EditorGUILayout.EnumPopup("Type", beController.newBlockType);
        
        EditorGUILayout.HelpBox("Header Gideline Directions: write a descriptive header text with the input tags, [inputfield] or [dropdown], in the desired positions. You can also set the textfield default value and dropdown options, exs.: [inputfield = 10], [dropdown = option1, option2]", MessageType.Info);
        EditorGUILayout.LabelField("Header Guideline");
        beController.newBlockHeaderGuideline = beController.newBlockHeaderGuideline = EditorGUILayout.TextArea(beController.newBlockHeaderGuideline, GUILayout.ExpandHeight(true), GUILayout.Width(EditorGUIUtility.currentViewWidth-60));
        beController.newBlockColor = EditorGUILayout.ColorField("New Color", beController.newBlockColor);
        
        if (GUILayout.Button("Build Block"))
        {
            beController.newBlockCreated = true;
            beController.BuildBlock(beController.newBlockHeaderGuideline, beController.newBlockInstructionName, beController.newBlockType, beController.newBlockColor);
        }

        if (beController.newBlockCreated)
        {
            GUILayout.BeginVertical("box");
            beController.TryAddComponent(beController.newBlockInstructionName);
            GUILayout.EndVertical();
        }
        
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Instruction Stack", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Checks all the blocks from the resources/prefabs/Blocks folder.", MessageType.Info);
        if (GUILayout.Button("Reimport All Prefab Block's\nInstructions to stack"))
        {
            beController.importLog = beController.ReimportInstructions();
        }
        if (beController.importLog != "")
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Result Log");
            if (GUILayout.Button("Clear Log"))
            {
                beController.importLog = "";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Result Log is only temporary, not saved in file.", MessageType.Info);
            EditorGUILayout.TextArea(beController.importLog, GUILayout.ExpandHeight(true), GUILayout.Width(EditorGUIUtility.currentViewWidth - 60));
        }
        GUILayout.EndVertical();

    }
    
    void DrawSeparator()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
    }

    void DrawThinSeparator()
    {
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.width + 15, rect.y));
        EditorGUILayout.EndHorizontal();
    }

    void DrawVerticalSeparator()
    {
        var rect = EditorGUILayout.BeginVertical();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.yMin));
        EditorGUILayout.EndVertical();
    }
}
