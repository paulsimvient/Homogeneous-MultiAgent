using UnityEngine;
using UnityEditor;

namespace DynamicCSharp.Editor
{
    [InitializeOnLoad]
    public class PlatformChecker
    {
        // Methods
        [InitializeOnLoadMethod]
        public static void CheckBuildTarget()
        {
            // Get the current build target
            BuildTarget current = EditorUserBuildSettings.activeBuildTarget;

            switch(current)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
#if UNITY_2017_3_OR_NEWER == false
                case BuildTarget.StandaloneOSXIntel:
                case BuildTarget.StandaloneOSXIntel64:
#endif
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    {
                        // The target platform is supported so dont do anything
                        return;
                    }
            }

            // Display an error to the user
            Debug.LogWarning(string.Format("The current build target '{0}' is not supported by DynamicC#. Please change the platform from the build settings menu", current));
        }
    }
}
