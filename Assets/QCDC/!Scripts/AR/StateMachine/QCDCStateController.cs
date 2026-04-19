using System.Collections.Generic;
using System;
using UnityEngine;



public class QCDCStateController : MonoBehaviour
{
    IState currState;
    Dictionary<Type, IState> stateReg;
    [SerializeField] Data_SpawnQCDC spawnData;
    [SerializeField] Data_PosingQCDC posingData;
    [SerializeField] Data_QCDC_Interaction interaction1Data;

    // OtherFields...
    Camera _mainCam;

    public Camera MainCamera
    {
        get
        { 
            _mainCam ??= Camera.main;
            return _mainCam;
        }
    }

    public QCDCInteractor QcdcInteractor
    {
        get; set;
    }
    
    void OnEnable()
    {
        InitializeStateRegistery();
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void Start()
    {
        InitiateStateChange(typeof(State_SpawnQCDC));
    }


    void Update()
    {
        currState?.OnUpdate();
    }


    void InitializeStateRegistery()
    {
        if (stateReg != null && stateReg.Count > 0)
            return;

        stateReg ??= new Dictionary<Type, IState>();

        stateReg.Add(typeof(State_SpawnQCDC), new State_SpawnQCDC(this, spawnData));
        stateReg.Add(typeof(State_PosingQCDC), new State_PosingQCDC(this, posingData));
        stateReg.Add(typeof(State_QCDC_Interaction), new State_QCDC_Interaction(this, interaction1Data));
    }

    public void InitiateStateChange(Type type)
    {
        if (!stateReg.ContainsKey(type))
            return;
        currState?.OnExit();
        currState = stateReg[type];
        currState?.OnEnter();
    }
    
}
