using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class VehicleDelivery : MonoBehaviour
{
    [Header("Locations")]
    [SerializeField] private Transform pickupPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private float interactionDistance = 2f;

    [Header("Payment")]
    [SerializeField] private int deliveryReward = 50;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text instructionText;

    private bool hasPackage;
    private int money;

    private void Update()
    {
        bool nearPickup = IsNear(pickupPoint);
        bool nearDelivery = IsNear(deliveryPoint);

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.eKey.wasPressedThisFrame)
        {
            if (!hasPackage && nearPickup)
            {
                hasPackage = true;
                Debug.Log("Package loaded!");
            }
            else if (hasPackage && nearDelivery)
            {
                hasPackage = false;
                money += deliveryReward;

                Debug.Log($"Delivery complete! Earned ${deliveryReward}.");
            }
        }

        UpdateUI(nearPickup, nearDelivery);
    }

    private bool IsNear(Transform point)
    {
        if (point == null)
            return false;

        float distance = Vector2.Distance(
            transform.position,
            point.position
        );

        return distance <= interactionDistance;
    }

    private void UpdateUI(bool nearPickup, bool nearDelivery)
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${money}";
        }

        if (instructionText == null)
            return;

        if (hasPackage)
        {
            instructionText.text = nearDelivery
                ? $"Press E to deliver (+${deliveryReward})"
                : "Cargo: 1 package. Drive to the delivery point.";
        }
        else
        {
            instructionText.text = nearPickup
                ? "Press E to load a package"
                : "Cargo: empty. Drive to the pickup point.";
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Interaction area in Scene view (gizmos)
        Gizmos.color = Color.green;

        if (pickupPoint != null)
        {
            Gizmos.DrawWireSphere(
                pickupPoint.position,
                interactionDistance
            );
        }

        Gizmos.color = Color.blue;

        if (deliveryPoint != null)
        {
            Gizmos.DrawWireSphere(
                deliveryPoint.position,
                interactionDistance
            );
        }
    }
}