using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// This example shows how you can load precompiled managed assemblies into a <see cref="ScriptDomain"/>. 
    /// </summary>
    public class LoadAssemblyExample : MonoBehaviour
    {
        // The domain used for external scripts
        private ScriptDomain domain = null;

        void Start()
        {
            // Should we enable the compiler for our domain
            // This is requried if we want to load C# scripts as opposed to assemblies
            bool initCompiler = true;

            // Create our domain
            domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);

            // Load an assembly into our domain
            // This assumes that a file called 'ModAssembly.dll' is next to the game .exe file
            ScriptAssembly assembly = domain.LoadAssembly("ModAssembly.dll");

            // List all types in the assembly
            foreach (ScriptType type in assembly.FindAllTypes())
                Debug.Log(type.ToString());
        }
    }
}
