using TMPro;
using UnityEngine;
using System.Collections;

public class Room4Billboard : MonoBehaviour
{
    public WorldVariable worldVariable;
    public float typeSpeed = 0.015f;

    private TextMeshProUGUI textUI;
    private bool showingIntroMessgae = false;
    private Coroutine typingRoutine;

    public string introMessage =
        "There will be monsters that can shoot bullets at you.\n" +
        "You can either <u>move</u> out of its way or try <u>hitting</u> the monster's bullets back to them.\n\n" +
        "Give it a few attempts \n" +
        "and when you're ready...";

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
        if (worldVariable.tutorialStage != 3) return;

        if (!showingIntroMessgae && worldVariable != null)
        {
            showingIntroMessgae = true;
            StartTyping(introMessage);
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