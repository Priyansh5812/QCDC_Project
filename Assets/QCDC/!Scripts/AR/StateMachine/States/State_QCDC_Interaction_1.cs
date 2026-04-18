using UnityEngine;

public class State_QCDC_Interaction_1 : IState
{
    private QCDCStateController stateController;
    private Data_QCDC_Interaction_1 data;

    public State_QCDC_Interaction_1(QCDCStateController controller, Data_QCDC_Interaction_1 data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {
        Debug.Log("State_QCDC_Interaction_1: Enter");
    }

    public void OnUpdate()
    {
        // TODO: add interaction 1 update logic
    }

    public void OnExit()
    {
        Debug.Log("State_QCDC_Interaction_1: Exit");
    }
}
