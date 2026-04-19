using DG.Tweening;
using UnityEngine;

public class State_QCDC_Interaction : IState
{
    private QCDCStateController stateController;
    private Data_QCDC_Interaction data;
    bool isChangingAnimation = false;
    Quaternion targetRotation;
    Vector3 targetScale;
    float initialScaleMag;
    float pinchDelta;
    float twistDelta;
    QCDCInteractor qcdcInteractor;
    public State_QCDC_Interaction(QCDCStateController controller, Data_QCDC_Interaction data)
    {
        stateController = controller;
        this.data = data;
    }

    #region STARTUP
    public void OnEnter()
    {
        Debug.Log("State_QCDC_Interaction_1: Enter");
        isChangingAnimation = true;
        qcdcInteractor ??= stateController.QcdcInteractor;
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_1 , OnChangeCompletion);
        data.twistDelta.EnableDirectActionIfModeUsed();
        data.pinchDelta.EnableDirectActionIfModeUsed();
        targetRotation = qcdcInteractor.transform.rotation;
        targetScale = qcdcInteractor.transform.localScale;
        initialScaleMag = targetScale.magnitude;

        //-------------
        InitListeners();
        PrepareStartup();
    }

    void InitListeners()
    {
        data.backBtn.onClick.AddListener(CloseView);
        data.tog_NormalView.onValueChanged.AddListener(OnNormalModeToggled);
        data.tog_ExplodedView.onValueChanged.AddListener(OnExplodedModeToggled);
    }

    async void PrepareStartup()
    {
        SetViewFor(QCDCAnimationState.INTERACTION_1);
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = true;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
    }

    void OnChangeCompletion() => isChangingAnimation = false;
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
        targetRotation *= Quaternion.Euler(0, -30.0f * twistDelta * data.twistDeltaModifier * data.twistMultiplier * Time.deltaTime, 0);
    }

    void ComputeScale()
    { 
        targetScale += Vector3.one * pinchDelta * data.pinchDeltaModifier;
        targetScale = Vector3.ClampMagnitude(targetScale, initialScaleMag * 1.5f);
        if (targetScale.x < initialScaleMag / 2)
            targetScale = Vector3.one * initialScaleMag / 2;
    }

    void InterpolationUpdate()
    {
        stateController.QcdcInteractor.transform.rotation = Quaternion.Slerp(stateController.QcdcInteractor.transform.rotation, targetRotation, data.lerpSpeed * Time.deltaTime);
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
        data.cgMain.interactable = false;
        isChangingAnimation = true;
        SetViewFor(QCDCAnimationState.INTERACTION_2);
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_2, OnExplodedModeCompletion);
        void OnExplodedModeCompletion()
        {
            isChangingAnimation = false;
            data.cgMain.interactable = true;
        }
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


    #region DEINIT
    void CloseView()
    {
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        _=data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        qcdcInteractor.SetAnimationState(QCDCAnimationState.INTERACTION_1 , OnAnimationSetCompletion);
        
        void OnAnimationSetCompletion()
        {
            SetViewFor(QCDCAnimationState.INTERACTION_1);
            stateController.InitiateStateChange(typeof(State_PosingQCDC));
        }
    }

    void DeInitListeners()
    {
        data.backBtn.onClick.RemoveListener(CloseView);
        data.tog_NormalView.onValueChanged.RemoveListener(OnNormalModeToggled);
        data.tog_ExplodedView.onValueChanged.RemoveListener(OnExplodedModeToggled);
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

