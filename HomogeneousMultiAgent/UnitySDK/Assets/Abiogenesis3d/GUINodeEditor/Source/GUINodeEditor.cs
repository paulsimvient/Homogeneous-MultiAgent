using UnityEngine;
using System.Collections.Generic;

using Type = System.Type;
using Activator = System.Activator;
using Regex = System.Text.RegularExpressions.Regex;

namespace GUINodeEditor {
    /// Serializes the type `Type` with FullSerializer because `Type` serialization does not work in Unity.
    [System.Serializable]
    public class TypeHolder: ISerializationCallbackReceiver {
        public string serializedType;
        public Type type;

        public void OnBeforeSerialize () {
            string s = StringSerializationAPI.Serialize (typeof(Type), type);
            if (s.StartsWith ("\"") && s.EndsWith ("\""))
                s = s.Substring (1, s.Length - 2);
            serializedType = s;
        }
        public void OnAfterDeserialize() {
            string s = serializedType;
            if (! s.StartsWith ("\"") && ! s.EndsWith ("\""))
                s = "\"" + s + "\"";
            type = (Type) StringSerializationAPI.Deserialize (typeof(Type), s);
        }
    }

    /// Non editor related serialization, this is what gets serialized to a text file.
    public class NodeLogic {
        public List<Node> nodes = new List<Node>();
        public Vector2 panningOffset = Vector2.zero;

        /// Calls `Update` of each node.
        public void Update () {
            foreach (Node node in nodes)
                node.Update ();
        }
    }
    /// Holds node connection data. Its `DockWindow` is the little box on node sides.
    public class Dock {
        /// Type for dock matching, only docks matched by type can be connected.
        public TypeHolder typeHolder;
        /// Used as an identifier.
        public string name;
        /// Value a dock is carrying. It is defined by your editor logic, you can get and set it.
        public object value;

        /// Parent node reference.
        public Node node;
        public DockWindow dockWindow;

        /// The list of docks it is connected to. These targets are always connected back to this dock.
        public List <Dock> targets;

        public Dock (Node node, Type type, string name, object initial) {
            this.node = node;

            this.typeHolder = new TypeHolder ();
            this.typeHolder.type = type;
            this.name = name;
            this.value = initial;

            this.dockWindow = new DockWindow ();
            this.dockWindow.dock = this;

            this.targets = new List<Dock> ();
        }
    }

    /// Helper for clarification of the docks side, as only output-input can be connected.
    public class DockInput : Dock {
        public DockInput (Node node, Type type, string name, object initial): base(node, type, name, initial) {}
    }

    /// Helper for clarification of the docks side, as only output-input can be connected.
    public class DockOutput : Dock {
        public DockOutput (Node node, Type type, string name, object initial): base(node, type, name, initial) {}
    }

    public class DockWindow : NodeEditorWindow {
        public Dock dock;
    }

    /// Holds node data, its `NodeWindow` renders that data in the editor.
    public class Node {
        /// If this is true, the connection will be drawn with bezierConfig.triggeredColor.
        public bool isTriggered;

        /// List of left side docks.
        public List<DockInput> inputs;
        /// List of right side docks.
        public List<DockOutput> outputs;

        /// Renders the node data in `override OnGUI`.
        public NodeWindow nodeWindow;

        /// Called externally from `NodeLogic.Update`. Place your logic here.
        public virtual void Update() {}

        public virtual void Init(Vector2 position = default(Vector2)) {
            Init (position, new NodeWindow());
        }

        /// <summary>
        /// Init with the specified position (usually position of the menuNode.rect),
        /// nodeWindow, parent node and title.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="nodeWindow">Node window.</param>
        /// <param name="node">Parent node.</param>
        /// <param name="title">Title.</param>
        public virtual void Init (Vector2 position, NodeWindow nodeWindow, string title = "") {
            this.nodeWindow = nodeWindow;
            this.nodeWindow.rect = new Rect (position, Vector2.zero);
            this.nodeWindow.node = this;
            this.nodeWindow.title = title;

            this.inputs = new List<DockInput>();
            this.outputs = new List<DockOutput>();
        }

        public T GetFirstTargetValue <T> (Dock dock, object returnIfNull = default (object)) {
            object defaultValue = returnIfNull != default(object) ? returnIfNull : default (T);
            return (T)(dock.targets.Count > 0 ? dock.targets [0].value : defaultValue);
        }

        public DockInput AddInput (Type type, string name = "", object initial = null, int insertAtIndex = -1) {
            if (initial == null)
                initial = CreateInstance (type);
            DockInput dockInput = new DockInput (this, type, name, initial);

            inputs.Insert (GetValidIndex (inputs.Count, insertAtIndex), dockInput);
            return dockInput;
        }

        public DockOutput AddOutput (Type type, string name = "", object initial = null, int insertAtIndex = -1) {
            if (initial == null)
                initial = CreateInstance (type);
            DockOutput dockOutput = new DockOutput (this, type, name, initial);

            outputs.Insert (GetValidIndex (outputs.Count, insertAtIndex), dockOutput);
            return dockOutput;
        }

