using UnityEngine;

public class ArenaSpawner : MonoBehaviour
{   
    [SerializeField] GameConfig config;
    HittableController[] registeredHittables;
    int hittableCount;
    float score;

    void Awake()
    {   
        registeredHittables = this.GetComponentsInChildren<HittableController>();
        InitializeSpawner();

    }

    void InitializeSpawner()
    {
        hittableCount = registeredHittables.Length;
        score = 0;
        InitializeHittables();
    }

    void OnEnable()
    {
        InitListeners();
    }

    void InitListeners()
    {
        EventManager.OnOrbKill.AddListener(HandleOrbKill);
        EventManager.GetScore.AddListener(GetScore);
        EventManager.OnGameRestart.AddListener(InitializeSpawner);
    }
    
    void InitializeHittables()
    {
        foreach(var i in registeredHittables)
        {
            i.InitializeHittable();
            i.SetHittableState(HittableState.COLLIDABLE);
        }
    }

    void HandleOrbKill()
    {   
        Debug.Log("Orb Killed");
        hittableCount--;
        score += config.scorePerKill;
        EventManager.PostOrbKill.Invoke();
        if(hittableCount <= 0)
        {
            EventManager.OnWaveCompleted.Invoke();
        }
    }

    float GetScore() => this.score;

    void DeInitListeners()
    {
        EventManager.OnOrbKill.RemoveListener(HandleOrbKill);
        EventManager.GetScore.RemoveListener(GetScore);
        EventManager.OnGameRestart.RemoveListener(InitializeSpawner);
    }

    void OnDisable()
    {
        DeInitListeners();
    }
}
