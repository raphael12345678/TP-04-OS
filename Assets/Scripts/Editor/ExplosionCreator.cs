using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Script ÉDITEUR : à placer dans Assets/Scripts/Editor (comme BulletCreator).
// Ajoute le menu "Tools > Créer l'explosion" qui fabrique un prefab Explosion
// (Particle System de feu + Particle System enfant de fumée) et le relie
// automatiquement au TargetSpawner et aux Target présents dans la scène.
public static class ExplosionCreator
{
    const string PrefabFolder = "Assets/Prefabs";
    const string PrefabPath = PrefabFolder + "/Explosion.prefab";

    [MenuItem("Tools/Créer l'explosion")]
    static void CreateExplosion()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // --- Feu (objet principal) ---
        GameObject root = CreateParticleObject("Explosion");
        ConfigureFire(root.GetComponent<ParticleSystem>());

        // --- Fumée (enfant) ---
        GameObject smoke = CreateParticleObject("Fumee");
        smoke.transform.SetParent(root.transform, false);
        smoke.transform.localPosition = Vector3.zero;
        ConfigureSmoke(smoke.GetComponent<ParticleSystem>());

        // --- Sauvegarde en prefab puis suppression de la scène ---
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        // --- Liaison automatique ---
        int linked = 0;
        foreach (TargetSpawner sp in Object.FindObjectsByType<TargetSpawner>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(sp, "Assigner l'explosion");
            sp.explosionPrefab = prefab;
            PrefabUtility.RecordPrefabInstancePropertyModifications(sp);
            EditorUtility.SetDirty(sp);
            linked++;
        }
        foreach (Target t in Object.FindObjectsByType<Target>(FindObjectsSortMode.None))
        {
            Undo.RecordObject(t, "Assigner l'explosion");
            t.explosionPrefab = prefab;
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
            EditorUtility.SetDirty(t);
            linked++;
        }
        if (linked > 0)
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("Explosion créée dans " + PrefabPath + " et reliée à " + linked + " objet(s) de la scène.");
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    // Crée un Particle System via le menu de Unity : il reçoit ainsi automatiquement
    // le bon matériau de particules pour l'URP (sinon les particules seraient roses).
    static GameObject CreateParticleObject(string name)
    {
        Selection.activeObject = null;
        GameObject go = null;
        if (EditorApplication.ExecuteMenuItem("GameObject/Effects/Particle System"))
            go = Selection.activeGameObject;

        if (go == null)
        {
            go = new GameObject(name);
            go.AddComponent<ParticleSystem>();
            Debug.LogWarning("Particle System créé sans matériau : si les particules sont roses, " +
                             "assigne un matériau de particules dans le module Renderer.");
        }

        go.name = name;
        go.transform.SetParent(null);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        return go;
    }

    static void ConfigureFire(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.3f), new Color(1f, 0.35f, 0.05f));
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)80) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 1f, 0.6f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0.05f), 0.5f),
                new GradientColorKey(new Color(0.3f, 0.1f, 0.05f), 1f) },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));
    }

    static void ConfigureSmoke(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 2f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.3f, 0.3f, 0.3f, 0.7f), new Color(0.6f, 0.6f, 0.6f, 0.5f));
        main.gravityModifier = -0.05f; // la fumée monte doucement
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.05f, (short)25) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f) },
            new[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.15f),
                new GradientAlphaKey(0f, 1f) });
        col.color = new ParticleSystem.MinMaxGradient(g);

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1.5f, AnimationCurve.Linear(0f, 0.4f, 1f, 1f));
    }
}
