using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // Singleton
    public static EnemyManager instance = null;

    // Instantiated in inspector
    [SerializeField]
    private Transform enemies;
    [SerializeField]
    private List<Transform> enemySpawnPositions;
    [SerializeField]
    private GameObject boarEnemyPrefab, mushroomEnemyPrefab, fairyEnemyPrefab, entEnemyPrefab, hagEnemyPrefab;
    //oozeEnemyPrefab, batSwarmEnemyPrefab, zombieEnemyPrefab, shadowEnemyPrefab, necromancerEnemyPrefab;

    // Instantiated in code
    private List<Round> enemyWaves;
    private int currentWaveNum;

    private IEnumerator enemyActionsCoroutine;
    private IEnumerator enemyEffectsCoroutine;

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
    }

    void Start()
    {
        enemyWaves = SetEnemyWaves();
        Reset();
    }

    private List<Round> SetEnemyWaves()
    {
        List<Round> combatRounds = new() {
            new(new List<GameObject>() { boarEnemyPrefab, boarEnemyPrefab }),
            new(new List<GameObject>() { mushroomEnemyPrefab, mushroomEnemyPrefab }),
            new(new List<GameObject>() { fairyEnemyPrefab, fairyEnemyPrefab, fairyEnemyPrefab }),
            new(new List<GameObject>() { entEnemyPrefab }),
            new(new List<GameObject>() { hagEnemyPrefab }),
            //new(new List<GameObject>() { oozeEnemyPrefab }),
            //new(new List<GameObject>() { batSwarmEnemyPrefab, batSwarmEnemyPrefab, batSwarmEnemyPrefab }),
            //new(new List<GameObject>() { zombieEnemyPrefab, zombieEnemyPrefab }),
            //new(new List<GameObject>() { shadowEnemyPrefab }),
            //new(new List<GameObject>() { necromancerEnemyPrefab }),
        };

        return combatRounds;
    }

    public GameObject GetEnemyPrefabByName(string enemyName)
    {
        return enemyName switch
        {
            "BoarEnemy" => boarEnemyPrefab,
            "Boar" => boarEnemyPrefab,
            "MushroomEnemy" => mushroomEnemyPrefab,
            "Mushroom" => mushroomEnemyPrefab,
            "FairyEnemy" => fairyEnemyPrefab,
            "Fairy" => fairyEnemyPrefab,
            "EntEnemy" => entEnemyPrefab,
            "Ent" => entEnemyPrefab,
            "HagEnemy" => hagEnemyPrefab,
            "Hag" => hagEnemyPrefab,
            _ => null,
        };
    }

    public void StartEnemyCombatTurn()
    {
        enemyActionsCoroutine = PerformEnemyRoundActions(GetCurrentEnemies());
        StartCoroutine(enemyActionsCoroutine);
    }

    private IEnumerator PerformEnemyRoundActions(List<Enemy> enemies)
    {
        WaitForSeconds enemyDelayWait = new WaitForSeconds(1);
        WaitForSeconds endOfEnemyTurnsDelayWait = new WaitForSeconds(1);
        int enemyIndex = 0;
        while(enemyIndex < enemies.Count && GameManager.instance.Player.CurrentLife > 0)
        {
            if(enemies[enemyIndex].CurrentLife > 0)
            {
                yield return enemyDelayWait;
                enemies[enemyIndex].PerformRoundAction();
            }
            enemyIndex++;
        }

        if(GameManager.instance.Player.CurrentLife > 0)
        {
            if(IsWaveOver())
            {
                CheckIfWaveIsOver();
            }
            else
            {
                yield return endOfEnemyTurnsDelayWait;
                GameManager.instance.ChangeCombatState(CombatState.PlayerTurn);
            }
        }
    }

    private bool IsWaveOver()
    {
        // Check all enemies and if none have health, the wave is over
        return enemies.GetComponentsInChildren<Enemy>().Where(enemy => enemy.CurrentLife > 0).ToList().Count == 0;
    }

    public void CheckIfWaveIsOver()
    {
        // Check all enemies and if none have health, the wave is over
        if(IsWaveOver())
        {
            // If the wave is over check if it is the last wave
            if(IsLastWave())
            {
                // If it is the last wave, end the game
                GameManager.instance.ChangeMenuState(MenuState.GameEnd);
            }
            else
            {
                // If there are more waves, change to CombatEnd to reward the player
                GameManager.instance.ChangeCombatState(CombatState.End);
            }
        }
    }

    public void SpawnNextWave()
    {
        currentWaveNum++;

        // If there are no more waves to spawn, end the game
        if(currentWaveNum >= enemyWaves.Count)
        {
            Debug.Log("You win! All waves defeated!");
            GameManager.instance.ChangeMenuState(MenuState.GameEnd);
            return;
        }

        Round round = enemyWaves[currentWaveNum];
        switch(round.Enemies.Count)
        {
            case 3:
                // Spawn the first (main) enemy in the middle position
                SpawnEnemy(round.Enemies[0], 2);
                // SPawn the remaining 2 enemies on the edge positions
                SpawnEnemy(round.Enemies[1], 0);
                SpawnEnemy(round.Enemies[2], 4);
                break;
            case 2:
                // Spawn both enemies in the second and fourth positions
                SpawnEnemy(round.Enemies[0], 1);
                SpawnEnemy(round.Enemies[1], 3);
                break;
            case 1:
                // Spawn the only enemy in the center spot
                SpawnEnemy(round.Enemies[0], 2);
                break;
            default:
                Debug.Log(string.Format("Error! Incorrect number of enemies: {0}!", round.Enemies.Count));
                break;
        }
    }

    public void SpawnSummon(GameObject enemy)
    {
        // Get leftmost available position
        int[] currentPositions = GetCurrentEnemies().Select(e => e.PositionIndex).ToArray();
        int positionIndex = -1;
        for(int i = 0; i < enemySpawnPositions.Count; i++)
        {
            if(!currentPositions.Contains(i))
            {
                positionIndex = i;
                break;
            }
        }

        // Spawn the enemy at that position
        if(positionIndex != -1)
        {
            SpawnEnemy(enemy, positionIndex);
        }
        else
        {
            Debug.Log("Error! No available positions to spawn summon!");
        }
    }

    private void SpawnEnemy(GameObject enemy, int positionIndex)
    {
        Vector2 position = enemySpawnPositions[positionIndex].position;
        GameObject newSceneEnemy = Instantiate(enemy, position, Quaternion.identity, enemies);
        newSceneEnemy.name = enemy.name + GetCurrentEnemies().Count;

        newSceneEnemy.GetComponent<Enemy>().SetPositionIndex(positionIndex);
    }

    public bool IsLastWave()
    {
        return currentWaveNum == enemyWaves.Count - 1;
    }

    public Enemy GetRandomEnemy()
    {
        List<Enemy> currentEnemies = GetCurrentEnemies();
        int randomEnemyIndex = UnityEngine.Random.Range(0, currentEnemies.Count);
        return currentEnemies[randomEnemyIndex];
    }

    public List<Enemy> GetCurrentEnemies()
    {
        return enemies.GetComponentsInChildren<Enemy>().ToList();
    }

    public IEnumerator ProcessEffectsOnEnemies()
    {
        WaitForSeconds enemyEffectsDelayWait = new WaitForSeconds(1);
        int enemyIndex = 0;
        List<Enemy> enemies = GetCurrentEnemies();

        while(enemyIndex < enemies.Count)
        {
            if(enemies[enemyIndex].HasEffectsToProcess())
            {
                yield return enemyEffectsDelayWait;
                enemyEffectsCoroutine = enemies[enemyIndex].ProcessEffects();
                StartCoroutine(enemyEffectsCoroutine);
            }
            else
            {
                enemies[enemyIndex].MarkProcessed();
            }
            enemyIndex++;
        }

        // Wait for all enemies to process all of their effects
        while(enemies.Where(e => !e.HasBeenProcessed).Count() > 0)
        {
            yield return enemyEffectsDelayWait;
        }

        if(IsWaveOver())
        {
            GameManager.instance.ChangeCombatState(CombatState.End);
        }
        else
        {
            StartEnemyCombatTurn();
        }
    }

    public void Reset()
    {
        currentWaveNum = -1;

        for(int i = enemies.childCount - 1; i >= 0; i--)
        {
            Destroy(enemies.GetChild(i).gameObject);
        }
    }
}
