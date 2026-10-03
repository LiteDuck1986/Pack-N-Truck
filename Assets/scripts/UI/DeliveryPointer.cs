using UnityEngine;

// Run after the camera's normal LateUpdate.
[DefaultExecutionOrder(100)]
public class DeliveryPointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private WarehouseSceneController warehouseController;

    [Header("Appearance")]
    [SerializeField, Min(0f)] private float edgePadding = 20f;

    [Tooltip("-90 for a sprite pointing up; 0 for one pointing right.")]
    [SerializeField] private float rotationOffset = -90f;

    private RectTransform canvasRect;

    private void Awake()
    {
        if (worldCamera == null || uiCanvas == null ||
            arrow == null || deliveryPoint == null ||
            warehouseController == null)
        {
            Debug.LogError("Assign all Delivery Pointer references.", this);
            enabled = false;
            return;
        }

        if (uiCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Debug.LogError(
                "Delivery Pointer requires a Screen Space - Overlay Canvas.",
                this
            );

            enabled = false;
            return;
        }

        canvasRect = uiCanvas.GetComponent<RectTransform>();

        arrow.anchorMin = new Vector2(0.5f, 0.5f);
        arrow.anchorMax = new Vector2(0.5f, 0.5f);
        arrow.pivot = new Vector2(0.5f, 0.5f);

        arrow.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        TrailerCargo cargo = warehouseController.Cargo;

        bool hasDelivery = !warehouseController.InWarehouse &&
            cargo != null &&
            cargo.CargoCount > 0;

        if (!hasDelivery)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        Vector3 screenPosition =
            worldCamera.WorldToScreenPoint(deliveryPoint.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            new Vector2(screenPosition.x, screenPosition.y),
            null,
            out Vector2 targetPosition
        );

        Rect bounds = canvasRect.rect;

        // hide the pointer when the destination is visible
        if (screenPosition.z > 0f && bounds.Contains(targetPosition))
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        Vector2 direction = targetPosition - bounds.center;

        if (screenPosition.z <= 0f)
            direction = -direction;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.up;

        float arrowRadius = arrow.rect.size.magnitude * 0.5f;

        float halfWidth = Mathf.Max(
            1f,
            bounds.width * 0.5f - edgePadding - arrowRadius
        );

        float halfHeight = Mathf.Max(
            1f,
            bounds.height * 0.5f - edgePadding - arrowRadius
        );

        float horizontalDistance = Mathf.Abs(direction.x) > 0.0001f
            ? halfWidth / Mathf.Abs(direction.x)
            : float.PositiveInfinity;

        float verticalDistance = Mathf.Abs(direction.y) > 0.0001f
            ? halfHeight / Mathf.Abs(direction.y)
            : float.PositiveInfinity;

        float distance = Mathf.Min(
            horizontalDistance,
            verticalDistance
        );

        arrow.anchoredPosition = direction * distance;

        float angle = Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg;

        arrow.localRotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffset
        );

        arrow.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (arrow != null)
            arrow.gameObject.SetActive(false);
    }
}