using UnityEngine;

public class AudioPrefab : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    void Start()
    {
        
    }

    void Update()
    {
        if(!audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
