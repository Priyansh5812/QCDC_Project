using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class HittableView
{
    HittableController controller;
    Slider healthUI;
    Image fillImage;
    public HittableView(Slider healthUI, Image fillImage)
    {
        this.healthUI = healthUI;
        this.fillImage = fillImage;
    }

    public void ToggleHealthUI(bool isActive)
    {
        healthUI.gameObject.SetActive(isActive);
    }

    public void UpdateHealthUI(float t)
    {
        healthUI.value = t;
        fillImage.color = Color.Lerp(Color.red , Color.green, t);   
    }

    public void InitiateKillAnimation(MeshRenderer mesh , Action OnAnimationCompleted)
    {
        if(!mesh.gameObject.activeSelf)
            return;

        Sequence seq = DOTween.Sequence();
        seq.Append(mesh.transform.DOLocalMoveY(2.0f, 1f));
        seq.Join(mesh.transform.DOShakeScale(1.0f));
        if(OnAnimationCompleted != null)
            seq.OnComplete(OnAnimationCompleted.Invoke);
    }

    
}