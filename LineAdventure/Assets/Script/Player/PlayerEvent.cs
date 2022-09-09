using UniRx;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Player/PlayerEvent")]
    public class PlayerEvent : ScriptableObject
    {
        public Subject<Vector3> moveEvent = new Subject<Vector3>();
    }
}
