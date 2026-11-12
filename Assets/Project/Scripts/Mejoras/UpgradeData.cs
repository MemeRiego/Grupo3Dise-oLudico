using System;
using UnityEngine;

public enum Rarity { Comun, Rara, Epica }

// Cada stat se suma entre todas las mejoras compradas. Los % se leen con GameSession.Mult.
public enum UpgradeStat
{
  MaxSpeed,            // % velocidad máx y motor
  BrakeForce,          // %
  Steering,            // %
  DamageTaken,         // % (negativo = menos daño)
  MaxHealth,           // vida plana
  MinImpactSpeed,      // plano
  DamageSlowdown,      // % que se ignora del frenado por daño
  SecondsPerLeg,       // segundos
  FreeBoardingSeconds, // segundos
  FareBonus,           // $ por boleto bien
  Penalty,             // % (negativo = multa más chica)
  StreakBonus,         // $ cada 5 seguidos
  ExtraLife,           // cantidad por nivel
  SavingsInterest,     // % de lo ahorrado
  StarTip,             // % extra de propina por estrella
  ShopSlots,           // cartas extra
  FreeReroll           // rerolls gratis por taller
}

public enum UnlockRule
{
  DeEntrada, TerminarNivel1, TerminarDebajo60, TerminarDebajo25, TodasATiempo,
  CincoErroresEnPartida, RachaDe10, PerderPorDestruido, Ahorrar1500, TresEstrellas, TerminarPartida
}

[Serializable]
public struct StatModifier
{
  public UpgradeStat stat;
  public float perLevel;
}

[CreateAssetMenu(menuName = "Bondi/Mejora")]
public class UpgradeData : ScriptableObject
{
  public string id;              // único, sin espacios: "motor", "frenos"...
  public string displayName;
  [TextArea] public string description;
  public Sprite icon;
  public Rarity rarity;
  public int price = 250;
  [Min(1)] public int maxLevel = 1;
  public StatModifier[] modifiers;
  public UnlockRule unlock;
  [TextArea] public string unlockText; // lo que se lee en la grilla si está bloqueada
}