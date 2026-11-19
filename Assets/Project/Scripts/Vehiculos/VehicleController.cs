using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VehicleController : MonoBehaviour
{

  [Tooltip("Velocidad m�xima hacia adelante, en unidades por segundo.")]
  [SerializeField] private float maxSpeed = 8f;

  [Tooltip("Velocidad m�xima en reversa.")]
  [SerializeField] private float reverseSpeed = 3f;

  [Tooltip("Fuerza del motor. M�s alto = acelera m�s r�pido.")]
  [SerializeField, Min(0f)] private float engineForce = 1800f;

  [Tooltip("Fuerza de frenado. M�s alto = frena en menos distancia.")]
  [SerializeField, Min(0f)] private float brakeForce = 4500f;

  [Tooltip("Resistencia al rodar. M�s alto = pierde velocidad m�s r�pido.")]
  [SerializeField, Min(0f)] private float rollingResistance = 180f;

  [Tooltip("Resistencia del aire. Se nota m�s a velocidades altas.")]
  [SerializeField, Min(0f)] private float airResistance = 20f;

  [Tooltip("Agarre lateral. M�s alto = se desliza menos de costado.")]
  [SerializeField, Min(0f)] private float lateralGrip = 6f;

  [Tooltip("�ngulo m�ximo de las ruedas delanteras.")]
  [SerializeField] private float maxSteeringAngle = 30f;

  [Tooltip("Velocidad con la que cambia la direcci�n de las ruedas.")]
  [SerializeField] private float steeringSpeed = 90f;

  [Tooltip("Distancia entre las ruedas delanteras y traseras.")]
  [SerializeField, Min(0.1f)] private float wheelbase = 3.4f;



  private Rigidbody2D rb;
  private float throttleInput;
  private float steeringInput;
  private float currentSteeringAngle;
  private bool brakeInput;
  private float forwardContact;
  private float reverseContact;
  private float speedMultiplier = 1f;
  private float steeringMultiplier = 1f;
  public float ThrottleInput => throttleInput;
  public float MaxSpeed => maxSpeed * upgradeSpeed; // antes: maxSpeed (así el velocímetro escala)
  public bool IsBraking => brakeInput || Vector2.Dot(rb.linearVelocity, transform.up) * throttleInput < 0f;
  public float CurrentSteeringAngle => currentSteeringAngle;
  public float Wheelbase => wheelbase;
  private float upgradeSpeed = 1f;
  private float upgradeBrake = 1f;
  private float upgradeSteering = 1f;

  // Estas dos vinieron de main (las lee el código de los autos de tránsito). Opcional: así devuelven el valor real del bondi mejorado:
  public float BrakingDeceleration => brakeForce * upgradeBrake / rb.mass;
  public float MaxSteeringAngle => maxSteeringAngle * upgradeSteering;

  private void Awake()
  {
    rb = GetComponent<Rigidbody2D>();
  }

  public void SetInput(float throttle, float steering, bool brake)
  {
    throttleInput = Mathf.Clamp(throttle, -1f, 1f);
    steeringInput = Mathf.Clamp(steering, -1f, 1f);
    brakeInput = brake;
  }

  private void FixedUpdate()
  {
    float deltaTime = Time.fixedDeltaTime;
    Vector2 forward = transform.up;
    float speed = Vector2.Dot(rb.linearVelocity, forward);

    bool isBraking = brakeInput || speed * throttleInput < 0f;

    if (isBraking)
      ApplyBraking(speed, forward, deltaTime);
    else
      ApplyEngine(speed, forward);

    forwardContact = 0f;
    reverseContact = 0f;
    float resistance = rollingResistance + airResistance * speed * speed;
    float stopForce = Mathf.Abs(speed) * rb.mass / deltaTime;
    float availableForce = isBraking ? Mathf.Max(0f, stopForce - brakeForce * upgradeBrake) : stopForce;
    resistance = Mathf.Min(resistance, availableForce);

    rb.AddForce(-forward * Mathf.Sign(speed) * resistance);

    // agarre para que no se deslice de costado.
    Vector2 sideways = transform.right;
    float lateralSpeed = Vector2.Dot(rb.linearVelocity, sideways);
    float grip = Mathf.Min(lateralGrip, 1f / deltaTime);

    rb.AddForce(-sideways * lateralSpeed * rb.mass * grip);

    currentSteeringAngle = Mathf.MoveTowards(currentSteeringAngle, steeringInput * maxSteeringAngle * steeringMultiplier * upgradeSteering, steeringSpeed * upgradeSteering * deltaTime);

    float turnRate = -speed / wheelbase * Mathf.Tan(currentSteeringAngle * Mathf.Deg2Rad) * Mathf.Rad2Deg;
    rb.angularVelocity = Mathf.MoveTowards(rb.angularVelocity, turnRate, steeringSpeed * 4f * deltaTime);
  }

  private void OnCollisionEnter2D(Collision2D collision) => ReadVehicleContact(collision);
  private void OnCollisionStay2D(Collision2D collision) => ReadVehicleContact(collision);

  private void ReadVehicleContact(Collision2D collision)
  {
    if (collision.rigidbody == null || !collision.rigidbody.TryGetComponent<VehicleController>(out _)) return;

    for (int i = 0; i < collision.contactCount; i++)
    {
      float alignment = Vector2.Dot(collision.GetContact(i).normal, transform.up);
      forwardContact = Mathf.Max(forwardContact, -alignment);
      reverseContact = Mathf.Max(reverseContact, alignment);
    }
  }

  private void ApplyEngine(float speed, Vector2 forward)
  {
    if (throttleInput == 0f || speedMultiplier <= 0f) return;

    float speedLimit = (throttleInput > 0f ? maxSpeed * upgradeSpeed : reverseSpeed) * speedMultiplier;
    float power = 1f - Mathf.InverseLerp(speedLimit * 0.85f, speedLimit, Mathf.Abs(speed));

    power *= 1f - (throttleInput > 0f ? forwardContact : reverseContact);
    rb.AddForce(forward * throttleInput * engineForce * upgradeSpeed * power * speedMultiplier);
  }

  private void ApplyBraking(float speed, Vector2 forward, float deltaTime)
  {
    float stopForce = Mathf.Abs(speed) * rb.mass / deltaTime;
    float force = Mathf.Min(brakeForce * upgradeBrake, stopForce);

    rb.AddForce(-forward * Mathf.Sign(speed) * force);
  }

  public void SetDamageMultipliers(float speed, float steering)
  {
    speedMultiplier = Mathf.Clamp01(speed);
    steeringMultiplier = Mathf.Clamp01(steering);
  }

  public void StopVehicle()
  {
    SetInput(0f, 0f, false);
    rb.linearVelocity = Vector2.zero;
    rb.angularVelocity = 0f;
    enabled = false;
  }

  public void SetUpgradeMultipliers(float speed, float brake, float steering)
  {
    upgradeSpeed = Mathf.Max(0.1f, speed);
    upgradeBrake = Mathf.Max(0.1f, brake);
    upgradeSteering = Mathf.Max(0.1f, steering);
  }
}
