using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class VehicleDelivery : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WarehouseSceneController warehouseController;
    [SerializeField] private Transform pickupPoint;
    [SerializeField] private Transform deliveryPoint;

    [Header("Delivery")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private int rewardPerPackage = 50;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text instructionText;

    private int money;

    private void Update()
    {
        if (warehouseController == null ||
            warehouseController.InWarehouse)
        {
            return;
        }

        TrailerCargo cargo = warehouseController.Cargo;

        int packageCount = cargo != null ? cargo.CargoCount : 0;
        float cargoWeight = cargo != null ? cargo.CargoWeight : 0f;

        bool nearPickup = IsNear(pickupPoint);
        bool nearDelivery = IsNear(deliveryPoint);

        Keyboard keyboard = Keyboard.current;

        // Pickup interaction is handled by the scene controller.
        if (!nearPickup && nearDelivery && packageCount > 0 &&
            keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            int deliveredCount = cargo.UnloadCargo();
            int payment = deliveredCount * rewardPerPackage;

            money += payment;

            Debug.Log(
                $"Delivered {deliveredCount} packages. Earned ${payment}."
            );

            packageCount = cargo.CargoCount;
            cargoWeight = cargo.CargoWeight;
        }

        if (moneyText != null)
            moneyText.text = $"${money}";

        if (instructionText == null)
            return;

        string cargoSummary =
            $"Cargo: {packageCount} packages | {cargoWeight:0.0} kg";

        if (nearPickup)
        {
            instructionText.text =
                $"{cargoSummary}\nPress E to enter the warehouse";
        }
        else if (nearDelivery && packageCount > 0)
        {
            int payment = packageCount * rewardPerPackage;

            instructionText.text =
                $"{cargoSummary}\nPress E to deliver (+${payment})";
        }
        else if (packageCount == 0)
        {
            instructionText.text =
                "Cargo: empty. Visit the warehouse to load packages.";
        }
        else
        {
            instructionText.text =
                $"{cargoSummary}\nDrive to the delivery point.";
        }
    }

    private bool IsNear(Transform point)
    {
        if (point == null)
            return false;

        return Vector2.Distance(transform.position, point.position)
            <= interactionDistance;
    }
}