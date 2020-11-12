using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;

// config for the editor GUI
namespace GUINodeEditor {

    [System.Serializable]
    public class NodeEditorConfig {
        /// <summary>>
        /// Update is not called out of play mode.
        /// If true, nodeEditor will call nodeLogic.Update that calls Update for each node.
        /// </summary>
        public bool runUpdateInEditMode = true;

        /// Standard Unity GUISkin that changes the appearance of GUI elements.
        public GUISkin guiSkin;

        /// Minimap is drawn while panning or moving.
        public bool drawMinimap = true;
        public NodeEditorMinimap nodeEditorMinimap = new NodeEditorMinimap ();

        public DrawTextureOnScreen drawTextureOnScreen = new DrawTextureOnScreen ();

        /// Node window position is snapped to grid when dragging ends.
        public bool snapToGrid = true;
        public DrawGridOnScreen drawGridOnScreen = new DrawGridOnScreen ();

        /// If using a custom dock Texture, adjust this to get pixel perfect placement.
        public Rect dockRect = new Rect (0, 5, 14, 10);
        public Texture2D dockTexture;

        /// Offset of the tooltip from mousePosition.
        public Vector2 tooltipOffset = new Vector2 (10, 10);
        /// If tooltip with dock type should be drawn.
        public bool drawDockTypeTooltip = true;

        /// <summary>
        /// To be able to draw docks outside the the node window, another rect is
        /// wrapped around it that draws both node window and docks.
        /// This is for how much the docks overflow that inner node window.
        /// </summary>
        public Vector2 GetWindowOverflow () {
            return new Vector2 (dockRect.size.x / 2 + dockRect.position.x, 0);
        }

        public BezierConfig bezierConfig = new BezierConfig ();
    }

    [System.Serializable]
    public class BezierConfig {
        /// how many lines should each curve have. More means smoother.
        [Range (1, 20)] public int precision = 10;
        public Color normalColor = Color.black;
        /// color used when node.isTriggered is true.
        public Color triggeredColor = Color.green;
        public Color connectingColor = Color.cyan;
    }

    /// <summary>
    /// This is the node editor engine, it runs all the core logic of window manipulation.
    /// Inherits from MonoBehaviour, all editor configs are serialized here.
    /// </summary>
    [ExecuteInEditMode]
    public class NodeEditor: MonoBehaviour {
        public NodeEditorConfig config = new NodeEditorConfig ();

        /// Currently loaded file name. Has to be set externally like from SaveLoadGUI.
        public string saveLoadName = "defaultSave";
        /// Name of the folder where save/load files are kept (Resources folder exists in builds).
        public string saveLoadResourcesFolderName = "";

        /// last path that was either saved or loaded. This will be used to load after deserialize.
        public string lastSaveLoadPath = "";

        /// Menu type, to specify which menu node will be used for this editor.
        public TypeHolder menuTypeHolder = new TypeHolder ();

        /// Holds nodeEditor data like nodes, serialized with FullSerializer for polymorphism support.
        [NonSerialized] public NodeLogic nodeLogic = new NodeLogic ();

        [NonSerialized] public List<NodeWindow> selectedWindows = new List<NodeWindow> ();
        [NonSerialized] public NodeWindow hoveredWindow;

        [NonSerialized] public bool isDragging;
        [NonSerialized] public Vector2 startDraggingPosition;
        [NonSerialized] public NodeWindow mainDraggedWindow;

        [NonSerialized] public bool isPanning;
        [NonSerialized] public Vector2 oldPanningOffset;
        [NonSerialized] public Vector2 startPanningPosition;
        [NonSerialized] public bool isPrePanning;
        [NonSerialized] public Vector2 prePanningPosition;

        [NonSerialized] public bool isSelecting;
        [NonSerialized] public Vector2 startSelectPosition;
        [NonSerialized] public Vector2 startSelectPanningOffset;

        [NonSerialized] public bool isConnecting;
        [NonSerialized] public Dock startConnectDock;
        [NonSerialized] public bool shouldEndConnecting;

        [NonSerialized] public bool isDeconnecting;
        [NonSerialized] public Dock startDeconnectDock;
        [NonSerialized] public Dock endDeconnectDock;

        [NonSerialized] public NodeWindow renamingWindow;
        [NonSerialized]public string renamingName;

        [NonSerialized] public Node nodeMenu;
        [NonSerialized] public bool shouldSpawnMenu;

        [NonSerialized] public Dictionary<NodeWindow, bool> windowVisibilities = new Dictionary<NodeWindow, bool> ();

        [NonSerialized] bool didInitialLoad;

        ///<summary>
        /// Gets NodeEditor by the name of its gameObject. If not found,
        /// creates a gameObject with that name, attaches a new NodeEditor and returns it.
        ///</summary>
        public static NodeEditor GetOrCreateNodeEditor (string nodeEditorName, Type menuType = default(Type)) {
            GameObject neGO = GameObject.Find (nodeEditorName);
            NodeEditor ne;

            if (neGO == null) {
                neGO = new GameObject (nodeEditorName);
                neGO.AddComponent<NodeEditor> ();
            }
            ne = neGO.GetComponent<NodeEditor> ();

            // force set menuTypeName
            if (menuType != default(Type))
                ne.menuTypeHolder.type = menuType;

            return ne;
        }

