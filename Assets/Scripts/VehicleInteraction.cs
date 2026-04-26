using UnityEngine;

public class VehicleInteraction : MonoBehaviour
{
    [Header("Setup")]
    public GameObject mainPlayer;
    public MonoBehaviour vehicleController;
    public Transform vehicleCameraTarget;
    public Transform exitPoint;

    [Header("Rider Settings")]
    [Tooltip("Drag the character model that stays on the bike/auto here")]
    public GameObject dummyRider;

    private Transform originalPlayerTarget;
    private ThirdPersonCamera mainCamScript;
    private bool isDriving = false;
    private bool isPlayerNear = false;

    void Start()
    {
        if (vehicleController != null) vehicleController.enabled = false;

        // Ensure the dummy rider is hidden at the start
        if (dummyRider != null) dummyRider.SetActive(false);

        mainCamScript = Camera.main.GetComponent<ThirdPersonCamera>();
        if (mainCamScript != null) originalPlayerTarget = mainCamScript.target;
    }

    private void EnterVehicle()
    {
        isDriving = true;
        mainPlayer.SetActive(false);
        vehicleController.enabled = true;

        // Show the dummy rider sitting in the vehicle
        if (dummyRider != null) dummyRider.SetActive(true);

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

        // Hide the dummy rider when you get out
        if (dummyRider != null) dummyRider.SetActive(false);

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