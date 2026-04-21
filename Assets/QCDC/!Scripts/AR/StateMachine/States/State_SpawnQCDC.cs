using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR.ARFoundation;
public class State_SpawnQCDC : IState
{
    private QCDCStateController stateController;
    private Data_SpawnQCDC data;
    bool wasReadPerformed = false;
    bool isUnderAnimation = false;
    bool isAssetLoading = false;
    bool isAssetLoaded = false;
    bool isSpawning = false;
    AsyncOperationHandle<GameObject> handle;
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
        InitiateAssetLoad();
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

    async void SpawnQCDC()
    {
        if (isSpawning)
            return;

        if (data.interactor.TryGetCurrentARRaycastHit(out var raycastHit))
        {
            isSpawning = true;
            SetActiveSpawningOverlay(true);

            while (isAssetLoading)
                await UniTask.Yield();

            SetActiveSpawningOverlay(false);
            if (handle.Result == null)
            {
                isSpawning = false;
                return;
            }

            //var qcdc = GameObject.Instantiate(data.qcdcPrefab, null);
            var qcdc = GameObject.Instantiate(handle.Result, null).GetComponent<QCDCInteractor>();
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
            isSpawning = false;
        }
    }

    void SetActiveSpawningOverlay(bool value)
    { 
        data.spawningOverlay.SetActive(value);
    }

    void InitiateAssetLoad()
    {
        if (isAssetLoaded || isAssetLoading)
            return;

        isAssetLoading = true;
        var handle = data.qcdcAsset.LoadAssetAsync();
        handle.Completed += OnAssetLoadCompleted;
    }

    void OnAssetLoadCompleted(AsyncOperationHandle<GameObject> obj)
    {
        isAssetLoading = false;
        isAssetLoaded = true;
        handle = obj;
        stateController.SetAssetLoadHandle(obj);
        
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