        /// Calls nodeLogic.Update.
        public void Update () {
            HandleInitialLoad ();
            nodeLogic.Update ();
        }

        void Awake () {
            HandleInitialLoad ();
        }
        /// OnGUI of the editor, handles drawing of windows and all the functionality.
        public void DrawNodeWindows () {
            HandleGUISkin ();
            DrawConnecting ();
            DrawNodes ();
            HandleSelecting ();
            HandleEvents ();
            DrawTooltip ();
        }

        /// For UnityEditor, if Repaint should be called, to prevent low fps.
        public bool ShouldRepaint () {
            return isPanning || isConnecting || isDeconnecting || isDragging || isSelecting;
        }

        void DrawTooltip () {
            string tooltip = GUI.tooltip;
            if (tooltip == "")
                return;

            tooltip = tooltip.Substring (tooltip.LastIndexOf('.') + 1);

            Rect tooltipRect = new Rect (
                Event.current.mousePosition + config.tooltipOffset,
                GUI.skin.box.CalcSize(new GUIContent(tooltip)));

            GUI.Box (tooltipRect, tooltip);
        }

        /// Draws debug with some useful editor states.
        public void DrawDebug () {
            GUILayout.Label ("mousePosition: " +Event.current.mousePosition.ToString());
            GUILayout.Label ("panningOffset: " + nodeLogic.panningOffset.ToString());
            GUILayout.Label ("isPanning: " +isPanning);
            GUILayout.Label ("isConnecting: " +isConnecting);
            GUILayout.Label ("isDeconnecting: " +isDeconnecting);
            GUILayout.Label ("isRenaming: " + (renamingWindow != null));
            GUILayout.Label ("isDragging: " +isDragging);
            GUILayout.Label ("hovered: " + (hoveredWindow != null ? hoveredWindow.title : "null"));
            GUILayout.Label ("isSelecting: " +isSelecting);
            GUILayout.Label ("selected: ");
            foreach (NodeWindow nw in selectedWindows)
                GUILayout.Label ("    " + nw.title);
        }

        void HandleInitialLoad () {
            if (! didInitialLoad) {
                Load ();
                didInitialLoad = true;
            }
        }
        void HandleGUISkin () {
            GUI.skin = config.guiSkin != null ? config.guiSkin : GUI.skin;
        }

        #region DRAWING
        public void DrawBackground () {
            config.drawTextureOnScreen.OnGUI ();
        }

        public void DrawGrid () {
            // set panningOffset
            config.drawGridOnScreen.panningOffset = nodeLogic.panningOffset;
            config.drawGridOnScreen.OnGUI ();
        }

        public void DrawMinimap () {
            if (! isPanning && ! isDragging)
                return;
            if (!config.drawMinimap)
                return;

            config.nodeEditorMinimap.nodes = nodeLogic.nodes;
            config.nodeEditorMinimap.panningOffset = nodeLogic.panningOffset;
            config.nodeEditorMinimap.dockRectSize = config.dockRect.size;
            config.nodeEditorMinimap.rect = new Rect (0, 0, Screen.width, Screen.height);
            config.nodeEditorMinimap.DrawMinimap ();
        }
        #endregion

        void EndRenaming (bool applyName = false) {
            if (renamingWindow == null)
                return;
            if (applyName)
                renamingWindow.title = renamingName;
            renamingWindow = null;
        }

        bool IsMouseHoveringTitle () {
            if (hoveredWindow == null)
                return false;
            Rect hoveredTitleRect = WithOffset (hoveredWindow.rect);
            hoveredTitleRect.height = GetTitleOffset().y;
            return hoveredTitleRect.Contains (Event.current.mousePosition);
        }

        Vector2 GetTitleOffset () {
            return new Vector2 (0, GUI.skin.window.border.top);
        }

        Rect WithOffset (Rect original) {
            return new Rect (WithOffset (original.position), original.size);
        }

        Vector2 WithOffset (Vector2 original) {
            return original + nodeLogic.panningOffset;
        }

