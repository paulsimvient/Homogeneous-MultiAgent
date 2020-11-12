using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{ 
    /// <summary>
    /// An example interface used for communication between game and external code.
    /// </summary>
    public interface IExampleInterface
    {
        /// <summary>
        /// An example method.
        /// </summary>
        void SayHello();

        /// <summary>
        /// An example method.
        /// </summary>
        void SayGoodbye();
    }

    /// <summary>
    /// This example shows how an interface approach can be used to call into external code.
    /// The loaded code implements the <see cref="IExampleInterface"/> which is then used to invoke methods once the code is loaded. 
    /// </summary>
    public class ScriptInterfaceExample : MonoBehaviour
    {
        // The domain used for external scripts
        private ScriptDomain domain = null;

        private string sourceCode =
            "using UnityEngine;" +
            "using DynamicCSharp.Demo;" +
            "class Test : IExampleInterface" +
            "{" +
            "   public void SayHello()" +
            "   {" +
            "       Debug.Log(\"Hello - From loaded code\");" +
            "   }" +
            "   public void SayGoodbye()" +
            "   {" +
            "       Debug.Log(\"Goodbye - From loaded code\");" +
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

            // We know that the 'Test' class implements the 'IExampleInterface' so we can access the implementation like this:
            IExampleInterface instance = proxy.GetInstanceAs<IExampleInterface>(true);

            // Call the hello method on the instance
            instance.SayHello();

            // Call the goodbye method in the instance
            instance.SayGoodbye();
        }
    }
}
