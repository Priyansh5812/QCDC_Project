using UnityEngine;

/// <summary>
/// Simple billboarding behaviour. Rotates the GameObject to face the main
/// camera while constraining rotation so the object only turns around the
/// vertical axis (y-axis). Useful for UI elements or sprites that should
/// always face the user but remain upright.
/// </summary>
public class Billboarding : MonoBehaviour
{
    /// <summary>
    /// Optional speed used for smoothing rotation. Kept as a serialized field
    /// so it can be adjusted in the inspector, but is not currently used in
    /// this implementation. Consider using <see cref="lerpSpeed"/> inside
    /// <see cref="BillboardToCamera"/> if a smooth rotation is desired.
    /// </summary>
    [SerializeField] float lerpSpeed = 5.0f;

    /// <summary>
    /// Cached reference to the camera that the object will face. Defaults to
    /// <see cref="Camera.main"/> when the component starts.
    /// </summary>
    Camera cam;

    /// <summary>
    /// Initialize cached camera reference.
    /// </summary>
    void Start()
    {
        // Use the main camera if none has been assigned yet.
        cam ??= Camera.main;
    }

    /// <summary>
    /// Called every frame to update the object's rotation so it faces the
    /// camera.
    /// </summary>
    void Update()
    {
        BillboardToCamera();
    }

    /// <summary>
    /// Rotate the object to look at the camera, then zero out the vertical
    /// component of the forward vector so the object remains upright and
    /// only rotates around the y-axis.
    /// </summary>
    void BillboardToCamera()
    {
        // Make the transform look directly at the camera position.
        this.transform.LookAt(cam.transform);

        // Preserve only the horizontal direction so the object stays upright.
        Vector3 vec = this.transform.forward;
        vec.y = 0;
        this.transform.forward = vec;
    }
}
