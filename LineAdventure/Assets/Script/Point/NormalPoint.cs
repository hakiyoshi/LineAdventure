using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode] 
public class NormalPoint : MonoBehaviour
{
    [field: SerializeField] public Vector3 Normal { get; private set; }
}
