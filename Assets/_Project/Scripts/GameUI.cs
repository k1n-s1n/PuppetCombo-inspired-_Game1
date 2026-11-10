using System.Collections.Generic;
using UnityEngine;

// Draws the story UI: subtitles (speech bar at the bottom) and the current objective (top of screen).
// Put ONE of these on any object in the scene (e.g. an empty "GameUI" object).
// Other scripts reach it through GameUI.Instance, e.g.
//     GameUI.Instance.Say("You", "Ugh, snack run again...");
//     GameUI.Instance.SetObjective("Use the toilet");
public class GameUI : MonoBehaviour
{
    // "Singleton": a static reference to the one GameUI in the scene, so any script can use it
    // without needing an Inspector slot.
    public static GameUI Instance { get; private set; }

    [Header("Subtitles")]
    [Tooltip("Seconds a line stays on screen, plus extra time per character so long lines last longer.")]
    public float baseLineDuration = 1.5f;
    public float secondsPerCharacter = 0.05f;

    [Tooltip("Colour of the speaker's name, e.g. \"You:\".")]
    public Color speakerColour = new Color(1f, 0.85f, 0.4f);

    [Header("Objective")]
    [Tooltip("How long the objective flashes when it changes, so the player notices it.")]
    public float objectiveFlashTime = 1.5f;

    // One line of speech waiting to be shown.
    private struct Line
    {
        public string speaker;
        public string text;
        public float duration;
    }

    // Lines are queued so several can be sent at once and play one after another.
    private readonly Queue<Line> queue = new Queue<Line>();
    private Line currentLine;
    private bool showingLine;
    private float lineEndsAt;

    private string objective = "";
    private float objectiveChangedAt = -100f;

    private GUIStyle subtitleStyle;
    private GUIStyle objectiveStyle;

    // True while a subtitle is on screen or waiting. Story scripts can wait on this.
    public bool IsSpeaking => showingLine || queue.Count > 0;

    void Awake()
    {
        Instance = this;
    }

    // Queue a subtitle. Leave speaker empty for narration with no name.
    // Pass a duration to override the automatic length.
    public void Say(string speaker, string text, float duration = -1f)
    {
        if (duration <= 0f)
            duration = baseLineDuration + text.Length * secondsPerCharacter;

        queue.Enqueue(new Line { speaker = speaker, text = text, duration = duration });
    }

    // Remove any subtitles still on screen or waiting (e.g. when a jumpscare interrupts).
    public void ClearSubtitles()
    {
        queue.Clear();
        showingLine = false;
    }

    // Change the objective at the top of the screen. Pass "" to hide it.
    public void SetObjective(string text)
    {
        objective = text;
        objectiveChangedAt = Time.time;
    }

    void Update()
    {
        // Move on to the next queued line when the current one has finished.
        if (showingLine && Time.time >= lineEndsAt)
            showingLine = false;

        if (!showingLine && queue.Count > 0)
        {
            currentLine = queue.Dequeue();
            lineEndsAt = Time.time + currentLine.duration;
            showingLine = true;
        }
    }

    void CreateStyles()
    {
        subtitleStyle = new GUIStyle(GUI.skin.label);
        subtitleStyle.alignment = TextAnchor.MiddleCenter;
        subtitleStyle.wordWrap = true;
        subtitleStyle.richText = true;   // lets us colour the speaker name with <color> tags
        subtitleStyle.fontSize = Mathf.Max(16, Screen.height / 32);

        objectiveStyle = new GUIStyle(GUI.skin.label);
        objectiveStyle.alignment = TextAnchor.MiddleCenter;
        objectiveStyle.fontSize = Mathf.Max(14, Screen.height / 38);
    }

    void OnGUI()
    {
        if (subtitleStyle == null) CreateStyles();

        if (showingLine) DrawSubtitle();
        if (objective != "") DrawObjective();
    }

    void DrawSubtitle()
    {
        // Semi-transparent black bar along the bottom of the screen.
        float barHeight = Screen.height * 0.12f;
        Rect bar = new Rect(0f, Screen.height - barHeight - Screen.height * 0.04f, Screen.width, barHeight);
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = Color.white;

        string text = currentLine.text;
        if (!string.IsNullOrEmpty(currentLine.speaker))
        {
            string hex = ColorUtility.ToHtmlStringRGB(speakerColour);
            text = "<color=#" + hex + ">" + currentLine.speaker + ":</color> " + text;
        }

        // Keep the text away from the screen edges.
        Rect textRect = new Rect(bar.x + Screen.width * 0.1f, bar.y, Screen.width * 0.8f, bar.height);
        subtitleStyle.normal.textColor = Color.white;
        GUI.Label(textRect, text, subtitleStyle);
    }

    void DrawObjective()
    {
        // Flash between white and yellow for a moment after the objective changes.
        float sinceChange = Time.time - objectiveChangedAt;
        bool flashing = sinceChange < objectiveFlashTime && Mathf.Repeat(sinceChange * 4f, 1f) < 0.5f;
        Color textColour = flashing ? Color.yellow : Color.white;

        Rect rect = new Rect(0f, Screen.height * 0.04f, Screen.width, 40f);
        string text = "Objective: " + objective;

        // Black shadow first, then the coloured text on top, so it's readable on any background.
        objectiveStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height), text, objectiveStyle);
        objectiveStyle.normal.textColor = textColour;
        GUI.Label(rect, text, objectiveStyle);
    }
}
