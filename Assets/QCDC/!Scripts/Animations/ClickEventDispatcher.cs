using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickEventDispatcher : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent onPointerDown = new UnityEvent();
    [SerializeField] private UnityEvent onPointerUp = new UnityEvent();

    [SerializeField] private bool RestrictToInteractable = false;
    private Button btn_main;
    private void Awake()
    {
        btn_main ??= GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {   
        if(RestrictToInteractable)
        {
            if(!btn_main.IsInteractable())
                return;
        }

        onPointerDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {   

        onPointerUp?.Invoke();
    }
}
