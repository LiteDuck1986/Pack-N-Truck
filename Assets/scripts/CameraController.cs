using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.15f;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 1f;
    [SerializeField] private float maxZoom = 1.5f;
    [SerializeField] private float zoomStep = 0.1f;
    [SerializeField] private float zoomSmoothTime = 0.15f;

    [Header("UI")]
    [SerializeField] private TMP_Text zoomText;

    private Camera cam;
    private float startingSize;
    private float zoom = 1f;
    private float cameraZ;

    private Vector3 followVelocity;
    private float zoomVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        startingSize = cam.orthographicSize;
        cameraZ = transform.position.z;

        zoom = Mathf.Clamp(1f, minZoom, maxZoom);
        cam.orthographicSize = startingSize / zoom;

        UpdateZoomText();
    }

    private void Start()
    {
        // Begin centered on the vehicle.
        if (target != null)
        {
            transform.position = new Vector3(
                target.position.x,
                target.position.y,
                cameraZ
            );
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll == 0f)
            return;

        // Each scroll input changes zoom by one step.
        zoom += Mathf.Sign(scroll) * zoomStep;
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

        UpdateZoomText();
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 targetPosition = new Vector3(
                target.position.x,
                target.position.y,
                cameraZ
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref followVelocity,
                followSmoothTime
            );
        }

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            startingSize / zoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    private void UpdateZoomText()
    {
        if (zoomText != null)
        {
            zoomText.text = $"{zoom:0.0}x";
        }
    }
}