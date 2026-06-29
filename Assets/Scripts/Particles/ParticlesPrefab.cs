using UnityEngine;

public class ParticlesPrefab : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem ps;

    private float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
        StartParticleSystem();
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.CurrentMenuState == MenuState.Game
            && !UIManager.instance.IsSubMenuShowing
            && ps.isPlaying)
        {
            timer += Time.deltaTime;
            if(timer >= ps.main.duration)
            {
                StopParticleSystem();
            }
        }        
    }

    public void StartParticleSystem()
    {
        timer = 0f;
        ps.Play();
    }

    private void StopParticleSystem() 
    { 
        ps.Stop();
    }
}
