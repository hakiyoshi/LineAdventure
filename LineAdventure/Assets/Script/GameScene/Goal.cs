using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float goalRadius = 1.0f;
    [SerializeField] private AudioSource audioSource;

    // Update is called once per frame
    void Update()
    {
        var length = (player.position - transform.position).magnitude;
        if(length <= goalRadius)
        {
            //DOVirtual.Float(1.0f, 0.0f, 0.1f, x => GetComponent<AudioSource>().volume = x);
            Initiate.Fade("Title", Color.black, 2.0f);
        }
    }
}
