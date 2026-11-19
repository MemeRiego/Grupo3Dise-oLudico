using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(VehicleController))]

public class PlayerBusController : MonoBehaviour
{
    private VehicleController vehicle;
    private VehicleAudio vehicleAudio;
   [SerializeField] private PauseManager pauseManager;


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
        HandlePause();
    }

    private void HandleMovement()
    {
        float throttle = Input.GetAxisRaw("Vertical");
        float steering = Input.GetAxisRaw("Horizontal");
        bool brake = Input.GetKey(KeyCode.Space);

        vehicle.SetInput(throttle, steering, brake);
    }

    private void HandlePause()
    {

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            pauseManager.TogglePause();
        }

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
