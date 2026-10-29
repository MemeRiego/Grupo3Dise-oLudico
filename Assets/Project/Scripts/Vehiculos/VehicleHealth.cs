using System;
using UnityEngine;
using UnityEngine.Events;

public class VehicleHealth : MonoBehaviour
{
    [Tooltip("Vida máxima del vehículo.")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    [SerializeField] private UnityEvent onDestroyed = new UnityEvent();

    public float CurrentHealth { get; private set; }
    public float HealthRatio => CurrentHealth / maxHealth;
    public bool IsDestroyed => CurrentHealth <= 0f;

    public event Action<float> HealthChanged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed || damage <= 0f) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
        Debug.Log("mi current health es " + CurrentHealth);
        HealthChanged?.Invoke(HealthRatio);

        if (IsDestroyed)
            onDestroyed.Invoke();
    }
}
