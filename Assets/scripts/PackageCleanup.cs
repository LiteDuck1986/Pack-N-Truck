using UnityEngine;

public class PackageCleanup : MonoBehaviour
{
    [SerializeField] private BoxCollider2D safetyArea;
    [SerializeField] private Transform packagesRoot;
    [SerializeField] private PackageDragController dragController;

    private void Awake()
    {
        if (safetyArea == null || packagesRoot == null ||
            dragController == null)
        {
            Debug.LogError(
                "Assign all Package Cleanup references.",
                this
            );

            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!safetyArea.enabled)
            return;

        Bounds safeBounds = safetyArea.bounds;

        Rigidbody2D[] packages =
            packagesRoot.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D body in packages)
        {
            // This doesnt delete package if its held by the player
            if (body == dragController.HeldBody)
                continue;

            BoxCollider2D box = body.GetComponent<BoxCollider2D>();

            if (box == null || !box.enabled)
                continue;

            Bounds packageBounds = box.bounds;

            bool completelyOutside =
                packageBounds.max.x < safeBounds.min.x ||
                packageBounds.min.x > safeBounds.max.x ||
                packageBounds.max.y < safeBounds.min.y ||
                packageBounds.min.y > safeBounds.max.y;

            if (!completelyOutside)
                continue;

            // Removes it immediately from the scene
            body.gameObject.SetActive(false);
            Destroy(body.gameObject);
        }
    }
}