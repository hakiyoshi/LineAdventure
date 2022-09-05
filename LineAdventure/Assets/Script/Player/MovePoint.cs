using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.VisualScripting;
using UnityEngine;
using UniRx;

public class MovePoint : MonoBehaviour
{
    [SerializeField] private PlayerEvent playerEvent;
    [SerializeField] private LineCurve lineCurve;

    private void Start()
    {
        playerEvent.moveEvent.Subscribe(x =>
        {
            var right = RightNearPointCheck(x);
            var left = LeftNearPointCheck(x);
            
            if (x.x < 0.0f)
            {
                //左に移動している場合左優先
                if(left != null)
                    SwapLeftPoint(left);
                else
                    SwapRightPoint(right);
            }
            else
            {
                //右に移動している場合右優先
                if (right != null)
                    SwapRightPoint(right);
                else
                    SwapLeftPoint(left);
            }
        });
    }

    private Transform RightNearPointCheck(Vector3 moveVec)
    {
        return NearPointCheck(moveVec, lineCurve.RightPoint, lineCurve.LeftPoint);
    }

    private Transform LeftNearPointCheck(Vector3 moveVec)
    {
        return NearPointCheck(moveVec, lineCurve.LeftPoint, lineCurve.RightPoint);
    }

    private Transform NearPointCheck(Vector3 moveVec, Transform a, Transform b)
    {
        Transform nearPoint = null;
        float nearDot = 0.0f;

        //左足チェック
        NearbyPoints nearbyPoints = a.GetComponent<NearbyPoints>();
        
        foreach (var point in nearbyPoints.nearbyPoints)
        {
            //左右入れ替え防止
            if(point.transform.position == b.position || 
               point.transform == nearPoint)
                continue;

            //移動方向とポイントへの方向ベクトルで一番近い物を選ぶ
            var dot = Vector3.Dot(
                moveVec.normalized, (point.transform.position - a.position).normalized);
            if (dot > nearDot && CheckMove(nearbyPoints, point))
            {
                //CheckMove(nearbyPoints, point);
                nearPoint = point.transform;
                nearDot = dot;
            }
            else if (Mathf.Approximately(dot, nearDot))
            {
                if(nearPoint != null && 
                   point.transform.position == nearPoint.position && 
                   point.transform.rotation == a.rotation && CheckMove(nearbyPoints, point))
                {
                    //CheckMove(nearbyPoints, point);
                    nearPoint = point.transform;
                    nearDot = dot;
                }
            }
        }

        return nearPoint;
    }
    
    //移動可能かチェックする
    private bool CheckMove(NearbyPoints basePoint, NearbyPoints movePoint)
    {
        var basePosi = basePoint.transform.position;
        var moveVec = movePoint.transform.position - basePosi;
        var playerVec = lineCurve.StandardControlPosition - basePosi;

        foreach (var wallPoint in basePoint.WallPoints)
        {
            //ベクトル
            var wallVec = wallPoint.transform.position - basePosi;

            //内積
            var playerCross = Vector3.Cross(wallVec.normalized, playerVec.normalized);
            var moveCross = Vector3.Cross(wallVec.normalized, moveVec.normalized);

            //お互いの富豪が違う場合壁を挟んでいる
            if(Mathf.Approximately(moveCross.z, 0.0f) || Mathf.Approximately(playerCross.z, 0.0f))
                continue;

            var playerSign = Mathf.Sign(playerCross.z);
            if (!Mathf.Approximately(playerSign, Mathf.Sign(moveCross.z)))
            {
                //最短経路でヒットした場合最長経路でヒットするかチェックする
                foreach (var wall in basePoint.WallPoints)
                {
                    if (wall == wallPoint || wall == movePoint)
                        continue;
                    
                    wallVec = wall.transform.position - basePosi;
                    playerCross = Vector3.Cross(wallVec.normalized, playerVec.normalized);
                    
                    if(Mathf.Approximately(playerCross.z, 0.0f))
                        continue;

                    if (Mathf.Approximately(-playerSign, Mathf.Sign(playerCross.z)))
                        return false;
                }
            }
        }

        return true;
    }

    private void SwapRightPoint(Transform point)
    {
        if(point == null)
            return;
        
        var value = lineCurve.RightPoint;
        lineCurve.RightPoint = point;
        lineCurve.LeftPoint = value;
    }
    
    private void SwapLeftPoint(Transform point)
    {
        if(point == null)
            return;
        
        var value = lineCurve.LeftPoint;
        lineCurve.LeftPoint = point;
        lineCurve.RightPoint = value;
    }
}
