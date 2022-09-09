using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

public class NearbyPoints : MonoBehaviour
{
    [field: SerializeField] public List<NearbyPoints> nearbyPoints { get; private set; }
    
    [field: SerializeField] public List<NearbyPoints> WallPoints { get; private set; }
    
#if UNITY_EDITOR
    [SerializeField] private float hitRadius = 2.0f;
    
    [Button]
    public void CheckPoint()
    {
        var hit = Physics2D.CircleCastAll(transform.position, hitRadius, Vector2.zero);
        nearbyPoints.Clear();
        foreach (var hit2D in hit)
        {
            if(hit2D.transform.gameObject == gameObject)
                continue;
            
            nearbyPoints.Add(hit2D.transform.GetComponent<NearbyPoints>());
        }
        
        //横にあるものを先頭にする
        nearbyPoints.Sort((a, b) =>
        {
            var value = Mathf.Abs(a.transform.position.y) > Mathf.Abs(b.transform.position.y);
            return value ? 0 : 1;
        });
    }

    private void OnDrawGizmos()
    {
        if (Selection.gameObjects.Any(activeObject => activeObject == gameObject))
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
    }
#endif
}
