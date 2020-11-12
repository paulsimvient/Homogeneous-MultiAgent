using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// First drawn as a button, then you have to draw it externally
/// to appear outside the buttons area rect.
/// </summary>
/*!
```
// draw popup
if (p.identifier != null) {
    // get local rect
    Rect popupRect = p.GetListRect();

    // add position of your parent element
    popupRect.position += n.nodeWindow.rect.position;

    // draw a window because overlapping elements will consume click events
    GUI.Window (-i, popupRect, (id) => p.DrawList (), "", GUI.skin.box);

    // you might have to check for event on more places to close properly
    if (Event.current.type == EventType.mouseDown)
        p.identifier = null;
}
```
*/

public class Popup {
    Rect box;
    int currentIndex;
    GUIContent[] items;

    /// If this is not null, popup is opened and should be drawn externally.
    public object identifier;

    bool didUserSelect;

    /// <summary>
    /// Gets the popup list rect.
    /// </summary>
    /// <returns>The popup list rect.</returns>
    public Rect GetListRect() {
        return new Rect(
            box.x,
            box.y + box.height,
            box.width,
            box.height * items.Length);
    }
    /// <summary>>
    /// Gets the user-selected index when changed, otherwise current index.
    /// This is not done in the same frame as the change happens from the externally drawn popup.
    /// </summary>
    int GetSelectedIndex (Rect _box, int _currentIndex, GUIContent[] _items, object _identifier) {
        // if clicked for dropdown
        if(GUI.Button(new Rect(_box), _items[_currentIndex])) {
            // close menu on second click
            if (identifier == _identifier)
                didUserSelect = true;
            else {
                // sync for external popup
                box = _box;
                currentIndex = _currentIndex;
                items = _items;
                identifier = _identifier;
            }
        }

        // return externally clicked item index
        if (didUserSelect && identifier == _identifier) {
            identifier = null;
            didUserSelect = false;
            return currentIndex;
        }
        return _currentIndex;
    }

    /// Draws GUI.SelectionGrid of 1 column.
    public void DrawList() {
        Rect listRect = GetListRect ();
        listRect.position = Vector2.zero;

        // set top and bottom margin to 0
        GUIStyle s = new GUIStyle (GUI.skin.button);
        s.margin.top = 0;
        s.margin.bottom = 0;
        currentIndex = GUI.SelectionGrid (listRect, currentIndex, items, 1, s);

        didUserSelect = GUI.changed;
    }
    /// Enum popup
    public Enum EnumPopup (Enum currentEnum) {
        Type type = currentEnum.GetType();
        if (! type.IsEnum)
            throw new Exception("parameter _enum must be of type System.Enum");

        Enum[] array = Enum.GetValues(type).Cast<Enum>().ToArray<Enum>();
        string[] names = Enum.GetNames(type);
        int num = Array.IndexOf(array, currentEnum);
        // num = PopupStuff(... num ...);

        if (num < 0 || num >= names.Length)
            return currentEnum;

        GUIContent[] contents = names.Select ((s) => new GUIContent(s)).ToArray ();
        GUILayout.Label("");
        num = GetSelectedIndex (GUILayoutUtility.GetLastRect(), num, contents, type);
        return array[num];
    }

}
