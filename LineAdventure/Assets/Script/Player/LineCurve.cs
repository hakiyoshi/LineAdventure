using System;
using DG.Tweening;
using NaughtyAttributes;
using UniRx;
using UnityEditor;
using UnityEngine;
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

        //現在踏んでいるポイント情報
        [SerializeField] private NormalPoint leftPoint;
        [SerializeField] private NormalPoint rightPoint;
        
        //上書きされる前のポイント
        private NormalPoint beforePoint;

        public Transform LeftPoint
        {
            set
            {
                if (leftMove != null && !leftMove.active)
                {
                    beforePoint = leftPoint;
                    leftPoint = value.GetComponent<NormalPoint>();
                    
                }
            }
            
            get { return leftPoint.transform; }
        }
        
        public Transform RightPoint
        {
            set
            {
                if (rightMove != null && !rightMove.active)
                {
                    beforePoint = rightPoint;
                    rightPoint = value.GetComponent<NormalPoint>();
                }
            }
            
            get { return rightPoint.transform; }
        }
        
        //現在いる座標
        [ShowNativeProperty] public Vector3 LeftPosition { get; private set; }
        [ShowNativeProperty] public Vector3 RightPosition { get; private set; }
        [NaughtyAttributes.ReadOnly] public Vector3 ControlPosition;
        
        //基準のコントロール座標
        [ShowNativeProperty] public Vector3 StandardControlPosition { get; private set; }

        [ShowNonSerializedField] private Vector3 playerNormal;

        
        
        private LineRenderer lineRenderer;

        private Tweener leftMove;
        private Tweener rightMove;

        // Start is called before the first frame update
        private void Awake()
        {
            TryGetComponent(out lineRenderer);
            lineRenderer.positionCount = NumPoint;

            this.ObserveEveryValueChanged(_ => leftPoint).Subscribe(_ =>
            {
                if (leftMove == null || !leftMove.active)
                    leftMove = DOVirtual.Vector3(
                        LeftPosition, 
                        LeftPoint.position, MoveSpeed, x => LeftPosition = x);
            }).AddTo(this);
            
            this.ObserveEveryValueChanged(_ => rightPoint).Subscribe(_ =>
            {
                if(rightMove == null || !rightMove.active) 
                    rightMove = DOVirtual.Vector3(
                        RightPosition, 
                        RightPoint.position, MoveSpeed, x => RightPosition = x);
            }).AddTo(this);
        }

        private void LateUpdate()
        {
            //ポイントが動いた際に対応
            if (leftMove != null && !leftMove.active)
                LeftPosition = LeftPoint.position;
            
            if (rightMove != null && !rightMove.active)
                RightPosition = RightPoint.position;
            
            //基準のコントロールポイント更新
            UpdateStandardControlPoint();
            
            {
                //座標の更新
                var center = (LeftPosition - RightPosition) * 0.5f;
                transform.position = RightPosition + center;

                var vec = (LeftPoint.position - RightPoint.position);
                var crossz = Vector3.Cross(vec.normalized, playerNormal).z; 
                if (crossz > 0.0f)
                {
                    (leftPoint, rightPoint) = (rightPoint, leftPoint);
                }
            }

            //線書き換え
            UpdatePoint(lineRenderer, NumPoint);
        }

        private void UpdatePoint(LineRenderer line, int numPoint)
        {
            for (int i = 0; i < numPoint; i++)
            {
                line.SetPosition(i,
                    CalcBezierCurvePoint(
                        LeftPosition, RightPosition, ControlPosition, ((float) i) / (numPoint - 1)));
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
            return CalcBezierCurvePoint(LeftPosition, RightPosition, ControlPosition, t);
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
            Vector3 up = CheckNormal(leftPoint, rightPoint);
            
            //設定チェック
            if (up.x == 0.0f && up.y == 0.0f)
            {
                //上方向求める
                up = -Vector3.Cross(center.normalized, Vector3.forward);
            }
            else if(up.x != 0.0f && up.y != 0.0)
            {
                up.y = 0.0f;
            }

            playerNormal = up.normalized;

            

            return (left + center) + (playerNormal * height);
        }

        private Vector3 CheckNormal(NormalPoint a, NormalPoint b)
        {
            var up = Vector3.zero;
            
            //xが同じ場合
            if (Math.Abs(a.Normal.x - b.Normal.x) < 0.0001f)
            {
                up.x = a.Normal.x;
            }
            
            //yが同じ場合
            if (Math.Abs(a.Normal.y - b.Normal.y) < 0.0001f)
            {
                up.y = b.Normal.y;
            }

            return up;
        }

        public Vector3 CulcControlPoint()
        {
            return CulcControlPoint(LeftPosition, RightPosition, ControlHeight);
        }

        public void UpdateStandardControlPoint()
        {
            StandardControlPosition = CulcControlPoint(LeftPosition, RightPosition, ControlHeight);
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
                SetStartAndEndPoint();
            }
        }

        private void SetStartAndEndPoint()
        {
            LeftPosition = LeftPoint.position;
            RightPosition = RightPoint.position;
            
            //コントロールポイントを変更
            UpdateStandardControlPoint();
            ControlPosition = StandardControlPosition;

            //コントロールオブジェクトの座標を変更
            controllerObject.transform.position = GetBezierCurvePosition(0.5f);

            UpdatePoint();
        }

        private void UpdateStartPoint()
        {
            if (EditorApplication.isPlaying)
                return;
            
            if ((leftPoint != null && LeftPoint.hasChanged) || 
                (rightPoint != null && RightPoint.hasChanged))
            {
                SetStartAndEndPoint();
            }
        }
#endif
    }
}