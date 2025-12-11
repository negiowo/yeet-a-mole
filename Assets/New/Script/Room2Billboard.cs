using TMPro;
using UnityEngine;
using System.Collections;

public class Room2Billboard : MonoBehaviour
{
    public WorldVariable worldVariable;
    public float typeSpeed = 0.015f;

    private TextMeshProUGUI textUI;
    private bool showingClearMessgae = false;
    private bool showingExplodedMessgae = false;
    private Coroutine typingRoutine;

    public string introMessage =
        "Oh hey another mole! \n" +
        "Quick it's gonna explode!";

    public string explodedMessage =
        "Ouch! That hurts!\n" +
        "Maybe these <u>explosive moles</u> should not be hit with a bat. \n " +
        "Press A on your right controller, which is the thumb button close to you, to pull out a shovel. \n" +
        "Now <b>DIG</b> it up.";

    public string clearedMessage =
        "There we go! \n" +
        "The explosive mole dropped a red (explosive) ball. \n" +
        "In the next room you will learn how to use these balls.\n" +
        "Proceed to the next room by pressing the X button on your left controller, which is the thumb button close to you.";

    void Awake()
    {
        textUI = GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        StartCoroutine(TypeText(introMessage));
    }

    void Update()
    {
        // Only start typing when player is here
        if (worldVariable.tutorialStage != 2) return;

        // When an explosive mole explodes, switch message
        if (!showingClearMessgae && !showingExplodedMessgae && worldVariable != null && worldVariable.tutorialRoom2Exploded)
        {
            showingExplodedMessgae = true;
            StartTyping(explodedMessage);
        }

        // When an explosive mole is correctly digged up, switch message again
        if (!showingClearMessgae && worldVariable != null && worldVariable.tutorialRoomClear)
        {
            showingClearMessgae = true;
            StartTyping(clearedMessage);
        }
    }

    private void StartTyping(string msg)
    {
        // stop any previous typing
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }
        typingRoutine = StartCoroutine(TypeText(msg));
    }

    private IEnumerator TypeText(string fullText)
    {
        textUI.text = fullText;
        textUI.maxVisibleCharacters = 0;

        int totalChars = fullText.Length;

        while (textUI.maxVisibleCharacters < totalChars)
        {
            textUI.maxVisibleCharacters++;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        typingRoutine = null;
    }
}