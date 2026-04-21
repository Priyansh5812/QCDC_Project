using DG.Tweening;
using UnityEngine;

public class State_PosingQCDC : IState
{
    private QCDCStateController stateController;
    private Data_PosingQCDC data;
    Vector2 posDelta;
    float pinchDelta;
    Vector3 InitialScale;
    Vector3 targetPosition;
    Quaternion targetRotation;
    Vector3 targetScale;
    Transform qcdcTransform;
    float initialScaleMag;
    bool canComputePose;
    bool canLookRotation;
    public State_PosingQCDC(QCDCStateController controller, Data_PosingQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {
        Debug.Log("State_PosingQCDC: Enter");
        SetTrackedPlanes(true);
        data.dragDelta.EnableDirectActionIfModeUsed();
        data.pinchDelta.EnableDirectActionIfModeUsed();
        data.twistDelta.EnableDirectActionIfModeUsed();
        qcdcTransform = stateController.QcdcInteractor.transform;
        //----------------
        targetPosition = qcdcTransform.position;
        targetRotation = qcdcTransform.rotation;
        InitialScale = targetScale = qcdcTransform.localScale;
        initialScaleMag = InitialScale.magnitude;
        canComputePose = true;
        canLookRotation = data.lookRotationToggle.isOn;
        //----------------
        data.btn_finalizePosition.onClick.AddListener(OnFinalizePosition);
        PrepareView();
    }

    async void PrepareView()
    {
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = true;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
    }

    public void OnUpdate()
    {
        ReadDelta();
        ComputeQCDCPose();
        LerpPose();
    }

    void ReadDelta()
    {
        posDelta = data.dragDelta.ReadValue();
        pinchDelta = data.pinchDelta.ReadValue();
    }

    void ComputeQCDCPose()
    {
        if (!canComputePose)
            return;

        canLookRotation = data.lookRotationToggle.isOn;

        if(canLookRotation)
            ComputeLookRotation();
        else
            ComputeTwistRotation();
        ComputePosition();
        ComputeScale();
    }

    void ComputeLookRotation()
    {   
        Vector3 targetDirection = stateController.MainCamera.transform.position - qcdcTransform.position;
        targetDirection.y = 0f;
        Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        rotation *= Quaternion.AngleAxis(90f, Vector3.up);

        targetRotation = rotation;
    }

    void ComputeTwistRotation()
    {
        targetRotation *= Quaternion.Euler(0, -30.0f * data.twistDelta.ReadValue() * data.twistDeltaModifier * data.twistMultiplier * Time.deltaTime, 0);
    }

    void ComputePosition()
    {
        Vector3 verticalDirection = (stateController.MainCamera.transform.position - qcdcTransform.position).normalized;
        verticalDirection.y = 0f;
        verticalDirection *= -1;
        Vector3 horizontalDirection = Vector3.Cross(Vector3.up, verticalDirection).normalized;

        targetPosition += (verticalDirection * posDelta.y + horizontalDirection * posDelta.x).normalized * data.dragSpeed * Time.deltaTime;
    }

    void ComputeScale()
    {
        targetScale += Vector3.one * pinchDelta * data.pinchDeltaModifier;
        targetScale = Vector3.ClampMagnitude(targetScale, initialScaleMag * 1.5f);
        if (targetScale.x < initialScaleMag / 2)
            targetScale = Vector3.one * initialScaleMag / 2;
    }

    void LerpPose()
    {
        qcdcTransform.rotation = Quaternion.Slerp(qcdcTransform.rotation, targetRotation, data.lerpSpeed * Time.deltaTime);
        qcdcTransform.position = Vector3.Lerp(qcdcTransform.position, targetPosition, data.lerpSpeed * Time.deltaTime);
        qcdcTransform.localScale = Vector3.Lerp(qcdcTransform.localScale, targetScale, data.lerpSpeed * Time.deltaTime);
    }

    void OnFinalizePosition()
    {
        CloseView();
    }

    async void CloseView()
    {
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        stateController.InitiateStateChange(typeof(State_QCDC_Interaction));
    }

    void SetTrackedPlanes(bool value)
    {
        foreach (var i in data.planeManager.trackables)
        { 
            i.gameObject.SetActive(value);
        }

        data.planeManager.enabled = value;
    }

    public void OnExit()
    {
        SetTrackedPlanes(false);
        data.dragDelta.DisableDirectActionIfModeUsed();
        data.pinchDelta.DisableDirectActionIfModeUsed();
        data.twistDelta.DisableDirectActionIfModeUsed();
        data.btn_finalizePosition.onClick.RemoveListener(OnFinalizePosition);
        Debug.Log("State_PosingQCDC: Exit");
    }
}
