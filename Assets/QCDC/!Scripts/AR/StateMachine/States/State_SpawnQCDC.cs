using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.XR.ARFoundation;
public class State_SpawnQCDC : IState
{
    private QCDCStateController stateController;
    private Data_SpawnQCDC data;
    bool wasReadPerformed = false;
    bool isUnderAnimation = false;

    public State_SpawnQCDC(QCDCStateController controller, Data_SpawnQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {   
        Debug.Log("State_SpawnQCDC: Enter");
        data.inputButtonReader.EnableDirectActionIfModeUsed();
        PrepareView();
    }

    // Check for Spawning...
    public void OnUpdate()
    {
#if UNITY_EDITOR
        SpawnQCDC_EDITOR();
    #else

        if (wasReadPerformed && !isUnderAnimation)
        { 
            SpawnQCDC();
            return;
        }
        wasReadPerformed = false;
        wasReadPerformed = data.inputButtonReader.ReadWasPerformedThisFrame();
#endif
    }

    void SpawnQCDC_EDITOR()
    {
        var qcdc = GameObject.Instantiate(data.qcdcPrefab, null);
        qcdc.transform.position = stateController.transform.position;
        Vector3 direction = stateController.MainCamera.transform.position - qcdc.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
            qcdc.transform.rotation = targetRotation;
        }

        stateController.QcdcInteractor = qcdc;
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = false;
        data.cgMain.alpha = 0.0f;
        stateController.InitiateStateChange(typeof(State_PosingQCDC));
    }

    void SpawnQCDC()
    {
        if (data.interactor.TryGetCurrentARRaycastHit(out var raycastHit))
        {
            var qcdc = GameObject.Instantiate(data.qcdcPrefab, null);
            qcdc.transform.position = raycastHit.pose.position;
            Vector3 direction = stateController.MainCamera.transform.position - qcdc.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
                qcdc.transform.rotation = targetRotation;
            }

            stateController.QcdcInteractor = qcdc;

            TryAddARAnchor(qcdc);
            
            CloseView();
        }
    }

    void TryAddARAnchor(QCDCInteractor interactor)
    {
        if (!interactor.gameObject.TryGetComponent<ARAnchor>(out var anchor))
        { 
            interactor.gameObject.AddComponent<ARAnchor>();
        }
    }

    async void PrepareView()
    {
        isUnderAnimation = true;
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = true;
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        isUnderAnimation = false;
    }

    async void CloseView()
    {
        isUnderAnimation = true;
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        isUnderAnimation = false;
        stateController.InitiateStateChange(typeof(State_PosingQCDC));
    }


    public void OnExit()
    {
        data.inputButtonReader.DisableDirectActionIfModeUsed();
        Debug.Log("State_SpawnQCDC: Exit");
    }
}

