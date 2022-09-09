using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    private bool isFade = false;
    [SerializeField] private GameState gameState;

    // Update is called once per frame
    void Update()
    {
        if (!isFade && Keyboard.current.enterKey.isPressed)
        {
            isFade = false;
            gameState.SetState(GameState.State.Game);
            DOVirtual.Float(1.0f, 0.0f, 0.1f, x => GetComponent<AudioSource>().volume = x);
            Initiate.Fade("Stage01", Color.black, 2.0f);
        }
    }
}
