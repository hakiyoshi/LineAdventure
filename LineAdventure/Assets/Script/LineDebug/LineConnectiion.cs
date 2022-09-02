using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Stage
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(LineRenderer))]
    public class LineConnectiion : MonoBehaviour
    {

        [SerializeField]private List<Transform> LinePoint;

        private void OnEnable()
        {
            EditorApplication.update += UpdateLinePoint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateLinePoint;
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
                LinePoint.Add(child);
            }
            
            SetLine(GetComponent<LineRenderer>());
            
            SceneView.RepaintAll();
        }
    }
}
#endif