        void HandleEvents ()
        {
            Event e = Event.current;

            // remove focus from input
            if (e.type == EventType.MouseDown && hoveredWindow == null)
                GUI.FocusControl (null);

            // on mouse down
            if (e.type == EventType.MouseDrag) {
                // on left mouse down
                if (e.button == 0) {
                    // start selection if clicked background and not connecting
                    if (! isSelecting && hoveredWindow == null && ! isConnecting) {
                        Rect screenRect = new Rect (0, 0, Screen.width, Screen.height);
                        if (screenRect.Contains (e.mousePosition)) {
                            isSelecting = true;
                            startSelectPosition = e.mousePosition;
                            startSelectPanningOffset = nodeLogic.panningOffset;
                        }
                    }
                }
            }

            if (e.type == EventType.MouseDown && e.button == 1)
                shouldEndConnecting = true;

            if (e.type == EventType.MouseUp && e.button == 1) {
                shouldSpawnMenu = ! isConnecting && ! isDeconnecting;

                if (shouldEndConnecting) {
                    EndDeconnecting (removeConnection: true);
                    EndConnecting ();
                    shouldEndConnecting = false;
                }
            }

            // select dragged window if it was unselected
            if (e.type == EventType.MouseDrag && hoveredWindow != null && ! isConnecting) {
                if ( !selectedWindows.Contains (hoveredWindow) )
                    ClickSelect (hoveredWindow);
            }

            if (e.type == EventType.MouseDrag && e.button == 1)
                // on drag prevent menu spawn
                shouldSpawnMenu = false;

            // pan on drag rmb or click mmb
            if (e.type == EventType.MouseDown && e.button == 1 ||
                e.type == EventType.MouseDown && e.button == 2 ||
                (isSelecting && e.type == EventType.MouseDown && (e.button == 1 || e.button == 2))
            ) {
                if (! isPanning && ! isPrePanning) {
                    isPrePanning = true;
                    nodeMenu = null;
                    prePanningPosition = e.mousePosition;
                }
            }

            if (isPrePanning && ! isPanning) {
                if ((prePanningPosition - e.mousePosition).magnitude > 4 && e.type == EventType.Repaint) {
                    isPanning = true;
                    // on start panning store oldPanningOffset
                    oldPanningOffset = nodeLogic.panningOffset;
                    startPanningPosition = e.mousePosition;

                    // dont deconnect if panning
                    shouldEndConnecting = false;
                }
            }

            // on mouse up
            if (e.type == EventType.MouseUp || Event.current.rawType == EventType.MouseUp) {
                if (e.button == 0) {
                    // close menu if clicked on another window
                    if (nodeMenu != null && nodeMenu.nodeWindow != hoveredWindow)
                        nodeMenu = null;

                    // select clicked window if not dragging
                    if (! isDragging && ! isConnecting)
                        ClickSelect (hoveredWindow);

                    // stop dragging
                    if (isDragging) {
                        if (config.snapToGrid) {
                            float snapValue = config.drawGridOnScreen.gridUnit / 2;
                            Vector2 mouseDiff = e.mousePosition - startDraggingPosition;
                            Vector2 mouseSnappedDiff = mouseDiff - Snap (mouseDiff, snapValue);
                            foreach (NodeWindow nw in selectedWindows) {
                                nw.rect.position = Snap (nw.rect.position - mouseSnappedDiff, snapValue) + new Vector2 (-2, 0);
                            }
                        }

                        isDragging = false;
                        mainDraggedWindow = null;
                    }

                    // end selecting
                    isSelecting = false;

                    // move to end for render order
                    List<Node> selectedNodes = new List<Node> ();
                    foreach (NodeWindow nw in selectedWindows)
                        selectedNodes.Add (nw.node);
                    nodeLogic.nodes.RemoveAll ((n) => selectedNodes.Contains(n));
                    nodeLogic.nodes.AddRange (selectedNodes);
                }

                if (! isPanning && e.button == 1) {
                    // if we did not drag
                    if (shouldSpawnMenu) {
                        // spawn menu node if clicked on background or title
                        if (IsMouseHoveringTitle () || hoveredWindow == null && ! isConnecting) {
                            // set node type to custom menu
                            //Type menuType = Assembly.GetExecutingAssembly ().GetType (menuTypeName);
                            if (menuTypeHolder.type != default (Type)) {
                                MethodInfo method = GetType ().GetMethod ("CreateNewWindow")
                                    .MakeGenericMethod (new Type[] { menuTypeHolder.type });
                                nodeMenu = (Node)method.Invoke (this, new object[] { e.mousePosition, true });
                            }
                            //else Debug.LogWarning ("Unknown Node menu type: " + menuType.ToString());
                            else Debug.LogWarning ("menuType not set.");
                        }
                        // remove menu if not clicked on background or title
                        else nodeMenu = null;
                    }
                }

                if (e.button == 0 || e.button == 1 || e.button == 2) {
                    // end panning
                    isPanning = false;
                    isPrePanning = false;
                }
            }

            if (hoveredWindow == null && e.clickCount == 2 && isConnecting) {
                EndDeconnecting (removeConnection: true);
                EndConnecting ();
            }

            // start renaming window on title doubleclick
            if (IsMouseHoveringTitle() && e.clickCount == 2) {
                renamingWindow = hoveredWindow;
                renamingName = hoveredWindow.title;
            }

            // move all selected windows on window title drag
            if (e.type == EventType.MouseDrag &&
                hoveredWindow != null &&
                ! (isSelecting || isConnecting || isDragging || isPrePanning || isPanning) &&
                (nodeMenu == null || nodeMenu.nodeWindow != hoveredWindow)
            ) {
                // set window to mouse distance offset by dragging the title
                if (! isDragging && IsMouseHoveringTitle()) {
                    isDragging = true;
                    mainDraggedWindow = hoveredWindow;
                    startDraggingPosition = e.mousePosition;

                    nodeMenu = null;
                    // set offsets for all selected windows
                    foreach (NodeWindow nw in selectedWindows)
                        // this is the distance from each selected window to mouse
                        nw.dragStartOffset = nw.rect.position - e.mousePosition;
                }
            }

            // set panning offset
            if (isPanning) {
                Vector2 mouseOffset = Event.current.mousePosition - startPanningPosition;
                nodeLogic.panningOffset = oldPanningOffset + mouseOffset;
            }

            // move all selected windows according to mouse position
            if (isDragging)
                foreach (NodeWindow nw in selectedWindows)
                    nw.rect.position = e.mousePosition + nw.dragStartOffset;

            // exit renaming if clicked on background or pressed Esc
            if (e.type == EventType.MouseDown && hoveredWindow == null || e.keyCode == KeyCode.Escape)
                EndRenaming ();
            // apply renaming if pressed Enter
            if (e.keyCode == KeyCode.Return)
                EndRenaming(applyName: true);

            // delete selected if clicked Del
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
                DeleteSelected();
        }

