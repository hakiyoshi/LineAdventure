using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExitGame : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.escapeKey.isPressed)
        {
            #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
