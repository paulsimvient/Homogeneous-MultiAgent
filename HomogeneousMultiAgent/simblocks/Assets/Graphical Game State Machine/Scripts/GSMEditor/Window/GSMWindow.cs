using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GSM
{
    public partial class GSMWindow : EditorWindow
    {
        internal GSMStateMachine machine;


        ////////////////////// Colors ///////////////////////
        [System.NonSerialized] internal Color stateColorDefault = new Color(0.35f, 0.35f, 0.35f);
        [System.NonSerialized] internal Color stateDividerColorDefault = new Color(0.6f, 0.6f, 0.6f);
        [System.NonSerialized] internal Color stateFontColorDefault = new Color(1f, 1f, 1f);
        [System.NonSerialized] internal Color stateFontColorContent = new Color(0.8f, 0.8f, 0.8f);
        [System.NonSerialized] internal Color stateColorInspected = new Color(0.5f, 0.5f, 1f);
        [System.NonSerialized] internal Color stateColorActive = new Color(1f, 0.5f, 0.5f);
        [System.NonSerialized] internal Color stateColorTerminating = new Color(1f, 0f, 0f);
        [System.NonSerialized] internal Color windowColorDefault = new Color(0.7f, 0.7f, 0.7f);
        [System.NonSerialized] internal Color machineBackgroundColor = new Color(0.2f, 0.2f, 0.2f);
        [System.NonSerialized] internal Color machineGridColor = new Color(0.23f, 0.23f, 0.23f);
        [System.NonSerialized] internal Color machineGridColorBig = new Color(0.25f, 0.25f, 0.25f);
        [System.NonSerialized] internal Color textColorError = new Color(1f, 0.3f, 0.3f);
        [System.NonSerialized] internal Color textColorWarning = new Color(1f, 0.8f, 0.2f);
        [System.NonSerialized] internal Color eventColor = new Color(0.6f, 0.6f, 0.6f);
        [System.NonSerialized] internal Color callbackColorDefault = new Color(0.55f, 0.55f, 0.55f);
        [System.NonSerialized] internal Color callbackColorSelected = new Color(0.45f, 0.45f, 0.68f);
        [System.NonSerialized] internal Color callbackColorSwap = new Color(0.3f, 0.3f, 0.85f);



        ///////////////////// Styles /////////////////////////////
        internal GUIStyle machineTextStyle;
        internal GUIStyle titleStyle;
        internal GUIStyle headerStyle;
        internal GUIStyle defaultStyle;
        internal GUIStyle defaultStyleOutgrayed;



        ////////////////////// Bottom box Bounds /////////////////////////
        private readonly int boxPadding = 8;
        private readonly int buttonWidth = 45;
        private float indentOffset;
        private Rect boxRect;
        private Rect labelRect;
        private Rect fieldRect;
        private Rect buttonRect;


        ////////////////////// Window bounds Bounds /////////////////////
        internal float sideWindowWidth = 285;
        internal Rect MachineBounds { get { return new Rect(0, 0, position.width, position.height - boxRect.height); } }
        internal Rect LeftSideWindowBounds { get { return new Rect(0, 0, sideWindowWidth, position.height - boxRect.height); } }
        internal Rect RightSideWindowBounds { get { return new Rect(position.width - sideWindowWidth, position.height - 145 - boxRect.height, sideWindowWidth, 145); } }


        //////////////////// Runtime variables /////////////////
        internal IInspectable InspectedObject { get; private set; }
        internal GSMState draggedState;
        private readonly Dictionary<KeyCode, bool> keysDown = new Dictionary<KeyCode, bool>();
        internal static bool hasUnsavedChanges = false;
        internal static bool transitionMode;
        internal static GSMState transitionFrom;
        internal static Vector2 mousePosition;

        internal void SetInspectedObject(IInspectable o)
        {
            InspectedObject = o;
            foldoutIngoingEdges = false;
            foldoutOutgoingEdges = false;
            isLeftScreenMinimized = false;
            GUI.FocusControl(null);
            GUI.changed = true;
        }

        #region Startup

        [MenuItem("Window/Graphical State Machine Editor")]
        public static GSMWindow ShowWindow()
        {
            var window = (GSMWindow)GetWindow(typeof(GSMWindow));
            window.titleContent = GSMUtilities.GetContent("Graphical State Machine Editor|Edit your state machine here");
            window.minSize = Vector2.one * (window.sideWindowWidth * 2 + 200);
            return window;
        }


        void OnEnable()
        {
            wantsMouseEnterLeaveWindow = true;
            wantsMouseMove = true;
        }
        #endregion

        #region Drawing

        #region GUI

        void OnGUI()
        {
            #region Styles
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };


            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12
            };

            defaultStyle = new GUIStyle(GUI.skin.label)
            {

            };

            machineTextStyle = new GUIStyle(GUI.skin.label);
            machineTextStyle.normal.textColor = Color.white;


            defaultStyleOutgrayed = new GUIStyle(defaultStyle);
            defaultStyleOutgrayed.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
            #endregion

            if (machine != null)
                EditorUtility.SetDirty(machine);

            CalculateRects();

            DrawCenterScreen();
            DrawLeftScreen();
            DrawRightScreen();
            DrawBottomBox();
            ProcessEvents();
            TrackMouse();


            ////////////////// SAVING /////////////////
            if (hasUnsavedChanges)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                hasUnsavedChanges = false;
                GUI.changed = true;
            }

            if (GUI.changed)
                Repaint();
        }

        private void CalculateRects()
        {
            indentOffset = boxPadding + EditorGUI.indentLevel * 15f;

            //Rect for the bottom box containing the asset
            boxRect = new Rect(0,
                position.height - (boxPadding * 2 + EditorGUIUtility.singleLineHeight),
                position.width,
                boxPadding * 2 + EditorGUIUtility.singleLineHeight);

            //Label inside the bottom box containing the label "Machine"
            labelRect = new Rect(indentOffset,
                boxRect.y + boxPadding,
                EditorGUIUtility.labelWidth - indentOffset - boxPadding,
                EditorGUIUtility.singleLineHeight);

            //Rect containing the field of the machine
            fieldRect = new Rect(labelRect.xMax - indentOffset,
                boxRect.y + boxPadding,
                position.width - labelRect.width - (machine == null ? boxPadding + buttonWidth : 0) - boxPadding,
                EditorGUIUtility.singleLineHeight);

            //Rect containing the "new" button
            buttonRect = new Rect(fieldRect.xMax + boxPadding,
                boxRect.y + boxPadding,
                buttonWidth,
                EditorGUIUtility.singleLineHeight);
        }

        private void TrackMouse()
        {
            if (Event.current.type == EventType.MouseMove)
            {
                mousePosition = Event.current.mousePosition;
            }
        }

        private Vector2 dragOffset;
        private Vector2 dragStart;
        private void ProcessEvents()
        {
            if (machine == null)
                return;

            Event evt = Event.current;


            //KEYS DOWN

            if (evt.type == EventType.KeyDown)
            {
                keysDown[evt.keyCode] = true;
            }
            if (evt.type == EventType.KeyUp)
            {
                keysDown[evt.keyCode] = false;
            }



            //DELETE SELECTION
            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Delete && InspectedObject != null)
            {
                if (InspectedObject is GSMEdge)
                {
                    machine.DeleteEdge(InspectedObject as GSMEdge);
                    SetInspectedObject(null);
                }
                if (InspectedObject is GSMState)
                {
                    machine.DeleteState(InspectedObject as GSMState);
                    SetInspectedObject(null);
                }
            }


            //DRAG OBJECTS
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                foreach(var state in machine.states)
                {
                    if(state.bounds.Move(machine.offset).Contains(evt.mousePosition))
                    {
                        draggedState = state;
                        dragOffset = Vector2.zero;
                        dragStart = state.bounds.position;
                        evt.Use();
                    }
                }
            }

            if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                draggedState = null; dragOffset = Vector2.zero;
            }

            if (evt.type == EventType.MouseDrag && evt.button == 0)
            {
                if (draggedState != null)
                {
                    dragOffset += evt.delta;
                    draggedState.bounds = new Rect(dragStart + dragOffset, draggedState.bounds.size);
                    if (evt.shift)
                    {
                        var a = draggedState.bounds;
                        draggedState.bounds = new Rect(Mathf.Round(a.x / bigTileSize) * bigTileSize,
                            Mathf.Round(a.y / bigTileSize) * bigTileSize,
                            a.width, a.height);
                    }
                    evt.Use();
                    GUI.changed = true;
                }
            }


        }

        #endregion




        #region Screen drawing
        /// <summary>
        /// Draws the bottom box where you can insert your machine asset
        /// </summary>
        private void DrawBottomBox()
        {

            ///////////////////// Draw bottom box ///////////////////////
            EditorGUI.DrawRect(boxRect, windowColorDefault);
            EditorGUI.DrawRect(new Rect(boxRect.x, boxRect.y, boxRect.width, 1), Color.gray);
            EditorGUI.PrefixLabel(labelRect, GSMUtilities.GetContent("Machine|A created machine file"));

            var m = (GSMStateMachine)EditorGUI.ObjectField(fieldRect, this.machine, typeof(GSMStateMachine), false);
            if (m == null)
            {
                GUILayout.BeginArea(buttonRect);
                if (GUILayout.Button("New"))
                {
                    m = GSMStateMachineFactory.CreateGSMFileAtPath("Assets/New Game State Machine.asset");
                }
                GUILayout.EndArea();
            }


            if (machine == null && m != null || m != machine)
                SetMachine(m);



        }




        [System.NonSerialized] internal float bigTileSize = 120;
        [System.NonSerialized] internal float smallTilesPerBig = 10;
        private void DrawCenterScreen()
        {
            var offset = machine != null ? machine.offset : Vector2.zero;

            EditorGUI.DrawRect(MachineBounds, machineBackgroundColor);

            float smallTileSize = bigTileSize / smallTilesPerBig;
            float smallStartX = offset.x % smallTileSize;
            float smallStartY = offset.y % smallTileSize;
            int smallXAmount = (int)(MachineBounds.width / smallTileSize + smallTilesPerBig + 1);
            int smallYAmount = (int)(MachineBounds.height / smallTileSize + 1);

            float bigStartX = offset.x % bigTileSize;
            float bigStartY = offset.y % bigTileSize;
            int bigXAmount = (int)(MachineBounds.width / bigTileSize + 3);
            int bigYAmount = (int)(MachineBounds.height / bigTileSize + 1);

            for (int i = 0; i < smallXAmount; i++)
            {
                EditorGUI.DrawRect(new Rect(smallStartX + i * smallTileSize, 0, 1, MachineBounds.size.y), machineGridColor);
            }


            for (int i = 0; i < smallYAmount; i++)
            {
                EditorGUI.DrawRect(new Rect(0, smallStartY + i * smallTileSize, MachineBounds.size.x, 1), machineGridColor);
            }

            for (int i = 0; i < bigXAmount; i++)
            {
                EditorGUI.DrawRect(new Rect(bigStartX + i * bigTileSize, 0, 2, MachineBounds.size.y), machineGridColorBig);
            }


            for (int i = 0; i < bigYAmount; i++)
            {
                EditorGUI.DrawRect(new Rect(0, bigStartY + i * bigTileSize, MachineBounds.size.x, 2), machineGridColorBig);
            }


            if (machine == null)
                return;

            machine.Draw();
        }
        #endregion


        #endregion

        private void SetMachine(GSMStateMachine machine)
        {
            this.machine = machine;
            machine.window = this;
            machine.FocusMachine();
        }


        #region Open by Doubleclick
        /// <summary>
        /// Called when user doubleclicks a GSM asset
        /// Opens GSM Editor and assigns the clicked asset
        /// </summary>
        /// <param name="instanceID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        [UnityEditor.Callbacks.OnOpenAsset()]
        public static bool OnOpen(int _, int line)
        {
            GSMStateMachine machine = Selection.activeObject as GSMStateMachine;
            if (machine != null)
            {
                var window = ShowWindow();
                window.SetMachine(machine);
                return true;
            }
            return false;
        }
        #endregion
    }

}