        Vector2 Snap (Vector2 vector, float snapValue) {
            return new Vector2 (Snap (vector.x, snapValue), Snap (vector.y, snapValue));
        }
        float Snap (float value, float snapValue) {
            return Mathf.Round (Mathf.Round (value / snapValue) * snapValue + 0.01f);
        }

        void HandleSelecting () {
            Event e = Event.current;
            if (! isSelecting || e.type != EventType.Repaint)
                return;

            Vector2 selectingPanningOffset = startSelectPanningOffset - nodeLogic.panningOffset;
            Vector2 pos = startSelectPosition - selectingPanningOffset;
            // determine selection box rect size
            Vector2 selectionSize = e.mousePosition - startSelectPosition + selectingPanningOffset;
            // offset for width if selection is dragged to the left
            if (selectionSize.x < 0) {
                selectionSize.x *= -1;
                pos.x -= selectionSize.x;
            }
            // offset for height if selection is dragged upwards
            if (selectionSize.y < 0) {
                selectionSize.y *= -1;
                pos.y -= selectionSize.y;
            }
            Rect selectionRect = new Rect (pos, selectionSize);

            // change opacity
            Color origColor = GUI.color;
            GUI.color = Drawing.MultOpacity (origColor, 0.1f);
            // draw selection rect
            GUI.DrawTexture(selectionRect, Texture2D.whiteTexture);
            // reset color
            GUI.color = origColor;

            // add to selected windows if they overlap with selection rect
            foreach (Node node in nodeLogic.nodes) {
                Rect nodeRect = GetInnerOverflowRect (WithOffset (node.nodeWindow.rect));
                nodeRect.position += config.GetWindowOverflow ();

                if (selectionRect.Overlaps (nodeRect)) {
                    if (!selectedWindows.Contains (node.nodeWindow))
                        selectedWindows.Insert (0, node.nodeWindow);
                }
                else selectedWindows.Remove (node.nodeWindow);
            }
        }

        Vector2 GetDockCenter (Dock dock) {
            Rect rect = GetDockRect (dock, dock.GetType () == typeof(DockOutput));
            rect.position += config.GetWindowOverflow ();
            return rect.center;
        }

        // drawing a connection from clicked dock to mouse position
        void DrawConnecting () {
            if (! isConnecting || startConnectDock == null)
                return;

            // centered start dock position
            Vector2 start = GetDockCenter (startConnectDock) + WithOffset (startConnectDock.node.nodeWindow.rect).position
                + new Vector2 (config.dockRect.size.x/2, 0);
            Vector2 end = Event.current.mousePosition;

            if (startConnectDock.GetType() == typeof (DockInput))
                Bezier.DrawBezier (end, start, config.bezierConfig.connectingColor);
            else Bezier.DrawBezier (start, end, config.bezierConfig.connectingColor);
        }

        void EndDeconnecting (bool removeConnection = false) {
            if (removeConnection && startDeconnectDock != null && endDeconnectDock != null) {
                startDeconnectDock.targets.Remove (endDeconnectDock);
                endDeconnectDock.targets.Remove (startDeconnectDock);
            }
            // reset values
            startDeconnectDock = null;
            endDeconnectDock = null;
            isDeconnecting = false;

            // EndConnecting ();
        }

        void HandleDockClick (Dock dock) {
            if (Event.current.button == 0) DoConnecting (dock);
            if (Event.current.button == 1) DoDeconnecting (dock); 
        }

        /// Moves the connection from one dock to another.
        public void MoveConnection (DockOutput moveTargetDock, DockInput fromDock, DockInput toDock) {
            DeconnectDocks (moveTargetDock, fromDock);
            ConnectDocks (moveTargetDock, toDock);
        }
        public void MoveConnection (DockInput moveTargetDock, DockOutput fromDock, DockOutput toDock) {
            DeconnectDocks (moveTargetDock, fromDock);
            ConnectDocks (moveTargetDock, toDock);
        }

        /// Deconnects the docks, remoces each other from their targets.
        public void DeconnectDocks (Dock a, Dock b) {
            a.targets.Remove (b);
            b.targets.Remove (a);
        }

        void DoDeconnecting (Dock dock) {
            isDeconnecting = true;

            // prevent rmb from deconnecting immediately
            shouldEndConnecting = false;

            startDeconnectDock = dock;
            endDeconnectDock = GetNextTarget(startDeconnectDock);
            // end previous connecting
            EndConnecting ();
            DoConnecting (endDeconnectDock);
        }

        int _tmpDockIndex = 0;
        Dock GetNextTarget(Dock parent) {
            int i = 0;
            foreach (Dock dock in parent.targets) {
                if (i++ == _tmpDockIndex) {
                    _tmpDockIndex = i % parent.targets.Count;
                    return dock;
                }
            }
            _tmpDockIndex = 0;
            return null;
        }

        void EndConnecting () {
            startConnectDock = null;
            isConnecting = false;
        }

        /// Returns if the two docks are of the matching type and not from the same parent node.
        public bool IsAllowedConnectionBetweenDocks (Dock startDock, Dock endDock) {
            if (startDock.node == endDock.node)
                return false;
            return startDock.typeHolder.type == endDock.typeHolder.type;
        }

