using UnityEngine;

public class UIVelocimeter : MonoBehaviour
{
    [SerializeField] private Rigidbody2D vehicleBody;
    [SerializeField] private RectTransform needle;

    [SerializeField, Min(1f)] private float maxSpeed = 120f;
    [SerializeField, Min(0f)] private float speedMultiplier = 3.6f;
    [SerializeField] private float zeroAngle = 120f;
    [SerializeField] private float maxAngle = -120f;


    private void Start()
    {
        if (vehicleBody != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            vehicleBody = player.GetComponent<Rigidbody2D>();

        if (vehicleBody == null)
        {
            Debug.LogWarning("No se encontró el Rigidbody2D del Player.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (vehicleBody == null || needle == null) return;

        float speed = vehicleBody.linearVelocity.magnitude * speedMultiplier;
        float amount = Mathf.Clamp01(speed / maxSpeed);
        float angle = Mathf.Lerp(zeroAngle, maxAngle, amount);

        needle.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}
