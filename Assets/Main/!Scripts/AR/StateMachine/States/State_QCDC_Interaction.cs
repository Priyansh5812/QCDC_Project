using DG.Tweening;
using UnityEngine;

public class State_QCDC_Interaction : IState
{
    // Manages the interaction UI and manipulation of the spawned QCDC. The
    // interaction state supports rotating via twist input, scaling via
    // pinch input and switching between normal and exploded views.
    private ArenaStateController stateController;
    private Data_QCDC_Interaction data;
    bool isChangingAnimation = false;
    Quaternion targetRotation;
    Vector3 targetScale;
    float initialScaleMag;
    float pinchDelta;
    float twistDelta;
    QCDCInteractor qcdcInteractor;
    public State_QCDC_Interaction(ArenaStateController controller, Data_QCDC_Interaction data)
    {
        stateController = controller;
        this.data = data;
    }

    #region STARTUP
    public void OnEnter()
    {
        Debug.Log("State_QCDC_Interaction_1: Enter");

        InitListeners();
        isChangingAnimation = true;
        //qcdcInteractor ??= stateController.ArenaSpawnerInstance;
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_1 , OnChangeCompletion);
        data.twistDelta.EnableDirectActionIfModeUsed();
        data.pinchDelta.EnableDirectActionIfModeUsed();
        targetRotation = qcdcInteractor.transform.rotation;
        targetScale = qcdcInteractor.transform.localScale;
        initialScaleMag = targetScale.magnitude;

        //-------------
    }

    void InitListeners()
    {
        data.backBtn.onClick.AddListener(CloseView);
        data.tog_NormalView.onValueChanged.AddListener(OnNormalModeToggled);
        data.tog_ExplodedView.onValueChanged.AddListener(OnExplodedModeToggled);
        data.leftTraversal.onClick.AddListener(OnLeftTraversal);
        data.rightTraversal.onClick.AddListener(OnRightTraversal);
        data.playSim.onClick.AddListener(PlaySimulation);
    }

    async void PrepareStartup()
    {
        qcdcInteractor.PopulateDescription(data.partDescription);
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = false;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        data.cgMain.blocksRaycasts = true;
        Debug.Log("Startuped");
        SetViewFor(QCDCAnimationState.INTERACTION_1);
    }

    void OnChangeCompletion()
    { 
        // Called when the initial animation finishes; we now enable user
        // interaction and UI.
        isChangingAnimation = false;
        PrepareStartup();
    }
    #endregion

    public void OnUpdate()
    {
        if (isChangingAnimation)
            return;

        ReadInputs();
        ComputeRotation();
        ComputeScale();
        InterpolationUpdate();
    }

    void ReadInputs()
    {
        twistDelta = data.twistDelta.ReadValue();
        pinchDelta = data.pinchDelta.ReadValue();
    }

    void ComputeRotation()
    {   
        // Apply twist input to the desired rotation.
        targetRotation *= Quaternion.Euler(0, -30.0f * twistDelta * data.twistDeltaModifier * data.twistMultiplier * Time.deltaTime, 0);
    }

    void ComputeScale()
    { 
        // Update the target uniform scale based on pinch input and clamp it
        // to a reasonable range.
        targetScale += Vector3.one * pinchDelta * data.pinchDeltaModifier;
        targetScale = Vector3.ClampMagnitude(targetScale, initialScaleMag * 1.5f);
        if (targetScale.x < initialScaleMag / 2)
            targetScale = Vector3.one * initialScaleMag / 2;
    }

    void InterpolationUpdate()
    {
        // Smoothly interpolate the transform towards the target rotation.
        stateController.ArenaSpawnerInstance.transform.rotation = Quaternion.Slerp(stateController.ArenaSpawnerInstance.transform.rotation, targetRotation, data.lerpSpeed * Time.deltaTime);
    }

    #region CALLBACKS

    void OnNormalModeToggled(bool value)
    {
        if (value)
            SwitchNormalMode();
    }

    void OnExplodedModeToggled(bool value)
    {
        if (value)
            SwitchExplodedMode();
    }

    void SwitchNormalMode()
    {
        Debug.Log("Switched to Normal");
        data.cgMain.interactable = false;
        isChangingAnimation = true;
        SetViewFor(QCDCAnimationState.INTERACTION_1);
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_1 , OnNormalModeCompletion);
        void OnNormalModeCompletion()
        {
            isChangingAnimation = false;
            data.cgMain.interactable = true;
        }
    }


    void SwitchExplodedMode()
    {
        Debug.Log("Switched to Exploded");
        data.cgMain.interactable = false;
        isChangingAnimation = true;
        qcdcInteractor.PopulateDescription(data.partDescription);
        SetViewFor(QCDCAnimationState.INTERACTION_2);
        qcdcInteractor.PlayAnimationFor(QCDCAnimationState.INTERACTION_2);
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_2, OnExplodedModeCompletion);
        void OnExplodedModeCompletion()
        {
            isChangingAnimation = false;
            data.cgMain.interactable = true;
            
        }
    }

    void PlaySimulation()
    {
        qcdcInteractor.PlayAnimationFor(QCDCAnimationState.INTERACTION_1);
    }

    #endregion

    void SetViewFor(QCDCAnimationState state)
    {
        data.cgInteraction_1.alpha = state == QCDCAnimationState.INTERACTION_1 ? 1 : 0;
        data.cgInteraction_2.alpha = state == QCDCAnimationState.INTERACTION_1 ? 0 : 1;
        data.cgInteraction_1.interactable = data.cgInteraction_1.blocksRaycasts = state == QCDCAnimationState.INTERACTION_1;
        data.cgInteraction_2.interactable = data.cgInteraction_2.blocksRaycasts = state == QCDCAnimationState.INTERACTION_2;
        data.tog_ExplodedView.SetIsOnWithoutNotify(state == QCDCAnimationState.INTERACTION_2);
        data.tog_NormalView.SetIsOnWithoutNotify(state == QCDCAnimationState.INTERACTION_1);
    }

    void OnLeftTraversal()
    {
        qcdcInteractor.OnTraversal(1);
        qcdcInteractor.PopulateDescription(data.partDescription);
    }

    void OnRightTraversal()
    { 
        qcdcInteractor.OnTraversal(-1);
        qcdcInteractor.PopulateDescription(data.partDescription);
    }


    #region DEINIT
    void CloseView()
    {
        // Fade out UI and reset animation/state before transitioning back to
        // the posing state.
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        _=data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_1 , OnAnimationSetCompletion);
        qcdcInteractor.PlayAnimationFor(QCDCAnimationState.INTERACTION_2); // Reset the anim state
        void OnAnimationSetCompletion()
        {
            SetViewFor(QCDCAnimationState.INTERACTION_1);
            stateController.InitiateStateChange(typeof(State_PosingArena));
        }
    }

    void DeInitListeners()
    {
        data.backBtn.onClick.RemoveListener(CloseView);
        data.tog_NormalView.onValueChanged.RemoveListener(OnNormalModeToggled);
        data.tog_ExplodedView.onValueChanged.RemoveListener(OnExplodedModeToggled);
        data.leftTraversal.onClick.RemoveListener(OnLeftTraversal);
        data.rightTraversal.onClick.RemoveListener(OnRightTraversal);
        data.playSim.onClick.RemoveListener(PlaySimulation);
    }
    public void OnExit()
    {
        DeInitListeners();
        Debug.Log("State_QCDC_Interaction_1: Exit");
        data.twistDelta.DisableDirectActionIfModeUsed();
        data.pinchDelta.DisableDirectActionIfModeUsed();
    }
    #endregion
}