        public void ConnectDocks (DockOutput output, DockInput input) {
            ConnectDocks (input, output);
        }

        public void ConnectDocks (DockInput input, DockOutput output) {
            // if not already added, add each dock as target to other dock
            if (! input.targets.Contains(output)) input.targets.Add (output);
            if (! output.targets.Contains(input)) output.targets.Add (input);
        }

        // TODO detect and warn for recursive connection
        void DoConnecting (Dock dock) {
            if (isSelecting || isDragging)
                return;

            // if dock is start dock
            if (startConnectDock == null) {
                isConnecting = true;
                startConnectDock = dock;
            }
            // if we are not connecting docks with the same parent node
            else if (startConnectDock.node != dock.node) {
                #region DETECT_INPUT(LEFT)_AND_OUTPUT(RIGHT)_DOCK
                // OUTPUT-input will be switched to input-OUTPUT for both ways connecting
                DockInput input = null;
                DockOutput output = null;

                // if connecting input with OUTPUT
                if (startConnectDock.GetType () == typeof(DockInput) && dock.GetType () == typeof(DockOutput)) {
                    input = (DockInput)startConnectDock;
                    output = (DockOutput)dock;
                }
                // if connecting OUTPUT with input
                else if (startConnectDock.GetType () == typeof(DockOutput) && dock.GetType () == typeof(DockInput)) {
                    input = (DockInput)dock;
                    output = (DockOutput)startConnectDock;
                }
                #endregion

                // if input does not allow connecting to this output check for its sibling outputs (Node_Changer)
                if (input != null && output != null && ! IsAllowedConnectionBetweenDocks (input, output))
                    output = output.node.outputs.Find (x => x.typeHolder.type == input.typeHolder.type);

                if (output != null) {
                    ConnectDocks (input, output);
                    EndDeconnecting (removeConnection: dock != startDeconnectDock);
                    EndConnecting ();
                }
            }
        }

        /// If <Shift> is held toggles selection, else deselects all and selects just it.
        public void ClickSelect (NodeWindow nw) {
            // fix for generic menu that consumes Event.current
            if (Event.current == null)
                return;

            // if are already selecting with mouse drag
            if (isSelecting)
                return;

            // if clicking on the menu node
            if (nodeMenu != null && nodeMenu.nodeWindow == nw)
                return;

            // toggle selection
            if (Event.current.shift) {
                if (selectedWindows.Contains (nw))
                    UnselectWindow (nw);
                else SelectWindow (nw);
            }
            // select single 
            else {
                DeselectAll ();
                SelectWindow (nw);
            }
        }

        public void UnselectWindow (NodeWindow nw) {
            // if node is selected, unselect it
            if (selectedWindows.Contains (nw))
                selectedWindows.Remove (nw);
        }

        public void SelectWindow (NodeWindow nw) {
            if (nw == null || isDragging)
                return;

            // if already selected, remove it so it gets added to end of list
            if (selectedWindows.Contains (hoveredWindow))
                selectedWindows.Remove (nw);
            // select
            selectedWindows.Insert(0, nw);

            // if already selected, move it to end of list for render order
            if (nodeLogic.nodes.Contains(nw.node))
                nodeLogic.nodes.Remove (nw.node);
            nodeLogic.nodes.Add (nw.node);
        }

        /// Call this from the menu node without parameters, it will get menu position.
        public T CreateNewWindow <T> (Vector2 position = default(Vector2), bool isMenu = false) where T: Node, new() {
            if (position == default(Vector2))
                position = nodeMenu.nodeWindow.rect.position;

            // revert panning, menu has global position
            if (! isMenu)
                position -= nodeLogic.panningOffset;

            position -= config.GetWindowOverflow ();

            // snap to grid
            if (! isMenu && config.snapToGrid)
                position = Snap (position, config.drawGridOnScreen.gridUnit / 2);

            T node = new T ();
            node.Init (position);

            if (isMenu)
                ((NodeWindow_Menu)node.nodeWindow).clickedWindow = hoveredWindow;

            // if no title set timestamp
            string ms = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond).ToString ();
            if (node.nodeWindow.title == "")
                node.nodeWindow.title = ms.Substring (5);

            InitializeWindow (node.nodeWindow);
            if (! isMenu) {
                nodeLogic.nodes.Add (node);
                // automatically select new node
                ClickSelect (node.nodeWindow);
                nodeMenu = null;
            }

            return node;
        }

        // called upon new node and deserialization
        void InitializeWindow (NodeWindow nw) {
            // set reference to this editor
            nw.nodeEditor = this;
        }

