using UnityEngine;

public static class Drawing {
    /// Multiply opacity.
    public static Color MultOpacity (Color color, float opacity) {
        return new Color (color.r, color.g, color.b, color.a * opacity);
    }

    public static Rect GetRightRectFromPoints (Vector2 a, Vector2 b) {
        Vector2 position = new Vector2 (Mathf.Min (a.x, b.x), Mathf.Min (a.y, b.y));
        Vector2 scale = new Vector2 (Mathf.Abs (a.x - b.x), Mathf.Abs (a.y - b.y));
        return new Rect (position, scale);
    }
}
