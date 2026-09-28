using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Exercice 6 : fait apparaître des cibles à des positions aléatoires, à intervalle régulier.
// À placer sur un objet vide ("TargetSpawner"). La zone d'apparition est la boîte jaune
// visible dans la vue Scene quand l'objet est sélectionné.
public class TargetSpawner : MonoBehaviour
{
    [Header("Cibles")]
    [Tooltip("Le prefab de la cible (ex : ShootingTarget)")]
    public GameObject targetPrefab;
    [Tooltip("Le prefab de l'explosion, donné à chaque cible créée")]
    public GameObject explosionPrefab;

    [Header("Zone d'apparition (centrée sur cet objet)")]
    public Vector3 areaSize = new Vector3(8f, 1f, 4f);
    [Tooltip("Distance minimale entre deux cibles")]
    public float minSpacing = 1.2f;

    [Header("Timing")]
    [Tooltip("Temps entre deux apparitions (secondes)")]
    public float spawnInterval = 3f;
    [Tooltip("Nombre maximum de cibles en même temps")]
    public int maxTargets = 5;
    [Tooltip("Durée de vie d'une cible non touchée (0 = infinie)")]
    public float targetLifetime = 0f;

    [Header("Orientation")]
    [Tooltip("Les cibles se tournent vers le joueur")]
    public bool facePlayer = true;
    [Tooltip("Rotation supplémentaire appliquée à chaque cible (en degrés, sur X, Y et Z).\n" +
             "Ex : Y = 180 si la cible est de dos, Z = 180 si elle a la tête en bas.")]
    public Vector3 rotationOffset = Vector3.zero;

    private readonly List<Target> aliveTargets = new List<Target>();

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            aliveTargets.RemoveAll(t => t == null); // retire les cibles déjà détruites
            if (aliveTargets.Count < maxTargets)
                SpawnTarget();
        }
    }

    void SpawnTarget()
    {
        if (targetPrefab == null)
        {
            Debug.LogWarning("TargetSpawner : aucun Target Prefab assigné.");
            return;
        }

        // Cherche une position aléatoire pas trop proche des autres cibles
        Vector3 position = transform.position;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            position = transform.position + new Vector3(
                Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                Random.Range(-areaSize.y / 2f, areaSize.y / 2f),
                Random.Range(-areaSize.z / 2f, areaSize.z / 2f));

            if (IsFarEnough(position)) break;
        }

        // Orientation : face au joueur (seulement autour de l'axe vertical)
        Quaternion rotation = targetPrefab.transform.rotation;
        if (facePlayer && Camera.main != null)
        {
            Vector3 toPlayer = Camera.main.transform.position - position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.01f)
                rotation = Quaternion.LookRotation(toPlayer);
        }
        rotation *= Quaternion.Euler(rotationOffset);

        GameObject obj = Instantiate(targetPrefab, position, rotation);

        // Ajoute le script Target s'il n'est pas déjà sur le prefab
        Target target = obj.GetComponent<Target>();
        if (target == null) target = obj.AddComponent<Target>();
        target.spawner = this;
        if (target.explosionPrefab == null) target.explosionPrefab = explosionPrefab;

        aliveTargets.Add(target);

        if (targetLifetime > 0f)
            Destroy(obj, targetLifetime);
    }

    bool IsFarEnough(Vector3 position)
    {
        foreach (Target t in aliveTargets)
        {
            if (t != null && Vector3.Distance(t.transform.position, position) < minSpacing)
                return false;
        }
        return true;
    }

    public void OnTargetDestroyed(Target target)
    {
        aliveTargets.Remove(target);
    }

    // Dessine la zone d'apparition dans la vue Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.8f);
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
