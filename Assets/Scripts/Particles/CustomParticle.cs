using UnityEngine;

public class CustomParticle : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    private CustomParticleSystem parentSystem;
    private Vector2 velocity;
    private float currentLifetime, lifetime;

    void Start()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        currentLifetime = 0f;
    }

    void Update()
    {
        if(!parentSystem.IsPaused)
        {
            currentLifetime += Time.deltaTime;
            rb.linearVelocity = velocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

        if(currentLifetime > lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetInitalValues(CustomParticleSystem parentSystem, Vector2 velocity, float lifetime)
    {
        this.parentSystem = parentSystem;
        this.velocity = velocity;
        this.lifetime = lifetime;
    }
}
