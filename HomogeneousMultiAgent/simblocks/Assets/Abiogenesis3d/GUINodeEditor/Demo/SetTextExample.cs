using UnityEngine;
using Enum = System.Enum;

[RequireComponent (typeof (GetColorData))]
[RequireComponent (typeof (GetEnumStateData))]

public class SetTextExample : MonoBehaviour {
    void Update () {
        TextMesh textMesh = GetComponent<TextMesh> ();

        string text = "";
        StackState stackState = GetComponent<GetEnumStateData> ().stackState;
        if (stackState != null) {
            textMesh.color = GetComponent<GetColorData> ().color;
            foreach (string enumKey in stackState.state.Keys) {
                Enum e = (Enum)stackState.state [enumKey];
                text += e.ToString () + "\n";
            }
        }
        textMesh.text = text;
    }
}
