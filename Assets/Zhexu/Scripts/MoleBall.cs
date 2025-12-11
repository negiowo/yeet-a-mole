using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
public class MoleBall : MonoBehaviour
{
    public enum BallState { Spawning, Held, Thrown, ReadyToYeet, Yeeted }

    private WorldVariable worldVariable;
    private Collider racketCol;

    [Header("Status")]
    public BallState currentState = BallState.Spawning;
    public int damage = 1;

    [Header("Visuals")]
    public Material normalMat; // Normal brown color
    public Material readyMat;  // Glowing yellow color (indicates it's ready to Yeet)
    public Material yeetMat;   // Light blue color (indicates it's yeeted by the racket)
    public bool isExplosive = false;
    private Rigidbody rb;
    private XRGrabInteractable interactable;
    private Renderer rend;
    private Coroutine pullRoutine;

    void Start()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        if (normalMat != null)
        {
            rend.material = normalMat;
        }
        
        currentState = BallState.Spawning;

        // find the world variable holder object
        worldVariable = FindFirstObjectByType<WorldVariable>();

        // find the racket's collider
        GameObject racketObj = GameObject.FindGameObjectWithTag("Racket");
        if (racketObj != null)
        {
            racketCol = racketObj.GetComponent<Collider>();
        }

        // ignore racket collision by default
        //if (racketCol != null)
        //{
        //    Physics.IgnoreCollision(racketCol, GetComponent<Collider>(), true);
        //}
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        interactable = GetComponent<XRGrabInteractable>();
        rend = GetComponent<Renderer>();

        // Subscribe to grab and release events
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnThrow);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        currentState = BallState.Held;
        // When grabbed, reset the material
        if(normalMat != null) rend.material = normalMat;
    }

    void OnThrow(SelectExitEventArgs args)
    {
        currentState = BallState.Thrown;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Core Logic: Only balls that are thrown and hit the ground become "bullets"
        if (currentState == BallState.Thrown && collision.gameObject.CompareTag("Ground"))
        {
            ActivateYeetState();
        }

        //Debug.Log($"Ball '{name}' collided with {collision.collider.name} ");
    }

    void ActivateYeetState()
    {
        currentState = BallState.ReadyToYeet;
        // Debug.Log("Ball is READY TO YEET!");
        
        // Visual feedback: change color
        if (readyMat != null) rend.material = readyMat;

        // Enable collision with racket when ready to yeet
        //if (racketCol != null)
        //{
        //    Physics.IgnoreCollision(racketCol, GetComponent<Collider>(), false);
        //}
    }

    public void yeeted()
    {
        currentState = BallState.Yeeted;

        // Visual feedback: change color
        if (yeetMat != null) rend.material = yeetMat;

        // In the actual level, yeeted balls disappear in 3 seconds to prevent overflowing objects in scene
        if (worldVariable.tutorialStage > 4) Destroy(gameObject, 3f);
    }
}