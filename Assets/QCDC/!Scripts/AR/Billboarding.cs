using UnityEngine;

public class Billboarding : MonoBehaviour
{
    [SerializeField] float lerpSpeed = 5.0f;
    Camera cam;
    void Start()
    {
        cam ??= Camera.main;
    }

    void Update()
    {
        BillboardToCamera();    
    }


    void BillboardToCamera()
    {
        this.transform.LookAt(cam.transform);
        Vector3 vec = this.transform.forward;
        vec.y = 0;
        this.transform.forward = vec;
    }
}
