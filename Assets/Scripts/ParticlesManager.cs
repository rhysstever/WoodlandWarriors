using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    // Singleton
    public static ParticlesManager instance = null;

    // Set in inspector
    // === Healing Particle System Data ===
    [SerializeField]
    private GameObject healParticlePrefab;
    [SerializeField]
    private Vector2 healParticleVelocity;
    [SerializeField]
    private float healSpawnRate, healSpawnLifetime, healParticleLifetime;
    // === Burn Particle System Data ===
    [SerializeField]
    private GameObject burnParticlePrefab;
    [SerializeField]
    private Vector2 burnParticleVelocity;
    [SerializeField]
    private float burnSpawnRate, burnSpawnLifetime, burnParticleLifetime;
    // === Poison Particle System Data ===
    [SerializeField]
    private GameObject poisonParticlePrefab;
    [SerializeField]
    private Vector2 poisonParticleVelocity;
    [SerializeField]
    private float poisonSpawnRate, poisonSpawnLifetime, poisonParticleLifetime;

    [SerializeField]
    private Color resetColor, takeDamageColor, burnColor, poisonColor;

    private Dictionary<ActionType, ParticleSystemData> particleSystemDataMap;

    // Properties
    public Color ResetColor { get { return resetColor; } }
    public Color TakeDamageColor { get { return takeDamageColor; } }
    public Color BurnColor { get { return burnColor; } }
    public Color PoisonColor { get { return poisonColor; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        particleSystemDataMap = new Dictionary<ActionType, ParticleSystemData>();
        particleSystemDataMap.Add(
            ActionType.Heal,
            new ParticleSystemData(
                healParticlePrefab, healParticleVelocity,
                healSpawnRate, healSpawnLifetime, healParticleLifetime
            )
        );
        particleSystemDataMap.Add(
            ActionType.Burn,
            new ParticleSystemData(
                burnParticlePrefab, burnParticleVelocity,
                burnSpawnRate, burnSpawnLifetime, burnParticleLifetime
            )
        );
        particleSystemDataMap.Add(
            ActionType.Poison,
            new ParticleSystemData(
                poisonParticlePrefab, poisonParticleVelocity,
                poisonSpawnRate, poisonSpawnLifetime, poisonParticleLifetime
            )
        );
    }

    public ParticleSystemData GetParticleSystemData(ActionType actionType)
    {
        if(particleSystemDataMap.ContainsKey(actionType))
        {
            return particleSystemDataMap[actionType];
        }
        else
        {
            Debug.Log(string.Format("Error! No particle system data for type {0}. Returning first data...", actionType));
            return particleSystemDataMap[particleSystemDataMap.Keys.First()];
        }
    }
}

public class ParticleSystemData
{
    private GameObject particlePrefab;
    private Vector2 particleVelocity;
    private float spawnRate, spawnLifetime, particleLifetime;

    public GameObject ParticlePrefab { get { return particlePrefab; } }
    public Vector2 ParticleVelocity { get { return particleVelocity; } }
    public float SpawnRate { get { return spawnRate; } }
    public float SpawnLifetime { get { return spawnLifetime; } }
    public float ParticleLifetime { get { return particleLifetime; } }

    public ParticleSystemData(
        GameObject particlePrefab, Vector2 particleVelocity, 
        float spawnRate, float spawnLifetime, float particleLifetime)
    {
        this.particlePrefab = particlePrefab;
        this.particleVelocity = particleVelocity;
        this.spawnRate = spawnRate;
        this.spawnLifetime = spawnLifetime;
        this.particleLifetime = particleLifetime;
    }
}
