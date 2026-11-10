using System.Collections;
using UnityEngine;

// The player's car. At the "Head back to your car" stage, pressing E on it ends the chapter:
// the screen fades to black and shows "End of Chapter 1". Before that, it just says you're not done yet.
// Put this on the car's root object (it needs a collider so the E ray can hit it).
public class CarEnding : MonoBehaviour, IInteractable
{
    [Tooltip("Seconds the fade to black takes.")]
    public float fadeTime = 2.5f;

    [Tooltip("Seconds of black before the title appears.")]
    public float titleDelay = 1f;

    public string title = "END OF CHAPTER 1";
    public string subtitle = "To be continued...";

    [Tooltip("What the player says if they try the car too early.")]
    public string tooEarlyLine = "Can't leave yet. They'll kill me if I show up without the snacks.";

    private bool ending;
    private float fade;          // 0 = clear, 1 = fully black
    private float titleAlpha;    // 0 = hidden, 1 = fully visible
    private GUIStyle titleStyle, subtitleStyle;

    public string GetPrompt()
    {
        if (ending) return "";
        return StoryManager.Instance.stage == StoryManager.Stage.ReturnToCar ? "Drive off" : "Get in car";
    }

    public void Interact(PlayerInteractor player)
    {
        if (StoryManager.Instance.stage != StoryManager.Stage.ReturnToCar)
        {
            if (!GameUI.Instance.IsSpeaking) GameUI.Instance.Say("You", tooEarlyLine);
            return;
        }
        StartCoroutine(EndChapter(player));
    }

    IEnumerator EndChapter(PlayerInteractor player)
    {
        ending = true;
        StoryManager.Instance.OnEvent("ChapterEnded");

        // Freeze the player and hide the game HUD (objective, subtitles, crosshair and prompts).
        player.GetComponent<FirstPersonController>().canControl = false;
        player.enabled = false;
        GameUI.Instance.ClearSubtitles();
        GameUI.Instance.SetObjective("");

        // Fade to black, wait a moment, then fade the title in.
        for (float t = 0f; t < 1f; t += Time.deltaTime / fadeTime)
        {
            fade = t;
            yield return null;
        }
        fade = 1f;
        yield return new WaitForSeconds(titleDelay);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 1.5f)
        {
            titleAlpha = t;
            yield return null;
        }
        titleAlpha = 1f;
    }

    void OnGUI()
    {
        if (!ending) return;

        // Draw on top of every other OnGUI script (lower depth = drawn later, on top).
        GUI.depth = -100;

        GUI.color = new Color(0f, 0f, 0f, fade);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (titleAlpha <= 0f) return;
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            titleStyle.fontSize = Mathf.Max(24, Screen.height / 12);
            subtitleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic };
            subtitleStyle.fontSize = Mathf.Max(16, Screen.height / 30);
        }
        titleStyle.normal.textColor = new Color(0.85f, 0.1f, 0.1f, titleAlpha);     // blood red
        subtitleStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, titleAlpha);
        GUI.Label(new Rect(0f, Screen.height * 0.38f, Screen.width, Screen.height * 0.15f), title, titleStyle);
        GUI.Label(new Rect(0f, Screen.height * 0.53f, Screen.width, Screen.height * 0.08f), subtitle, subtitleStyle);
    }
}
