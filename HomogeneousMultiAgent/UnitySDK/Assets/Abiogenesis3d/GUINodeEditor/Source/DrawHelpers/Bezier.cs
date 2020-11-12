using UnityEngine;
using System;

public static class Bezier {
    #region MATERIAL
    static Material lineMaterial;

    static void CreateLineMaterial () {
        if (! lineMaterial) {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 1);
            // lineMaterial.EnableKeyword("_ZTest");
            //lineMaterial.SetInt("_ZTest",1);
        }
    }
    #endregion

    /// Draws the bezier curve.
    public static void DrawBezier (Vector2 start, Vector2 end, Color color, float opacity = 1, float thickness = 2, float precision = 15) {
        float maxCurvatureOfTan = 100f;
        float finalCurvatureOfTan = maxCurvatureOfTan;
        float distance = Vector2.Distance(start, end);
        float maxDistance = 400f;
        if(distance < maxDistance)
            finalCurvatureOfTan *= distance/maxDistance;

        Vector2 startTan, endTan;

        // use this for left-right
        startTan = start + Vector2.right *finalCurvatureOfTan;
        endTan = end + Vector2.left *finalCurvatureOfTan;

        /*
        // use this for up-down
        startTan = start + Vector2.down *finalCurvatureOfTan;
        endTan = end + Vector2.up *finalCurvatureOfTan;
        */

        color = Drawing.MultOpacity (color, opacity);

        Vector2 p = start;
        Vector2 q;

        CreateLineMaterial ();
        lineMaterial.SetPass (0);

        if (color == default(Color))
            color = Color.black;

        GL.Begin (GL.QUADS);
        GL.Color (color);
        for (int i = 1; i <= precision; ++i) {
            float t = i / precision;
            q = p;
            p = b0 (t) * start + b1 (t) * startTan + b2 (t) * endTan + b3 (t) * end;

            Vector2 thicknessVector = 0.5f * thickness * Vector3.Cross (new Vector3 (0, 0, 1), p - q).normalized;
            GL.Vertex (p - thicknessVector);
            GL.Vertex (p + thicknessVector);
            GL.Vertex (q + thicknessVector);
            GL.Vertex (q - thicknessVector);
        }
        GL.End ();
    }

    static Func<float, float> b0 = (a) => (1 - a) * (1 - a) * (1 - a);
    static Func<float, float> b1 = (a) => a * (1 - a) * (1 - a) * 3;
    static Func<float, float> b2 = (a) => a * a * (1 - a) * 3;
    static Func<float, float> b3 = (a) => a * a * a;

}
