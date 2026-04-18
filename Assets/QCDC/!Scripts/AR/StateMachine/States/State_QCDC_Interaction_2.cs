using UnityEngine;

public class State_QCDC_Interaction_2 : IState
{
    private QCDCStateController stateController;
    private Data_QCDC_Interaction_2 data;

    public State_QCDC_Interaction_2(QCDCStateController controller, Data_QCDC_Interaction_2 data)
    {
        stateController = controller;
        this.data = data;
    }

    public void OnEnter()
    {
        Debug.Log("State_QCDC_Interaction_2: Enter");
    }

    public void OnUpdate()
    {
        // TODO: add interaction 2 update logic
    }

    public void OnExit()
    {
        Debug.Log("State_QCDC_Interaction_2: Exit");
    }
}
