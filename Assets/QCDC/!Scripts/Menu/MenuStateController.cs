using System;
using System.Collections.Generic;
using UnityEngine;
using Pkay.Utils;

public class MenuStateController : MonoBehaviour
{
    private readonly Dictionary<Type, IMonoState> stateReg = new();
    [SerializeField] AuthScreenData authScreenData;
    [SerializeField] GameScreenData gameScreenData;
    private bool isChangingState = false;
    IMonoState currentState;
    private void Start()
    {
        // Target a high framerate for responsive UI. Not essential for auth, but kept as original.
        Application.targetFrameRate = 144;
        ConstructMenuStates();
        InitiateStateChange(typeof(AuthScreenView));
    }
    private void ConstructMenuStates()
    {
        // Construct the available menu states and provide them with the
        // required data objects.
        stateReg.Add(typeof(AuthScreenView), new AuthScreenView(authScreenData,this));
        stateReg.Add(typeof(GameScreenView), new GameScreenView(gameScreenData,this));
    }

    /// <summary>
    /// Initiates a transition from the current menu state to a new one.
    /// This orchestrates disabling the current state and enabling the new
    /// state while ensuring only one transition runs at a time.
    /// </summary>
    public void InitiateStateChange(Type newStateType)
    {   

        if (isChangingState)
        {
            Utils.Warn($"Already changing state.");
            return;
        }

        IMonoState newState;
        if (stateReg.ContainsKey(newStateType))
            newState = stateReg[newStateType];
        else
        {
            Utils.Error($"Unknown {newStateType} state type");
            return;
        }

        isChangingState = true;
        Debug.Log("Setted True");
        if (currentState != null)
            currentState.OnDisable(OnInitialCompleted);
        else
            OnInitialCompleted();


        void OnInitialCompleted()
        {
            currentState = newState;

            // If the state has already been started previously, just enable
            // it. Otherwise call OnEnable and then Start.
            if (currentState.IsAlreadyTriggered)
                currentState.OnEnable(OnEnableCompleted);
            else
                currentState.OnEnable(OnEnableThenStart);
        }

        void OnEnableThenStart()
        {
            currentState.Start(OnStartCompleted);
        }

        void OnEnableCompleted()
        {
            isChangingState = false;
            Debug.Log("Setted false");
        }

        void OnStartCompleted()
        {
            isChangingState = false;
            Debug.Log("Setted false");
        }
    }

    private void OnDisable()
    {
        currentState?.OnDisable();
    }
}
