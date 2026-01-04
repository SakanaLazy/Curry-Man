using UnityEngine;

public class SideScrollerCamera : MonoBehaviour
{
    // === Following Movement ===
    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(6f, 2f, -10f);
    public float smoothSpeed = 5f;

    [Header("Camera Bounds")]
    [Tooltip("Minimum and maximum X values the camera can move to.")]
    public float minX = -10f;
    public float maxX = 25f;

    [Tooltip("Optional Z bounds (useful if you have slight 2.5D depth).")]
    public float minZ = -5f;
    public float maxZ = 5f;

    [Header("Optional FOV Control")]
    public bool useZoom = false;
    public float defaultFOV = 60f;
    public float zoomFOV = 50f;
    public float zoomSpeed = 2f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam && useZoom)
            cam.fieldOfView = defaultFOV;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // --- Follow Logic ---
        Vector3 desiredPos = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

        // --- Clamp Camera Movement ---
        smoothedPos.x = Mathf.Clamp(smoothedPos.x, minX, maxX);
        smoothedPos.z = Mathf.Clamp(smoothedPos.z, minZ, maxZ);

        transform.position = smoothedPos;
    }
}
