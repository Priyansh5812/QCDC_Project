using UnityEngine;

public class QCDCInteractor : MonoBehaviour
{

    [SerializeField] ParticleSystem smokeParticleSystem;

    public void PlaySmokeParticle()
    { 
        smokeParticleSystem.Play();
    }

}
