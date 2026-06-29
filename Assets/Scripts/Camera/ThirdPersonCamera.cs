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

    [Header("Shoulder Aim")]
    [SerializeField] private float normalSideOffset = 0f;
    [SerializeField] private float aimSideOffset = 1.2f;
    [SerializeField] private float aimHeightOffset = 0.2f;

    private float yaw;
    private float pitch = 20f;
    private float currentDistance;
    private float currentSideOffset;
    private float currentHeightOffset;

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
        float targetSideOffset = IsAiming ? aimSideOffset : normalSideOffset;
        float targetHeightOffset = IsAiming ? aimHeightOffset : 0f;

        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            aimSmoothSpeed * Time.deltaTime
        );

        currentSideOffset = Mathf.Lerp(
            currentSideOffset,
            targetSideOffset,
            aimSmoothSpeed * Time.deltaTime
        );

        currentHeightOffset = Mathf.Lerp(
            currentHeightOffset,
            targetHeightOffset,
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

        Vector3 targetPosition =
            target.position +
            Vector3.up * (height + currentHeightOffset);

        Vector3 sideOffset =
            rotation * Vector3.right * currentSideOffset;

        Vector3 cameraPosition =
            targetPosition +
            sideOffset -
            rotation * Vector3.forward * currentDistance;

        Vector3 lookPosition =
            targetPosition +
            sideOffset;

        transform.position = cameraPosition;
        transform.LookAt(lookPosition);
    }
}