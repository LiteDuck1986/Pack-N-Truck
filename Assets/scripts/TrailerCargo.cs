using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TrailerCargo : MonoBehaviour
{
    [Header("Storage")]
    [SerializeField] private BoxCollider2D storageArea;
    [SerializeField] private Transform packagesRoot;
    [SerializeField] private PackageDragController dragController;

    [Header("Truck Capacity")]
    [SerializeField, Min(0.1f)] private float maxCargoWeight = 10f;

    public float MaxCargoWeight => maxCargoWeight;

    [Header("UI")]
    [SerializeField] private TMP_Text cargoText;
    [SerializeField] private TMP_Text loadingStatusText;

    [Header("Boundary Check")]
    [SerializeField, Min(0f)] private float tolerance = 0.01f;

    public int CargoCount { get; private set; }
    public float CargoWeight { get; private set; }

    private int protrudingPackages;
    private bool setupValid;
    private readonly List<Rigidbody2D> loadedPackages =
    new List<Rigidbody2D>();

    private void LateUpdate()
    {
        RefreshCargo();
    }

    public bool CanLeave()
    {
        RefreshCargo();

        return setupValid &&
            !dragController.IsDragging &&
            protrudingPackages == 0 &&
            CargoWeight <= maxCargoWeight;
    }

    public void SetMaxCargoWeight(float weight)
    {
        maxCargoWeight = Mathf.Max(0.1f, weight);

        if (gameObject.activeInHierarchy)
            RefreshCargo();
    }

    private void RefreshCargo()
    {
        loadedPackages.Clear();

        CargoCount = 0;
        CargoWeight = 0f;
        protrudingPackages = 0;

        setupValid = storageArea != null &&
            storageArea.enabled &&
            packagesRoot != null &&
            dragController != null;

        if (!setupValid)
        {
            SetStatus("Assign the Trailer Cargo references.");
            return;
        }

        Bounds area = storageArea.bounds;

        Rigidbody2D[] packages =
            packagesRoot.GetComponentsInChildren<Rigidbody2D>();

        foreach (Rigidbody2D body in packages)
        {
            BoxCollider2D box = body.GetComponent<BoxCollider2D>();

            if (box == null || !box.enabled || !body.simulated)
                continue;

            Bounds packageBounds = box.bounds;
            bool fullyInside = IsFullyInside(packageBounds, area);

            if (fullyInside)
            {
                // Held packages do not count as loaded.
                if (body != dragController.HeldBody)
                {
                    loadedPackages.Add(body);
                    CargoCount++;
                    CargoWeight += body.mass;
                }
            }
            else if (OverlapsStorage(packageBounds, area))
            {
                protrudingPackages++;
            }
        }

        if (cargoText != null)
        {
            cargoText.text =
                $"Cargo: {CargoCount} packages | " +
                $"{CargoWeight:0.00} / {maxCargoWeight:0.00} kg";
        }

        if (dragController.IsDragging)
        {
            SetStatus("Release the package before leaving.");
        }
        else if (protrudingPackages > 0)
        {
            SetStatus(
                "A package sticks out. Move it fully inside or outside."
            );
        }
        else if (CargoWeight > maxCargoWeight)
        {
            float excessWeight = CargoWeight - maxCargoWeight;

            SetStatus(
                $"Truck overweight by {excessWeight:0.00} kg. Remove some cargo."
            );
        }
        else
        {
            SetStatus("Ready to leave. Press Esc to return.");
        }
    }

    public int UnloadCargo()
    {
        int deliveredCount = 0;

        foreach (Rigidbody2D package in loadedPackages)
        {
            if (package == null)
                continue;

            package.gameObject.SetActive(false);
            Destroy(package.gameObject);
            deliveredCount++;
        }

        loadedPackages.Clear();

        CargoCount = 0;
        CargoWeight = 0f;

        if (cargoText != null)
        {
            cargoText.text =
                $"Cargo: 0 packages | 0.00 / {maxCargoWeight:0.00} kg";
        }

        SetStatus("Ready to leave. Press Esc to return.");

        return deliveredCount;
    }

    private bool IsFullyInside(Bounds package, Bounds area)
    {
        return package.min.x >= area.min.x - tolerance &&
            package.max.x <= area.max.x + tolerance &&
            package.min.y >= area.min.y - tolerance &&
            package.max.y <= area.max.y + tolerance;
    }

    private bool OverlapsStorage(Bounds package, Bounds area)
    {
        float overlapX =
            Mathf.Min(package.max.x, area.max.x) -
            Mathf.Max(package.min.x, area.min.x);

        float overlapY =
            Mathf.Min(package.max.y, area.max.y) -
            Mathf.Max(package.min.y, area.min.y);

        return overlapX > tolerance && overlapY > tolerance;
    }

    private void SetStatus(string message)
    {
        if (loadingStatusText != null)
            loadingStatusText.text = message;
    }
}