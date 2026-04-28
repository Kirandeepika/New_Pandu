using UnityEngine;

public class VehicleInteraction : MonoBehaviour
{
    [Header("Setup")]
    public GameObject mainPlayer;
    public MonoBehaviour vehicleController;
    public Transform vehicleCameraTarget;
    public Transform exitPoint;

    [Header("Rider Settings")]
    public GameObject dummyRider;

    [Header("Audio Settings")]
    [Tooltip("Drag the AudioSource component from this vehicle here")]
    public AudioSource vehicleEngineAudio;

    private Transform originalPlayerTarget;
    private ThirdPersonCamera mainCamScript;
    private bool isDriving = false;
    private bool isPlayerNear = false;

    void Start()
    {
        if (vehicleController != null) vehicleController.enabled = false;

        if (dummyRider != null) dummyRider.SetActive(false);

        // Ensure engine is off at the start
        if (vehicleEngineAudio != null) vehicleEngineAudio.Stop();

        mainCamScript = Camera.main.GetComponent<ThirdPersonCamera>();
        if (mainCamScript != null) originalPlayerTarget = mainCamScript.target;
    }

    private void EnterVehicle()
    {
        isDriving = true;
        mainPlayer.SetActive(false);
        vehicleController.enabled = true;

        if (dummyRider != null) dummyRider.SetActive(true);

        // Start the engine sound
        if (vehicleEngineAudio != null) vehicleEngineAudio.Play();

        if (mainCamScript != null)
        {
            mainCamScript.target = vehicleCameraTarget;
        }
    }

    private void ExitVehicle()
    {
        isDriving = false;
        mainPlayer.transform.position = exitPoint.position;
        mainPlayer.SetActive(true);
        vehicleController.enabled = false;

        if (dummyRider != null) dummyRider.SetActive(false);

        // Stop the engine sound
        if (vehicleEngineAudio != null) vehicleEngineAudio.Stop();

        if (mainCamScript != null)
        {
            mainCamScript.target = originalPlayerTarget;
        }
    }

    void Update()
    {
        if (!isDriving && isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            EnterVehicle();
        }
        else if (isDriving && Input.GetKeyDown(KeyCode.F))
        {
            ExitVehicle();
        }
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) isPlayerNear = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) isPlayerNear = false; }
}