        public int GetValidIndex (int count, int index) {
            index = Mathf.Clamp (index, -(count + 1), count);
            if (index < 0)
                index =  count + 1 + index;
            return index;
        }

        public object CreateInstance (Type type) {
            if (type == typeof(string)) return "";
            if (type == typeof(void)) return null;
            return Activator.CreateInstance(type);
        }

        public DockInput GetDockInputByName (string name) {
            for (int i = 0; i < inputs.Count; ++i)
                if (inputs [i].name == name)
                    return inputs [i];
            Debug.LogWarning ("No DockInput found with name \"" + name + "\"");
            return default (DockInput);
        }

        public DockOutput GetDockOutputByName (string name) {
            for (int i = 0; i < outputs.Count; ++i)
                if (outputs [i].name == name)
                    return outputs [i];
            Debug.LogWarning ("No DockOutputs found with name \"" + name + "\"");
            return default (DockOutput);
        }

    }

    public class NodeWindow_Menu: NodeWindow {
        public NodeWindow clickedWindow;
    }

    public class NodeWindow : NodeEditorWindow {
        public Node node = new Node ();
        public Popup popup = new Popup();

        private static Dictionary<string, Vector2> cachedSizes = new Dictionary<string, Vector2>();
        public Vector2 cachedSize {
            get {
                return cachedSizes.ContainsKey(this.GetType().ToString())
                    ? cachedSizes[this.GetType().ToString()] : default(Vector2);
            }
            set {cachedSizes [this.GetType().ToString()] = value;}
        }

        public virtual float GetWindowWidth () {return 140;}
        public virtual float GetWindowHeight () {return -1;}

        public virtual void SetWindowSize (Vector2 size) {
            rect = new Rect (rect.position, size);
            cachedSize = size;
        }

        public Color SetOpacity (Color c, float opacity) {
            return new Color (c.r, c.g, c.b, opacity);
        }

        public void DrawTooltip (string tooltip) {
            if (popup.identifier != null && popup.GetListRect ().Contains (Event.current.mousePosition))
                return;
            Rect lastRect = GUILayoutUtility.GetLastRect ();
            GUI.Label (lastRect, new GUIContent ("", tooltip));
        }

        public void DrawDock (Dock dock, bool isTitleRow = false) {
            // placeholder for input dock
            // TODO why 1, multiplication?
            Rect titleRect = new Rect (1, 0, 0, 0);
            if (isTitleRow)
                GUI.Label (titleRect, "");
            else GUILayout.Label ("", GUILayout.Width (0));

            // saving dock position for drawing connections
            if (Event.current.type == EventType.Repaint)
                dock.dockWindow.rect = isTitleRow ? titleRect: GUILayoutUtility.GetLastRect ();
        }
    }

    public class PopAnywhereStack {
        public List<object> stack = new List<object> ();
        object objectThatChangedStackLast = null;

        public object Head (object toReturnIfNull) {
            return stack.Count > 0 ? stack [0] : toReturnIfNull;
        }

        public void HandleInsertRemove (object obj, bool active, object instance) {
            if (active) {
                if (objectThatChangedStackLast != obj && !stack.Contains (instance)) {
                    stack.Insert (0, instance);
                    objectThatChangedStackLast = this;
                }
            }
            else if (stack.Contains (instance)) {
                stack.Remove (instance);
                if (objectThatChangedStackLast == this)
                    objectThatChangedStackLast = null;
            }
        }
    }

    public class NodeEditorWindow {
        public Rect rect = new Rect ();
        public Color backgroundColor;
        public string title = "";

        [System.NonSerialized]
        public NodeEditor nodeEditor;

        [System.NonSerialized]
        public Vector2 dragStartOffset;

        public virtual void OnGUI() {}
    }

    public class NumberField {
        // storing text field string while typing
        string focusedInputText = "";
        bool isFocused;

        public int Int (int val) {
            return (int) Float (val);
        }

        public float Float (float val) {
            if (Event.current.keyCode == KeyCode.Return && isFocused)
                GUI.FocusControl (null);

            float ret = val;
            string displayString;

            string keyboardControl = GUIUtility.GetControlID(FocusType.Keyboard).ToString();
            string focusedControl = GUI.GetNameOfFocusedControl ();

            bool currentIsFocused = keyboardControl == focusedControl;

            if (currentIsFocused) {
                // first focus
                if (! isFocused) {
                    focusedInputText = val.ToString ();
                    isFocused = true;
                }
                displayString = focusedInputText;
            }
            // current not focused
            else {
                // first blur
                if (isFocused) {
                    ret = 0;
                    float.TryParse (focusedInputText, out ret);
                    isFocused = false;
                }
                // is not focused
                displayString = ret.ToString ();
                focusedInputText = ret.ToString ();
            }

            GUI.SetNextControlName (keyboardControl);
            string tmp = GUILayout.TextField (displayString, GUILayout.Width (35));

            if (isFocused) {
                tmp = Regex.Replace (tmp, @"[^0-9\.\-]+", "");
                tmp = Regex.Replace (tmp, @"\.+", ".");
                tmp = Regex.Replace (tmp, @"\-+", "-");
                focusedInputText = tmp;
            }

            return ret;
        }

    }
}
