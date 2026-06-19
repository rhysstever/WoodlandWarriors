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
    [SerializeField]
    private bool healHasRandomizedSpawn;
    // === Burn Particle System Data ===
    [SerializeField]
    private GameObject burnParticlePrefab;
    [SerializeField]
    private Vector2 burnParticleVelocity;
    [SerializeField]
    private float burnSpawnRate, burnSpawnLifetime, burnParticleLifetime;
    [SerializeField]
    private bool burnHasRandomizedSpawn;
    // === Poison Particle System Data ===
    [SerializeField]
    private GameObject poisonParticlePrefab;
    [SerializeField]
    private Vector2 poisonParticleVelocity;
    [SerializeField]
    private float poisonSpawnRate, poisonSpawnLifetime, poisonParticleLifetime;
    [SerializeField]
    private bool poisonHasRandomizedSpawn;

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
                healParticlePrefab,
                healParticleVelocity,
                healSpawnRate, healSpawnLifetime, healParticleLifetime,
                healHasRandomizedSpawn
            )
        );
        particleSystemDataMap.Add(
            ActionType.Burn,
            new ParticleSystemData(
                burnParticlePrefab,
                burnParticleVelocity,
                burnSpawnRate, burnSpawnLifetime, burnParticleLifetime,
                burnHasRandomizedSpawn
            )
        );
        particleSystemDataMap.Add(
            ActionType.Poison,
            new ParticleSystemData(
                poisonParticlePrefab,
                poisonParticleVelocity,
                poisonSpawnRate, poisonSpawnLifetime, poisonParticleLifetime,
                poisonHasRandomizedSpawn
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
    private bool hasRandomizedSpawn;

    public GameObject ParticlePrefab { get { return particlePrefab; } }
    public Vector2 ParticleVelocity { get { return particleVelocity; } }
    public float SpawnRate { get { return spawnRate; } }
    public float SpawnLifetime { get { return spawnLifetime; } }
    public float ParticleLifetime { get { return particleLifetime; } }
    public bool HasRandomizedSpawn { get { return hasRandomizedSpawn; } }

    public ParticleSystemData(
        GameObject particlePrefab, 
        Vector2 particleVelocity, 
        float spawnRate, float spawnLifetime, float particleLifetime,
        bool hasRandomizedSpawn)
    {
        this.particlePrefab = particlePrefab;
        this.particleVelocity = particleVelocity;
        this.spawnRate = spawnRate;
        this.spawnLifetime = spawnLifetime;
        this.particleLifetime = particleLifetime;
        this.hasRandomizedSpawn = hasRandomizedSpawn;
    }
}
