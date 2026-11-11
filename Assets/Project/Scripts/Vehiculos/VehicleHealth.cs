using System;
using UnityEngine;
using UnityEngine.Events;

public class VehicleHealth : MonoBehaviour
{
  [Tooltip("Vida m�xima del veh�culo.")]
  [SerializeField, Min(1f)] private float maxHealth = 100f;

  [SerializeField] private UnityEvent onDestroyed = new UnityEvent();

  public float CurrentHealth { get; private set; }
  public float HealthRatio => CurrentHealth / maxHealth;
  public bool IsDestroyed => CurrentHealth <= 0f;
  public float MaxHealth => maxHealth;
  public int ExtraLives { get; set; }

  public event Action<float> HealthChanged;

  private void Awake()
  {
    CurrentHealth = maxHealth;
  }

  public void TakeDamage(float damage)
  {
    if (IsDestroyed || damage <= 0f) return;

    if (CurrentHealth - damage <= 0f && ExtraLives > 0)
    {
      ExtraLives--;                       // Rueda de auxilio
      CurrentHealth = maxHealth * 0.2f;
      HealthChanged?.Invoke(HealthRatio);
      return;
    }

    CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
    Debug.Log("mi current health es " + CurrentHealth);
    HealthChanged?.Invoke(HealthRatio);

    if (IsDestroyed)
      onDestroyed.Invoke();
  }

  // healDifference = true cuando comprás Chasis: sube el máximo y te cura lo que subió.
  public void SetMaxHealth(float newMax, bool healDifference)
  {
    float diff = newMax - maxHealth;
    maxHealth = Mathf.Max(1f, newMax);
    if (healDifference && diff > 0f) CurrentHealth += diff;
    CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);
    HealthChanged?.Invoke(HealthRatio);
  }

  public void SetHealth(float value)
  {
    CurrentHealth = Mathf.Clamp(value, 1f, maxHealth);
    HealthChanged?.Invoke(HealthRatio);
  }
  public void Heal(float amount) => SetHealth(CurrentHealth + amount);

}
