using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Exercice 5 : tirer avec l'arme en VR.
// À placer sur l'arme (l'objet qui a le XR Grab Interactable).
// Quand on tient l'arme et qu'on appuie sur la gâchette, une balle sort du canon.
[RequireComponent(typeof(XRGrabInteractable))]
public class Gun : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Le prefab de la balle (doit avoir un Rigidbody)")]
    public Rigidbody bulletPrefab;
    [Tooltip("Point vide placé au bout du canon, axe bleu (Z) vers l'avant")]
    public Transform firePoint;

    [Header("Réglages du tir")]
    [Tooltip("Vitesse donnée à la balle au moment du tir (m/s)")]
    public float shootForce = 10f;
    [Tooltip("La balle tombe-t-elle avec la gravité ?")]
    public bool bulletUseGravity = true;
    [Tooltip("Temps avant que la balle disparaisse (secondes)")]
    public float bulletLifetime = 1f;
    [Tooltip("Temps minimum entre deux tirs (secondes)")]
    public float fireCooldown = 0.15f;

    private XRGrabInteractable grabInteractable;
    private Collider[] gunColliders;
    private float lastShotTime = -999f;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        gunColliders = GetComponentsInChildren<Collider>();
    }

    void OnEnable()
    {
        // "activated" = appui sur la gâchette pendant qu'on tient l'objet
        grabInteractable.activated.AddListener(OnTriggerPressed);
    }

    void OnDisable()
    {
        grabInteractable.activated.RemoveListener(OnTriggerPressed);
    }

    void OnTriggerPressed(ActivateEventArgs args)
    {
        Shoot();
    }

    public void Shoot()
    {
        if (Time.time - lastShotTime < fireCooldown) return;
        lastShotTime = Time.time;

        // 1. Faire apparaître la balle au bout du canon
        Rigidbody bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // 2. La balle ne doit pas percuter l'arme elle-même en sortant
        Collider bulletCollider = bullet.GetComponent<Collider>();
        if (bulletCollider != null)
        {
            foreach (Collider c in gunColliders)
                Physics.IgnoreCollision(bulletCollider, c);
        }

        // 3. Propulser la balle vers l'avant du canon
        bullet.useGravity = bulletUseGravity;
        bullet.AddForce(firePoint.forward * shootForce, ForceMode.VelocityChange);

        // 4. Supprimer la balle après quelques secondes pour ne pas surcharger la scène
        Destroy(bullet.gameObject, bulletLifetime);
    }
}