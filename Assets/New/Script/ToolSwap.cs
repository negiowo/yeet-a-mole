using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class ToolSwap : MonoBehaviour
{
    [Header("Tools")]
    public GameObject bat;
    public GameObject shovel;
    public GameObject racket;

    private WorldVariable worldVariable;

    [Header("Input")]
    // A button  -> rotate swapping between bat, shovel, and racket
    public InputActionReference toolSwap;
    // B button  -> bat (no longer used)
    public InputActionReference batAction;
    // Side trigger -> racket (no longer used)
    public InputActionReference racketAction;

    private int currentToolIndex; // 0=bat, 1=shovel, 2=racket

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetTool(bat);
        currentToolIndex = 0;
    }

    private void Awake()
    {
        worldVariable = FindAnyObjectByType<WorldVariable>();
    }

    void OnEnable()
    {
        //if (batAction != null)
        //{
        //    batAction.action.performed += OnBatPressed;
        //    batAction.action.Enable();
        //}

        if (toolSwap != null)
        {
            toolSwap.action.performed += OnToolSwapPressed;
            toolSwap.action.Enable();
        }

        //if (racketAction != null)
        //{
        //    racketAction.action.performed += OnRacketPressed;
        //    racketAction.action.Enable();
        //}
    }

    void OnDisable()
    {
        //if (batAction != null)
        //    batAction.action.performed -= OnBatPressed;

        if (toolSwap != null)
            toolSwap.action.performed -= OnToolSwapPressed;

        //if (racketAction != null)
        //    racketAction.action.performed -= OnRacketPressed;
    }

    void SetTool(GameObject tool)
    {
        if (bat != null) bat.SetActive(false);
        if (shovel != null) shovel.SetActive(false);
        if (racket != null) racket.SetActive(false);

        if (tool != null) tool.SetActive(true);
    }

    void OnBatPressed(InputAction.CallbackContext ctx)
    {
        SetTool(bat);
    }

    void OnShovelPressed(InputAction.CallbackContext ctx)
    {
        SetTool(shovel);
    }

    void OnRacketPressed(InputAction.CallbackContext ctx)
    {
        SetTool(racket);
    }

    void OnToolSwapPressed(InputAction.CallbackContext ctx)
    {
        //Debug.Log("TutorialStage: " + worldVariable.tutorialStage);
        //Debug.Log("TutorialRoom2Exploded: " + worldVariable.tutorialRoom2Exploded);

        if (currentToolIndex == 0)
        {
            // Only allow tool swap after the player has seen mole explosion
            if (worldVariable.tutorialStage < 2 || !worldVariable.tutorialRoom2Exploded) return;

            currentToolIndex = 1;
            SetTool(shovel);
        }
        else if (currentToolIndex == 1)
        {
            // Only can pull out racket after the player has entered tutorial room 3
            if (worldVariable.tutorialStage < 3) return;

            worldVariable.tutorialRoom3RacketOut = true;
            currentToolIndex = 2;
            SetTool(racket);
        }
        else if (currentToolIndex == 2)
        {
            currentToolIndex = 0;
            SetTool(bat);
        }
    }
}
