using UnityEngine;
using System.Collections.Generic;

namespace GUINodeEditor {

    [System.Serializable]
    public class NodeEditorMinimap {
        public List<Node> nodes;

        [Range (0.2f, 1)] public float opacity = 0.6f;
        [HideInInspector] public Vector2 panningOffset;
        [HideInInspector] public Vector2 dockRectSize;
        public Rect rect;
        public bool drawScreenRect = true;
        [Range (0.2f, 1)] public float screenRectOpacity = 0.4f;

        [Range (0.1f, 1)] public float scale = 0.35f;

        public void DrawMinimap () {
            Vector2 innerAreaSize = 0.5f * (rect.size);

            Vector2 positionOffset = scale * (panningOffset - innerAreaSize) + innerAreaSize;

            // get view on screen dimensions
            Vector2 viewSize = scale * rect.size;
            Vector2 viewPos = 0.5f * new Vector2 (rect.width - viewSize.x, rect.height - viewSize.y);
            Rect viewRect = new Rect (viewPos, viewSize);

            // save original color to reset afterwards
            Color origColor = GUI.color;

            GUI.color = Drawing.MultOpacity (origColor, screenRectOpacity);
            if (drawScreenRect)
                GUI.Box (viewRect, "");

            GUI.color = origColor;

            Vector2 dockWidthOffset = new Vector2 (dockRectSize.x, 0);

            foreach (Node n in nodes) {
                Rect nodeRect = new Rect (
                    scale * (n.nodeWindow.rect.position + dockWidthOffset) + positionOffset,
                    scale * (n.nodeWindow.rect.size - dockWidthOffset));

                // set color and opacity
                Color windowColor = n.nodeWindow.backgroundColor;
                if (windowColor == default(Color))
                    windowColor = origColor;
                GUI.color = Drawing.MultOpacity (windowColor, opacity);

                // draw node rect
                GUI.Box (nodeRect, "", GUI.skin.button);

                // reset color
                GUI.color = origColor;

                // draw title
                GUIStyle s = new GUIStyle (GUI.skin.label);
                s.wordWrap = true;
                s.alignment = TextAnchor.UpperCenter;
                s.fontSize =  (int)(0.9f * (GUI.skin.label.fontSize == 0 ? 12 : GUI.skin.label.fontSize));
                s.padding.top = 0;
                s.padding.bottom = -20;
                GUI.Label (nodeRect, n.nodeWindow.title, s);

                // draw node connections
                // TODO move out, action?
                n.nodeWindow.nodeEditor.DrawNodeConnections (n, positionOffset, scale, isMinimap: true);
            }

            // reset color
            GUI.color = origColor;

        }
    }
}