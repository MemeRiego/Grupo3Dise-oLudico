using UnityEngine;
[RequireComponent(typeof(VehicleController))]

public class PlayerBusController : MonoBehaviour
{
    private VehicleController vehicle;
    private VehicleAudio vehicleAudio;


    private void Awake()
    {
        vehicle = GetComponent<VehicleController>();
        vehicleAudio = GetComponent<VehicleAudio>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        HandleMovement();
        HandleHorn();
    }

    private void HandleMovement()
    {
        float throttle = Input.GetAxisRaw("Vertical");
        float steering = Input.GetAxisRaw("Horizontal");
        bool brake = Input.GetKey(KeyCode.Space);

        vehicle.SetInput(throttle, steering, brake);
    }

    private void HandleHorn()
    {
        vehicleAudio.SetHorn(Input.GetKey(KeyCode.F));
    }

    private void OnDisable()
    {
        //limpio ordenes
        vehicle.SetInput(0f, 0f, false);
        vehicleAudio.SetHorn(false);
    }
}