        /// Also used in minimap.
        public void DrawNodeConnections (Node n, Vector2 positionOffset = default(Vector2), float scale = 1, bool isMinimap = false) {
            Rect screenRect = new Rect (0, 0, Screen.width, Screen.height);

            // draw bezier connections from all OUTPUTS to inputs
            // both direction render in one list iteration makes connections appear on top or under based on nodes order
            // you can debug visually both direction connectivity (change black to red below and change selection)
            foreach (DockOutput dockOutput in n.outputs) {
                foreach (DockInput dockInput in dockOutput.targets) {

                    // dockInput and dockOutput are swapped to get correct bezier curvature
                    Vector2 end = scale * (GetDockCenter(dockOutput) + dockOutput.node.nodeWindow.rect.position) + positionOffset;
                    Vector2 start = scale * (GetDockCenter(dockInput) + dockInput.node.nodeWindow.rect.position) + positionOffset;

                    // apply panning offset
                    if (! isMinimap) {
                        start = WithOffset (start);
                        end = WithOffset (end);
                    }

                    if (screenRect.Overlaps (Drawing.GetRightRectFromPoints (start, end))) {
                        bool isTriggered = dockOutput.node.isTriggered;
                        if (!IsDisconnecting (dockInput, dockOutput))
                            // end and start are swapped as we are drawing in oposite direction
                            Bezier.DrawBezier (end, start,
                                color: isTriggered ? config.bezierConfig.triggeredColor: config.bezierConfig.normalColor,
                                opacity: isMinimap ? 0.5f * config.nodeEditorMinimap.opacity : 0.75f,
                                precision: isMinimap ? config.bezierConfig.precision / 2 : config.bezierConfig.precision);
                    }
                }
            }

        }

        void DrawNodes () {
            // reset hovered window
            hoveredWindow = null;

            // calculate window and screen rect overlaps
            if (Event.current.type == EventType.Layout) {
                Rect screenRect = new Rect (0, 0, Screen.width, Screen.height);
                foreach (Node n in nodeLogic.nodes)
                    windowVisibilities [n.nodeWindow] = screenRect.Overlaps (WithOffset (n.nodeWindow.rect));
            }
            //draw nodes
            int i = 0;

            foreach (Node n in nodeLogic.nodes) {
                // skip ongui if window does not overlap the screen
                if (! windowVisibilities.ContainsKey (n.nodeWindow) || ! windowVisibilities [n.nodeWindow]) {
                    i++;
                    DrawNodeConnections (n);
                    continue;
                }

                // apply cached size to prevent visual glitches
                if (n.nodeWindow.rect.size == Vector2.zero)
                    n.nodeWindow.rect.size = n.nodeWindow.cachedSize;

                // apply panning
                Rect nodeWindowRect = WithOffset (n.nodeWindow.rect);

                // (TODO)
                Rect r = GetInnerOverflowRect (WithOffset (n.nodeWindow.rect));
                r.position += config.GetWindowOverflow ();
                // detect hovered, if multiple the last is the top one
                if (r.Contains (Event.current.mousePosition))
                    hoveredWindow = n.nodeWindow;

                // apply overflow
                nodeWindowRect = GetOuterOverflowRect (nodeWindowRect);

                Popup p = n.nodeWindow.popup;
                if (isSelecting)
                    p.identifier = null;

                // if this is the last selected node, make it a window
                if (p.identifier == null && nodeMenu == null && ! isSelecting && ! isPanning && ! isDragging &&
                    selectedWindows.Count > 0 && n.nodeWindow == selectedWindows[0]) {
                    GUI.FocusWindow (i);
                    GUI.Window (i++, nodeWindowRect, DrawNodeWindow, "", GUI.skin.label);
                }
                else {
                    GUILayout.BeginArea (nodeWindowRect, GUI.skin.label);
                    DrawNodeWindow (i++);
                    GUILayout.EndArea ();
                }
                // draw bezier connections
                DrawNodeConnections (n);

                // draw popups
                if (p.identifier == null)
                    continue;

                DeselectAll ();

                // get local rect
                Rect popupRect = p.GetListRect();
                // add node position
                // (TODO)
                popupRect.position += n.nodeWindow.rect.position + 2 * config.GetWindowOverflow();
                popupRect = WithOffset (popupRect);

                GUI.Box (popupRect, "");
                GUI.Box (popupRect, "");
                GUI.Window (-i, popupRect, (id) => p.DrawList (), "", GUI.skin.box);

                if (Event.current.type == EventType.MouseDown)
                    p.identifier = null;
            }
            if (nodeMenu != null) {
                GUI.FocusWindow (-2);
                Rect menuRect = nodeMenu.nodeWindow.rect;
                menuRect.position -= config.GetWindowOverflow ();
                GUI.Window (-2, menuRect, DrawNodeWindow, "", GUI.skin.label);
            }
        }

        /// Clears selectedWindows list.
        public void DeselectAll () {
            selectedWindows.Clear ();
        }

        Rect GetOuterOverflowRect (Rect rect) {
            Vector2 overflowOffset = config.GetWindowOverflow ();
            return new Rect (
                rect.position + overflowOffset,
                rect.size);
        }
        Rect GetInnerOverflowRect (Rect rect) {
            float overflow = config.GetWindowOverflow ().x;
            return new Rect (
                rect.position + new Vector2 (overflow, 0),
                rect.size - new Vector2 (overflow * 2, 0));
        }
        Rect GetLocalInnerOverflowRect (Rect rect) {
            float overflow = config.GetWindowOverflow ().x;
            return new Rect (
                new Vector2(overflow, 0),
                new Vector2 (rect.size.x + 2 * - overflow, rect.size.y));
        }

        public bool IsNodeSelected (Node node) {
            return selectedWindows.Contains (node.nodeWindow);
        }

