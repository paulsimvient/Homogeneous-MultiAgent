using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// This example shows how to create a <see cref="ScriptDomain"/> that scripts and assemblies can be loaded in to.
    /// </summary>
    public class CreateDomainExample : MonoBehaviour
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

            if (domain == null)
                Debug.LogError("Failed to create ScriptDomain");
        }
    }
}
