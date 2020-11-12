using UnityEngine;

[System.Serializable]

public class DrawGridOnScreen {
    public Texture2D gridTexture;
    [Range(0, 1)] public float gridOpacity = 1f;
    /// Edge length of the grid square in pixels
    [Range(25, 50)] public float gridUnit = 25;

    /// <summary>
    /// For performance reasons (until I generate the bigger texture from script),
    /// bigger texture with grid tiled should be provided.
    /// This is the number of times the grid has fit in the bigger texture
    /// </summary>
    [Range(1, 50)] public float gridMultiplyFactor = 20;

    /// Offset of the whole editor area
    [HideInInspector]
    public Vector2 panningOffset;

    public DrawGridOnScreen () {}

    public void OnGUI () {
        if (gridTexture == null) return;

        // TODO construct a larger texture to reduce calls
        if (gridUnit < 25) {
            Debug.LogWarning ("gridUnit should be at least 25");
            return;
        }

        Vector2 texGridOffset = new Vector2 (panningOffset.x % gridUnit, panningOffset.y % gridUnit);

        Color origColor = GUI.color;
        GUI.color = new Color (origColor.r, origColor.g, origColor.b, gridOpacity);

        float _gridUnit = gridUnit * gridMultiplyFactor;
        // draw grid
        for (int i = 0; i < Screen.width/_gridUnit + 2; ++i) {
            for (int j = 0; j < Screen.height/_gridUnit + 2; ++j) {
                GUI.DrawTexture(new Rect(
                    (i - 1) * _gridUnit + texGridOffset.x,
                    (j - 1) * _gridUnit + texGridOffset.y,
                    _gridUnit, _gridUnit
                ), gridTexture);
            }
        }
        GUI.color = origColor;
    }
}
