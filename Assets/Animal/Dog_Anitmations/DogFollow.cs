using UnityEngine;

public class DogFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2f;
    public float speed = 3f;

    private bool playerNearby = false;
    private bool isFollowing = false;

    public Animator animator;
    public AudioSource audioSource; // ADD THIS

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Stop();
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.V))
        {
            isFollowing = true;

            // Bark once when called
            audioSource.Play();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            isFollowing = false;

            animator.SetBool("isRunning", false);

            // Stop sound
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

            // Play running sound (loop)
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            animator.SetBool("isRunning", false);
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