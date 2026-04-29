using UnityEngine;
using UnityEngine.UI;

public class BuffaloFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 3f;
    public float speed = 2f;

    [Header("Mobile UI")]
    public Button CallButton;
    public Button StopButton;

    [Header("Follow Timer")]
    public float followDuration = 30f;

    private bool playerNearby = false;
    private bool isFollowing = false;
    private float followTimer = 0f;

    private Animator animator;
    public AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.Stop();

        if (CallButton != null)
        {
            CallButton.onClick.AddListener(OnCall);
            CallButton.gameObject.SetActive(false);
        }

        if (StopButton != null)
        {
            StopButton.onClick.AddListener(OnStop);
            StopButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // ── Keyboard input ────────────────────────────────────────────────
        if (playerNearby && Input.GetKeyDown(KeyCode.V))
            OnCall();

        if (Input.GetKeyDown(KeyCode.L))
            OnStop();

        // ── Button visibility ─────────────────────────────────────────────
        if (CallButton != null)
            CallButton.gameObject.SetActive(playerNearby && !isFollowing);

        if (StopButton != null)
            StopButton.gameObject.SetActive(isFollowing);

        // ── Follow + Timer logic ──────────────────────────────────────────
        if (isFollowing)
        {
            followTimer -= Time.deltaTime;

            if (followTimer <= 0f)
            {
                OnStop(); // Timer expired → go idle
                return;
            }

            FollowPlayer();
        }
    }

    private void OnCall()
    {
        if (!playerNearby) return;
        isFollowing = true;
        followTimer = followDuration; // Reset timer every time you call

        if (audioSource != null)
            audioSource.Play();
    }

    private void OnStop()
    {
        isFollowing = false;
        followTimer = 0f;
        animator.SetBool("isRunning", false);

        if (audioSource != null)
            audioSource.Stop();
    }

    void FollowPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > followDistance)
        {
            Vector3 target = player.position;
            target.y = transform.position.y;
            Vector3 direction = (target - transform.position).normalized;

            transform.position += direction * speed * Time.deltaTime;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            animator.SetBool("isRunning", true);

            if (audioSource != null && !audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            animator.SetBool("isRunning", false);

            if (audioSource != null)
                audioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }
}