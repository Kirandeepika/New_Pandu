using UnityEngine;
using StarterAssets;

public class BikeSystem : MonoBehaviour
{
    public ThirdPersonController playerController;

    public GameObject mainPlayer;

    public GameObject bikeRider;

    public MonoBehaviour bikeController;

    public GameObject playerCamera;
    public GameObject bikeCamera;

    public float enterDistance = 3f;

    private bool isRiding = false;

    public AudioSource audioSource;

    public Transform exitPoint;

    void Start()
    {
        bikeRider.SetActive(false);
        bikeCamera.SetActive(false);

        if (bikeController != null)
            bikeController.enabled = false;

        audioSource = GetComponent<AudioSource>();
        if(audioSource != null )
        {
            audioSource.Stop();
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(mainPlayer.transform.position, transform.position);

        // ENTER
        if (distance < enterDistance && Input.GetKeyDown(KeyCode.E) && !isRiding)
        {
            EnterBike();
        }

        // EXIT
        if (isRiding && Input.GetKeyDown(KeyCode.F))
        {
            ExitBike();
        }
    }

    void EnterBike()
    {
        isRiding = true;

        // Disable player control
        if (playerController != null)
            playerController.enabled = false;

        // Hide player visually (optional)
        mainPlayer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;

        // Show rider
        bikeRider.SetActive(true);

        // Enable bike control
        if (bikeController != null)
            bikeController.enabled = true;

        // Switch camera
        playerCamera.SetActive(false);
        bikeCamera.SetActive(true);

        audioSource.Play();
    }

    void ExitBike()
    {
        isRiding = false;

        // Move player
        mainPlayer.transform.position = exitPoint.position;
        mainPlayer.transform.rotation = exitPoint.rotation;

        // Enable player control
        if (playerController != null)
            playerController.enabled = true;

        // Show player mesh
        mainPlayer.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;

        // Disable bike
        if (bikeController != null)
            bikeController.enabled = false;

        // Hide rider
        bikeRider.SetActive(false);

        // Switch camera
        playerCamera.SetActive(true);
        bikeCamera.SetActive(false);

        audioSource.Stop();
    }
}