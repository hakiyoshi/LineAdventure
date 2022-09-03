using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Stage
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class LineConnectiion : MonoBehaviour
    {

        [field: SerializeField]public List<NearbyPoints> LinePoint { get; private set; }

        private void OnEnable()
        {
            #if UNITY_EDITOR
            EditorApplication.update += UpdateLinePoint;
            #endif
        }

        private void OnDisable()
        {
            #if UNITY_EDITOR
            EditorApplication.update -= UpdateLinePoint;
            #endif
        }

        private void UpdateLinePoint()
        {
            if (EditorApplication.isPlaying)
                return;
            
            var line = GetComponent<LineRenderer>();
            
            //保持しているポイントと数が違う場合
            if (LinePoint.Count != line.positionCount)
            {
                line.positionCount = LinePoint.Count;
            }
            
            //線設定
            SetLine(line);
            
            //壁ポイントを追加
            for (int i = 0; i < LinePoint.Count; i++)
            {
                var point = LinePoint[i];
                if(i > 0 && !point.WallPoints.Contains(LinePoint[i - 1]) && point != LinePoint[i - 1])
                    point.WallPoints.Add(LinePoint[i - 1]);
                
                if(i < LinePoint.Count - 1 && !point.WallPoints.Contains(LinePoint[i + 1]) && point != LinePoint[i + 1])
                    point.WallPoints.Add(LinePoint[i + 1]);
            }
            
            SceneView.RepaintAll();
        }

        private void OnValidate()
        {
            UpdateLinePoint();
        }

        private void SetLine(LineRenderer line)
        {
            for (int i = 0; i < LinePoint.Count; i++)
            {
                line.SetPosition(i, LinePoint[i].transform.position);
            }
        }
        
        [Button]
        private void SetLinePoint()
        {
            LinePoint.Clear();
            foreach (Transform child in transform)
            {
                LinePoint.Add(child.GetComponent<NearbyPoints>());
            }
            
            SetLine(GetComponent<LineRenderer>());
            
            
            
            SceneView.RepaintAll();
        }
    }
}
