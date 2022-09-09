using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Player;
using UnityEngine;
using UniRx;

public class InputAssist : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer[] sprite;

    [SerializeField] private PlayerEvent playerEvent;

    private bool isAnime = false;
    private bool isAssist = true;

    private float moveTimeCount = 0.0f;

    private void Start()
    {
        playerEvent.moveEvent.Subscribe(x =>
        {
            //アシスト中かつ移動した場合
            if (isAssist && !isAnime)
            {
                isAssist = false;
                ChangeAlfa(this.GetCancellationTokenOnDestroy(), 0.0f, 0.5f).Forget();
            }

            moveTimeCount = 0.0f;
        }).AddTo(this);
    }

    private void Update()
    {
        moveTimeCount += Time.deltaTime;
        if (!isAnime && !isAnime && moveTimeCount >= 5.0f)
        {
            isAssist = true;
            ChangeAlfa(this.GetCancellationTokenOnDestroy(), 1.0f, 0.5f).Forget();
        }
    }

    async UniTask ChangeAlfa(CancellationToken token, float alfa, float time)
    {
        token.ThrowIfCancellationRequested();
        isAnime = true;
        
        var sequence = DOTween.Sequence(); 
        foreach (var color in sprite)
        {
            var color1 = color.color;
            sequence.Join(color.DOColor(new Color(color1.r, color1.g, color1.b, alfa), time));
        }
        
        await sequence.Play().AsyncWaitForCompletion();

        isAnime = false;
    }
}
