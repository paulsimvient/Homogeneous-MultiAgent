using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// This example shows how un-compiled C# scripts can be loaded into a <see cref="ScriptDomain"/>. 
    /// </summary>
    public class LoadScriptExample : MonoBehaviour
    {
        // The domain used for external scripts
        private ScriptDomain domain = null;

        private string sourceCode =
            "using UnityEngine;" +
            "class Test" +
            "{" +
            "   public void TestMethod()" +
            "   {" +
            "       Debug.Log(\"Hello World - From loaded code\");" +
            "   }" +
            "}";

        void Start()
        {
            // Should we enable the compiler for our domain
            // This is requried if we want to load C# scripts as opposed to assemblies
            bool initCompiler = true;

            // Create our domain
            domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);

            // Load the source code into our domain
            // The code must be compiled before it can be loaded
            ScriptType type = domain.CompileAndLoadScriptSource(sourceCode);

            // Print the type to the console
            Debug.Log(type.ToString());
        }
    }
}
