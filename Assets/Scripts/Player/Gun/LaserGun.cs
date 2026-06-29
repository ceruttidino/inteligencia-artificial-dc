using UnityEngine;

public class LaserGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Gun")]
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask shootMask;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (playerCamera == null || firePoint == null || projectilePrefab == null)
        {
            Debug.LogError("Faltan referencias en LaserGun");
            return;
        }

        Ray cameraRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint = cameraRay.origin + cameraRay.direction * range;

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, range, shootMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = cameraHit.point;
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Collider projectileCollider = projectile.GetComponent<Collider>();
        Collider[] playerColliders = GetComponentsInParent<Collider>();

        if (projectileCollider != null)
        {
            foreach (Collider playerCollider in playerColliders)
            {
                Physics.IgnoreCollision(projectileCollider, playerCollider);
            }
        }

        LaserProjectile laserProjectile = projectile.GetComponent<LaserProjectile>();

        if (laserProjectile == null)
        {
            Debug.LogError("El prefab no tiene LaserProjectile");
            Destroy(projectile);
            return;
        }

        Debug.DrawRay(cameraRay.origin, cameraRay.direction * range, Color.green, 2f);
        Debug.DrawLine(firePoint.position, targetPoint, Color.red, 2f);

        laserProjectile.Init(shootDirection);
    }
}