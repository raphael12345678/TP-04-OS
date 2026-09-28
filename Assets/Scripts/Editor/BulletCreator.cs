using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Script ÉDITEUR : il doit être placé dans un dossier nommé "Editor" (ex : Assets/Scripts/Editor).
// Il ajoute un menu "Tools > Créer la balle" qui fabrique automatiquement le prefab Bullet
// (corps cylindrique + pointe arrondie + Rigidbody + collider + tag) et le relie au script Gun.
public static class BulletCreator
{
    const string PrefabFolder = "Assets/Prefabs";
    const string PrefabPath = PrefabFolder + "/Bullet.prefab";

    [MenuItem("Tools/Créer la balle")]
    static void CreateBullet()
    {
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        EnsureTag("Bullet");

        // --- Objet principal ---
        GameObject root = new GameObject("Bullet");
        root.tag = "Bullet";

        // --- Corps : cylindre couché dans l'axe Z (axe du tir) ---
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Corps";
        Object.DestroyImmediate(body.GetComponent<Collider>());
        body.transform.SetParent(root.transform, false);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        body.transform.localScale = new Vector3(0.02f, 0.025f, 0.02f); // 2 cm de diamètre, 5 cm de long
        body.GetComponent<MeshRenderer>().sharedMaterial =
            GetOrCreateMaterial("Laiton", new Color(0.85f, 0.65f, 0.2f));

        // --- Pointe : sphère à l'avant du cylindre ---
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tip.name = "Pointe";
        Object.DestroyImmediate(tip.GetComponent<Collider>());
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0f, 0f, 0.025f);
        tip.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        tip.GetComponent<MeshRenderer>().sharedMaterial =
            GetOrCreateMaterial("Cuivre", new Color(0.72f, 0.4f, 0.2f));

        // --- Collider unique sur le parent ---
        CapsuleCollider col = root.AddComponent<CapsuleCollider>();
        col.direction = 2; // axe Z
        col.radius = 0.01f;
        col.height = 0.07f;
        col.center = new Vector3(0f, 0f, 0.005f);

        // --- Physique ---
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.mass = 0.01f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // --- Sauvegarde en prefab puis suppression de la scène ---
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        // --- On relie automatiquement le prefab au script Gun s'il existe dans la scène ---
        Gun gun = Object.FindFirstObjectByType<Gun>();
        if (gun != null && prefab != null)
        {
            Undo.RecordObject(gun, "Assigner la balle");
            gun.bulletPrefab = prefab.GetComponent<Rigidbody>();
            PrefabUtility.RecordPrefabInstancePropertyModifications(gun);
            EditorUtility.SetDirty(gun);
            EditorSceneManager.MarkSceneDirty(gun.gameObject.scene);
            Debug.Log("Balle créée et reliée au script Gun de " + gun.name);
        }
        else
        {
            Debug.Log("Balle créée dans " + PrefabPath + ". Glisse-la dans le champ Bullet Prefab du script Gun.");
        }

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    static Material GetOrCreateMaterial(string name, Color color)
    {
        string path = PrefabFolder + "/" + name + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        mat = new Material(shader);
        mat.SetColor("_BaseColor", color); // URP
        mat.SetColor("_Color", color);     // Built-in (au cas où)
        mat.SetFloat("_Metallic", 0.8f);
        mat.SetFloat("_Smoothness", 0.7f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void EnsureTag(string tag)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
        if (assets == null || assets.Length == 0) return;

        SerializedObject tagManager = new SerializedObject(assets[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}