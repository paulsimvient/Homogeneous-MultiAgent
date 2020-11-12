using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using DynamicCSharp.Security;
using System;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace DynamicCSharp
{
    public enum RestrictionMode
    {
        Inclusive,
        Exclusive,
    }

    /// <summary>
    /// Represent the global settings for Dynamic C#.
    /// Settings can be modified at runtime by accessing the current settings using <see cref="DynamicCSharp.Settings"/>. 
    /// Values configured in the editor settings window will be loaded.
    /// </summary>
    public sealed class DynamicCSharp : ScriptableObject
    {
        // Private
        private const string editorSettingsDirectory = "/Resources";
        private const string settingsLocation = "DynamicCSharp_Settings";
        private const BindingFlags defaultFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public;

        private static DynamicCSharp instance = null;

        // Public
        /// <summary>
        /// Should name searches be case sensitive.
        /// </summary>
        public bool caseSensitiveNames = false;

        /// <summary>
        /// Should internal, private and protected types also be discovered.
        /// </summary>
        public bool discoverNonPublicTypes = true;

        /// <summary>
        /// Should internal, private and protected members also be discovered.
        /// </summary>
        public bool discoverNonPublicMembers = true;

        /// <summary>
        /// Should code be security checked before it is loaded.
        /// </summary>
        public bool securityCheckCode = true;

#if DEBUG
        /// <summary>
        /// Should debug symbols be loaded when compiing scripts.
        /// </summary>
        public bool debugMode = true;
#else
        public readonly bool debugMode = false;
#endif

        /// <summary>
        /// You can specify a folder that the compiler should use to generate any necessary files. 
        /// You should make sure that calling app has IO access to the specified folder otherwise the compiler may fail.
        /// </summary>
        [HideInInspector]
        public string compilerWorkingDirectory = "";

        /// <summary>
        /// All references that any compiled scripts should use.
        /// By default these are 'UnityEngine.dll' and 'Assembly-CSharp.dll'.
        /// </summary>
        public string[] assemblyReferences =
        {
            "Assembly-CSharp.dll"
        };

        public static readonly string[] unityAssemblyReferences =
        {
#if UNITY_2017_2_OR_NEWER
            "UnityEngine.AudioModule.dll",
            "UnityEngine.CoreModule.dll",
            "UnityEngine.JSONSerializeModule.dll",
            "UnityEngine.ParticleSystemModule.dll",
            "UnityEngine.PhysicsModule.dll",
            "UnityEngine.UIModule.dll",
#else
            "UnityEngine.dll",
            "UnityEngine.UI.dll",
#endif
        };

        public RestrictionMode namespaceRestrictionMode = RestrictionMode.Exclusive;
        public RestrictionMode assemblyRestrictionMode = RestrictionMode.Exclusive;

        /// <summary>
        /// An array of namespace restriction used by the verification process.
        /// </summary>
        public NamespaceRestriction[] namespaceRestrictions =
        {
            new NamespaceRestriction("System.IO"),
            new NamespaceRestriction("System.Reflection"),
        };

        /// <summary>
        /// An array of reference restrictions used by the verification process.
        /// </summary>
        public ReferenceRestriction[] referenceRestrictions =
        {            
            new ReferenceRestriction("UnityEditor.dll"),
            new ReferenceRestriction("Mono.Cecil.dll"),
        };

        // Properties
        /// <summary>
        /// Access the global settings for Dynamic C#.
        /// These settings will be loaded from resources when accessed.
        /// If the settigns cannot be loaded then default values will be used.
        /// </summary>
        public static DynamicCSharp Settings
        {
            get
            {
                // Load the settings if they are not already loaded
                if (instance == null)
                    instance = LoadSettings();

                // Get the settings instance
                return instance;
            }
        }

        /// <summary>
        /// Returns true if the current platform is supported.
        /// PC, Mac and Linux are officially supported.
        /// </summary>
        public static bool IsPlatformSupported
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                // Editor, windows, mac and linux standlaones are supported
                return true;
#else
                // Anything else is not supported
                return false;
#endif
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor only. Returns the current install path relative to the project folder of Dynamic C#.
        /// This allows Dynamic C# to be moved into a sub folder for organization.
        /// </summary>
        public static string InstallLocation
        {
            get
            {
                // Default install location
                string path = "Assets/DynamicCSharp";

#if UNITY_WEBPLAYER == false
                // Get the directory info for the assets folder
                DirectoryInfo info = new DirectoryInfo(Application.dataPath);

                // Find all directories in the project
                foreach (DirectoryInfo dir in FindAllSubDirectories(info))
                {
                    // Check for matching name
                    if(dir.Name == "DynamicCSharp")
                    {
                        // Convert the paths to uri
                        Uri assets = new Uri(info.FullName, UriKind.Absolute);
                        Uri folder = new Uri(dir.FullName, UriKind.Absolute);

                        // Get the install path relative to the project folder
                        Uri result = assets.MakeRelativeUri(folder);

                        // Get the result string
                        path = result.OriginalString;

                        // Dont check anymore directories
                        break;
                    }
                }
#endif
                return path;
            }
        }
#endif

                /// <summary>
                /// Get all security restrictions.
                /// </summary>
        public IEnumerable<Restriction> Restrictions
        {
            get
            {
                // Get all namespace restrictions
                foreach (Restriction r in namespaceRestrictions)
                    yield return r;

                // Get all reference restrictions
                foreach (Restriction r in referenceRestrictions)
                    yield return r;
            }
        }

        // Constructor
        public DynamicCSharp()
        {
            int currentSize = assemblyReferences.Length;

            Array.Resize(ref assemblyReferences, currentSize + unityAssemblyReferences.Length);

            for(int i = currentSize; i < assemblyReferences.Length; i++)
                assemblyReferences[i] = unityAssemblyReferences[i - currentSize];

        }

#if UNITY_EDITOR
        /// <summary>
        /// Add a new assembly reference to the settings.
        /// </summary>
        /// <param name="referenceName">The reference name to add</param>
        public void AddAssemblyReference(string referenceName)
        {
            // Add the reference name
            ArrayUtility.Add(ref assemblyReferences, referenceName);
        }

        /// <summary>
        /// Remove the assembly reference at the specified index.
        /// </summary>
        /// <param name="index">The index to remove at</param>
        public void RemoveAssemblyReference(int index)
        {
            // Remove the reference from the array
            ArrayUtility.RemoveAt(ref assemblyReferences, index);
        }

        /// <summary>
        /// Add a new reference restriction to the settings.
        /// </summary>
        /// <param name="referenceName">The name of the reference to restrict</param>
        public void AddReferenceRestriction(string referenceName)
        {
            // Create the restriction
            ReferenceRestriction restriction = new ReferenceRestriction(referenceName);

            // Add to array
            ArrayUtility.Add(ref referenceRestrictions, restriction);
        }

        /// <summary>
        /// Remove the reference restriction at the specified index.
        /// </summary>
        /// <param name="index">The index to remove at</param>
        public void RemoveReferenceRestriction(int index)
        {
            // Remove the selected item
            ArrayUtility.RemoveAt(ref referenceRestrictions, index);
        }

        /// <summary>
        /// Add a new namespace restriction to the settings.
        /// </summary>
        /// <param name="namespaceName">The namespace to restrict</param>
        public void AddNamespaceRestriction(string namespaceName)
        {
            // Create a restriction
            NamespaceRestriction restriction = new NamespaceRestriction(namespaceName);

            // Add to array
            ArrayUtility.Add(ref namespaceRestrictions, restriction);
        }

        /// <summary>
        /// Remove the namespace restriction at the specified index.
        /// </summary>
        /// <param name="index">The index to remove at</param>
        public void RemoveNamespaceRestriction(int index)
        {
            // Remove the selected item
            ArrayUtility.RemoveAt(ref namespaceRestrictions, index);
        }
#endif

        // Methods
        internal BindingFlags GetTypeBindings()
        {
            // Default flags
            BindingFlags flags = defaultFlags;

            // Check for non public flags
            if(discoverNonPublicTypes == true)
                flags |= BindingFlags.NonPublic;

            return flags;
        }

        internal BindingFlags GetMemberBindings()
        {
            // Default flags
            BindingFlags flags = defaultFlags;

            // Check for non public flags
            if(discoverNonPublicMembers == true)
                flags |= BindingFlags.NonPublic;

            return flags;
        }

        private static DynamicCSharp LoadSettings()
        {
            // Try to load the asset
            UnityEngine.Object result = Resources.Load(settingsLocation);

            // Check for success
            if (result != null)
                return result as DynamicCSharp;

            // Use default settings
            Debug.LogWarning("DynamicCSharp: Failed to load settings - Default values will be used");

            // Create a new instance
            return CreateInstance<DynamicCSharp>();
        }


        /// <summary>
        /// Save the specified settings to the Unity project.
        /// Editor only.
        /// </summary>
        /// <param name="save">The settings to save</param>
        public static void SaveAsset(DynamicCSharp save)
        {
#if UNITY_EDITOR && UNITY_WEBPLAYER == false
            // Check for existing
            if(AssetDatabase.Contains(save) == false)
            {
                // Get the full path
                string fullPath = Path.Combine(InstallLocation + editorSettingsDirectory, settingsLocation + ".asset");

                // Locate the file info
                FileInfo info = new FileInfo(fullPath);

                // Get the directory for the file
                DirectoryInfo dir = info.Directory;

                // Make sure the directory exists
                if (dir.Exists == false)
                    dir.Create();

                // Create the asset
                AssetDatabase.CreateAsset(save, fullPath);

                // Import the folder
                AssetDatabase.Refresh();
            }

            // Mark as dirty
            EditorUtility.SetDirty(save);

            // Save assets
            AssetDatabase.SaveAssets();
#endif
        }

        /// <summary>
        /// Load the settings from the Unity project.
        /// Editor only.
        /// </summary>
        public static void LoadAsset()
        {
#if UNITY_EDITOR && UNITY_WEBPLAYER == false
            // Get the full path
            string fullPath = Path.Combine(InstallLocation + editorSettingsDirectory, settingsLocation + ".asset");

            // Try to load the asset
            DynamicCSharp result = AssetDatabase.LoadAssetAtPath(fullPath, typeof(DynamicCSharp)) as DynamicCSharp;

            // Check for error
            if (result == null)
                Debug.LogWarning("Failed to load settings from '" + fullPath + "'");

            // Load into active settings
            instance = result;
#endif
        }

#if UNITY_EDITOR && UNITY_WEBPLAYER == false
        [MenuItem("Assets/DynamicCSharp/Create New Settings")]
        private static void CreateNewSettings()
        {
            // Create an instance
            DynamicCSharp asset = ScriptableObject.CreateInstance<DynamicCSharp>();

            // Get the full path
            string fullPath = Path.Combine(editorSettingsDirectory, settingsLocation);
            
            // Create an asset id at the path
            string id = AssetDatabase.GenerateUniqueAssetPath(fullPath);

            // Create the asset
            AssetDatabase.CreateAsset(asset, id);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static IEnumerable<DirectoryInfo> FindAllSubDirectories(DirectoryInfo start)
        {
            foreach(DirectoryInfo child in start.GetDirectories())
            {
                // Get the child directory
                yield return child;

                // Get all grand child directories with a recursive call
                foreach (DirectoryInfo grandChild in FindAllSubDirectories(child))
                    yield return grandChild;

            }
        }
#endif
    }
}
