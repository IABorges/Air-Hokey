using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private float popupDuration = 1.2f;

    [Header("Cores")]
    [SerializeField] private Color playerColor = new Color(0.25f, 0.85f, 1.00f);
    [SerializeField] private Color aiColor     = new Color(1.00f, 0.36f, 0.45f);
    [SerializeField] private Color panelColor  = new Color(0.05f, 0.06f, 0.10f, 0.80f);

    [Header("Textos")]
    [SerializeField] private string playerLabelText = "VOCÊ";
    [SerializeField] private string aiLabelText     = "OPONENTE";

    private int playerScore;
    private int aiScore;

    public int PlayerScore => playerScore;
    public int AiScore => aiScore;

    private TMP_Text playerScoreText;
    private TMP_Text aiScoreText;
    private RectTransform playerBadge;
    private RectTransform aiBadge;
    private Image playerBadgeImage;
    private Image aiBadgeImage;

    private CanvasGroup popupGroup;
    private RectTransform popupRect;
    private Image popupAccent;
    private TMP_Text popupTitle;
    private TMP_Text popupSubtitle;

    private Coroutine popupCoroutine;
    private Coroutine playerPunch;
    private Coroutine aiPunch;

    private static Sprite roundedSprite;


    private void Awake()
    {
        CreateUI();
        UpdateScore();
    }

    public void PlayerScored()
    {
        playerScore++;
        UpdateScore();
        Punch(ref playerPunch, playerBadge, playerBadgeImage, playerColor);
        ShowPopup("GOL!", playerLabelText, playerColor);
    }

    public void AIScored()
    {
        aiScore++;
        UpdateScore();
        Punch(ref aiPunch, aiBadge, aiBadgeImage, aiColor);
        ShowPopup("GOL!", aiLabelText, aiColor);
    }

    public void ResetScore()
    {
        playerScore = 0;
        aiScore = 0;
        UpdateScore();
    }

    private void UpdateScore()
    {
        if (playerScoreText != null) playerScoreText.text = playerScore.ToString();
        if (aiScoreText != null) aiScoreText.text = aiScore.ToString();
    }


    private void CreateUI()
    {
        GameObject canvasObject = new GameObject(
            "GameCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1000, 2000);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform panel = CreatePanel("ScorePanel", canvasObject.transform, panelColor, 28);
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.sizeDelta = new Vector2(680f, 170f);
        panel.anchoredPosition = new Vector2(0f, -40f);

        CreateAccent(panel, -1, aiColor);
        CreateAccent(panel, +1, playerColor);

        BuildSide(panel, -1, aiLabelText, aiColor, out aiScoreText, out aiBadge, out aiBadgeImage);
        BuildSide(panel, +1, playerLabelText, playerColor, out playerScoreText, out playerBadge, out playerBadgeImage);

        TMP_Text separator = CreateText("Separator", panel, 40f, FontStyles.Bold);
        separator.text = "×";
        separator.color = new Color(1f, 1f, 1f, 0.55f);
        RectTransform separatorRect = separator.rectTransform;
        Center(separatorRect);
        separatorRect.sizeDelta = new Vector2(80f, 80f);
        separatorRect.anchoredPosition = new Vector2(0f, -16f);

        popupRect = CreatePanel("GoalPopup", canvasObject.transform, new Color(0.04f, 0.05f, 0.09f, 0.92f), 36);
        Center(popupRect);
        popupRect.sizeDelta = new Vector2(820f, 300f);
        popupRect.anchoredPosition = Vector2.zero;

        popupGroup = popupRect.gameObject.AddComponent<CanvasGroup>();
        popupGroup.blocksRaycasts = false;
        popupGroup.interactable = false;

        GameObject accentObject = new GameObject("Accent", typeof(RectTransform), typeof(Image));
        accentObject.transform.SetParent(popupRect, false);
        popupAccent = accentObject.GetComponent<Image>();
        popupAccent.sprite = GetRoundedSprite();
        popupAccent.type = Image.Type.Sliced;
        popupAccent.raycastTarget = false;
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = accentRect.anchorMax = new Vector2(0.5f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.sizeDelta = new Vector2(160f, 10f);
        accentRect.anchoredPosition = new Vector2(0f, -26f);

        popupTitle = CreateText("PopupTitle", popupRect, 96f, FontStyles.Bold);
        RectTransform titleRect = popupTitle.rectTransform;
        titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.pivot = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(760f, 120f);
        titleRect.anchoredPosition = new Vector2(0f, 30f);
        popupTitle.characterSpacing = 6f;

        popupSubtitle = CreateText("PopupSubtitle", popupRect, 40f, FontStyles.Bold);
        RectTransform subtitleRect = popupSubtitle.rectTransform;
        subtitleRect.anchorMin = subtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        subtitleRect.pivot = new Vector2(0.5f, 0.5f);
        subtitleRect.sizeDelta = new Vector2(760f, 80f);
        subtitleRect.anchoredPosition = new Vector2(0f, -60f);
        popupSubtitle.color = new Color(1f, 1f, 1f, 0.85f);

        popupGroup.alpha = 0f;
        popupRect.gameObject.SetActive(false);
    }

    private void BuildSide(
        RectTransform parent,
        int side,                
        string label,
        Color color,
        out TMP_Text scoreText,
        out RectTransform badge,
        out Image badgeImage)
    {
        float x = 195f * side;

        TMP_Text labelText = CreateText("Label" + side, parent, 26f, FontStyles.Bold);
        labelText.text = label;
        labelText.color = color;
        labelText.characterSpacing = 10f;
        RectTransform labelRect = labelText.rectTransform;
        Center(labelRect);
        labelRect.sizeDelta = new Vector2(280f, 40f);
        labelRect.anchoredPosition = new Vector2(x, 42f);

        badge = CreatePanel("Badge" + side, parent, new Color(color.r, color.g, color.b, 0.16f), 24);
        Center(badge);
        badge.sizeDelta = new Vector2(130f, 84f);
        badge.anchoredPosition = new Vector2(x, -24f);
        badgeImage = badge.GetComponent<Image>();

        scoreText = CreateText("Score" + side, badge, 62f, FontStyles.Bold);
        scoreText.color = Color.white;
        Stretch(scoreText.rectTransform);
    }

    private void CreateAccent(RectTransform parent, int side, Color color)
    {
        GameObject accentObject = new GameObject("Accent" + side, typeof(RectTransform), typeof(Image));
        accentObject.transform.SetParent(parent, false);

        Image image = accentObject.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(color.r, color.g, color.b, 0.9f);
        image.raycastTarget = false;

        RectTransform rect = accentObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = new Vector2(280f, 8f);
        rect.anchoredPosition = new Vector2(150f * side, 14f);
    }

    private RectTransform CreatePanel(string name, Transform parent, Color color, int radius)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 32f / Mathf.Max(1, radius);
        image.color = color;
        image.raycastTarget = false;

        return panelObject.GetComponent<RectTransform>();
    }

    private TMP_Text CreateText(string name, Transform parent, float size, FontStyles style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font == null)
        {
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.enableWordWrapping = false;
        text.raycastTarget = false;

        Material instancedMaterial = text.fontMaterial;
        if (instancedMaterial != null)
        {
            text.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            text.outlineWidth = 0.12f;
        }

        return text;
    }


    private void Punch(ref Coroutine slot, RectTransform target, Image image, Color color)
    {
        if (slot != null) StopCoroutine(slot);
        slot = StartCoroutine(PunchRoutine(target, image, color));
    }

    private IEnumerator PunchRoutine(RectTransform target, Image image, Color color)
    {
        Color idle = new Color(color.r, color.g, color.b, 0.16f);
        Color flash = new Color(color.r, color.g, color.b, 0.85f);

        float duration = 0.45f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);

            float scale = 1f + 0.35f * Mathf.Sin(t * Mathf.PI) * (1f - t * 0.35f);
            target.localScale = Vector3.one * scale;

            if (image != null)
            {
                image.color = Color.Lerp(flash, idle, t);
            }

            yield return null;
        }

        target.localScale = Vector3.one;
        if (image != null) image.color = idle;
    }

    private void ShowPopup(string title, string who, Color color)
    {
        if (popupCoroutine != null) StopCoroutine(popupCoroutine);
        popupCoroutine = StartCoroutine(ShowPopupCoroutine(title, who, color));
    }

    private IEnumerator ShowPopupCoroutine(string title, string who, Color color)
    {
        popupTitle.text = title;
        popupTitle.color = color;
        popupSubtitle.text = $"{who}  •  {aiScore} × {playerScore}";
        popupAccent.color = color;

        popupRect.gameObject.SetActive(true);

        float inDuration = 0.22f;
        float time = 0f;
        while (time < inDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / inDuration);
            popupGroup.alpha = t;
            popupRect.localScale = Vector3.one * EaseOutBack(t, 0.7f, 1f);
            yield return null;
        }

        popupGroup.alpha = 1f;
        popupRect.localScale = Vector3.one;

        float hold = Mathf.Max(0f, popupDuration - 0.22f - 0.28f);
        yield return new WaitForSecondsRealtime(hold);


        float outDuration = 0.28f;
        time = 0f;
        while (time < outDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / outDuration);
            popupGroup.alpha = 1f - t;
            popupRect.localScale = Vector3.one * Mathf.Lerp(1f, 1.08f, t);
            yield return null;
        }

        popupGroup.alpha = 0f;
        popupRect.localScale = Vector3.one;
        popupRect.gameObject.SetActive(false);
        popupCoroutine = null;
    }

    private static float EaseOutBack(float t, float from, float to)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float eased = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        return Mathf.LerpUnclamped(from, to, eased);
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;

        roundedSprite = BuildRoundedSprite(32);
        return roundedSprite;
    }

    private static Sprite BuildRoundedSprite(int radius)
    {
        int size = radius * 2 + 4;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name = "RoundedRect"
        };

        Color[] pixels = new Color[size * size];
        float r = radius;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                float cx = Mathf.Clamp(px, r, size - r);
                float cy = Mathf.Clamp(py, r, size - r);

                float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(r - distance + 0.5f); // borda suavizada

                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius)
        );
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Center(RectTransform rect)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}