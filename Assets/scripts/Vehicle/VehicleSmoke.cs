using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleSmoke : MonoBehaviour
{
    [SerializeField] private SpriteRenderer vehicleSprite;
    [SerializeField] private ParticleSystem smoke;

    [SerializeField] private float minimumSpeed = 0.1f;

    private Rigidbody2D rb;
    private Vector3 rightFacingPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (vehicleSprite == null || smoke == null)
        {
            Debug.LogError("Assign the Vehicle Smoke references.", this);
            enabled = false;
            return;
        }

        // Set up the exhaust position with the sprite facing right.
        rightFacingPosition = smoke.transform.localPosition;

        var main = smoke.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        smoke.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    private void LateUpdate()
    {
        // Mirror the exhaust position when the sprite faces left.
        Vector3 position = rightFacingPosition;

        if (vehicleSprite.flipX)
            position.x = -position.x;

        smoke.transform.localPosition = position;

        bool moving = rb.linearVelocity.sqrMagnitude >
            minimumSpeed * minimumSpeed;

        if (moving && !smoke.isEmitting)
        {
            smoke.Play();
        }
        else if (!moving && smoke.isEmitting)
        {
            // Existing smoke fades naturally.
            smoke.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }
    }

    private void OnDisable()
    {
        if (smoke != null)
        {
            smoke.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}