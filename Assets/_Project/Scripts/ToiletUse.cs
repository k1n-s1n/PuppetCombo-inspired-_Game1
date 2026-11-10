using UnityEngine;
using UnityEngine.InputSystem;

// PLACEHOLDER for the toilet minigame: look at the toilet, press E to start,
// then HOLD E while a bar fills. Letting go pauses the bar. When it's full, the story moves on.
// (The proper aiming minigame is Tier 3 and will replace this.)
// Put this on the Toilet object (it needs a collider so the player's raycast can hit it).
public class ToiletUse : MonoBehaviour, IInteractable
{
    [Tooltip("Seconds of holding E needed to fill the bar.")]
    public float holdTime = 3f;

    [Tooltip("Line the player says when finished.")]
    public string finishedLine = "What a mess... not my problem, heh.";

    private bool inUse;           // true while the minigame is running
    private float progress;       // 0 to 1
    private FirstPersonController playerMovement;
    private GUIStyle labelStyle;

    public string GetPrompt()
    {
        // Only usable at the right point in the story, and not while already in use.
        if (inUse || StoryManager.Instance.stage != StoryManager.Stage.UseToilet) return "";
        return "Use toilet";
    }

    public void Interact(PlayerInteractor player)
    {
        inUse = true;
        progress = 0f;

        // Freeze the player's movement and mouse look while using the toilet.
        playerMovement = player.GetComponent<FirstPersonController>();
        if (playerMovement != null) playerMovement.canControl = false;
    }

    void Update()
    {
        if (!inUse) return;

        // Fill the bar only while E is held down.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.eKey.isPressed)
            progress += Time.deltaTime / holdTime;

        if (progress >= 1f) Finish();
    }

    void Finish()
    {
        inUse = false;
        if (playerMovement != null) playerMovement.canControl = true;

        GameUI.Instance.Say("You", finishedLine);
        StoryManager.Instance.OnEvent("UsedToilet");
    }

    void OnGUI()
    {
        if (!inUse) return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = Mathf.Max(14, Screen.height / 36);
            labelStyle.normal.textColor = Color.white;
        }

        // Bar in the middle-lower part of the screen: dark background, yellow fill.
        float width = Screen.width * 0.3f;
        float height = Screen.height * 0.03f;
        Rect back = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.65f, width, height);

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(back, Texture2D.whiteTexture);
        GUI.color = new Color(1f, 0.85f, 0.2f);
        GUI.DrawTexture(new Rect(back.x + 2f, back.y + 2f, (back.width - 4f) * progress, back.height - 4f), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUI.Label(new Rect(0f, back.y - 40f, Screen.width, 36f), "Hold [E]", labelStyle);
    }
}
