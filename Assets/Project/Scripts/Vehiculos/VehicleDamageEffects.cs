using System;
using UnityEngine;

public class VehicleDamageEffects : MonoBehaviour
{
  [Serializable]
  private class DamageStage
  {
    [Tooltip("Se aplica cuando queda este porcentaje de vida o menos. 0.6 = 60%.")]
    [Range(0f, 2f)] public float healthThreshold = 0.6f;

    [Tooltip("1 = normal, 0.5 = mitad de potencia y velocidad m�xima.")]
    [Range(0f, 2f)] public float speedMultiplier = 0.7f;

    [Tooltip("1 = giro normal, 0.5 = mitad del angulo maximo.")]
    [Range(0f, 2f)] public float steeringMultiplier = 0.8f;

    public Color color = Color.yellow;
    public bool showSmoke;
  }

  [Tooltip("Sprite de la carrocer�a.")]
  [SerializeField] private SpriteRenderer bodyRenderer;

  [Tooltip("Opcional. Part�culas ubicadas donde esta el motor.")]
  [SerializeField] private ParticleSystem smoke;

  [SerializeField]
  private DamageStage[] stages = new DamageStage[]
  {
        new DamageStage { healthThreshold = 0.6f, speedMultiplier = 0.7f, steeringMultiplier = 0.8f, color = Color.yellow, showSmoke = false },
        new DamageStage { healthThreshold = 0.25f, speedMultiplier = 0.4f, steeringMultiplier = 0.5f, color = Color.red, showSmoke = true }
  };

  private VehicleHealth health;
  private VehicleController vehicle;
  private Color originalColor;
  public float SlowdownReduction { get; set; } // 0 = normal, 0.5 = el daño frena la mitad

  private void Awake()
  {
    health = GetComponent<VehicleHealth>();
    vehicle = GetComponent<VehicleController>();

    if (bodyRenderer != null)
      originalColor = bodyRenderer.color;
  }

  private void OnEnable()
  {
    health.HealthChanged += UpdateEffects;
  }

  private void Start()
  {
    UpdateEffects(health.HealthRatio);
  }

  private void OnDisable()
  {
    health.HealthChanged -= UpdateEffects;
  }

  private void UpdateEffects(float healthRatio)
  {
    if (health.IsDestroyed)
    {
      ApplyColor(Color.black);
      SetSmoke(true);
      vehicle.StopVehicle();
      return;
    }

    DamageStage selectedStage = null;

    foreach (DamageStage stage in stages)
    {
      if (stage == null || healthRatio > stage.healthThreshold) continue;

      if (selectedStage == null || stage.healthThreshold < selectedStage.healthThreshold)
        selectedStage = stage;
    }

    if (selectedStage == null)
    {
      vehicle.SetDamageMultipliers(1f, 1f);
      ApplyColor(Color.white);
      SetSmoke(false);
      return;
    }

    vehicle.SetDamageMultipliers(Mathf.Lerp(selectedStage.speedMultiplier, 1f, SlowdownReduction), Mathf.Lerp(selectedStage.steeringMultiplier, 1f, SlowdownReduction));
    ApplyColor(selectedStage.color);
    SetSmoke(selectedStage.showSmoke);
  }

  private void ApplyColor(Color color)
  {
    if (bodyRenderer != null)
      bodyRenderer.color = originalColor * color;
  }

  private void SetSmoke(bool active)
  {
    if (smoke == null) return;

    if (active)
    {
      if (!smoke.isPlaying)
        smoke.Play();
    }
    else
    {
      smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
  }
}
