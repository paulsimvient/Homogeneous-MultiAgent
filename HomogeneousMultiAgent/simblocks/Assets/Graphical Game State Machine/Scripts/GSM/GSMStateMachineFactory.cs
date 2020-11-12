using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace GSM
{
    class GSMStateMachineFactory
    {
        [MenuItem("Assets/Create/Graphical State Machine", priority = 4)]
        static void CreateGSMFile()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateStateMachine>(), "New Grapical State Machine.asset", null, null);
        }

        public static GSMStateMachine CreateGSMFileAtPath(string path)
        {
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            var gsm = ScriptableObject.CreateInstance<GSMStateMachine>();
            gsm.name = Path.GetFileName(path);
            gsm.machineName = gsm.name.Substring(0, gsm.name.LastIndexOf('.'));
            gsm.InsertState(new GSMState() { name = "Start State"});
            AssetDatabase.CreateAsset(gsm, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return gsm;
        }
    }

    class DoCreateStateMachine : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var profile = GSMStateMachineFactory.CreateGSMFileAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(profile);
        }
    }
}
