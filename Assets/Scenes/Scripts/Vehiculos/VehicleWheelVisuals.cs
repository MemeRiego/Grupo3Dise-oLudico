using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleWheelVisuals : MonoBehaviour
{
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;

    private VehicleController vehicle;
    private Quaternion leftInitialRotation;
    private Quaternion rightInitialRotation;

    private void Awake()
    {
        vehicle = GetComponent<VehicleController>();

        leftInitialRotation = frontLeftWheel.localRotation;
        rightInitialRotation = frontRightWheel.localRotation;
    }

    private void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(
            0f, 0f, -vehicle.CurrentSteeringAngle
        );

        frontLeftWheel.localRotation = leftInitialRotation * rotation;
        frontRightWheel.localRotation = rightInitialRotation * rotation;
    }
}
