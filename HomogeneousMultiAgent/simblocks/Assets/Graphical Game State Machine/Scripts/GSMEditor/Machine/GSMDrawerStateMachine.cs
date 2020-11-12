using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace GSM
{
    //Used for drawing
    public partial class GSMStateMachine
    {
        private Rect MachineBounds { get { return window.MachineBounds; } }
        private bool isMouseOver = false;
        private bool isScrolling = false;
        public GSMWindow window;
        internal Vector2 offset;
        internal List<(GSMEdge, int)> highlightedEdges = new List<(GSMEdge, int)>();

        private Vector2 MachineMidpoint { get
            {
                Vector2 target = Vector2.zero;
                foreach (var state in states)
                {
                    target += state.bounds.center;
                }
                target /= states.Count;
                return target;
            } }

        /// <summary>
        /// Main Draw function. Controls the others
        /// </summary>
        /// <param name="window"></param>
        internal void Draw()
        {
            ProcessEvents();
            DrawTransitions();
            DrawStates();
            PerformScrolling();

        }

        private void DrawStates()
        {
            foreach (var state in states)
            {
                DrawState(state);
            }
        }

        private void DrawTransitions()
        {
            //Existing Transitions
            foreach (var edge in edges)
            {
                DrawEdge(edge);
            }



            //New Transition
            if (!GSMWindow.transitionMode)
                return;

            DrawArrow(GSMWindow.transitionFrom.bounds.center + offset,
                GSMWindow.mousePosition,
                (GSMWindow.mousePosition - GSMWindow.transitionFrom.bounds.center - offset).normalized,
                GSMWindow.transitionFrom.bounds.center + offset,
                GSMWindow.mousePosition, Color.white);
            GUI.changed = true;
        }

        #region Event Handling
        internal virtual void ProcessEvents()
        {
            CheckRightClick();
            CheckDoubleClick();
            CheckLeftClick();
            CheckMouseEnter();
            CheckMouseLeave();
            CheckDrag();
        }

        #region Checker

        private void CheckDrag()
        {
            Event evt = Event.current;
            if (evt.button == 0 && evt.type == EventType.MouseDrag)
            {
                OnDrag(evt);
            }
        }


        private void CheckLeftClick()
        {
            Event evt = Event.current;
            if (evt.button == 0 &&
                MachineBounds.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.MouseDown)
                    OnLeftDown(evt);
                if (evt.type == EventType.MouseUp)
                    OnLeftUp(evt);
            }
        }

        private void CheckDoubleClick()
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseDown &&
                evt.button == 0 &&
                evt.clickCount == 2 &&
                MachineBounds.Contains(evt.mousePosition))
            {
                OnDoubleClick(evt);
            }
        }

        private void CheckRightClick()
        {
            Event evt = Event.current;
            if (evt.button == 1 &&
                MachineBounds.Contains(evt.mousePosition))
            {
                if (evt.type == EventType.MouseDown)
                    OnRightDown(evt);                
                else if (evt.type == EventType.MouseUp)
                    OnRightUp(evt);
            }
        }

        private void CheckMouseEnter()
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseMove &&
                MachineBounds.Contains(evt.mousePosition) &&
                !isMouseOver)
            {
                isMouseOver = true;
                OnMouseEnter(evt);
            }
        }

        private void CheckMouseLeave()
        {
            Event evt = Event.current;
            if (evt.type == EventType.MouseMove &&
                !MachineBounds.Contains(evt.mousePosition) &&
                isMouseOver)
            {
                isMouseOver = false;
                OnMouseLeave(evt);
            }
        }

        #endregion

        #region Handler

        protected virtual bool OnRightDown(Event evt) {
            if (GSMWindow.transitionMode)
            {
                GSMWindow.transitionMode = false;
                return true;
            }


            // If a state was clicked
            foreach (var state in states)
            {
                if (state.bounds.Move(offset).Contains(evt.mousePosition))
                {
                    var isStartState = state.id == startStateID;

                    GenericMenu options = new GenericMenu();
                    options.AddItem(new GUIContent("Make Edge"), false, () => OnOptionMakeEdge(state));
                    options.AddItem(new GUIContent("Edit"), false, () => OnOptionEdit(state));
                    options.AddItem(new GUIContent("Duplicate"), false, () => OnOptionDuplicate(state));
                    options.AddItem(new GUIContent("Snap to grid"), false, () => OnOptionSnapToGrid(state));
                    options.AddSeparator("");
                    options.AddItem(new GUIContent("Set Start State"), isStartState, () => SetStartState(state));

                    if (saveActiveState || isRunning)
                        options.AddItem(new GUIContent("Set Active State"), false, () => OnOptionSetActiveState(state));
                    else
                        options.AddDisabledItem(new GUIContent("Set Active State"));

                    options.AddItem(new GUIContent("Set Terminating"), state.isTerminating, () => state.isTerminating = !state.isTerminating);

                    options.AddItem(new GUIContent("Hide Editor Warnings"), state.hideWarningsInEditor, () => state.hideWarningsInEditor = !state.hideWarningsInEditor);
                    options.AddItem(new GUIContent("Hide Console Warnings"), state.hideWarningsInConsole, () => state.hideWarningsInConsole = !state.hideWarningsInConsole);
                    options.AddSeparator("");
                    options.AddItem(new GUIContent("Delete"), false, () => DeleteState(state));
                    options.ShowAsContext();
                    return true;
                }
            }

            //Otherwise
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Add State"), false, () => OnOptionAddState(evt.mousePosition));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Focus Machine"), false, () => FocusMachine());
            if (StartState != null)
                menu.AddItem(new GUIContent("Focus Start State"), false, () => {
                    Focus(StartState.bounds.center);
                    window?.SetInspectedObject(StartState);
                });
            else menu.AddDisabledItem(new GUIContent("Focus Start State"));


            if ((saveActiveState || isRunning) && activeStateID != -1)
            {
                menu.AddItem(new GUIContent("Focus Active State"), false, () => Focus(ActiveState.bounds.center));
            }
            else menu.AddDisabledItem(new GUIContent("Focus Active State"));


            if (states.Count > 0)
                foreach (var state in states)
                {
                    menu.AddItem(new GUIContent("Focus State/" + state.name), false, () =>
                    {
                        Focus(state.bounds.center);
                        window?.SetInspectedObject(state);
                    });
                }
            else menu.AddDisabledItem(new GUIContent("Focus State/No State Found"));



            menu.ShowAsContext();
            return true;
        }



        public void FocusMachine()
        {
            Focus(MachineMidpoint);
        }

        public void Focus(Vector2 point)
        {
            offset = Vector2.right * window.sideWindowWidth + MachineBounds.size * 0.5f - point;
            GUI.changed = true;
        }


        protected virtual bool OnLeftDown(Event evt) {

            //Setting down transition
            foreach (var state in states)
            {
                if(state.bounds.Move(offset).Contains(evt.mousePosition))
                {
                    if (GSMWindow.transitionMode) //IF we are making a new transition, place it!
                    {
                        GSMWindow.transitionMode = false;
                        GSMState origin = GSMWindow.transitionFrom;
                        InsertEdgeBetween(origin, state);
                        return true;
                    }
                }
            }
            return false;
        }
        protected virtual bool OnDoubleClick(Event evt) { return false; }
        protected virtual bool OnMouseEnter(Event evt) { return false; }
        protected virtual bool OnMouseLeave(Event evt) { return false; }
        protected virtual bool OnLeftUp(Event evt)
        {
            foreach (var state in states)
            {
                if(state.bounds.Move(offset).Contains(evt.mousePosition) && !window.LeftSideWindowBounds.Contains(evt.mousePosition))
                {
                    window.SetInspectedObject(state);
                    return true;
                }
            }

            return false;
        }
        protected virtual bool OnRightUp(Event evt) { return false; }
        protected virtual bool OnDrag(Event evt) {

            return false;
        }


        #endregion


        #region Scrolling
        private void PerformScrolling()
        {
            Event evt = Event.current;
            if (evt.button == 2) //If middle button is clicked
            {
                if (evt.type == EventType.MouseDown)
                {
                    isScrolling = true;
                }
                else if (evt.type == EventType.MouseUp)
                {
                    isScrolling = false;
                }
                else if (evt.type == EventType.MouseDrag && isScrolling)
                {
                    offset += evt.delta;
                    GUI.changed = true;
                }
            }


            if(evt.type == EventType.ScrollWheel)
            {
                //todo zoom
            }
        }

        #endregion

        #endregion

        #region Options Menu
        private void OnOptionEdit(GSMState state)
        {
            window.SetInspectedObject(state);
        }

        private void OnOptionMakeEdge(GSMState state)
        {
            GSMWindow.transitionFrom = state;
            GSMWindow.transitionMode = true;
        }

        private void OnOptionAddState(Vector2 mousePosition)
        {
            GSMState state = CreateUniqueState(false);
            state.bounds = new Rect(mousePosition, Vector2.one * 50).Move(-offset);

            InsertState(state);
            window.SetInspectedObject(state);
        }

        private void OnOptionSnapToGrid(GSMState state)
        {
            Vector2 newPos = new Vector2(Mathf.Round(state.bounds.x / window.bigTileSize), Mathf.Round(state.bounds.y / window.bigTileSize)) * window.bigTileSize;
            state.bounds = new Rect(newPos, state.bounds.size);
        }

        private void OnOptionDuplicate(GSMState state)
        {
            GSMState newState = new GSMState
            {
                name = state.name + " Copy",
                bounds = state.bounds.Move(Vector2.right * (state.bounds.size.x + 8)),
            };

            if (state.onStateEntered != null)
                newState.onStateEntered = state.onStateEntered.Clone();

            if (state.onStateSetActive != null)
                newState.onStateSetActive = state.onStateSetActive.Clone();

            if (state.onStateStay != null)
                newState.onStateStay = state.onStateStay.Clone();

            if (state.onStateLeft != null)
                newState.onStateLeft = state.onStateLeft.Clone();

            InsertState(newState);
        }

        public void OnOptionSetActiveState(GSMState state)
        {
            if (state != null && (saveActiveState || isRunning))
            {
                ActiveState = state;
                ActiveState.onStateSetActive.Invoke(errorOnFailedInvoke);
            }
        }

        #endregion
    }
}
