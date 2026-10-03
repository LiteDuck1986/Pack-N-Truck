using UnityEngine;
using TMPro;

[RequireComponent(typeof(SurfaceEffector2D))]
public class ConveyorSupply : MonoBehaviour
{
    [Header("Package Supply")]
    [SerializeField] private GameObject packagePrefab;
    [SerializeField] private Transform packagesRoot;
    [SerializeField] private Transform spawnPoint;

    [SerializeField, Min(0.1f)]
    private float spawnInterval = 2f;

    [Header("Conveyor")]
    [SerializeField] private float beltSpeed = 1.5f;

    [Header("Sensors")]
    [SerializeField] private BoxCollider2D spawnCheck;
    [SerializeField] private BoxCollider2D endCheck;
    [SerializeField] private LayerMask packageLayers;

    [Header("Warehouse Supply Limit")]
    [SerializeField, Min(1)]
    private int maxPackagesInWarehouse = 20;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text supplyText;

    private SurfaceEffector2D surface;
    private float spawnTimer;

    private void Awake()
    {
        surface = GetComponent<SurfaceEffector2D>();

        if (packagePrefab == null || packagesRoot == null ||
            spawnPoint == null || spawnCheck == null ||
            endCheck == null || packageLayers.value == 0)
        {
            Debug.LogError(
                "Assign all Conveyor Supply references and package layers.",
                this
            );

            enabled = false;
            return;
        }

        // The first package can spawn immediately.
        spawnTimer = spawnInterval;
    }

    private void FixedUpdate()
    {
        bool endOccupied = IsOccupied(endCheck);

        // A package waiting at the end pauses the belt.
        surface.speed = endOccupied ? 0f : beltSpeed;

        if (endOccupied)
        {
            SetStatus("Conveyor paused: collect the package.");
            return;
        }

        // Includes packages on the belt, floor, and in the trailer.
        if (packagesRoot.childCount >= maxPackagesInWarehouse)
        {
            SetStatus("Supply paused: deliver some packages.");
            return;
        }

        spawnTimer = Mathf.Min(
            spawnTimer + Time.fixedDeltaTime,
            spawnInterval
        );

        if (IsOccupied(spawnCheck))
        {
            SetStatus("Supply waiting for spawn space.");
            return;
        }

        SetStatus("Conveyor running.");

        if (spawnTimer < spawnInterval)
            return;

        Instantiate(
            packagePrefab,
            spawnPoint.position,
            Quaternion.identity,
            packagesRoot
        );

        spawnTimer = 0f;
    }

    private bool IsOccupied(BoxCollider2D sensor)
    {
        // Sensors must remain unrotated.
        Bounds area = sensor.bounds;

        Collider2D package = Physics2D.OverlapBox(
            new Vector2(area.center.x, area.center.y),
            new Vector2(area.size.x, area.size.y),
            0f,
            packageLayers
        );

        return package != null;
    }

    private void SetStatus(string message)
    {
        if (supplyText != null)
            supplyText.text = message;
    }

    private void OnDisable()
    {
        if (surface != null)
            surface.speed = 0f;
    }
}