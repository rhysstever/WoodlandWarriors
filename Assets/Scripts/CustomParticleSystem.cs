using UnityEngine;

public class CustomParticleSystem : MonoBehaviour
{
    [SerializeField]
    private ActionType actionType;

    private ParticleSystemData particleSystemData;
    private BoxCollider2D spawnArea;
    private bool isEnabled, isPaused;
    private float spawnTimer, totalTimer;
    private float xMin, xMax, yMin, yMax;

    public bool IsPaused { get { return isPaused; } }

    void Start()
    {
        particleSystemData = ParticlesManager.instance.GetParticleSystemData(actionType);
        spawnArea = GetComponent<BoxCollider2D>();
        spawnTimer = particleSystemData.SpawnRate;
        totalTimer = 0f;
        xMin = spawnArea.offset.x - spawnArea.size.x / 2; 
        xMax = spawnArea.offset.x + spawnArea.size.x / 2; 
        yMin = spawnArea.offset.y - spawnArea.size.y / 2;
        yMax = spawnArea.offset.y + spawnArea.size.y / 2;
    }

    void Update()
    {
        if(isEnabled && !isPaused)
        {
            spawnTimer += Time.deltaTime;
            totalTimer += Time.deltaTime;
            if(spawnTimer >= particleSystemData.SpawnRate)
            {
                SpawnParticle();
                spawnTimer = 0f;
            }

            if(totalTimer >= particleSystemData.SpawnLifetime)
            {
                isEnabled = false;
                // Reset timers
                totalTimer = 0f;
                spawnTimer = 0f;
            }
        }
    }

    public void EnableParticles() { 
        isEnabled = true;
        spawnTimer = particleSystemData.SpawnRate;
        totalTimer = 0f;
    }
    public void Pause() { isPaused = true; }
    public void UnPause() { isPaused = false; }

    private void SpawnParticle()
    {
        Vector2 spawnPos = transform.position + new Vector3(
            Random.Range(xMin, xMax),
            Random.Range(yMin, yMax)
        );

        GameObject newParticle = Instantiate(particleSystemData.ParticlePrefab, transform);
        newParticle.transform.position = spawnPos;
        newParticle.GetComponent<CustomParticle>().SetInitalValues(
            this, particleSystemData.ParticleVelocity, particleSystemData.ParticleLifetime
        );
    }
}
