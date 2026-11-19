using UnityEngine;

public class TrafficAudio : MonoBehaviour
{
    [SerializeField] private AudioSource engine;
    [SerializeField] private AudioSource horn;
    [SerializeField, Range(0f, 1f)] private float engineVolume = 0.16f;
    [SerializeField, Range(0f, 1f)] private float hornVolume = 0.25f;
    [SerializeField, Min(1f)] private float audibleDistance = 18f;

    private TrafficAgent agent;
    private Rigidbody2D body;
    private Transform listener;
    private float hornTime;
    private bool honked;
    private float idlePitch;

    private void Awake()
    {
        agent = GetComponent<TrafficAgent>();
        body = GetComponent<Rigidbody2D>();
        idlePitch = Random.Range(0.85f, 0.95f);
    }

    private void Start()
    {
        var camera = Camera.main;
        if (camera != null) listener = camera.transform;
    }

    private void Update()
    {
        if (Time.timeScale == 0f || listener == null) return;

        Vector2 offset = transform.position - listener.position;
        float speed = body.linearVelocity.magnitude;
        if (speed > 0.3f) honked = false;

        if (!agent.isActiveAndEnabled || offset.sqrMagnitude >= audibleDistance * audibleDistance)
        {
            StopSounds();
            return;
        }

        float gain = 1f - offset.magnitude / audibleDistance;
        float pan = Mathf.Clamp(offset.x / (audibleDistance * 0.5f), -1f, 1f);
        engine.panStereo = horn.panStereo = pan;
        engine.volume = engineVolume * gain;
        horn.volume = hornVolume * gain;
        engine.pitch = idlePitch + Mathf.Clamp01(speed / agent.CruiseSpeed) * 0.35f;
        if (!engine.isPlaying && engine.clip != null) engine.Play();

        if (horn.loop)
        {
            if (!horn.isPlaying && horn.clip != null) horn.Play();
            return;
        }

        if (!agent.BlockedByTraffic || speed > 0.15f)
        {
            hornTime = 0f;
            return;
        }

        if (honked) return;
        if (hornTime == 0f) hornTime = Time.time + Random.Range(4f, 7f);
        if (Time.time < hornTime) return;

        honked = true;
        if (horn.clip == null) return;
        horn.Play();
        horn.SetScheduledEndTime(UnityEngine.AudioSettings.dspTime + 0.3);
    }

    private void StopSounds()
    {
        hornTime = 0f;
        if (engine.isPlaying) engine.Stop();
        if (horn.isPlaying) horn.Stop();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        using (new UnityEditor.Handles.DrawingScope(Color.green))
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.forward, audibleDistance);
    }
#endif

    private void OnDisable()
    {
        StopSounds();
        honked = false;
    }
}
