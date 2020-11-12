using UnityEngine;

[System.Serializable]
public class DrawTextureOnScreen {
    public Texture2D backgroundTexture;
    [Range (0, 1)]
    public float backgroundOpacity = 1;

    // constructor
    public DrawTextureOnScreen () {}

    public void OnGUI () {
        if (backgroundTexture == null)
            return;

        // save original color to reset afterwards
        Color origColor = GUI.color;

        // multiply opacities
        GUI.color = new Color (origColor.r, origColor.g, origColor.b, origColor.a * backgroundOpacity);

        // draw texture on full screen
        GUI.DrawTexture(new Rect (0, 0, Screen.width, Screen.height), backgroundTexture);

        // reset color
        GUI.color = origColor;
    }
}
