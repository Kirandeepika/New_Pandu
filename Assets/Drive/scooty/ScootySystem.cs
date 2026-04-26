using UnityEngine;
using StarterAssets; // Required for ThirdPersonController and StarterAssetsInputs

public class ScootySystem : MonoBehaviour
{
    [Header("References")]
    public ThirdPersonController playerController;
    public GameObject mainPlayer;
    public GameObject bikeRider;
    public MonoBehaviour bikeController;

    [Header("Cameras")]
    public GameObject playerCamera;
    public GameObject bikeCamera;

    [Header("Settings")]
    public float enterDistance = 3f;
    public Transform exitPoint;

    private bool isRiding = false;
    private AudioSource audioSource;
    private CharacterController charController;
    private StarterAssetsInputs playerInputs; // Added this

    void Start()
    {
        bikeRider.SetActive(false);
        bikeCamera.SetActive(false);

        if (bikeController != null)
            bikeController.enabled = false;

        audioSource = GetComponent<AudioSource>();

        if (mainPlayer != null)
        {
            charController = mainPlayer.GetComponent<CharacterController>();
            playerInputs = mainPlayer.GetComponent<StarterAssetsInputs>(); // Cache inputs
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(mainPlayer.transform.position, transform.position);

        if (distance < enterDistance && Input.GetKeyDown(KeyCode.E) && !isRiding)
        {
            EnterBike();
        }
        else if (isRiding && Input.GetKeyDown(KeyCode.F))
        {
            ExitBike();
        }
    }

    void EnterBike()
    {
        isRiding = true;

        // 1. Disable Controls and Physics
        if (playerController != null) playerController.enabled = false;
        if (charController != null) charController.enabled = false;

        // 2. Clear Inputs (Prevents player from "walking" while sitting)
        if (playerInputs != null)
        {
            playerInputs.move = Vector2.zero;
            playerInputs.sprint = false;
        }

        mainPlayer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        bikeRider.SetActive(true);

        if (bikeController != null) bikeController.enabled = true;

        playerCamera.SetActive(false);
        bikeCamera.SetActive(true);

        if (audioSource != null) audioSource.Play();
    }

    void ExitBike()
    {
        isRiding = false;

        if (bikeController != null) bikeController.enabled = false;
        bikeRider.SetActive(false);

        // 1. MUST disable CharacterController before moving transform
        if (charController != null) charController.enabled = false;

        // 2. Move Player
        mainPlayer.transform.position = exitPoint.position;
        mainPlayer.transform.rotation = exitPoint.rotation;

        // 3. Re-enable Physics and Logic
        if (charController != null) charController.enabled = true;
        if (playerController != null) playerController.enabled = true;

        mainPlayer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;

        playerCamera.SetActive(true);
        bikeCamera.SetActive(false);

        if (audioSource != null) audioSource.Stop();
    }
}