using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ArenaStateController : MonoBehaviour
{
    IState currState;
    Dictionary<Type, IState> stateReg;
    [SerializeField] Data_SpawnQCDC spawnData;
    [SerializeField] Data_PosingQCDC posingData;
    [SerializeField] Data_Gameplay gameplayData;
    [SerializeField] Data_GameEnd gameEndData;

    // Cached main camera and addressables handle for cleanup.
    Camera _mainCam;
    public Camera MainCamera
    {
        get
        { 
            _mainCam ??= Camera.main;
            return _mainCam;
        }
    }

    /// <summary>
    /// Reference to the runtime spawned interactor. States read and modify
    /// this to control the in-scene QCDC instance.
    /// </summary>
    public ArenaSpawner ArenaSpawnerInstance
    {
        get; set;
    }
    
    void Awake()
    {
        VerifySceneReferences();
    }

    void VerifySceneReferences()
    {   
        if(spawnData.planeManager == null)
            spawnData.planeManager = FindFirstObjectByType<ARPlaneManager>();

        if(spawnData.interactor == null)
            spawnData.interactor = FindFirstObjectByType<XRRayInteractor>();

        if(posingData.planeManager == null)
            posingData.planeManager = spawnData.planeManager;
    }

    void OnEnable()
    {
        // Ensure registry is constructed and lock orientation for AR.
        InitializeStateRegistery();
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Start()
    {
        // Start the workflow with the spawn state.
        InitiateStateChange(typeof(State_SpawnArena));
    }


    void Update()
    {
        // Forward per-frame updates to the active state.
        currState?.OnUpdate();
    }


    void InitializeStateRegistery()
    {
        if (stateReg != null && stateReg.Count > 0)
            return;

        stateReg ??= new Dictionary<Type, IState>();

        // Register concrete state instances with their required data.
        stateReg.Add(typeof(State_SpawnArena), new State_SpawnArena(this, spawnData));
        stateReg.Add(typeof(State_PosingArena), new State_PosingArena(this, posingData));
        stateReg.Add(typeof(State_Gameplay), new State_Gameplay(this, gameplayData));
        stateReg.Add(typeof(State_GameEnd), new State_GameEnd(this, gameEndData));
    }

    /// <summary>
    /// Transition to the given state type. Calls the current state's
    /// OnExit and the target state's OnEnter.
    /// </summary>
    public void InitiateStateChange(Type type)
    {
        if (!stateReg.ContainsKey(type))
            return;
        currState?.OnExit();
        currState = stateReg[type];
        currState?.OnEnter();
    }


}
