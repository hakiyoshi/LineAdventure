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
        Transform nearPoint = null;
        float nearDot = 0.0f;
        
        //右チェック
        NearbyPoints nearbyPoints = lineCurve.RightPoint.GetComponent<NearbyPoints>();
        
        foreach (var point in nearbyPoints.nearbyPoints)
        {
            //反対側と同じ場合何もしない
            if(point.transform.position == lineCurve.LeftPoint.position || 
               point.transform == nearPoint)
                continue;

            //移動方向とポイントへの方向ベクトルで一番近い物を選ぶ
            var dot = Vector3.Dot(
                moveVec.normalized, (point.transform.position - lineCurve.RightPoint.position).normalized);
            if (dot > nearDot)
            {
                nearPoint = point.transform;
                nearDot = dot;
            }
            else if (dot == nearDot)
            {
                if(nearPoint != null && 
                   point.transform.position == nearPoint.position && 
                   point.transform.rotation == lineCurve.RightPoint.rotation)
                {
                    nearPoint = point.transform;
                    nearDot = dot;
                }
            }
        }

        return nearPoint;
    }

    private Transform LeftNearPointCheck(Vector3 moveVec)
    {
        Transform nearPoint = null;
        float nearDot = 0.0f;

        //左足チェック
        NearbyPoints nearbyPoints = lineCurve.LeftPoint.GetComponent<NearbyPoints>();
        
        foreach (var point in nearbyPoints.nearbyPoints)
        {
            //左右入れ替え防止
            if(point.transform.position == lineCurve.RightPoint.position || 
               point.transform == nearPoint)
                continue;

            //移動方向とポイントへの方向ベクトルで一番近い物を選ぶ
            var dot = Vector3.Dot(
                moveVec.normalized, (point.transform.position - lineCurve.LeftPoint.position).normalized);
            if (dot > nearDot)
            {
                nearPoint = point.transform;
                nearDot = dot;
            }
            else if (dot == nearDot)
            {
                if(nearPoint != null && 
                   point.transform.position == nearPoint.position && 
                   point.transform.rotation == lineCurve.LeftPoint.rotation)
                {
                    nearPoint = point.transform;
                    nearDot = dot;
                }
            }
        }

        return nearPoint;
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