        void DrawDocks (Node n) {
            Event e = Event.current;
            // inputs
            foreach (DockInput dockInput in n.inputs) {

                // skip docks that have not been drawn in dockWindow.OnGUI
                if (dockInput.dockWindow.rect.position == default(Vector2))
                    continue;

                // dock texture
                Texture2D dockTexture = config.dockTexture != null ? config.dockTexture : GUI.skin.window.normal.background;

                // set dock color
                Color origColor = GUI.color;

                // if node is selected use same color to match highlight
                if (IsNodeSelected (dockInput.node))
                    GUI.color = dockInput.node.nodeWindow.backgroundColor != default(Color) ?
                        dockInput.node.nodeWindow.backgroundColor : Color.white;

                if (startConnectDock != null && startConnectDock.GetType () == typeof(DockOutput)) {
                    bool setOnNormal = false;
                    if (IsAllowedConnectionBetweenDocks (dockInput, startConnectDock)) {
                        GUI.color = Color.green;
                        setOnNormal = true;
                    }
                    else if (startConnectDock.node != dockInput.node &&
                        startConnectDock.node.outputs.Find(x => x.typeHolder.type == dockInput.typeHolder.type) != null) {
                        GUI.color = Color.yellow;
                        setOnNormal = true;
                    }
                    if (setOnNormal)
                        dockTexture = config.dockTexture != null ? config.dockTexture : GUI.skin.window.onNormal.background;
                }
                //if (dockInput.dockWindow.backgroundColor != default(Color))
                //    GUI.color = dockInput.dockWindow.backgroundColor;

                Rect dockRect = GetDockRect (dockInput);

                // draw dock
                GUI.DrawTexture (dockRect, dockTexture);
                if (config.drawDockTypeTooltip)
                    GUI.Label (dockRect, new GUIContent ("", dockInput.typeHolder.type.ToString()));

                // start dock connecting
                if ((e.type == EventType.MouseUp || (! isConnecting && e.type == EventType.MouseDrag)) && dockRect.Contains (e.mousePosition))
                    n.nodeWindow.nodeEditor.HandleDockClick (dockInput);

                // reset dock color
                GUI.color = origColor;
            }

            // outputs
            foreach (DockOutput dockOutput in n.outputs) {

                // skip docks that have not been drawn in dockWindow.OnGUI
                if (dockOutput.dockWindow.rect.position == default(Vector2))
                    continue;

                // dock texture
                Texture2D dockTexture = config.dockTexture != null ? config.dockTexture : GUI.skin.window.normal.background;

                // set dock color
                Color origColor = GUI.color;

                // if node is selected use same color to match highlight
                if (IsNodeSelected (dockOutput.node))
                    GUI.color = dockOutput.node.nodeWindow.backgroundColor != default(Color) ?
                        dockOutput.node.nodeWindow.backgroundColor : Color.white;

                if (startConnectDock != null && startConnectDock.GetType () == typeof(DockInput)) {
                    bool setOnNormal = false;
                    if (IsAllowedConnectionBetweenDocks (startConnectDock, dockOutput)) {
                        GUI.color = Color.green;
                        setOnNormal = true;
                    }
                    else if (startConnectDock.node.inputs.Find(x => x.typeHolder.type == dockOutput.typeHolder.type) != null) {
                        GUI.color = startConnectDock.node.outputs.Count == 1 ? Color.green : Color.yellow;
                        setOnNormal = true;
                    }
                    if (setOnNormal)
                        dockTexture = config.dockTexture != null ? config.dockTexture : GUI.skin.window.onNormal.background;
                }
                //if (dockOutput.dockWindow.backgroundColor != default(Color))
                //    GUI.color = dockOutput.dockWindow.backgroundColor;

                Rect dockRect = GetDockRect (dockOutput, isRight: true);

                // draw dock
                GUI.DrawTexture (dockRect, dockTexture);
                if (config.drawDockTypeTooltip)
                    GUI.Label (dockRect, new GUIContent ("", dockOutput.typeHolder.type.ToString()));

                // start dock connecting
                if ((e.type == EventType.MouseUp || (! isConnecting && e.type == EventType.MouseDrag)) && dockRect.Contains (e.mousePosition))
                    n.nodeWindow.nodeEditor.HandleDockClick (dockOutput);

                // reset dock color
                GUI.color = origColor;
            }
        }

        /// Gets the position where dock rect should be rendered, local to its parent node.
        Rect GetDockRect (Dock dock, bool isRight = false) {
            return new Rect (
                new Vector2 (
                    - config.GetWindowOverflow().x * (isRight ? 1 : -1)
                    + config.dockRect.position.x * (isRight ? 1 : -1)
                    - config.dockRect.size.x /2
                    + (isRight ? -1 : 0)
                    + (isRight ? dock.node.nodeWindow.rect.size.x : 0),

                    dock.dockWindow.rect.position.y + config.dockRect.position.y),
                config.dockRect.size);
        }

