using DG.Tweening;
using UnityEngine;

public class State_Gameplay : IState
{
    // Manages the interaction UI and manipulation of the spawned QCDC. The
    // interaction state supports rotating via twist input, scaling via
    // pinch input and switching between normal and exploded views.
    private ArenaStateController stateController;
    private Data_Gameplay data;
    bool isChangingAnimation = false;
    ArenaSpawner arenaInstance;
    public State_Gameplay(ArenaStateController controller, Data_Gameplay data)
    {
        stateController = controller;
        this.data = data;
    }

    #region STARTUP
    public void OnEnter()
    {
        Debug.Log("State_QCDC_Interaction_1: Enter");

        InitListeners();
        arenaInstance ??= stateController.ArenaSpawnerInstance;
        isChangingAnimation = true;
        PrepareStartup();
        UpdateScoreUI();
    }

    void InitListeners()
    {
        EventManager.PostOrbKill.AddListener(UpdateScoreUI);
        EventManager.OnWaveCompleted.AddListener(ProceedGameEnd);
        data.btn_fire.onClick.AddListener(FireBullet);
    }

    async void PrepareStartup()
    {
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = false;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        data.cgMain.blocksRaycasts = true;
        Debug.Log("Startuped");

    }

    #endregion

    public void OnUpdate()
    {
        //noop
    }

    void FireBullet()
    {
        Camera cam = stateController.MainCamera;
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 0.1f;
        var bullet = BulletPooler.Get(BulletType.Default,null,spawnPos,Quaternion.LookRotation(cam.transform.forward)) as BulletController;
        if(bullet == null)
        {
            Debug.LogError("Bullet was NULL");
        }
        else
        {
            bullet.gameObject.SetActive(true);
        }
    }

    void UpdateScoreUI()
    {
        float score = EventManager.GetScore.Invoke();
        Debug.Log("Got Score : "+score.ToString());
        Debug.Log($"Score  is Null : {data.score == null}");
        data.score?.SetText($"Score : {score}");
    }

    void ProceedGameEnd()
    {
        stateController.InitiateStateChange(typeof(State_GameEnd));
    }

    #region DEINIT
    void CloseView()
    {
        // Fade out UI and reset animation/state before transitioning back to
        // the posing state.
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        _=data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
    }

    void DeInitListeners()
    {   
        EventManager.PostOrbKill.RemoveListener(UpdateScoreUI);
        EventManager.OnWaveCompleted.RemoveListener(ProceedGameEnd);
        data.btn_fire.onClick.RemoveListener(FireBullet);
    }

    public void OnExit()
    {
        DeInitListeners();
        CloseView();
    }
    #endregion
}

