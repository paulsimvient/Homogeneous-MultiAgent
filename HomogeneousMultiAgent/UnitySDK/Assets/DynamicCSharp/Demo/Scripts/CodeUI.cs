using System;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicCSharp.Demo
{
    /// <summary>
    /// The main script for the code editor interface.
    /// </summary>
    public sealed class CodeUI : MonoBehaviour
    {
        // Events
        /// <summary>
        /// Invoked when the new button is clicked.
        /// </summary>
        public static Action<CodeUI> onNewClicked;
        /// <summary>
        /// Invoked when the load button is clicked.
        /// </summary>
        public static Action<CodeUI> onLoadClicked;
        /// <summary>
        /// Invoked when the compile button is clicked.
        /// </summary>
        public static Action<CodeUI> onCompileClicked;

        // Public
        /// <summary>
        /// Main editor canvas.
        /// </summary>
        public GameObject codeEditorObject;
        /// <summary>
        /// Help window canvas.
        /// </summary>
        public GameObject helpObject;
        /// <summary>
        /// Code editor text field.
        /// </summary>
        public InputField codeEditor;

        // Methods
        /// <summary>
        /// Called by Unity.
        /// </summary>
        public void Start()
        {
            // Load a blank script
            OnNewClicked();
        }

        /// <summary>
        /// Reset the code editor to a blank template.
        /// </summary>
        public void OnNewClicked()
        {
            // Trigger event
            if (onNewClicked != null)
                onNewClicked(this);
        }

        /// <summary>
        /// Open the example template script in the code editor.
        /// </summary>
        public void OnExampleClicked()
        {
            // Trigger event
            if (onLoadClicked != null)
                onLoadClicked(this);
        }

        /// <summary>
        /// Show available commands for the code editor.
        /// </summary>
        public void OnShowHelpClicked()
        {
            helpObject.SetActive(true);
            codeEditorObject.SetActive(false);
        }

        /// <summary>
        /// Hide help and return to the code editor.
        /// </summary>
        public void OnHideHelpClicked()
        {
            helpObject.SetActive(false);
            codeEditorObject.SetActive(true);
        }

        /// <summary>
        /// Run the source code.
        /// </summary>
        public void OnRunClicked()
        {
            // Trigger event
            if (onCompileClicked != null)
                onCompileClicked(this);
        }
    }
}
