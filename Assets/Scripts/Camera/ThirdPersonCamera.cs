using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Camera")]
    [SerializeField] private float normalDistance = 5f;
    [SerializeField] private float aimDistance = 3f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minVerticalAngle = -20f;
    [SerializeField] private float maxVerticalAngle = 60f;
    [SerializeField] private float aimSmoothSpeed = 10f;

    private float yaw;
    private float pitch = 20f;
    private float currentDistance;

    public bool IsAiming { get; private set; }

    private void Start()
    {
        currentDistance = normalDistance;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        HandleAim();
        RotateCamera();
        FollowTarget();
    }

    private void HandleAim()
    {
        IsAiming = Input.GetMouseButton(1);

        float targetDistance = IsAiming ? aimDistance : normalDistance;

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            aimSmoothSpeed * Time.deltaTime
        );
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(
            pitch,
            minVerticalAngle,
            maxVerticalAngle
        );
    }

    private void FollowTarget()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 targetPosition = target.position + Vector3.up * height;

        Vector3 cameraPosition =
            targetPosition -
            rotation * Vector3.forward * currentDistance;

        transform.position = cameraPosition;
        transform.LookAt(targetPosition);
    }
}
