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

    private bool playerNearby = false;
    private bool isFollowing = false;

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

        if (isFollowing)
            FollowPlayer();
    }

    private void OnCall()
    {
        if (!playerNearby) return;
        isFollowing = true;

        if (audioSource != null)
            audioSource.Play();
    }

    private void OnStop()
    {
        isFollowing = false;
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