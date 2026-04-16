using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

public class AuthScreenView : IMonoState
{
    public bool IsAlreadyTriggered { get; private set; }
    AuthScreenData data;
    MenuStateController menuController;
    AuthScreenController controller;
    EmailAuthState emailAuthState;
    bool isChangingEmailAuthState = false;
    public AuthScreenView(AuthScreenData data , MenuStateController controller)
    {
        this.data = data;
        this.controller ??= new();
        this.menuController = controller;
    }

    public void OnEnable(Action OnEnableCompleted = null)
    {
        InitListeners();
        OnEnableCompleted?.Invoke();
    }

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

    async void ProceedGuestLogin()
    {
        OnAuthInitiated();
        await FirebaseManager.Instance.TryAnonymousLogin(OnAuthInitiated , OnAuthSuccess, OnAuthFailed);
    }

    void ProceedEmailPassAuth()
    {
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

    public void OnDisable(Action OnDisableCompleted = null)
    {
        DeInitListeners();
        OnDisableCompleted?.Invoke();
    }
}

public enum EmailAuthState
{ 
    LOGIN,
    SIGNUP  
}
