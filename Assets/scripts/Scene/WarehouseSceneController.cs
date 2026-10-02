using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class WarehouseSceneController : MonoBehaviour
{
    [Header("Overworld")]
    [SerializeField] private GameObject overworldRoot;
    [SerializeField] private Transform vehicle;
    [SerializeField] private Transform pickupPoint;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private TMP_Text interactionText;

    [Header("Warehouse")]
    [SerializeField] private string warehouseSceneName = "Warehouse";

    private GameObject warehouseRoot;
    private bool inWarehouse;
    private bool switchingScenes;

    private void Update()
    {
        if (switchingScenes)
            return;

        Keyboard keyboard = Keyboard.current;

        if (inWarehouse)
        {
            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame)
            {
                ReturnToOverworld();
            }

            return;
        }

        if (vehicle == null || pickupPoint == null)
            return;

        bool nearPickup = Vector2.Distance(
            vehicle.position,
            pickupPoint.position
        ) <= interactionDistance;

        if (interactionText != null)
        {
            interactionText.text = nearPickup
                ? "Press E to enter the warehouse"
                : "Drive to the pickup point";
        }

        if (nearPickup && keyboard != null &&
            keyboard.eKey.wasPressedThisFrame)
        {
            StartCoroutine(EnterWarehouse());
        }
    }

    private IEnumerator EnterWarehouse()
    {
        switchingScenes = true;

        Rigidbody2D vehicleBody = vehicle.GetComponent<Rigidbody2D>();

        if (vehicleBody != null)
        {
            vehicleBody.linearVelocity = Vector2.zero;
            vehicleBody.angularVelocity = 0f;
        }

        overworldRoot.SetActive(false);

        if (warehouseRoot == null)
        {
            Scene scene = SceneManager.GetSceneByName(
                warehouseSceneName
            );

            if (!scene.isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(
                    warehouseSceneName,
                    LoadSceneMode.Additive
                );

                scene = SceneManager.GetSceneByName(
                    warehouseSceneName
                );
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == "WarehouseRoot")
                {
                    warehouseRoot = root;
                    break;
                }
            }
        }

        if (warehouseRoot == null)
        {
            Debug.LogError(
                "Warehouse scene needs a root named WarehouseRoot."
            );

            overworldRoot.SetActive(true);
            switchingScenes = false;
            yield break;
        }

        warehouseRoot.SetActive(true);
        inWarehouse = true;
        switchingScenes = false;
    }

    private void ReturnToOverworld()
    {
        warehouseRoot.SetActive(false);
        overworldRoot.SetActive(true);
        inWarehouse = false;
    }
}