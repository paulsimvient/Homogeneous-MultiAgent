using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{ 
    /// <summary>
    /// This example shows how to create an instance of an external type and invoke one of its methods.
    /// </summary>
    public class ScriptProxyExample : MonoBehaviour
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
            ScriptType type = domain.CompileAndLoadScriptSource(sourceCode);

            // Create an instance of our type
            ScriptProxy proxy = type.CreateInstance();

            // Call the test method
            proxy.Call("TestMethod");
        }
    }
}
