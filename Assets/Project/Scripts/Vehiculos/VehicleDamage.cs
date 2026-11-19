using UnityEngine;

[RequireComponent(typeof(VehicleHealth), typeof(Rigidbody2D))]
public class VehicleDamage : MonoBehaviour
{
  [Tooltip("Capas de los objetos que pueden da�ar al vehiculo.")]
  [SerializeField] private LayerMask damageLayers;

  [Tooltip("Velocidad minima del impacto para recibir da�o.")]
  [SerializeField, Min(0f)] private float minImpactSpeed = 1.5f;

  [Tooltip("Da�o por cada unidad de velocidad que supera el minimo.")]
  [SerializeField, Min(0f)] private float damageMultiplier = 10f;

  [Tooltip("Tiempo minimo entre golpes. Evita contar varios contactos como muchos choques.")]
  [SerializeField, Min(0f)] private float damageCooldown = 0.25f;

  private VehicleHealth health;
  private float nextDamageTime;

  public event System.Action<float> Impact;

  public float DamageMultiplier { get; set; } = 1f;   // Paragolpes
  public float ExtraMinImpactSpeed { get; set; }      // Amortiguadores

  private void Awake()
  {
    health = GetComponent<VehicleHealth>();
  }

  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (health.IsDestroyed || Time.time < nextDamageTime) return;
    if ((damageLayers.value & (1 << collision.collider.gameObject.layer)) == 0) return;

    float impactSpeed = 0f;

    for (int i = 0; i < collision.contactCount; i++)
    {
      ContactPoint2D contact = collision.GetContact(i);
      float speed = Mathf.Abs(Vector2.Dot(contact.relativeVelocity, contact.normal));
      impactSpeed = Mathf.Max(impactSpeed, speed);
    }

    float minSpeed = minImpactSpeed + ExtraMinImpactSpeed;
    if (impactSpeed <= minSpeed) return;
    float damage = (impactSpeed - minSpeed) * damageMultiplier * DamageMultiplier;
    nextDamageTime = Time.time + damageCooldown;

    Impact?.Invoke(impactSpeed);
    health.TakeDamage(damage);
  }
}
