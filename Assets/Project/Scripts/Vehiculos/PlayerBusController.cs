using UnityEngine;
[RequireComponent(typeof(VehicleController))]

public class PlayerBusController : MonoBehaviour
{
    private VehicleController vehicle;

    private void Awake()
    {
        vehicle = GetComponent<VehicleController>();
    }

    private void Update()
    {
        float throttle = Input.GetAxisRaw("Vertical");
        float steering = Input.GetAxisRaw("Horizontal");
        bool brake = Input.GetKey(KeyCode.Space);

        vehicle.SetInput(throttle, steering, brake);
    }

    private void OnDisable()
    {
        // limpio las órdenes cuando dejo de controlarlo.
        vehicle.SetInput(0f, 0f, false);
    }
}
