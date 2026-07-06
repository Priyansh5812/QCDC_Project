using UnityEngine;
using System.Collections;
public class HittableController : MonoBehaviour , IDamagable
{   
    [SerializeField] Collider collider;
    [SerializeField] ParticleSystem particleSystem;
    [SerializeField] MeshRenderer mesh;
    [SerializeField , Min(0f)] float health;

    private static readonly int ShakeSpeedID = Shader.PropertyToID("_ShakeSpeed");
    Coroutine hitRoutine = null;

    void Start()
    {
        mesh.sharedMaterial = new Material(mesh.sharedMaterial);
        //ToggleHittableState(true);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            HitEnemy();
        }
    }


    public void ReceiveDamage(float damageAmt)
    {
        health-= damageAmt;
        health = Mathf.Max(0f , health);

        if(health == 0f)
        {
            KillEnemy();
        }
        else
        {
            HitEnemy();
        }
    }


    public void HitEnemy()
    {
        if(hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitRoutine());
        
    }

    void ToggleHittableState(bool isActive)
    {
        collider.enabled = isActive;
        mesh.enabled = isActive;
    }

    public void KillEnemy()
    {
        
    }


    IEnumerator HitRoutine()
    {
        Vector3 startScale = Vector3.one;
        Vector3 randomScale = Vector3.one * Random.Range(0.85f, 1f);
        float hitDuration = 0.25f;
        float elapsed = 0f;

        particleSystem.Play();
        while (elapsed < hitDuration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / hitDuration);
            float t = Mathf.Sin(normalizedTime * Mathf.PI);

            mesh.transform.localScale = Vector3.Lerp(startScale, randomScale, t);
            mesh.sharedMaterial.SetFloat(ShakeSpeedID, t);

            yield return null;
        }

        mesh.transform.localScale = Vector3.one;
        mesh.sharedMaterial.SetFloat(ShakeSpeedID, 0f);
        hitRoutine = null;
    }


    IEnumerator DeadRoutine()
    {
        yield return null;
    }


}
