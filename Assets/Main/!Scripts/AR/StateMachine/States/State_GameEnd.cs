using UnityEngine;
using DG.Tweening;
public class State_GameEnd : IState
{

    ArenaStateController controller;
    Data_GameEnd data;

    public State_GameEnd(ArenaStateController controller , Data_GameEnd data)
    {
        this.controller = controller;
        this.data = data;
    }


    public void OnEnter()
    {
        PrepareStartup();
        InitListeners();
    }

    async void PrepareStartup()
    {
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = false;
        UpdateScoreUI();
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        data.cgMain.blocksRaycasts = true;
        Debug.Log("Startuped");

    }

    void InitListeners()
    {
        data.btn_playAgain.onClick.AddListener(HandlePlayAgain);
    }

    public void OnUpdate()
    {
        
    }

    void HandlePlayAgain()
    {   
        EventManager.OnGameRestart.Invoke();
        controller.InitiateStateChange(typeof(State_Gameplay));
    }

    void UpdateScoreUI()
    {
        float score = EventManager.GetScore.Invoke();
        data.score?.SetText($"Score : {score}");
    }

    void CloseView()
    {
        // Fade out UI and reset animation/state before transitioning back to
        // the posing state.
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        _=data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
    }

    void DeInitListeners()
    {
        data.btn_playAgain.onClick.RemoveListener(HandlePlayAgain);        
    }

    public void OnExit()
    {
        DeInitListeners();
        CloseView();
    }
}
