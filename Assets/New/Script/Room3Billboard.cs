using TMPro;
using UnityEngine;
using System.Collections;

public class Room3Billboard : MonoBehaviour
{
    public WorldVariable worldVariable;
    public float typeSpeed = 0.015f;

    private TextMeshProUGUI textUI;
    private bool showingClearMessgae = false;
    private bool showingRacketOutMessgae = false;
    private Coroutine typingRoutine;

    public string introMessage =
        "We are finally here for the <i>fun</i> part! \n" +
        "Press A on your right controller, which is the thumb button close to you, to pull out the racket.";

    public string racketOutMessage =
        "Now look around you can see balls lying around on the floor.\n" +
        "Step 1: pick up a ball with your left trigger.\n " +
        "Step 2: pull the left joystick to move the ball closer to you.\n" +
        "Step 3: throw it onto the floor and wait for it to bounce once.\n" +
        "Step 4: hit it out with your racket, aim for the dummy!\n" +
        "Friendly reminders: \n" +
        "The balls can only hurt enemy if they have BOUNCED.\n" +
        "It will take some practice to land your shots.";

    public string clearedMessage =
        "Congratulation! \n" +
        "Hopefully you have developed some knowledge of how to control your shots. \n" +
        "Proceed to the next and last room by pressing the X button on your left controller, which is the thumb button close to you.";

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
        if (worldVariable.tutorialStage != 3) return;

        // When player pulls out the racket, switch message
        if (!showingClearMessgae && !showingRacketOutMessgae && worldVariable != null && worldVariable.tutorialRoom3RacketOut)
        {
            showingRacketOutMessgae = true;
            StartTyping(racketOutMessage);
        }

        // When the dummy is successfully destroyed, switch message again
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