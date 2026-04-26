using UnityEngine;

public class BuffaloFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 3f;
    public float speed = 2f; // buffalo is slower than dog

    private bool playerNearby = false;
    private bool isFollowing = false;

    private Animator animator;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.Stop(); // no sound at start
    }

    void Update()
    {
        // Press V → start following
        if (playerNearby && Input.GetKeyDown(KeyCode.V))
        {
            isFollowing = true;

            // play buffalo sound once
            if (audioSource != null)
                audioSource.Play();
        }

        // Press L → stop
        if (Input.GetKeyDown(KeyCode.L))
        {
            isFollowing = false;

            animator.SetBool("isRunning", false);

            if (audioSource != null)
                audioSource.Stop();
        }

        if (isFollowing)
        {
            FollowPlayer();
        }
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

            // loop movement sound
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