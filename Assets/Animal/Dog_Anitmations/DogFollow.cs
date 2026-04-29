using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class DogFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2f;
    public float speed = 3f;

    [Header("Mobile UI")]
    public Button CallButton;
    public Button StopButton;

    [Header("Follow Timer")]
    public float followDuration = 30f;

    [Header("Damage")]
    public float damagePerSecond = 0.1f;
    private float _damageTimer = 0f;
    private bool _isTouchingPlayer = false;
    private ThirdPersonController _playerHealth;

    private bool playerNearby = false;
    private bool isFollowing = false;
    private float followTimer = 0f;

    public Animator animator;
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();

        // Cache the health component
        if (player != null)
            _playerHealth = player.GetComponent<ThirdPersonController>();

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

        // ── Follow + Timer ────────────────────────────────────────────────
        if (isFollowing)
        {
            followTimer -= Time.deltaTime;

            if (followTimer <= 0f)
            {
                OnStop();
                return;
            }

            FollowPlayer();
        }

        // ── Damage tick — only while following AND touching player ────────
        if (isFollowing && _isTouchingPlayer && _playerHealth != null)
        {
            _damageTimer += Time.deltaTime;

            if (_damageTimer >= 1f)
            {
                _playerHealth.TakeDamage(damagePerSecond);
                _damageTimer = 0f;
            }
        }
        else
        {
            _damageTimer = 0f; // reset if not touching
        }
    }

    private void OnCall()
    {
        if (!playerNearby) return;
        isFollowing = true;
        followTimer = followDuration;
        audioSource.Play();
    }

    private void OnStop()
    {
        isFollowing = false;
        followTimer = 0f;
        _damageTimer = 0f;
        animator.SetBool("isRunning", false);
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

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            animator.SetBool("isRunning", false);
            audioSource.Stop();
        }
    }

    // ── Proximity + damage collision ──────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            _isTouchingPlayer = true;

            if (_playerHealth == null)
                _playerHealth = other.GetComponent<ThirdPersonController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            _isTouchingPlayer = false;
            _damageTimer = 0f;
        }
    }
}