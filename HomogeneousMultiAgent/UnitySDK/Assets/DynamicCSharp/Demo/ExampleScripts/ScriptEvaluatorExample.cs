using UnityEngine;
using DynamicCSharp;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// This example shows how the <see cref="ScriptEvaluator"/> can be used to execute arbitrary C# code outside the context of a method body. 
    /// </summary>
    public class ScriptEvaluatorExample
    {
        // Private
        private ScriptDomain domain = null;
        private ScriptEvaluator evaluator = null;

        void Start()
        {
            // Create our domain
            domain = ScriptDomain.CreateDomain("EvalDomain", true);

            // Create our evaluator
            evaluator = new ScriptEvaluator(domain);

            // Add a using statement for UnityEngine. This will allow us ti access all types under the UnityEngine namespace
            evaluator.AddUsing("UnityEngine");
        }

        void onGUI()
        {
            if(GUILayout.Button("EvalMath") == true)
            {
                // Eval some C# math code
                Debug.Log(evaluator.Eval("return 6 * 3 + 20;"));
            }

            if(GUILayout.Button("EvalLoop") == true)
            {
                // Eval for loop code
                evaluator.Eval("for(int i = 0; i < 5; i++) Debug.Log(\"Hello World \" + i);");
            }

            if(GUILayout.Button("EvalVar") == true)
            {
                evaluator.BindVar("floatValue", 23.5f);

                // Eval var code
                evaluator.Eval("Debug.Log(floatValue + 4f);");
            }

            if(GUILayout.Button("EvalRefVar") == true)
            {
                Variable<float> shared = evaluator.BindVar<float>("floatValue", 12.3f);

                // Eval var reference code
                evaluator.Eval("floatValue *= 2;");
                
                Debug.Log(shared);
            }

            if(GUILayout.Button("EvalDelegate") == true)
            {
                evaluator.BindDelegate("callback", () =>
                {
                    Debug.Log("Hello from callback");
                });

                // Eval delegate code
                evaluator.Eval("callback();");
            }
        }
    }
}
