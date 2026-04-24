using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class NPCPatrol : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] points;

    [Header("Movement")]
    public float minSpeed = 1.2f;
    public float maxSpeed = 2.0f;

    [Header("Wait Settings")]
    public float minWaitTime = 1f;
    public float maxWaitTime = 3f;

    [Header("Optimization")]
    public Transform player;
    public float activeDistance = 40f;

    private int currentIndex = 0;
    private Transform target;
    private Animator anim;
    private float speed;
    private bool isWaiting = false;

    private CharacterController controller;

    private float gravity = -9.8f;
    private float velocityY;

    void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        speed = Random.Range(minSpeed, maxSpeed);

        // ✅ Always start from first point
        currentIndex = 0;
        target = points[currentIndex];
    }

    void Update()
    {
        if (player == null || points.Length == 0) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > activeDistance)
        {
            if (anim.enabled) anim.enabled = false;
            return;
        }
        else
        {
            if (!anim.enabled) anim.enabled = true;
        }

        if (!isWaiting)
        {
            Move();
        }
    }

    void Move()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        direction.Normalize();

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * 5f
            );
        }

        if (controller.isGrounded && velocityY < 0)
        {
            velocityY = -2f;
        }

        velocityY += gravity * Time.deltaTime;

        Vector3 move = direction * speed;
        move.y = velocityY;

        controller.Move(move * Time.deltaTime);

        anim.SetBool("isWalking", true);

        Vector3 flatPos = transform.position;
        flatPos.y = 0;

        Vector3 flatTarget = target.position;
        flatTarget.y = 0;

        if (Vector3.Distance(flatPos, flatTarget) < 0.3f)
        {
            StartCoroutine(SwitchTarget());
        }
    }

    IEnumerator SwitchTarget()
    {
        isWaiting = true;
        anim.SetBool("isWalking", false);

        float waitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(waitTime);

        // ✅ Go in order: 1 → 2 → 3 → 4 → repeat
        currentIndex++;

        if (currentIndex >= points.Length)
        {
            currentIndex = 0; // loop back to first point
        }

        target = points[currentIndex];

        isWaiting = false;
    }
}