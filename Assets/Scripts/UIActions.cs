using UnityEngine;

public class UIActions : MonoBehaviour
{
    public Renderer cubeRenderer; // glisser GrabbableCube ici

    // Appelé par le bouton : couleur aléatoire
    public void ChangeCubeColor()
    {
        cubeRenderer.material.color = Random.ColorHSV();
    }

    // Appelé par le slider : valeur entre 0 et 1 → taille entre 0.1 et 0.5
    public void SetCubeSize(float value)
    {
        cubeRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.1f, 0.5f, value);
    }
}