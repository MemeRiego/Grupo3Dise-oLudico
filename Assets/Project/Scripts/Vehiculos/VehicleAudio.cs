using UnityEngine;

public class VehicleAudio : MonoBehaviour
{
    [SerializeField] private AudioSource engineSource;
    [SerializeField] private AudioSource hornSource;
    [SerializeField] private AudioSource effectsSource;

    [SerializeField] private AudioClip brakeClip;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;
    [SerializeField] private AudioClip impactClip;
    [SerializeField] private AudioClip heavyImpactClip;
    [SerializeField, Range(0f, 1f)] private float impactVolume = 0.45f;
    [SerializeField, Min(0.1f)] private float heavyImpactSpeed = 4f;

    [Tooltip("Tono del motor cuando está regulando.")]
    [SerializeField] private float idlePitch = 0.8f;

    [Tooltip("Tono del motor a velocidad máxima.")]
    [SerializeField] private float maxPitch = 1.6f;

    [Tooltip("Cuánto sube el tono al pisar el acelerador.")]
    [SerializeField] private float throttlePitch = 0.15f;

    [SerializeField, Min(0f)] private float pitchSpeed = 1f;
    [SerializeField, Min(0f)] private float minBrakeSpeed = 1f;
    [SerializeField, Min(0f)] private float brakeCooldown = 1f;

    private VehicleDamage damage;
    private Rigidbody2D rb;
    private VehicleController vehicle;
    private bool wasBraking;
    private float nextBrakeTime;

    private void Awake()
    {
        damage = GetComponent<VehicleDamage>();
        rb = GetComponent<Rigidbody2D>();
        vehicle = GetComponent<VehicleController>();

        if (engineSource == null || hornSource == null || effectsSource == null)
        {
            Debug.LogError("Falta asignar los AudioSource del vehículo.", this);
            enabled = false;
            return;
        }

        engineSource.loop = true;
        hornSource.loop = true;
        effectsSource.loop = false;
        engineSource.pitch = idlePitch;
    }

    private void OnEnable()
    {
        if (damage != null) damage.Impact += PlayImpact;
    }

    private void PlayImpact(float speed)
    {
        AudioClip clip = speed >= heavyImpactSpeed && heavyImpactClip != null ? heavyImpactClip : impactClip;
        float volume = impactVolume * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(speed / heavyImpactSpeed));
        PlayEffect(clip, volume);
    }

    private void Start()
    {
        if (vehicle.enabled && engineSource.clip != null)
            engineSource.Play();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (!vehicle.enabled)
        {
            if (engineSource.isPlaying)
                engineSource.Stop();

            SetHorn(false);
            return;
        }

        float speed = rb.linearVelocity.magnitude;
        float amount = Mathf.Clamp01(speed / Mathf.Max(vehicle.MaxSpeed, 0.1f));
        float targetPitch = Mathf.Lerp(idlePitch, maxPitch, amount);

        if (!vehicle.IsBraking)
            targetPitch += Mathf.Abs(vehicle.ThrottleInput) * throttlePitch;

        engineSource.pitch = Mathf.MoveTowards(engineSource.pitch, targetPitch, pitchSpeed * Time.deltaTime);

        bool braking = vehicle.IsBraking && speed > minBrakeSpeed;

        if (braking && !wasBraking && Time.time >= nextBrakeTime)
        {
            PlayEffect(brakeClip);
            nextBrakeTime = Time.time + brakeCooldown;
        }

        wasBraking = braking;

        if (!engineSource.isPlaying && engineSource.clip != null)
            engineSource.Play();
    }

    public void SetHorn(bool active)
    {
        if (hornSource == null) return;

        if (!active)
        {
            hornSource.Stop();
            return;
        }

        if (!isActiveAndEnabled || !vehicle.enabled || Time.timeScale == 0f) return;

        if (hornSource.clip != null && !hornSource.isPlaying)
            hornSource.Play();
    }

    public void PlayDoorOpen()
    {
        PlayEffect(doorOpenClip);
    }

    public void PlayDoorClose()
    {
        PlayEffect(doorCloseClip);
    }

    private void PlayEffect(AudioClip clip, float volume = 1f)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f) return;

        if (clip != null)
            effectsSource.PlayOneShot(clip, volume);
    }

    private void OnDisable()
    {
        if (damage != null) damage.Impact -= PlayImpact;
        if (engineSource != null) engineSource.Stop();
        if (hornSource != null) hornSource.Stop();
        if (effectsSource != null) effectsSource.Stop();

        wasBraking = false;
    }
}
