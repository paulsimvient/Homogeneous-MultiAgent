using UnityEngine;
using UnityEditor;
using EditorDesignerUI;
using EditorDesignerUI.Controls;
using DynamicCSharp.Security;

namespace DynamicCSharp.Editor
{
    public sealed class SettingsWindow : DesignerWindow
    {
        // Methods
        [MenuItem("Tools/Dynamic C#/Settings", priority = 20)]
        public static SettingsWindow ShowWindow()
        {
            return ShowWindow<SettingsWindow>();
        }

        public override void OnEnable()
        {
            // Load settings from folder
            DynamicCSharp.LoadAsset();

            WindowTitle = "Dynamic C#";
            Layout.MinSize = new Vector2(480, 320);

            // Create ui controls
            CreateUI();
        }

        public override void OnDisable()
        {
            // Save the active settings
            DynamicCSharp.SaveAsset(DynamicCSharp.Settings);
        }

        private void CreateUI()
        {
            Label label = AddControl<Label>();
            {
                label.Content.Text = "Settings";
                label.Style = new VisualStyle(EditorStyle.BoldLabel);
            }

            Tab tab = AddControl<Tab>();
            {
                tab.Content.Text = string.Empty;

                IDesignerControl general = tab.AddItem("General");
                IDesignerControl security = tab.AddItem("Security");

                // Create the controls on the tabs
                CreateGeneralTab(general);
                CreateSecurityTab(security);
            }
        }

        private void CreateGeneralTab(IDesignerControl parent)
        {
            int labelWidth = 200;

            // Space
            parent.AddControl<Spacer>();

            // Case Sensitive Names setting
            HorizontalLayout a = parent.AddControl<HorizontalLayout>();
            {
                // Create the label
                Label label = a.AddControl<Label>();
                {
                    label.Content.Text = "Case Sensitive Names";
                    label.Content.Tooltip = "Should type and member name searches be case sensitive";
                    label.Layout.Size = new Vector2(labelWidth, 0);
                }

                // Create the toggle box
                ToggleBox box = a.AddControl<ToggleBox>();
                {
                    box.Checked = DynamicCSharp.Settings.caseSensitiveNames;
                    box.OnToggled += (object sender, bool value) =>
                    {
                        DynamicCSharp.Settings.caseSensitiveNames = value;
                    };
                }
            }

            // Discover Non-Public Types setting
            HorizontalLayout b = parent.AddControl<HorizontalLayout>();
            {
                // Create the label
                Label label = b.AddControl<Label>();
                {
                    label.Content.Text = "Discover non-public types";
                    label.Content.Tooltip = "Should types that are not marked 'public' be discovered";
                    label.Layout.Size = new Vector2(labelWidth, 0);
                }

                // Create the toggle box
                ToggleBox box = b.AddControl<ToggleBox>();
                {
                    box.Checked = DynamicCSharp.Settings.discoverNonPublicTypes;
                    box.OnToggled += (object sender, bool value) =>
                    {
                        DynamicCSharp.Settings.discoverNonPublicTypes = value;
                    };
                }
            }

            // Discover Non-Public Members
            HorizontalLayout c = parent.AddControl<HorizontalLayout>();
            {
                // Create the label
                Label label = c.AddControl<Label>();
                {
                    label.Content.Text = "Discover non-public members";
                    label.Content.Tooltip = "Should class members that are not parked 'public' be discovered";
                    label.Layout.Size = new Vector2(labelWidth, 0);
                }

                // Create the toggle box
                ToggleBox box = c.AddControl<ToggleBox>();
                {
                    box.Checked = DynamicCSharp.Settings.discoverNonPublicMembers;
                    box.OnToggled += (object sender, bool value) =>
                    {
                        DynamicCSharp.Settings.discoverNonPublicMembers = value;
                    };
                }
            }

            // Debug mode
            HorizontalLayout d = parent.AddControl<HorizontalLayout>();
            {
                // Create the label
                Label label = d.AddControl<Label>();
                {
                    label.Content.Text = "Debug Mode";
                    label.Content.Tooltip = "When enabled the compile will generate and load debug symbols";
                    label.Layout.Size = new Vector2(labelWidth, 0);
                }

                // Create the toggle box
                ToggleBox box = d.AddControl<ToggleBox>();
                {
                    box.Checked = DynamicCSharp.Settings.debugMode;
                    box.OnToggled += (object sender, bool value) =>
                    {
                        DynamicCSharp.Settings.debugMode = value;
                    };
                }
            }

            // Spacer
            parent.AddControl<Spacer>();

            EditListBox listbox = parent.AddControl<EditListBox>();
            {
                listbox.Content.Text = "Assembly References";
                listbox.ItemStyle = new VisualStyle(EditorStyle.ToolbarButton);

                foreach (string value in DynamicCSharp.Settings.assemblyReferences)
                    listbox.AddItem(value);

                // On add clicked
                listbox.OnAddClicked += (object sender) =>
                {
                    // Show an input dialog
                    InputDialog dialog = InputDialog.ShowDialog<InputDialog>(new Vector2(0, 0));

                    dialog.WindowTitle = "Add Assembly Reference";
                    dialog.Content = "Enter the name of the assembly you want to add including the '.dll' file extension";
                    dialog.CenterAt(UiEvent.mouseScreenPosition);

                    dialog.OnClosed += (object s, DialogResult result) =>
                    {
                        // Check for dialog accepted
                        if (result == DialogResult.OK)
                        {
                            InputDialog window = s as InputDialog;

                            // Make sure the input is not empty
                            if (string.IsNullOrEmpty(window.Text) == false)
                            {
                                // Add to listbox
                                listbox.AddItem(window.Text);

                                // Add to settings
                                DynamicCSharp.Settings.AddAssemblyReference(window.Text);
                            }
                        }
                    };
                };

                // On remove clicked
                listbox.OnRemoveClicked += (object sender) =>
                {
                    // Catch exceptions when the list is empty
                    try
                    {
                        // Remove selection
                        listbox.RemoveItem(listbox.SelectedItemName);

                        // Remove from settings
                        DynamicCSharp.Settings.RemoveAssemblyReference(listbox.SelectedIndex);
                    }
                    catch { }
                };
            }
        }

