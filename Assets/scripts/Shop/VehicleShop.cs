using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class VehicleShop : MonoBehaviour
{
    [Header("Shop Location")]
    [SerializeField] private Transform shopPoint;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private TMP_Text shopPrompt;

    [Header("Vehicle")]
    [SerializeField] private VehicleController vehicleController;
    [SerializeField] private SpriteRenderer vehicleSprite;
    [SerializeField] private BoxCollider2D vehicleCollider;
    [SerializeField] private VehicleSmoke vehicleSmoke;
    [SerializeField] private VehicleAudio vehicleAudio;

    [Header("Game Systems")]
    [SerializeField] private VehicleDelivery wallet;
    [SerializeField] private WarehouseSceneController warehouseController;

    [Header("Catalogue")]
    [Tooltip("Element 0 is the starting vehicle and is already owned.")]
    [SerializeField] private VehicleDefinition[] vehicles;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Image vehiclePreview;
    [SerializeField] private TMP_Text vehicleName;
    [SerializeField] private TMP_Text vehicleStats;
    [SerializeField] private TMP_Text shopMoney;
    [SerializeField] private TMP_Text shopStatus;
    [SerializeField] private Button actionButton;
    [SerializeField] private TMP_Text actionButtonText;

    public bool IsOpen { get; private set; }
    public VehicleDefinition EquippedVehicle { get; private set; }

    private readonly HashSet<VehicleDefinition> ownedVehicles =
        new HashSet<VehicleDefinition>();

    private Rigidbody2D vehicleBody;
    private int selectedIndex;
    private bool drivingWasEnabled;

    private void Awake()
    {
        if (shopPoint == null || vehicleController == null ||
            vehicleSprite == null || vehicleCollider == null ||
            wallet == null || warehouseController == null ||
            shopPanel == null || vehiclePreview == null ||
            vehicleName == null || vehicleStats == null ||
            shopMoney == null || shopStatus == null ||
            actionButton == null || actionButtonText == null ||
            vehicles == null || vehicles.Length == 0)
        {
            Debug.LogError("Assign the Vehicle Shop references.", this);
            enabled = false;
            return;
        }

        foreach (VehicleDefinition vehicle in vehicles)
        {
            if (vehicle == null || vehicle.sprite == null)
            {
                Debug.LogError(
                    "Every catalogue vehicle needs a data asset and sprite.",
                    this
                );

                enabled = false;
                return;
            }
        }

        vehicleBody = vehicleController.GetComponent<Rigidbody2D>();
        shopPanel.SetActive(false);
    }

    private void Start()
    {
        ownedVehicles.Add(vehicles[0]);
        ApplyVehicle(vehicles[0]);
    }

    private void Update()
    {
        if (warehouseController.InWarehouse)
            return;

        bool nearShop = Vector2.Distance(
            vehicleController.transform.position,
            shopPoint.position
        ) <= interactionDistance;

        if (shopPrompt != null)
        {
            shopPrompt.text = nearShop && !IsOpen
                ? "Press E to open the vehicle shop"
                : "";
        }

        Keyboard keyboard = Keyboard.current;

        if (IsOpen)
        {
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                CloseShop();

            return;
        }

        if (nearShop && keyboard != null &&
            keyboard.eKey.wasPressedThisFrame)
        {
            OpenShop();
        }
    }

    private void LateUpdate()
    {
        if (EquippedVehicle == null)
            return;

        Vector2 offset = EquippedVehicle.colliderOffset;

        if (vehicleSprite.flipX)
            offset.x = -offset.x;

        vehicleCollider.offset = offset;
    }

    public void OpenShop()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        drivingWasEnabled = vehicleController.enabled;
        vehicleController.enabled = false;

        vehicleBody.linearVelocity = Vector2.zero;
        vehicleBody.angularVelocity = 0f;

        shopPanel.SetActive(true);
        RefreshView();
    }

    public void CloseShop()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        shopPanel.SetActive(false);
        vehicleController.enabled = drivingWasEnabled;
    }

    public void PreviousVehicle()
    {
        selectedIndex =
            (selectedIndex - 1 + vehicles.Length) % vehicles.Length;

        RefreshView();
    }

    public void NextVehicle()
    {
        selectedIndex = (selectedIndex + 1) % vehicles.Length;
        RefreshView();
    }

    public void PurchaseOrEquip()
    {
        if (!IsOpen)
            return;

        VehicleDefinition selected = vehicles[selectedIndex];

        if (!ownedVehicles.Contains(selected))
        {
            if (wallet.TrySpend(selected.price))
                ownedVehicles.Add(selected);

            RefreshView();
            return;
        }

        if (GetLoadedWeight() <= selected.maxCargoWeight)
            ApplyVehicle(selected);

        RefreshView();
    }

    private float GetLoadedWeight()
    {
        TrailerCargo cargo = warehouseController.Cargo;
        return cargo != null ? cargo.CargoWeight : 0f;
    }

    private void ApplyVehicle(VehicleDefinition vehicle)
    {
        EquippedVehicle = vehicle;

        vehicleSprite.sprite = vehicle.sprite;
        vehicleController.SetMoveSpeed(vehicle.moveSpeed);
        vehicleCollider.size = vehicle.colliderSize;

        if (vehicleSmoke != null)
            vehicleSmoke.SetExhaustPosition(vehicle.exhaustPosition);

        if (vehicleAudio != null)
            vehicleAudio.SetReferenceSpeed(vehicle.moveSpeed);

        TrailerCargo cargo = warehouseController.Cargo;

        if (cargo != null)
            cargo.SetMaxCargoWeight(vehicle.maxCargoWeight);
    }

    private void RefreshView()
    {
        VehicleDefinition selected = vehicles[selectedIndex];

        bool owned = ownedVehicles.Contains(selected);
        bool equipped = selected == EquippedVehicle;
        bool fitsCargo = GetLoadedWeight() <= selected.maxCargoWeight;
        bool affordable = wallet.Money >= selected.price;

        vehiclePreview.sprite = selected.sprite;
        vehicleName.text = selected.displayName;

        vehicleStats.text =
            $"Speed: {selected.moveSpeed:0.0}\n" +
            $"Cargo capacity: {selected.maxCargoWeight:0.0} kg";

        shopMoney.text = $"Money: ${wallet.Money}";

        actionButtonText.text = !owned
            ? $"Purchase ${selected.price}"
            : equipped ? "Equipped" : "Equip";

        actionButton.interactable = !equipped &&
            (owned ? fitsCargo : affordable);

        if (equipped)
            shopStatus.text = "Currently equipped.";
        else if (owned && !fitsCargo)
            shopStatus.text = "Unload cargo before equipping this truck.";
        else if (owned)
            shopStatus.text = "Owned. You can equip this truck.";
        else
            shopStatus.text = affordable
                ? "Available to purchase."
                : "Not enough money.";
    }
}