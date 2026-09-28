using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Exercice 4 : clavier virtuel AZERTY utilisable avec le laser VR.
// À placer sur un Panel enfant du Canvas. Le clavier se construit tout seul au lancement,
// s'affiche quand on sélectionne l'InputField et se cache quand on appuie sur "Entrée".
public class VirtualKeyboard : MonoBehaviour
{
    [Header("Champ de texte à remplir")]
    public TMP_InputField targetField;

    [Header("Apparence des touches")]
    public float keyWidth = 60f;
    public float keyHeight = 60f;
    public float spacing = 6f;
    public float fontSize = 26f;

    private readonly string[] rows = { "1234567890", "AZERTYUIOP", "QSDFGHJKLM", "WXCVBN" };
    private readonly List<TextMeshProUGUI> letterLabels = new List<TextMeshProUGUI>();
    private bool upperCase = true;

    void Start()
    {
        // Si le champ n'a pas été rempli dans l'Inspector, on cherche l'InputField tout seul
        if (targetField == null)
            targetField = FindFirstObjectByType<TMP_InputField>();

        if (targetField == null)
        {
            Debug.LogError("VirtualKeyboard : aucun TMP_InputField trouvé. Remplis 'Target Field' dans l'Inspector.");
            gameObject.SetActive(false);
            return;
        }

        BuildKeyboard();

        // Le clavier apparaît quand on clique sur l'InputField avec le laser
        targetField.onSelect.AddListener(_ => gameObject.SetActive(true));

        gameObject.SetActive(false);
    }

    // ---------- Construction du clavier ----------

    void BuildKeyboard()
    {
        // Placement : en bas du Canvas, centré
        RectTransform panel = GetComponent<RectTransform>();
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0f);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, 20f);

        // Fond sombre
        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);

        // Les rangées sont empilées verticalement
        VerticalLayoutGroup column = GetComponent<VerticalLayoutGroup>();
        if (column == null) column = gameObject.AddComponent<VerticalLayoutGroup>();
        column.spacing = spacing;
        column.padding = new RectOffset(20, 20, 20, 20);
        column.childAlignment = TextAnchor.UpperCenter;
        column.childControlWidth = column.childControlHeight = true;
        column.childForceExpandWidth = column.childForceExpandHeight = false;

        // Le panel prend automatiquement la taille du clavier
        ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
        if (fitter == null) fitter = gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Rangées de caractères
        foreach (string rowChars in rows)
        {
            Transform row = CreateRow();
            foreach (char c in rowChars)
            {
                TextMeshProUGUI label = CreateKey(row, c.ToString(), keyWidth, () => TypeChar(c));
                if (char.IsLetter(c)) letterLabels.Add(label);
            }
        }

        // Dernière rangée : touches spéciales
        Transform lastRow = CreateRow();
        CreateKey(lastRow, "Maj", keyWidth * 1.5f, ToggleCase);
        CreateKey(lastRow, "Espace", keyWidth * 4f, () => AddText(" "));
        CreateKey(lastRow, "Effacer", keyWidth * 1.8f, Backspace);
        CreateKey(lastRow, "Entrée", keyWidth * 1.8f, Submit);
    }

    Transform CreateRow()
    {
        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.layer = gameObject.layer;
        row.transform.SetParent(transform, false);

        HorizontalLayoutGroup h = row.GetComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = h.childForceExpandHeight = false;
        return row.transform;
    }

    TextMeshProUGUI CreateKey(Transform parent, string label, float width, UnityEngine.Events.UnityAction onClick)
    {
        // La touche : une image cliquable
        GameObject key = new GameObject("Key_" + label,
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        key.layer = gameObject.layer;
        key.transform.SetParent(parent, false);

        key.GetComponent<Image>().color = Color.white;
        LayoutElement layout = key.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = keyHeight;
        key.GetComponent<Button>().onClick.AddListener(onClick);

        // Le texte affiché sur la touche
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.layer = gameObject.layer;
        textObj.transform.SetParent(key.transform, false);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = Color.black;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; // c'est l'image qui reçoit le clic, pas le texte
        return tmp;
    }

    // ---------- Actions des touches ----------

    void TypeChar(char c)
    {
        string s = upperCase ? char.ToUpper(c).ToString() : char.ToLower(c).ToString();
        AddText(s);
    }

    void AddText(string s)
    {
        if (targetField.characterLimit > 0 && targetField.text.Length >= targetField.characterLimit)
            return;
        targetField.text += s;
    }

    void Backspace()
    {
        if (targetField.text.Length > 0)
            targetField.text = targetField.text.Substring(0, targetField.text.Length - 1);
    }

    void ToggleCase()
    {
        upperCase = !upperCase;
        foreach (TextMeshProUGUI label in letterLabels)
            label.text = upperCase ? label.text.ToUpper() : label.text.ToLower();
    }

    void Submit()
    {
        Debug.Log("Texte saisi : " + targetField.text);
        gameObject.SetActive(false);
    }
}