using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class PackageDragController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera worldCamera;
    [SerializeField] private LayerMask packageLayers;

    [Header("Spring")]
    [SerializeField] private float frequency = 4f;
    [SerializeField, Range(0f, 1f)] private float dampingRatio = 0.7f;
    [SerializeField] private float maxForce = 150f;

    [Header("Cursor")]
    [SerializeField] private Texture2D draggingCursor;
    [SerializeField] private Vector2 draggingHotspot = new Vector2(16f, 16f);

    private Rigidbody2D heldBody;
    private TargetJoint2D dragJoint;
    private LineRenderer dragLine;
    public Rigidbody2D HeldBody => heldBody;
    public bool IsDragging => heldBody != null;

    private void Awake()
    {
        dragLine = GetComponent<LineRenderer>();
        dragLine.useWorldSpace = true;
        dragLine.positionCount = 2;
        dragLine.enabled = false;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null || worldCamera == null)
        {
            ReleasePackage();
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            bool overUI = EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject();

            if (!overUI)
                GrabPackage();
        }

        if (heldBody != null && !mouse.leftButton.isPressed)
            ReleasePackage();
    }

    private void FixedUpdate()
    {
        if (dragJoint != null && Mouse.current != null)
            dragJoint.target = GetCursorPosition();
    }

    private void LateUpdate()
    {
        bool dragging = heldBody != null && dragJoint != null;
        dragLine.enabled = dragging;

        if (!dragging)
            return;

        Vector3 grabPosition =
            heldBody.transform.TransformPoint(dragJoint.anchor);

        Vector2 cursor = GetCursorPosition();

        dragLine.SetPosition(0, grabPosition);
        dragLine.SetPosition(
            1,
            new Vector3(cursor.x, cursor.y, grabPosition.z)
        );
    }

    private Vector2 GetCursorPosition()
    {
        Vector2 screenPosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                -worldCamera.transform.position.z
            )
        );

        return new Vector2(worldPosition.x, worldPosition.y);
    }

    private void GrabPackage()
    {
        if (heldBody != null)
            return;

        Vector2 cursor = GetCursorPosition();
        Collider2D hit = Physics2D.OverlapPoint(cursor, packageLayers);

        if (hit == null)
            return;

        Rigidbody2D body = hit.attachedRigidbody;

        if (body == null || body.bodyType != RigidbodyType2D.Dynamic)
            return;

        heldBody = body;

        dragJoint = body.gameObject.AddComponent<TargetJoint2D>();
        dragJoint.autoConfigureTarget = false;
        dragJoint.anchor = body.transform.InverseTransformPoint(cursor);
        dragJoint.target = cursor;
        dragJoint.frequency = frequency;
        dragJoint.dampingRatio = dampingRatio;
        dragJoint.maxForce = maxForce;

        body.WakeUp();

        if (draggingCursor != null)
        {
            Cursor.SetCursor(
                draggingCursor,
                draggingHotspot,
                CursorMode.Auto
            );
        }
    }

    private void ReleasePackage()
    {
        if (dragJoint != null)
        {
            dragJoint.enabled = false;
            Destroy(dragJoint);
        }

        heldBody = null;
        dragJoint = null;

        if (dragLine != null)
            dragLine.enabled = false;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        ReleasePackage();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleasePackage();
    }
}