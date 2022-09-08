using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class LineControl : MonoBehaviour
    {
        [SerializeField] private LineCurve lineCurve;

        [SerializeField] private float speed = 0.2f;

        [SerializeField] private float lineLength = 1.0f;
        [SerializeField] private float returnForce = 0.1f;//基準の位置に戻る力

        [SerializeField] private PlayerEvent playerEvent;

        [SerializeField] private GameState gameState;

        private void OnValidate()
        {
            transform.position = lineCurve.GetBezierCurvePosition(0.5f);
        }

        private void Update()
        {
            //キー入力
            Vector3 inputMove = Vector3.zero;
            if (gameState.state == GameState.State.Game)
            {
                if (Keyboard.current.aKey.isPressed)
                {
                    inputMove.x -= 1.0f;
                }

                if (Keyboard.current.dKey.isPressed)
                {
                    inputMove.x += 1.0f;
                }

                if (Keyboard.current.wKey.isPressed)
                {
                    inputMove.y += 1.0f;
                }

                if (Keyboard.current.sKey.isPressed)
                {
                    inputMove.y -= 1.0f;
                }
            }
            
            //移動ベクトル計算
            Vector3 move = Vector3.zero;
            if(inputMove.sqrMagnitude == 0.0f)
            {
                //キー入力していない場合
                var vec = -(lineCurve.ControlPosition - lineCurve.StandardControlPosition);
                move = vec.normalized * (returnForce * Time.deltaTime);

                if (vec.magnitude <= returnForce * Time.deltaTime)
                {
                    move = Vector3.zero;
                    lineCurve.ControlPosition = lineCurve.StandardControlPosition;
                    
                    
                }
            }
            else
            {
                //入力している場合
                inputMove.Normalize();
                move = inputMove * (speed * Time.deltaTime);

                //基準から一定以上離れていたら
                var differenceStandard = lineCurve.ControlPosition + move - lineCurve.StandardControlPosition;
                var difference = differenceStandard.magnitude + - lineLength;
                if (difference > 0.0f)
                {
                    move -= (differenceStandard.normalized * difference);
                    playerEvent.moveEvent.OnNext(inputMove);
                }
            }

            //移動
            lineCurve.ControlPosition += move;
            
            //自分の座標を線のてっぺんにする
            transform.position = lineCurve.GetBezierCurvePosition(0.5f);
        }
    }
}
