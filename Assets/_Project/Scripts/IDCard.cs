using UnityEngine;

// Draws the player's student ID card in the middle of the screen (shown to the cashier at checkout).
// This is where the student number appears at the end of the game.
// Put ONE of these on the GameUI object. The Cashier calls IDCard.Instance.Show() / Hide().
public class IDCard : MonoBehaviour
{
    public static IDCard Instance { get; private set; }

    [Header("Card details")]
    public string institution = "HOLLOW PINES COMMUNITY COLLEGE";
    public string holderName = "Alex Carter";
    public string studentNumber = "33010198";
    public string dateOfBirth = "09/05/2005";
    public string course = "Computer Science";
    public string validUntil = "06/2027";

    [Header("Look")]
    public Color cardColour = new Color(0.93f, 0.91f, 0.84f);    // off-white plastic
    public Color headerColour = new Color(0.45f, 0.08f, 0.08f);  // dark red band
    public Color textColour = new Color(0.12f, 0.12f, 0.15f);

    [Tooltip("Text under the card telling the player what to do (empty = none).")]
    public string hint = "[E] Hand over ID";

    public bool IsShowing { get; private set; }

    private GUIStyle headerStyle, labelStyle, valueStyle, numberStyle, hintStyle;

    void Awake()
    {
        Instance = this;
    }

    public void Show() { IsShowing = true; }
    public void Hide() { IsShowing = false; }

    void CreateStyles(float cardHeight)
    {
        headerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, wordWrap = false };
        headerStyle.fontSize = Mathf.RoundToInt(cardHeight * 0.065f);
        headerStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = Mathf.RoundToInt(cardHeight * 0.045f);
        labelStyle.normal.textColor = new Color(0.4f, 0.4f, 0.42f);

        valueStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, wordWrap = false };
        valueStyle.fontSize = Mathf.RoundToInt(cardHeight * 0.06f);
        valueStyle.normal.textColor = textColour;

        numberStyle = new GUIStyle(valueStyle);
        numberStyle.fontSize = Mathf.RoundToInt(cardHeight * 0.09f);

        hintStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        hintStyle.fontSize = Mathf.Max(16, Screen.height / 30);
        hintStyle.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        if (!IsShowing) return;

        // Card is a credit-card shape (about 1.6 : 1), a bit over half the screen height tall.
        float h = Screen.height * 0.58f;
        float w = h * 1.6f;
        Rect card = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.4f, w, h);
        // (Re)make the text styles if the window size changed, so text scales with the card.
        if (headerStyle == null || headerStyle.fontSize != Mathf.RoundToInt(h * 0.065f)) CreateStyles(h);

        // Drop shadow, card, red header band.
        Box(new Rect(card.x + 8f, card.y + 8f, w, h), new Color(0f, 0f, 0f, 0.5f));
        Box(card, cardColour);
        Rect header = new Rect(card.x, card.y, w, h * 0.18f);
        Box(header, headerColour);
        GUI.Label(header, institution, headerStyle);

        // Photo: grey box with a simple head-and-shoulders silhouette.
        Rect photo = new Rect(card.x + w * 0.05f, card.y + h * 0.26f, w * 0.27f, h * 0.6f);
        Box(photo, new Color(0.62f, 0.66f, 0.7f));
        Color silhouette = new Color(0.3f, 0.32f, 0.36f);
        Box(new Rect(photo.x + photo.width * 0.32f, photo.y + photo.height * 0.15f, photo.width * 0.36f, photo.height * 0.36f), silhouette);
        Box(new Rect(photo.x + photo.width * 0.12f, photo.y + photo.height * 0.58f, photo.width * 0.76f, photo.height * 0.42f), silhouette);

        // Details to the right of the photo, in four rows: a small grey label above each value.
        // Field() returns the height it used, so y moves down row by row.
        float x = photo.xMax + w * 0.05f;
        float colWidth = card.xMax - x - w * 0.04f;
        float y = card.y + h * 0.23f;
        y += Field(x, y, colWidth, "NAME", holderName.ToUpper(), valueStyle);
        y += Field(x, y, colWidth, "STUDENT No.", studentNumber, numberStyle);
        Field(x + colWidth * 0.5f, y, colWidth * 0.5f, "VALID UNTIL", validUntil, valueStyle);   // same row as date of birth
        y += Field(x, y, colWidth * 0.5f, "DATE OF BIRTH", dateOfBirth, valueStyle);
        Field(x, y, colWidth, "COURSE", course, valueStyle);

        if (!string.IsNullOrEmpty(hint))
            GUI.Label(new Rect(0f, card.yMax + 20f, Screen.width, 40f), hint, hintStyle);
    }

    // Draws "LABEL" in small grey text with the value underneath. Returns the height used.
    float Field(float x, float y, float width, string label, string value, GUIStyle style)
    {
        float labelHeight = labelStyle.fontSize * 1.5f;
        float valueHeight = style.fontSize * 1.5f;
        GUI.Label(new Rect(x, y, width, labelHeight), label, labelStyle);
        GUI.Label(new Rect(x, y + labelHeight, width, valueHeight), value, style);
        return labelHeight + valueHeight;
    }

    // Fills a rectangle with a solid colour.
    static void Box(Rect r, Color c)
    {
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
