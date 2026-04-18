using UnityEngine;

public class State_PosingQCDC : IState
{
    private QCDCStateController stateController;
    private Data_PosingQCDC data;
    Vector2 posDelta;
    float pinchDelta;
    Vector3 InitialPosition;
    Quaternion InitialRotation;
    Vector3 InitialScale;
    Vector3 targetPosition;
    Quaternion targetRotation;
    Vector3 targetScale;
    Transform qcdcTransform;
    float lerpSpeed;
    float initialScaleMag;
    Vector3 currVel;

    public State_PosingQCDC(QCDCStateController controller, Data_PosingQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {
        Debug.Log("State_PosingQCDC: Enter");
        data.dragDelta.EnableDirectActionIfModeUsed();
        data.pinchDelta.EnableDirectActionIfModeUsed();
        qcdcTransform = stateController.QcdcInteractor.transform;

        //----------------
        InitialPosition = targetPosition = qcdcTransform.position;
        InitialRotation = targetRotation = qcdcTransform.rotation;
        InitialScale = targetScale = qcdcTransform.localScale;
        initialScaleMag = InitialScale.magnitude;
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
        ComputeRotation();
        ComputePosition();
        ComputeScale();
    }

    void ComputeRotation()
    {
        Vector3 targetDirection = stateController.MainCamera.transform.position - qcdcTransform.position;
        targetDirection.y = 0f;
        Quaternion rotation = Quaternion.LookRotation(targetDirection, Vector3.up);
        rotation *= Quaternion.AngleAxis(90f, Vector3.up);

        targetRotation = rotation;
    }

    void ComputePosition()
    {
        Vector3 verticalDirection = (stateController.MainCamera.transform.position - qcdcTransform.position).normalized;
        verticalDirection.y = 0f;
        verticalDirection *= -1;
        Vector3 horizontalDirection = Vector3.Cross(Vector3.up, verticalDirection).normalized;

        targetPosition += (verticalDirection * posDelta.x + horizontalDirection * -posDelta.y).normalized * data.dragSpeed * Time.deltaTime;
    }

    void ComputeScale()
    {
        targetScale += Vector3.one * pinchDelta * data.pinchDeltaModifier;
        targetScale = Vector3.ClampMagnitude(targetScale, initialScaleMag * 1.5f);
        if(targetScale.x < initialScaleMag/2)
            targetScale = Vector3.one * initialScaleMag/2;
    }

    void LerpPose()
    {
        qcdcTransform.rotation = Quaternion.Slerp(qcdcTransform.rotation, targetRotation, data.lerpSpeed * Time.deltaTime);
        qcdcTransform.position = Vector3.Lerp(qcdcTransform.position, targetPosition, data.lerpSpeed * Time.deltaTime);
        qcdcTransform.localScale = Vector3.Lerp(qcdcTransform.localScale, targetScale, data.lerpSpeed * Time.deltaTime);
    }

    public void OnExit()
    {
        data.dragDelta.DisableDirectActionIfModeUsed();
        data.pinchDelta.DisableDirectActionIfModeUsed();
        Debug.Log("State_PosingQCDC: Exit");
    }
}
