using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Build;
using UnityEngine;
using UniRx;

public class InputAssist : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer[] sprite;

    [SerializeField] private PlayerEvent playerEvent;

    private bool isAssist = true;

    private void Start()
    {
        playerEvent.moveEvent.Subscribe(x =>
        {
            //アシスト中かつ移動した場合
            if (isAssist && player.hasChanged)
            {
                isAssist = false;
                ChangeAlfa(this.GetCancellationTokenOnDestroy(), 0.0f, 0.5f).Forget();
            }
        }).AddTo(this);
    }

    async UniTask ChangeAlfa(CancellationToken token, float alfa, float time)
    {
        token.ThrowIfCancellationRequested();
        
        var sequence = DOTween.Sequence(); 
        foreach (var color in sprite)
        {
            sequence.Join(color.DOColor(new Color(color.color.r, color.color.g, color.color.b, alfa), time));
        }
        
        await sequence.Play().AsyncWaitForCompletion();

        isAssist = false;
    }
}
