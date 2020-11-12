using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// This example shows how to create an instance of an external mono behaviour and invoke a coroutine method.
    /// </summary>
    public class ScriptCoroutineExample : MonoBehaviour
    {
        // The domain used for external scripts
        private ScriptDomain domain = null;

        private string sourceCode =
            "using UnityEngine;" +
            "using System.Collections;" +
            "class Test : MonoBehaviour" +
            "{" +
            "   public IEnumerator TestMethod()" +
            "   {" +
            "       for(int i = 0; i < 10; i++)" +
            "       {" +
            "           Debug.Log(\"Hello World - From loaded behaviour code\");" +
            "           yield return new WaitForSeconds(1);" +
            "       }" +
            "   }" +
            "}";

        void Start()
        {
            // Should we enable the compiler for our domain
            // This is required if we want to load C# scripts as opposed to assemblies.
            bool initCompiler = true;

            // Create our domain
            domain = ScriptDomain.CreateDomain("ModDomain", initCompiler);

            // Load the source code into our domain
            ScriptType type = domain.CompileAndLoadScriptSource(sourceCode);

            // Create an instance of our type - We need to pass a game object because 'Test' inherits from monobehaviour
            ScriptProxy proxy = type.CreateInstance(gameObject);
            
            // Coroutines are supported by default (true) but support can be disabled by specifying 'false' as the value.
            // Any method that returns 'IEnumerator' will be called as a coroutine.
            proxy.supportCoroutines = true;

            // Call the test method as a coroutine
            proxy.Call("TestMethod");
        }
    }
}
