using UnityEngine;

public class LaserGun : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Gun")]
    [SerializeField] private float range = 100f;

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

        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        Vector3 targetPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            targetPoint = hit.point;
        }

        Vector3 shootDirection = (targetPoint - firePoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(shootDirection)
        );

        Collider projectileCollider = projectile.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (projectileCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(projectileCollider, playerCollider);
        }

        LaserProjectile laserProjectile = projectile.GetComponent<LaserProjectile>();

        if (laserProjectile == null)
        {
            Debug.LogError("El prefab no tiene LaserProjectile");
            Destroy(projectile);
            return;
        }

        laserProjectile.Init(shootDirection);
    }
}
