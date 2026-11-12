using UnityEngine;

[RequireComponent(typeof(VehicleController), typeof(VehicleHealth))]
public class BusUpgrades : MonoBehaviour
{
  private VehicleController vehicle;
  private VehicleHealth health;
  private VehicleDamage damage;
  private VehicleDamageEffects effects;
  private float baseMaxHealth;

  private void Awake()
  {
    vehicle = GetComponent<VehicleController>();
    health = GetComponent<VehicleHealth>();
    damage = GetComponent<VehicleDamage>();
    effects = GetComponent<VehicleDamageEffects>();
    baseMaxHealth = health.MaxHealth; // el del Inspector
  }

  private void Start()
  {
    ApplyStats();
    health.SetMaxHealth(baseMaxHealth + GameSession.Stat(UpgradeStat.MaxHealth), false);
    if (GameSession.Health >= 0f) health.SetHealth(GameSession.Health); // llega como terminó el nivel anterior
    health.ExtraLives = Mathf.RoundToInt(GameSession.Stat(UpgradeStat.ExtraLife));
  }

  public void ApplyStats()
  {
    vehicle.SetUpgradeMultipliers(
      GameSession.Mult(UpgradeStat.MaxSpeed),
      GameSession.Mult(UpgradeStat.BrakeForce),
      GameSession.Mult(UpgradeStat.Steering));
    if (damage != null)
    {
      damage.DamageMultiplier = GameSession.Mult(UpgradeStat.DamageTaken);
      damage.ExtraMinImpactSpeed = GameSession.Stat(UpgradeStat.MinImpactSpeed);
    }
    if (effects != null)
      effects.SlowdownReduction = Mathf.Clamp01(GameSession.Stat(UpgradeStat.DamageSlowdown) / 100f);
  }

  // La llama el taller después de cada compra.
  public void OnUpgradeBought()
  {
    ApplyStats();
    health.SetMaxHealth(baseMaxHealth + GameSession.Stat(UpgradeStat.MaxHealth), true);
  }
}