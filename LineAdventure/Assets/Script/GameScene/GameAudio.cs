using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DOVirtual.Float(0.0f, 1.0f, 0.1f, x => GetComponent<AudioSource>().volume = x);
    }
}
