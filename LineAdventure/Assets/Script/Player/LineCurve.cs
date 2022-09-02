using System;
using System.ComponentModel;
using System.Numerics;
using DG.Tweening;
using NaughtyAttributes;
using UniRx;
using UnityEditor;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Player
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class LineCurve : MonoBehaviour
    {
        
        [field: SerializeField] public int NumPoint { get; private set; } = 10;
        [field: SerializeField] public float ControlHeight { get; private set; } = 3.0f;
        [field: SerializeField] public float MoveSpeed { get; private set; } = 0.05f;

        //現在踏んでいるオブジェクト
        [SerializeField] private Transform leftPoint;
        [SerializeField] private Transform rightPoint;
        
        public Transform LeftPoint
        {
            set
            {
                if (leftMove != null && !leftMove.active)
                    leftPoint = value;
            }
            
            get { return leftPoint; }
        }
        
        public Transform RightPoint
        {
            set
            {
                if (rightMove != null && !rightMove.active)
                    rightPoint = value;
            }
            
            get { return rightPoint; }
        }
        
        //ベジェの座標情報
        [ShowNativeProperty] public Vector3 LeftPosition { get; private set; }
        [ShowNativeProperty] public Vector3 RightPosition { get; private set; }
        [NaughtyAttributes.ReadOnly] public Vector3 ControlPoint;
        
        [ShowNativeProperty] public Vector3 StandardControlPoint { get; private set; }
        
        private LineRenderer lineRenderer;

        private Tweener leftMove;
        private Tweener rightMove;

        // Start is called before the first frame update
        private void Start()
        {
            TryGetComponent(out lineRenderer);
            lineRenderer.positionCount = NumPoint;

            this.ObserveEveryValueChanged(_ => leftPoint).Subscribe(_ =>
            {
                if (leftMove == null || !leftMove.active)
                    leftMove = DOVirtual.Vector3(
                        LeftPosition, 
                        leftPoint.position, MoveSpeed, x => LeftPosition = x);
            });
            
            this.ObserveEveryValueChanged(_ => rightPoint).Subscribe(_ =>
            {
                if(rightMove == null || !rightMove.active) 
                    rightMove = DOVirtual.Vector3(
                        RightPosition, 
                        rightPoint.position, MoveSpeed, x => RightPosition = x);
            });
        }

        private void LateUpdate()
        {
            //ポイントが動いた際に対応
            if (leftMove != null && !leftMove.active)
                LeftPosition = leftPoint.position;
            
            if (rightMove != null && !rightMove.active)
                RightPosition = rightPoint.position;
            
            //基準のコントロールポイント更新
            UpdateStandardControlPoint();
            
            //線書き換え
            UpdatePoint(lineRenderer, NumPoint);
        }

        private void UpdatePoint(LineRenderer line, int numPoint)
        {
            for (int i = 0; i < numPoint; i++)
            {
                line.SetPosition(i,
                    CalcBezierCurvePoint(
                        LeftPosition, RightPosition, ControlPoint, ((float) i) / (numPoint - 1)));
            }
        }

        private static Vector3 CalcBezierCurvePoint(Vector3 start, Vector3 end, Vector3 control, float t)
        {
            var sc = Vector3.Lerp(start, control, t);
            var ce = Vector3.Lerp(control, end, t);
            var bezier = Vector3.Lerp(sc, ce, t);
            return bezier;
        }

        //ベジェカーブの座標を取得する 0.0~1.0
        public Vector3 GetBezierCurvePosition(float t)
        {
            return CalcBezierCurvePoint(LeftPosition, RightPosition, ControlPoint, t);
        }

        public void UpdatePoint()
        {
            var line = GetComponent<LineRenderer>();
            UpdatePoint(line, NumPoint);
        }

        //コントロール
        public Vector3 CulcControlPoint(Vector3 left, Vector3 right, float height)
        {
            //中央へのベクトルを計算
            var center = right - left;
            center *= 0.5f;
            
            //法線を確認する
            Vector3 up = Vector3.zero;
            
            //xが同じ場合
            if (Math.Abs(rightPoint.rotation.x - leftPoint.rotation.x) < 0.0001f)
            {
                up.x = rightPoint.rotation.x;
            }
            
            //yが同じ場合
            if (Math.Abs(rightPoint.rotation.y - leftPoint.rotation.y) < 0.0001f)
            {
                up.y = rightPoint.rotation.y;
            }
            
            //設定チェック
            if (up.x == 0.0f && up.y == 0.0f)
            {
                up = Vector3.Cross(center.normalized, Vector3.forward);
            }
            else if (up.x != 0.0f && up.y != 0.0f)
            {
                up.y = 0.0f;
            }

            return (left + center) + (up.normalized * height);
        }

        public Vector3 CulcControlPoint()
        {
            return CulcControlPoint(LeftPosition, RightPosition, ControlHeight);
        }

        public void UpdateStandardControlPoint()
        {
            StandardControlPoint = CulcControlPoint(LeftPosition, RightPosition, ControlHeight);
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall += UpdateStartPoint;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= UpdateStartPoint;
#endif
        }

#if UNITY_EDITOR
        [BoxGroup("EditorOnlyProprty")]
        [SerializeField] private GameObject controllerObject;
        
        
        private void OnValidate()
        {
            //ポイント更新
            if (NumPoint < 2)
                NumPoint = 2;
            
            //頂点数更新
            var line = GetComponent<LineRenderer>();
            line.positionCount = NumPoint;
            
            if(leftPoint != null && rightPoint != null)
            {
                var movePoint = controllerObject.GetComponent<MovePoint>();
                SetStartAndEndPoint(movePoint);
            }
        }

        private void SetStartAndEndPoint(MovePoint movePoint)
        {
            LeftPosition = leftPoint.position;
            RightPosition = rightPoint.position;
            
            //コントロールポイントを変更
            UpdateStandardControlPoint();
            ControlPoint = StandardControlPoint;

            //コントロールオブジェクトの座標を変更
            controllerObject.transform.position = GetBezierCurvePosition(0.5f);

            UpdatePoint();
        }

        private void UpdateStartPoint()
        {
            if (EditorApplication.isPlaying)
                return;

            var movePoint = controllerObject.GetComponent<MovePoint>();

            if ((leftPoint != null && leftPoint.hasChanged) || 
                (rightPoint != null && rightPoint.hasChanged))
            {
                SetStartAndEndPoint(movePoint);
            }
        }
#endif
    }
}