        void DrawNodeWindow (int index) {
            Node n = index == -2 ? nodeMenu : nodeLogic.nodes [index];

            Rect nodeRect = GetLocalInnerOverflowRect (n.nodeWindow.rect);

            // skin for selected windows
            GUIStyle s = new GUIStyle (GUI.skin.window);

            if (selectedWindows.Contains (n.nodeWindow))
                s.normal.background = s.onNormal.background;

            // save orig color to revert it after applying to rect or window
            Color origColor = GUI.color;
            // get custom nodeWindow color if any
            Color windowColor = n.nodeWindow.backgroundColor;
            if (windowColor == default(Color))
                windowColor = origColor;
            // apply node color
            GUI.color = windowColor;

            // this is a window style inside GUI.Window
            GUILayout.BeginArea (nodeRect, s);
            {
                // reset color to affect only background
                GUI.color = origColor;

                #region TITLE
                // center title text
                GUIStyle styleLabel = new GUIStyle (GUI.skin.label);
                GUIStyle styleTextField = new GUIStyle (GUI.skin.label);
                styleLabel.alignment = TextAnchor.UpperCenter;
                styleTextField.alignment = TextAnchor.UpperCenter;
                styleLabel.padding.bottom = 0;
                styleTextField.padding.bottom = 0;
                // text field for renaming title
                Rect titleRect = new Rect (0, 0, nodeRect.size.x, GetTitleOffset().y);
                if (renamingWindow == n.nodeWindow)
                    renamingName = GUI.TextField (titleRect, renamingName, 25, styleTextField);
                // title
                else GUI.Label (titleRect, n.nodeWindow.title, styleLabel);
                #endregion

                // call OnGUI from each node class
                n.nodeWindow.OnGUI ();

                // auto calculate height
                GUILayout.Label ("");
                if (Event.current.type == EventType.Repaint || (n.nodeWindow.rect.size == Vector2.zero && Event.current.type != EventType.Layout)) {
                    Rect endOfGUIRect = GUILayoutUtility.GetLastRect ();
                    float width = n.nodeWindow.GetWindowWidth ();
                    float height = n.nodeWindow.GetWindowHeight ();
                    if (height == -1) height = endOfGUIRect.y + GUI.skin.label.padding.bottom;
                    n.nodeWindow.SetWindowSize (new Vector2 (width, height));
                }
            }

            GUILayout.EndArea ();

            // draw docks outside inner window area
            DrawDocks (n);
        }

        // are docks currently disconnecting, so we prevent drawing their connection bezier
        bool IsDisconnecting (DockInput dockInput, DockOutput dockOutput) {
            if (! isDeconnecting || startDeconnectDock == null)
                return false;

            // OUTPUT-input will be switched to input-OUTPUT for both ways deconnecting
            Dock deconnInput = startDeconnectDock;
            Dock deconnOutput = endDeconnectDock;
            if (startDeconnectDock.GetType() == typeof(DockOutput)) {
                deconnInput = endDeconnectDock;
                deconnOutput = startDeconnectDock;
            }
            // do these input and output match holder variables
            return deconnInput == dockInput && deconnOutput == dockOutput;
        }

        // removes dock back connections from all of its target docks
        void RemoveDockFromItsTargets (Dock dock) {
            foreach (Dock target in dock.targets)
                target.targets.Remove (dock);
        }

        /// Triggered on <Del>.
        public void DeleteSelected () {
            List<NodeWindow> toDeleteWindows = new List<NodeWindow> (selectedWindows);
            // iterate all input and output docks and disconnect them
            foreach (NodeWindow nw in toDeleteWindows) {
                // remove this input dock from all OUTPUT docks that have it as its target
                foreach (DockInput dockInput in nw.node.inputs)
                    RemoveDockFromItsTargets (dockInput);
                // remove this OUTPUT dock from all input docks that this dock is targeting
                foreach (DockOutput dockOutput in nw.node.outputs)
                    RemoveDockFromItsTargets (dockOutput);
                // remove node
                nodeLogic.nodes.Remove (nw.node);
            }
            DeselectAll ();
        }

        /// Gets Resources folder path with fileName
        public string GetSaveLoadPath (string fileName) {
            // TODO check this elsewhere
            if (saveLoadResourcesFolderName == "")
                saveLoadResourcesFolderName = name + "Saves";

            return Path.Combine (saveLoadResourcesFolderName, fileName);
        }

        /// <summary>
        /// Saves the file with given fileName to save/load path in Resources folder.
        /// Overwrites if file with the same name exists (you have to check that separately).
        /// Sets the lastSaveLoadPath.
        /// </summary>
        public void Save (string fileName = "") {
            if (fileName == "") fileName = saveLoadName;
            if (fileName == "") return;

            string path = GetSaveLoadPath (fileName);
            SerializationSave (path);
            // set for loading after entering runtime
            lastSaveLoadPath = path;
        }

        void SerializationLoad (string path) { nodeLogic = Serialization.Load <NodeLogic> (path); }
        void SerializationSave (string path) { Serialization.Save <NodeLogic> (path, nodeLogic); }

        /// <summary>
        /// Loads the file with given fileName from save/load path in Resources folder.
        /// If no file is found, creates a new save file with that name.
        /// Sets the lastSaveLoadPath.
        /// </summary>
        public void Load (string fileName = "") {
            if (fileName == "")
                fileName = saveLoadName;

            // if there is no save file, create empty one
            string fullPath = Serialization.GetFullResourcesPath (GetSaveLoadPath (fileName)) + ".txt";
            if (! File.Exists (fullPath))
                Save (fileName);

            string path = GetSaveLoadPath (fileName);
            SerializationLoad (path);
            // set for loading after entering runtime
            lastSaveLoadPath = path;

            DeselectAll ();

            // initialize node windows
            foreach (Node n in nodeLogic.nodes)
                InitializeWindow (n.nodeWindow);
        }

    }
}