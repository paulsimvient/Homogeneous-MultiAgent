using UnityEngine;

public class Trigger {
    public bool isTriggered;

    public Source source;
    public KeyCode key;
    public int button;
    public Style style;

    public enum Source {
        Key,
        Mouse,
    }

    public enum Style {
        WhileHolding,
        OnUp,
        OnDown,
    }

    public void UpdateTrigger () {
        if (source == Source.Key) 
        switch (style) {
        case Style.WhileHolding: isTriggered = Input.GetKey (key); return;
        case Style.OnUp: isTriggered = Input.GetKeyUp(key); return;
        case Style.OnDown: isTriggered = Input.GetKeyDown (key); return;
        }

        if (source == Source.Mouse)
        switch (style) {
        case Style.WhileHolding: isTriggered = Input.GetMouseButton (button); return;
        case Style.OnUp: isTriggered = Input.GetMouseButtonUp (button); return;
        case Style.OnDown: isTriggered = Input.GetMouseButtonDown (button); return;
        }
    }
}
