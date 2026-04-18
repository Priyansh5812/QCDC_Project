using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class GameScreenView : IMonoState
{
    public bool IsAlreadyTriggered { get; private set; }
    GameScreenData data;
    MenuStateController controller;
    public GameScreenView(GameScreenData data, MenuStateController controller)
    {
        this.data = data;
        this.controller = controller;

    }

    public void OnEnable(Action OnEnableCompleted = null)
    {
        InitListeners();
        PrepareStartup();
        OnEnableCompleted?.Invoke();
    }

    void InitListeners()
    {
        data.btn_startDemo?.onClick.AddListener(OnStartGame);
        data.btn_signout?.onClick.AddListener(OnSignout);
    }



    public void Start(Action OnStartCompleted = null)
    {
        IsAlreadyTriggered = true;
        OnStartCompleted?.Invoke();
    }

    async void PrepareStartup()
    {
        data.cgMain.interactable = false;
        data.cgMain.alpha = 0.0f;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = data.cgMain.blocksRaycasts = true;
    }

    void OnStartGame()
    {
        data.cgMain.interactable = false;
        SceneManager.LoadScene(1);
    }

    async void OnSignout()
    {
        data.cgMain.interactable = false;
        _=FirebaseManager.Instance.SignOutUser();
        await UniTask.Yield();
        controller.InitiateStateChange(typeof(AuthScreenView));

    }



    void DeInitListeners()
    {
        data.btn_startDemo?.onClick.RemoveListener(OnStartGame);
        data.btn_signout?.onClick.RemoveListener(OnSignout);
    }
    public async void OnDisable(Action OnDisableCompleted = null)
    {
        DeInitListeners();
        data.cgMain.interactable = false;
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        OnDisableCompleted?.Invoke();
    }
}
