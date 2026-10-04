using UnityEngine;

[CreateAssetMenu(menuName = "Pack N Truck/Vehicle")]
public class VehicleDefinition : ScriptableObject
{
    public string displayName = "Starter Truck";

    [Min(0)] public int price;
    public Sprite sprite;

    [Min(0.1f)] public float moveSpeed = 5f;
    [Min(0.1f)] public float maxCargoWeight = 10f;

    [Header("Vehicle Collider")]
    public Vector2 colliderSize = new Vector2(2f, 1f);
    public Vector2 colliderOffset;

    [Header("Exhaust")]
    [Tooltip("Local position under Visual when the sprite faces right.")]
    public Vector3 exhaustPosition = new Vector3(-1f, -0.2f, 0f);
}