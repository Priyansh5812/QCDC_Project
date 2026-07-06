using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.XR.ARFoundation;
public class State_SpawnQCDC : IState
{
    private ArenaStateController stateController;
    private Data_SpawnQCDC data;
    bool wasReadPerformed = false;
    bool isUnderAnimation = false;
    bool isAssetLoading = false;
    bool isAssetLoaded = false;
    bool isSpawning = false;
    bool isSpawned = false;
    AsyncOperationHandle<GameObject> handle;
    public State_SpawnQCDC(ArenaStateController controller, Data_SpawnQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {   
        Debug.Log("State_SpawnQCDC: Enter");
        data.inputButtonReader.EnableDirectActionIfModeUsed();
        //PrepareView();
    }

    // Called each frame while this state is active. In editor the prefab
    // is instantiated immediately for convenience; on device it waits for
    // the input reader and spawns at the AR raycast hit.
    public void OnUpdate()
    {



#if UNITY_EDITOR
        SpawnArena_EDITOR();
#else

        if (wasReadPerformed && !isUnderAnimation)
        { 
            SpawnArena();
            return;
        }
        wasReadPerformed = false;
        wasReadPerformed = data.inputButtonReader.ReadWasPerformedThisFrame();
        Debug.Log(wasReadPerformed);
#endif
    }

    void SpawnArena_EDITOR()
    {   
        if(isSpawned)
            return;

        var arena = GameObject.Instantiate(data.arenaPrefab, null);
        arena.transform.position = stateController.transform.position;
        Vector3 direction = stateController.MainCamera.transform.position - arena.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.AngleAxis(90.0f, Vector3.up);
            arena.transform.rotation = targetRotation;
        }

        stateController.ArenaSpawnerInstance = arena;
        // data.cgMain.interactable = false;
        // data.cgMain.blocksRaycasts = false;
        // data.cgMain.alpha = 0.0f;
        isSpawned = true;
        //stateController.InitiateStateChange(typeof(State_PosingQCDC));
    }

    async void SpawnArena()
    {
        // if (isSpawning)
        //     return;

        Debug.Log("Entered Spawning");

        if (data.interactor.TryGetCurrentARRaycastHit(out var raycastHit))
        {
            isSpawning = true;
            //SetActiveSpawningOverlay(true);

            // while (isAssetLoading)
            //     await UniTask.Yield();

            // SetActiveSpawningOverlay(false);
            // if (handle.Result == null)
            // {
            //     isSpawning = false;
            //     return;
            // }

            Debug.Log("Spawn Intended");

            // Instantiate the loaded addressable prefab and set its pose.
            var arena = GameObject.Instantiate(data.arenaPrefab, null);
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
            //CloseView();
            isSpawning = false;
            isSpawned = true;
        }
    }

    void SetActiveSpawningOverlay(bool value)
    { 
        data.spawningOverlay.SetActive(value);
    }


    void TryAddARAnchor(ArenaSpawner interactor)
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
        //stateController.InitiateStateChange(typeof(State_PosingQCDC));
    }


    public void OnExit()
    {
        data.inputButtonReader.DisableDirectActionIfModeUsed();
        Debug.Log("State_SpawnQCDC: Exit");
    }
}

