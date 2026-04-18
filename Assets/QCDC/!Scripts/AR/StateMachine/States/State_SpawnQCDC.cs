using UnityEngine;

public class State_SpawnQCDC : IState
{
    private QCDCStateController stateController;
    private Data_SpawnQCDC data;
    bool wasReadPerformed = false;
    public State_SpawnQCDC(QCDCStateController controller, Data_SpawnQCDC data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {
        Debug.Log("State_SpawnQCDC: Enter");
        data.inputButtonReader.EnableDirectActionIfModeUsed();
    }

    // Check for Spawning...
    public void OnUpdate()
    {
        if (wasReadPerformed)
        { 
            SpawnQCDC();
            return;
        }
        wasReadPerformed = false;
        wasReadPerformed = data.inputButtonReader.ReadWasPerformedThisFrame();
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
            stateController.InitiateStateChange(typeof(State_PosingQCDC));
        }
    }

    public void OnExit()
    {
        data.inputButtonReader.DisableDirectActionIfModeUsed();
        Debug.Log("State_SpawnQCDC: Exit");
    }
}

