using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// Responsible for the tank demo gameplay.
    /// </summary>
    public sealed class TankManagerAsync : MonoBehaviour
    {
        // Private
        private ScriptDomain domain = null;
        private Vector2 startPosition;
        private Quaternion startRotation;
        private string initialText = "";
        private int counter = 0;
        private float timer = 0;
        
        private const string newTemplate = "BlankTemplate";
        private const string exampleTemplate = "ExampleTemplate";

        // Public
        /// <summary>
        /// The shell prefab that tanks are able to shoot.
        /// </summary>
        public GameObject bulletObject;
        /// <summary>
        /// The tank object that can be controlled via code.
        /// </summary>
        public GameObject tankObject;
        /// <summary>
        /// Status text that will be updated when async compiling.
        /// </summary>
        public Text statusText;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Awake()
        {
            if(statusText != null)
                initialText = statusText.text;

            // Create our script domain
            domain = ScriptDomain.CreateDomain("ScriptDomain", true);

            // Find start positions
            startPosition = tankObject.transform.position;
            startRotation = tankObject.transform.rotation;

            // Add listener for new button
            CodeUI.onNewClicked += (CodeUI ui) =>
            {
                // Load new file
                ui.codeEditor.text = Resources.Load<TextAsset>(newTemplate).text;
            };

            // Add listener for example button
            CodeUI.onLoadClicked += (CodeUI ui) =>
            {
                // Load example file
                ui.codeEditor.text = Resources.Load<TextAsset>(exampleTemplate).text;
            };

            CodeUI.onCompileClicked += (CodeUI ui) =>
            {
                // Try to run the script
                StartCoroutine(RunTankScript(ui.codeEditor.text));
            };
        }

        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Update()
        {
            if (statusText != null)
            {
                if (domain.CompilerService.IsCompiling == true)
                {
                    // Simple timed incrementor
                    if (Time.time > timer + 0.1f)
                    {
                        timer = Time.time;
                        counter++;

                        // Increase counter and wrap around
                        if (counter > 3)
                            counter = 0;

                        // Status text
                        statusText.text = "Compiling";

                        for (int i = 0; i < counter; i++)
                            statusText.text += '.';
                    }
                }
                else
                {
                    // Revert to default text
                    statusText.text = initialText;
                }
            }
        }

        /// <summary>
        /// Resets the demo game and runs the tank with the specified C# code controlling it.
        /// </summary>
        /// <param name="source">The C# sorce code to run</param>
        public IEnumerator RunTankScript(string source)
        {
            // Strip the old controller script
            TankController old = tankObject.GetComponent<TankController>();

            if (old != null)
                Destroy(old);

            // Reposition the tank at its start position
            RespawnTank();

            // Compile the script asynchronously
            AsyncCompileLoadOperation task = domain.CompileAndLoadScriptSourcesAsync(source);
            
            // Wait for compile to complete
            yield return task;            
            
            if (task.IsSuccessful == false)
            {
                Debug.LogError("Compile failed");
                yield break;
            }

            // Get the main compiled type
            ScriptType type = task.MainType;

            // Make sure the type inherits from 'TankController'
            if (type.IsSubtypeOf<TankController>() == true)
            {
                // Attach the component to the object
                ScriptProxy proxy = type.CreateInstance(tankObject);

                // Check for error
                if(proxy == null)
                {
                    // Display error
                    Debug.LogError(string.Format("Failed to create an instance of '{0}'", type.RawType));
                    yield break;
                }

                // Assign the bullet prefab to the 'TankController' instance
                proxy.Fields["bulletObject"] = bulletObject;    

                // Call the run tank method
                proxy.Call("RunTank");
            }
            else
            {
                Debug.LogError("The script must inherit from 'TankController'");
            }
        }

        /// <summary>
        /// Resets the tank at its starting location.
        /// </summary>
        public void RespawnTank()
        {
            tankObject.transform.position = startPosition;
            tankObject.transform.rotation = startRotation;
        }
    }
}
