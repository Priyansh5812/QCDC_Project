using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine.SceneManagement;

public class GameScreenView : IMonoState
{
    /// <summary>
    /// Indicates whether Start has been called already for this state.
    /// </summary>
    public bool IsAlreadyTriggered { get; private set; }
    GameScreenData data;
    MenuStateController controller;
    public GameScreenView(GameScreenData data, MenuStateController controller)
    {
        this.data = data;
        this.controller = controller;

    }

    /// <summary>
    /// Called when this view is enabled. Wires up UI listeners and animates
    /// the UI into view.
    /// </summary>
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
        // Mark this state as started to avoid repeating initial setup if
        // re-enabled later.
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
        // Trigger sign out and return to the auth screen state.
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
        data.cgMain.blocksRaycasts = false;
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        OnDisableCompleted?.Invoke();
    }
}
