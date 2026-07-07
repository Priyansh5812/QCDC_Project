using DG.Tweening;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR.ARFoundation;
public class State_SpawnArena : IState
{
    private ArenaStateController stateController;
    private Data_SpawnQCDC data;
    bool wasReadPerformed = false;
    bool isUnderAnimation = false;
    public State_SpawnArena(ArenaStateController controller, Data_SpawnQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {   
        Debug.Log("State_Spawn: Enter");
        data.inputButtonReader.EnableDirectActionIfModeUsed();
        PrepareView();
        
    }

    // Called each frame while this state is active. In editor the prefab
    // is instantiated immediately for convenience; on device it waits for
    // the input reader and spawns at the AR raycast hit.
    public void OnUpdate()
    {
        if (wasReadPerformed && !isUnderAnimation)
        { 
            SpawnArena();
            return;
        }
        wasReadPerformed = false;
        wasReadPerformed = data.inputButtonReader.ReadWasPerformedThisFrame();


        PlaneScanPass();
    }

    void SpawnArena()
    {

        if (data.interactor.TryGetCurrentARRaycastHit(out var raycastHit))
        {
            ArenaSpawner arena;
            arena = stateController.ArenaSpawnerInstance == null ? GameObject.Instantiate(data.arenaPrefab, null) : stateController.ArenaSpawnerInstance;
            arena.transform.position = raycastHit.pose.position;
            Vector3 direction = stateController.MainCamera.transform.position - arena.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
                arena.transform.rotation = targetRotation;
            }

            stateController.ArenaSpawnerInstance = arena;
            TryAddARAnchor(arena);

        }
    }

    void PlaneScanPass()
    {
        if(data.planeManager.trackables.count > 0 && !data.btn_confirmPlacement.gameObject.activeSelf)
        {   
            data.prompt?.SetText(data.msg_ClicktoSpawn);
            data.btn_confirmPlacement.gameObject.SetActive(true);
        }
    }


    void TryAddARAnchor(ArenaSpawner interactor)
    {
        if (!interactor.gameObject.TryGetComponent<ARAnchor>(out _))
        { 
            interactor.gameObject.AddComponent<ARAnchor>();
        }
    }

    async void PrepareView()
    {
        isUnderAnimation = true;
        data.cgMain.interactable = false;
        data.cgMain.blocksRaycasts = true;
        data.btn_confirmPlacement.onClick.AddListener(CloseView);
        await data.cgMain.DOFade(1f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        data.cgMain.interactable = true;
        data.btn_confirmPlacement.gameObject.SetActive(false);
        data.prompt?.SetText(data.msg_ScanForArea);
        isUnderAnimation = false;
    }
    
    async void CloseView()
    {
        isUnderAnimation = true;
        data.cgMain.interactable = data.cgMain.blocksRaycasts = false;
        data.btn_confirmPlacement.onClick.RemoveListener(CloseView);
        await data.cgMain.DOFade(0f, 0.25f).SetEase(Ease.OutSine).AsyncWaitForCompletion();
        stateController.InitiateStateChange(typeof(State_PosingArena));
        isUnderAnimation = false;
    }


    public void OnExit()
    {
        data.inputButtonReader.DisableDirectActionIfModeUsed();
        Debug.Log("State_SpawnArena: Exit");
    }
}

