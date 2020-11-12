using System.Collections.Generic;
using System.Linq;

using PopAnywhereStack = GUINodeEditor.PopAnywhereStack;

public class StackState {
    public Dictionary<string, object> initialState;
    public Dictionary<string, object> state;

    public Dictionary <string, PopAnywhereStack> stacks;

    public StackState (Dictionary <string, object> initialState) {
        // set state keys
        this.state = new Dictionary<string, object> (initialState);
        this.initialState = new Dictionary<string, object> (initialState);

        // fill state stacks
        this.stacks = new Dictionary<string, PopAnywhereStack> ();
        foreach (string key in initialState.Keys)
            this.stacks.Add (key, new PopAnywhereStack());
    }

    public void ChangeSubState (string key, object newSubState) {
        if (state [key].ToString() == newSubState.ToString())
            return;

        // set new value
        state [key] = newSubState;

        // if triggers are not active set it as initial
        if (AreStacksEmpty ())
            SetCurrentStateAsInitial ();
    }

    public void UpdateState () {
        foreach (string key in state.Keys.ToList())
            state [key] = stacks [key].Head (initialState [key]);
    }

    public bool AreStacksEmpty () {
        foreach (string key in state.Keys)
            if (stacks [key].stack.Count > 0)
                return false;
        return true;
    }

    public void SetCurrentStateAsInitial () {
        initialState = new Dictionary<string, object> (state);
    }

}
