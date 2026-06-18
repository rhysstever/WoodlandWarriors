public class EnemyWave
{
    private EnemyType enemyType;
    private int enemyCount;
    private bool isSummoningBossWave;

    public EnemyType EnemyType { get { return enemyType; } }
    public int EnemyCount { get { return enemyCount; } }
    public bool IsSummoningBossWave { get { return isSummoningBossWave; } }

    public EnemyWave(EnemyType enemyType, int enemyCount, bool isSummoningBossWave)
    {
        this.enemyType = enemyType;
        this.enemyCount = enemyCount;
        this.isSummoningBossWave = isSummoningBossWave;
    }

    public EnemyWave(EnemyType enemyType, int enemyCount) : this(enemyType, enemyCount, false)
    {

    }
}
