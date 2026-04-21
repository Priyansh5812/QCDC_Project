using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class AuthScreenView : IMonoState
{
    /// <summary>
    /// Tracks whether Start has already been called for this state.
    /// </summary>
    public bool IsAlreadyTriggered { get; private set; }

    // References to view data and controllers used to drive authentication UI.
    AuthScreenData data;
    MenuStateController menuController;
    AuthScreenController controller;
    EmailAuthState emailAuthState;
    bool isChangingEmailAuthState = false;

    /// <summary>
    /// Construct the auth view with its data and parent menu controller.
    /// </summary>
    public AuthScreenView(AuthScreenData data , MenuStateController controller)
    {
        this.data = data;
        this.controller ??= new();
        this.menuController = controller;
    }

    /// <summary>
    /// Called when the view becomes active. Sets up UI listeners and waits
    /// for Firebase initialization before showing the UI.
    /// </summary>
    public void OnEnable(Action OnEnableCompleted = null)
    {
        InitListeners();
        HandleInitialization();
        OnEnableCompleted?.Invoke();
    }

    /// <summary>
    /// Called once when the view starts. Marks the view as triggered so
    /// future enable calls can skip certain initialization steps.
    /// </summary>
    public void Start(Action OnStartCompleted = null)
    {   
        IsAlreadyTriggered = true;
        OnStartCompleted?.Invoke();
    }

    void InitListeners()
    {
        data.guest.onClick.AddListener(ProceedGuestLogin);
        data.loginToggle.onClick.AddListener(ToggleEmailPassAuthState);
        data.loginSignup.onClick.AddListener(ProceedEmailPassAuth);
        data.googleLogin.onClick.AddListener(ProceedGoogleSignIn);
    }

    void HandleInitialization()
    {
        // Prepare UI hidden state while we wait for firebase or auth check.
        data.cgMain.interactable = false;
        data.cgMain.alpha = 0.0f;

        if (FirebaseManager.IsFirebaseActive)
        {
            PrepareStartup();
        }
        else 
        {
            // If firebase isn't ready yet, subscribe to the initialized
            // event and let FirebaseManager handle de-init.
            FirebaseManager.OnFirebaseInitialized += PrepareStartup; // DeInit will be handled by the firebase
        }

    }


    async void PrepareStartup()
    {   
        // If there's already a signed-in user, skip to the game screen.
        if (FirebaseManager.Instance.GetCurrentFirebaseUser() != null)
        {
            menuController.InitiateStateChange(typeof(GameScreenView));
        }
        else 
        {
            await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
            data.cgMain.interactable = data.cgMain.blocksRaycasts = true;
        }
    }



    async void ProceedGuestLogin()
    {
        OnAuthInitiated();
        await FirebaseManager.Instance.TryAnonymousLogin(OnAuthInitiated , OnAuthSuccess, OnAuthFailed);
    }

    void ProceedEmailPassAuth()
    {
        // Route to login or signup flow based on current toggle state.
        switch (emailAuthState)
        {
            case EmailAuthState.LOGIN:
                ProceedEmailPassLogin();
                break;
            case EmailAuthState.SIGNUP:
                ProceedEmailPassSignup();
                break;
            default:
                // no-op
                break;
        }
    }

    async void ProceedEmailPassLogin()
    {
        if (!controller.PerformEmailPassChecks(data.emailField.text, data.passField.text, out var err))
        {
            OnAuthFailed(err);
            return;
        }

        OnAuthInitiated();

        await FirebaseManager.Instance.TryEmailPassLogin(data.emailField.text, data.passField.text, OnAuthSuccess, OnAuthFailed);
    }

    async void ProceedEmailPassSignup()
    {
        if (!controller.PerformEmailPassChecks(data.emailField.text, data.passField.text, out var err))
        {
            OnAuthFailed(err);
            return;
        }

        OnAuthInitiated();
        await FirebaseManager.Instance.TryEmailPassSignUp(data.emailField.text, data.passField.text, OnAuthSuccess, OnAuthFailed);
    }


    void OnAuthInitiated()
    { 
        // Disable UI while the auth flow is in progress.
        data.cgMain.interactable = false;
    }

    async void OnAuthSuccess()
    {
        data.authStatus?.SetText("Authentication Success!!!");
        await UniTask.Delay(500);
        menuController.InitiateStateChange(typeof(GameScreenView));
    }

    void OnAuthFailed(string reason)
    {
        // Show error and re-enable UI so the user can try again.
        data.authStatus.SetText(reason);
        data.cgMain.interactable = true;
    }


    async void ProceedGoogleSignIn()
    {
        OnAuthInitiated();
        FirebaseManager.Instance.TryGoogleSignIn(OnAuthSuccess, OnAuthFailed);
    }


    async void ToggleEmailPassAuthState()
    {
        if (isChangingEmailAuthState)
            return;
        isChangingEmailAuthState = true;
        data.cgMain.interactable = false;
        emailAuthState = emailAuthState == EmailAuthState.LOGIN ? EmailAuthState.SIGNUP : EmailAuthState.LOGIN;

        data.ToggleText?.SetText(emailAuthState == EmailAuthState.LOGIN ? "Login" : "Signup");
    
        // Animate image and text fades and a shake on the button to communicate the switch.
        data.ToggleButtonText?.SetText(emailAuthState == EmailAuthState.LOGIN ? data.emailAuthAsLoginMsg : data.emailAuthAsSignupMsg);
        await data.loginSignup.targetGraphic.rectTransform.DOShakeAnchorPos(0.5f, Vector2.right * 12.5f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        isChangingEmailAuthState = false;  
    }


    void DeInitListeners()
    {
        data.guest.onClick.RemoveListener(ProceedGuestLogin);
        data.loginToggle.onClick.RemoveListener(ToggleEmailPassAuthState);
        data.loginSignup.onClick.RemoveListener(ProceedEmailPassAuth);
        data.googleLogin.onClick.RemoveListener(ProceedGoogleSignIn);
    }

    public async void OnDisable(Action OnDisableCompleted = null)
    {
        DeInitListeners();
        data.cgMain.interactable = false;
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        OnDisableCompleted?.Invoke();
    }
}

public enum EmailAuthState
{ 
    LOGIN,
    SIGNUP  
}
