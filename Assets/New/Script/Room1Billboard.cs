using TMPro;
using UnityEngine;
using System.Collections;

public class Room1Billboard : MonoBehaviour
{
    public WorldVariable worldVariable;
    public float typeSpeed = 0.015f;

    private TextMeshProUGUI textUI;
    private bool showingClearMessgae = false;
    private Coroutine typingRoutine;

    public string introMessage =
        "Welcome to the tutorial room! \n" +
        "You should see a mole on the ground and a bat in your hand. \n" +
        "You know what to do. \n" +
        "Give it a <b>\"BONK\"</b>!";

    public string clearedMessage =
        "Well done! \n" +
        "The mole dropped a ball but we will worry about that in a later tutorial room. \n" +
        "For now you can proceed to the next room by pressing the <u>X button</u> on your left controller, \n" +
        "which is the thumb button close to you.";

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
        // When a mole is killed, switch message
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