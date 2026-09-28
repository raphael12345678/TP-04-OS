using UnityEngine;

// Exercice 6 : une cible qui explose quand une balle la touche.
// Peut être mis sur n'importe quel objet (cube, cône, ShootingTarget...).
// Le TargetSpawner l'ajoute automatiquement aux cibles qu'il crée.
public class Target : MonoBehaviour
{
    [Tooltip("Prefab de l'explosion (Particle System) à faire apparaître à l'impact")]
    public GameObject explosionPrefab;

    [Tooltip("Durée avant de supprimer l'explosion de la scène (secondes)")]
    public float explosionDuration = 4f;

    [HideInInspector] public TargetSpawner spawner; // rempli par le spawner
    private bool isHit = false;

    void Awake()
    {
        // 1. La cible doit avoir des colliders pour être touchée.
        //    S'il n'y en a aucun, on en ajoute un sur chaque pièce visible.
        if (GetComponentsInChildren<Collider>().Length == 0)
        {
            foreach (MeshFilter mf in GetComponentsInChildren<MeshFilter>())
                mf.gameObject.AddComponent<MeshCollider>();
        }

        // 2. Un Rigidbody "kinematic" (immobile) sur la racine permet de recevoir
        //    OnCollisionEnter ici, même si les colliders sont sur les pièces enfants.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // Appelé automatiquement par Unity quand un objet physique touche la cible
    void OnCollisionEnter(Collision collision)
    {
        if (isHit) return;

        // On ne réagit qu'aux balles (tag "Bullet" créé avec le prefab de balle)
        if (!collision.gameObject.CompareTag("Bullet")) return;

        Vector3 impactPoint = collision.GetContact(0).point;
        Destroy(collision.gameObject); // la balle disparaît
        Hit(impactPoint);
    }

    public void Hit(Vector3 impactPoint)
    {
        isHit = true;

        // Effet de particules à l'endroit de l'impact
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, impactPoint, Quaternion.identity);
            Destroy(fx, explosionDuration);
        }

        // On prévient le spawner qu'une place s'est libérée
        if (spawner != null)
            spawner.OnTargetDestroyed(this);

        Destroy(gameObject);
    }
}
