using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EnemyType
{
    Boar,
    Mushroom,
    Fairy,
    Ent,
    Hag
}

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
    private List<EnemyWave> enemyWaves;
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

    private List<EnemyWave> SetEnemyWaves()
    {
        List<EnemyWave> combatRounds = new() {
            // Area 1 enemies
            new EnemyWave(EnemyType.Mushroom, 2),
            new EnemyWave(EnemyType.Boar, 2),
            new EnemyWave(EnemyType.Fairy, 3),
            new EnemyWave(EnemyType.Ent, 1),
            new EnemyWave(EnemyType.Hag, 1, true),
            // Area 2 enemies
            //new(new List<GameObject>() { oozeEnemyPrefab }),
            //new(new List<GameObject>() { batSwarmEnemyPrefab, batSwarmEnemyPrefab, batSwarmEnemyPrefab }),
            //new(new List<GameObject>() { zombieEnemyPrefab, zombieEnemyPrefab }),
            //new(new List<GameObject>() { shadowEnemyPrefab }),
            //new(new List<GameObject>() { necromancerEnemyPrefab }),
        };

        return combatRounds;
    }

    public List<Action> GetEnemyActions(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Boar => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.Attack, 2, TargetType.Player),
                new Buff(ActionType.Attack, 2),
            },
            EnemyType.Mushroom => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.Poison, 2, TargetType.Player),
                new Action(ActionType.Attack, 2, TargetType.Player),
                new Buff(ActionType.Poison, 1),
            },
            EnemyType.Fairy => new List<Action>
            {
                new Action(ActionType.Defend, 2, TargetType.Self),
                new Action(ActionType.Burn, 1, TargetType.Player),
                new EnemySummon(1, EnemyType.Fairy),
                new Buff(ActionType.Burn, 1),
                // TODO: add summon of self
            },
            EnemyType.Ent => new List<Action>
            {
                new Action(ActionType.Poison, 6, TargetType.Player),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.Attack, 15, TargetType.Player),
                new Buff(ActionType.Attack, 10),
            },
            EnemyType.Hag => new List<Action>
            {
                new EnemySummon(2, EnemyType.Mushroom),
                new Action(ActionType.Defend, 5, TargetType.Self),
                new Action(ActionType.Burn, 5, TargetType.Player),
                new Action(ActionType.Poison, 10, TargetType.Player),
                new Action(ActionType.Heal, 10, TargetType.Self),
            },
            _ => new List<Action>()
        };
    }

    public GameObject GetEnemyPrefabByType(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.Boar => boarEnemyPrefab,
            EnemyType.Mushroom => mushroomEnemyPrefab,
            EnemyType.Fairy => fairyEnemyPrefab,
            EnemyType.Ent => entEnemyPrefab,
            EnemyType.Hag => hagEnemyPrefab,
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
        WaitForSeconds turnBannerDelayWait = new WaitForSeconds(UIManager.instance.TurnBannerVisibleTime);

        UIManager.instance.ToggleEnemyTurnBanner(true);
        yield return turnBannerDelayWait;
        UIManager.instance.ToggleEnemyTurnBanner(false);

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

        EnemyWave wave = enemyWaves[currentWaveNum];
        GameObject enemyToSpawn = GetEnemyPrefabByType(wave.EnemyType);
        switch(wave.EnemyCount)
        {
            case 3:
                // Spawn one enemy in the middle position and
                // 2 enemies on the edge positions
                SpawnEnemy(enemyToSpawn, 0).IncrementStartingRound();
                SpawnEnemy(enemyToSpawn, 2);
                SpawnEnemy(enemyToSpawn, 4).IncrementStartingRound();
                break;
            case 2:
                // Spawn both enemies in the second and fourth positions
                SpawnEnemy(enemyToSpawn, 1);
                SpawnEnemy(enemyToSpawn, 3).IncrementStartingRound();
                break;
            case 1:
                if(wave.IsSummoningBossWave)
                {
                    // Spawn the boss on the edge to prepare for when it summons
                    SpawnEnemy(enemyToSpawn, 4);
                }
                else
                {
                    // Spawn the only enemy in the center spot
                    SpawnEnemy(enemyToSpawn, 2);
                }
                break;
            default:
                Debug.Log(string.Format("Error! Incorrect number of enemies: {0}!", wave.EnemyCount));
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

    private Enemy SpawnEnemy(GameObject enemy, int positionIndex)
    {
        Vector2 position = enemySpawnPositions[positionIndex].position;
        GameObject newEnemyObject = Instantiate(enemy, position, Quaternion.identity, enemies);
        newEnemyObject.name = enemy.name + positionIndex;

        Enemy newEnemy = newEnemyObject.GetComponent<Enemy>();
        newEnemy.SetPositionIndex(positionIndex);
        return newEnemy;
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
