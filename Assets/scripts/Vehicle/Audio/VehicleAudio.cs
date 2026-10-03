using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource idleSource;
    [SerializeField] private AudioSource driveSource;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float idleVolume = 0.4f;
    [SerializeField, Range(0f, 1f)] private float driveVolume = 0.5f;

    [Header("Transition")]
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.3f;
    [SerializeField, Min(0f)] private float movingThreshold = 0.1f;

    [Header("Driving Pitch")]
    [SerializeField, Min(0.01f)] private float referenceSpeed = 5f;
    [SerializeField, Range(0.5f, 2f)] private float lowPitch = 0.9f;
    [SerializeField, Range(0.5f, 2f)] private float highPitch = 1.15f;

    private Rigidbody2D rb;
    private float driveBlend;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (idleSource == null || driveSource == null ||
            idleSource == driveSource ||
            idleSource.clip == null || driveSource.clip == null)
        {
            Debug.LogError(
                "Assign two different audio sources with engine clips.",
                this
            );

            enabled = false;
            return;
        }

        ConfigureSource(idleSource);
        ConfigureSource(driveSource);
    }

    private void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }

    private void OnEnable()
    {
        if (idleSource == null || driveSource == null)
            return;

        driveBlend = 0f;

        idleSource.volume = idleVolume;
        idleSource.pitch = 1f;

        driveSource.volume = 0f;
        driveSource.pitch = lowPitch;

        idleSource.Play();
        driveSource.Play();
    }

    private void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        float targetBlend = speed > movingThreshold ? 1f : 0f;

        driveBlend = Mathf.MoveTowards(
            driveBlend,
            targetBlend,
            Time.deltaTime / fadeDuration
        );

        idleSource.volume = idleVolume * (1f - driveBlend);
        driveSource.volume = driveVolume * driveBlend;

        float speedRatio = Mathf.Clamp01(speed / referenceSpeed);
        float targetPitch = Mathf.Lerp(
            lowPitch,
            highPitch,
            speedRatio
        );

        driveSource.pitch = Mathf.MoveTowards(
            driveSource.pitch,
            targetPitch,
            Time.deltaTime
        );
    }

    private void OnDisable()
    {
        if (idleSource != null)
            idleSource.Stop();

        if (driveSource != null)
            driveSource.Stop();
    }
}