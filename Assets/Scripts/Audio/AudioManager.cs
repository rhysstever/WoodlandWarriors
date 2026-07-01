using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager instance = null;

    // Set in inspector
    [SerializeField]
    private List<GameObject> damageTakenAudioPrefabs;
    [SerializeField]
    private List<GameObject> damageBlockedAudioPrefabs;
    [SerializeField]
    private List<GameObject> deathAudioPrefabs;
    [SerializeField]
    private GameObject healAudioPrefab;
    [SerializeField]
    private List<GameObject> attackAudioPrefabs;
    [SerializeField]    // Ally audio prefabs
    private GameObject squirrelAudioPrefab, frogAudioPrefab, ratAudioPrefab, newtAudioPrefab, toadAudioPrefab, porcupineAudioPrefab, hamsterAudioPrefab;
    [SerializeField]
    private GameObject spellAttackAudioPrefab;
    [SerializeField]
    private GameObject giveDefenseAudioPrefab;
    [SerializeField]
    private GameObject burnAudioPrefab;
    [SerializeField]
    private GameObject poisonAudioPrefab;
    [SerializeField]
    private GameObject spikesAudioPrefab;
    [SerializeField]
    private GameObject drinkAudioPrefab;

    public delegate void OnAudioDelegate();
    public static OnAudioDelegate onAttackAudioDelegate;
    public static OnAudioDelegate onDefendAudioDelegate;
    public static OnAudioDelegate onSquirrelAudioDelegate, onFrogAudioDelegate, onRatAudioDelegate, 
        onNewtAudioDelegate, onToadAudioDelegate, onPorcupineAudioDelegate, onHamsterAudioDelegate;
    public static OnAudioDelegate onSpellAttackAudioDelegate;
    public static OnAudioDelegate onDrinkAudioDelegate;
    public static OnAudioDelegate onHealAudioDelegate;
    public static OnAudioDelegate onBurnAudioDelegate;
    public static OnAudioDelegate onPoisonAudioDelegate;

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

        onAttackAudioDelegate += PlayAttackAudio;
        onDefendAudioDelegate += PlayGiveDefenseAudio;

        // Ally audio delegates
        onSquirrelAudioDelegate += PlaySquirrelAudio;
        onFrogAudioDelegate += PlayFrogAudio;
        onRatAudioDelegate += PlayRatAudio;
        onNewtAudioDelegate += PlayNewtAudio;
        onToadAudioDelegate += PlayToadAudio;
        onPorcupineAudioDelegate += PlayPorcupineAudio;
        onHamsterAudioDelegate += PlayHamsterAudio;

        onSpellAttackAudioDelegate += PlaySpellAttackAudio;
        onDrinkAudioDelegate += PlayDrinkAudio;
        onHealAudioDelegate += PlayHealAudio;
        onBurnAudioDelegate += PlayBurnAudio;
        onPoisonAudioDelegate += PlayPoisonAudio;
    }

    public void PlaySlotAttackAudio(ActionType actionType)
    {
        switch(actionType)
        {
            case ActionType.WeaponAttack:
                PlayAttackAudio();
                break;
            case ActionType.SpellAttack:
                PlaySpellAttackAudio();
                break;
            default:
                PlayAttackAudio();
                break;
        }
    }

    private void PlayAttackAudio()
    {
        int randIndex = Random.Range(0, attackAudioPrefabs.Count);
        CreateAudioObject(attackAudioPrefabs[randIndex]);
    }

    private void PlaySpellAttackAudio()
    {
        CreateAudioObject(spellAttackAudioPrefab);
    }

    public void PlayGiveDefenseAudio()
    {
        CreateAudioObject(giveDefenseAudioPrefab);
    }

    public void PlayAllyAudio(string allyName)
    {
        switch(allyName.ToLower())
        {
            case "squirrel":
                PlaySquirrelAudio();
                break;
            case "frog":
                PlayFrogAudio();
                break;
            case "rat":
                PlayRatAudio();
                break;
            case "newt":
                PlayNewtAudio();
                break;
            case "toad":
                PlayToadAudio();
                break;
            case "porcupine":
                PlayPorcupineAudio();
                break;
            case "hamster":
                PlayHamsterAudio();
                break;
        }
    }

    #region Ally Audio
    private void PlaySquirrelAudio()
    {
        CreateAudioObject(squirrelAudioPrefab);
    }

    private void PlayFrogAudio()
    {
        CreateAudioObject(frogAudioPrefab);
    }

    private void PlayRatAudio()
    {
        CreateAudioObject(ratAudioPrefab);
    }

    private void PlayNewtAudio()
    {
        CreateAudioObject(newtAudioPrefab);
    }

    private void PlayToadAudio()
    {
        CreateAudioObject(toadAudioPrefab);
    }

    private void PlayPorcupineAudio()
    {
        CreateAudioObject(porcupineAudioPrefab);
    }

    private void PlayHamsterAudio()
    {
        CreateAudioObject(hamsterAudioPrefab);
    }
    #endregion Ally Audio

    public void PlayHealAudio()
    {
        CreateAudioObject(healAudioPrefab);
    }

    public void PlayDrinkAudio()
    {
        CreateAudioObject(drinkAudioPrefab);
    }

    public void PlayDamageTakenAudio()
    {
        int randIndex = Random.Range(0, damageTakenAudioPrefabs.Count);
        CreateAudioObject(damageTakenAudioPrefabs[randIndex]);
    }

    public void PlayDamageBlockedAudio()
    {
        int randIndex = Random.Range(0, damageBlockedAudioPrefabs.Count);
        CreateAudioObject(damageBlockedAudioPrefabs[randIndex]);
    }

    public void PlayDeathAudio()
    {
        int randIndex = Random.Range(0, deathAudioPrefabs.Count);
        CreateAudioObject(deathAudioPrefabs[randIndex]);
    }

    public void PlayBurnAudio()
    {
        CreateAudioObject(burnAudioPrefab);
    }

    public void PlayPoisonAudio()
    {
        CreateAudioObject(poisonAudioPrefab);
    }

    public void PlaySpikesAudio()
    {
        CreateAudioObject(spikesAudioPrefab);
    }

    private void CreateAudioObject(GameObject audioPrefab)
    {
        Instantiate(audioPrefab, transform);
    }
}
