using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(ClickEventDispatcher))]
public class ClickAnimations : MonoBehaviour
{
    RectTransform m_Transform;
    private Tween m_tween = null;
    [SerializeField] private bool canIdleBounce = false;

    private void Awake()
    {
        m_Transform ??= GetComponent<RectTransform>();
    }

    private void Start()
    {
        InitiateIdleBounce();
    }



    public void ScaleDownAnimation()
    {
        if (m_Transform == null) return;

        if (m_tween != null)
        {
            if (m_tween.IsActive())
                m_tween.Kill();
        }

        m_tween = m_Transform.DOScale(Vector3.one * 0.8f, 0.25f).SetEase(Ease.OutCirc);

    }

    public void ScaleUpAnimation()
    {
        if (m_Transform == null) return;

        if (m_tween != null)
        {
            if (m_tween.IsActive())
                m_tween.Kill();
        }

        m_tween = m_Transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutCirc).OnComplete(InitiateIdleBounce);

    }

    private void InitiateIdleBounce()
    {
        if (!canIdleBounce)
            return;

        if (m_tween != null)
        {
            if (m_tween.IsActive())
                m_tween.Kill();
        }


        m_tween = this.transform.DOScale(Vector3.one * 1.125f, 1f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Normal, true).SetLoops(-1, LoopType.Yoyo);
    }

}