        private void CreateSecurityTab(IDesignerControl parent)
        {
            // Security check code
            HorizontalLayout a = parent.AddControl<HorizontalLayout>();
            {
                // Create the label
                Label label = a.AddControl<Label>();
                {
                    label.Content.Text = "Security Check Code";
                    label.Content.Tooltip = "When enabled, all code will be security checked before it can be loaded";
                }

                // Create the toggle box
                ToggleBox box = a.AddControl<ToggleBox>();
                {
                    box.Checked = DynamicCSharp.Settings.securityCheckCode;
                    box.OnToggled += (object sender, bool value) =>
                    {
                        DynamicCSharp.Settings.securityCheckCode = value;
                    };
                }
            }

            parent.AddControl<Spacer>();

            EditListBox referenceListbox = parent.AddControl<EditListBox>();
            {
                referenceListbox.Content.Text = "Assembly Reference Restrictions";
                referenceListbox.ItemStyle = new VisualStyle(EditorStyle.ToolbarButton);

                foreach (ReferenceRestriction value in DynamicCSharp.Settings.referenceRestrictions)
                    referenceListbox.AddItem(value.RestrictedName);

                // On add clicked
                referenceListbox.OnAddClicked += (object sender) =>
                {
                    // Show an input dialog
                    InputDialog dialog = InputDialog.ShowDialog<InputDialog>(new Vector2(0, 0));

                    dialog.WindowTitle = "Add Assembly Reference Restriction";
                    dialog.Content = "Enter the name of the assembly you want to restrict including the '.dll' file extension";
                    dialog.CenterAt(UiEvent.mouseScreenPosition);

                    dialog.OnClosed += (object s, DialogResult result) =>
                    {
                        if (result == DialogResult.OK)
                        {
                            InputDialog window = s as InputDialog;

                            if (string.IsNullOrEmpty(window.Text) == false)
                            {
                                // Add to listbox
                                referenceListbox.AddItem(window.Text);

                                // Add to settings
                                DynamicCSharp.Settings.AddReferenceRestriction(window.Text);
                            }
                        }
                    };
                };

                // On remove clicked
                referenceListbox.OnRemoveClicked += (object sender) =>
                {
                    // Catch exceptions cause by empty list
                    try
                    {
                        // Remove selection
                        referenceListbox.RemoveItem(referenceListbox.SelectedItemName);

                        // Remove from settings
                        DynamicCSharp.Settings.RemoveReferenceRestriction(referenceListbox.SelectedIndex);
                    }
                    catch { }
                };
            }

            parent.AddControl<Spacer>();

            EditListBox namespaceListbox = parent.AddControl<EditListBox>();
            {
                namespaceListbox.Content.Text = "Namespace Restrictions";
                namespaceListbox.ItemStyle = new VisualStyle(EditorStyle.ToolbarButton);

                foreach (NamespaceRestriction value in DynamicCSharp.Settings.namespaceRestrictions)
                    namespaceListbox.AddItem(value.RestrictedNamespace);

                // On add clicked
                namespaceListbox.OnAddClicked += (object sender) =>
                {
                    // Show an input dialog
                    InputDialog dialog = InputDialog.ShowDialog<InputDialog>(new Vector2(0, 0));

                    dialog.WindowTitle = "Add Namespace Restriction";
                    dialog.Content = "Enter the namespace you want to restrict. For example, 'System.IO'";
                    dialog.CenterAt(UiEvent.mouseScreenPosition);

                    dialog.OnClosed += (object s, DialogResult result) =>
                    {
                        if (result == DialogResult.OK)
                        {
                            InputDialog window = s as InputDialog;

                            if (string.IsNullOrEmpty(window.Text) == false)
                            {
                                // Add to listbox
                                namespaceListbox.AddItem(window.Text);

                                // Add to settings
                                DynamicCSharp.Settings.AddNamespaceRestriction(window.Text);
                            }
                        }
                    };
                };

                // On remove clicked
                namespaceListbox.OnRemoveClicked += (object sender) =>
                {
                    // Catch exceptions cause by empty list
                    try
                    {
                        // Remove selection
                        namespaceListbox.RemoveItem(namespaceListbox.SelectedItemName);

                        // Remove from settings
                        DynamicCSharp.Settings.RemoveNamespaceRestriction(namespaceListbox.SelectedIndex);
                    }
                    catch { }
                };
            }
        }
    }
}
