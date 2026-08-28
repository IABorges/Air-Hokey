using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Puck : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private CircleCollider2D circleCollider;
    private AudioSource audioSource;

    [Header("Movimento")]
    [SerializeField] private float initialSpeed = 6f;
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float hitSpeedGain = 1.06f;

    [Header("Angulo")]
    [SerializeField] private float minVerticalRatio = 0.35f;

    [Header("Limites do campo")]
    [SerializeField] private float minX = -2.3f;
    [SerializeField] private float maxX = 2.3f;
    [SerializeField] private float minY = -4.5f;
    [SerializeField] private float maxY = 4.5f;

    [Header("Gol")]
    [SerializeField] private float goalHalfWidth = 0.7f;

    [Header("Som")]
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private float minimumImpactForSound = 0.5f;
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.25f;

    [Header("Reset")]
    [SerializeField] private float resetDelay = 1.1f;
    [SerializeField] private float stuckTimeLimit = 1.5f;

    private Vector2 initialPosition;
    private bool resetting = false;
    private float stuckTimer;
    private float lastLaunchDirection = 1f;

    private ScoreManager scoreManager;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        audioSource = GetComponent<AudioSource>();

        scoreManager = FindFirstObjectByType<ScoreManager>();

        initialPosition = rb2d.position;

        rb2d.gravityScale = 0f;
        rb2d.linearDamping = 0f;
        rb2d.angularDamping = 0f;

        rb2d.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        LaunchPuck(Random.value > 0.5f ? 1f : -1f);
    }

    void FixedUpdate()
    {
        if (resetting)
            return;

        LimitSpeed();
        KeepMinimumSpeed();
        KeepVerticalMomentum();
        KeepInsideField();
        CheckIfStuck();
    }

    private void LimitSpeed()
    {
        if (rb2d.linearVelocity.magnitude > maxSpeed)
        {
            rb2d.linearVelocity =
                rb2d.linearVelocity.normalized * maxSpeed;
        }
    }

    private void KeepMinimumSpeed()
    {
        float speed = rb2d.linearVelocity.magnitude;

        if (speed < 0.2f)
            return;

        if (speed < minSpeed)
        {
            rb2d.linearVelocity =
                rb2d.linearVelocity.normalized * minSpeed;
        }
    }

    private void KeepVerticalMomentum()
    {
        Vector2 velocity = rb2d.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < 0.2f)
            return;

        float minVerticalSpeed = speed * minVerticalRatio;

        if (Mathf.Abs(velocity.y) >= minVerticalSpeed)
            return;

        float sign = velocity.y >= 0f ? 1f : -1f;
        velocity.y = minVerticalSpeed * sign;

        rb2d.linearVelocity = velocity.normalized * speed;
    }

    private void CheckIfStuck()
    {
        if (rb2d.linearVelocity.magnitude < 0.5f)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer >= stuckTimeLimit)
            {
                stuckTimer = 0f;
                LaunchPuck(-lastLaunchDirection);
            }

            return;
        }

        stuckTimer = 0f;
    }

    private void KeepInsideField()
    {
        Vector2 position = rb2d.position;
        Vector2 velocity = rb2d.linearVelocity;

        float radiusX = circleCollider.bounds.extents.x;
        float radiusY = circleCollider.bounds.extents.y;

        if (position.x - radiusX < minX)
        {
            position.x = minX + radiusX;
            velocity.x = Mathf.Abs(velocity.x);
        }

        if (position.x + radiusX > maxX)
        {
            position.x = maxX - radiusX;
            velocity.x = -Mathf.Abs(velocity.x);
        }

        bool canEnterGoal =
            Mathf.Abs(position.x) + radiusX <= goalHalfWidth;

        if (!canEnterGoal)
        {
            if (position.y - radiusY < minY)
            {
                position.y = minY + radiusY;
                velocity.y = Mathf.Abs(velocity.y);
            }

            if (position.y + radiusY > maxY)
            {
                position.y = maxY - radiusY;
                velocity.y = -Mathf.Abs(velocity.y);
            }
        }

        rb2d.position = position;
        rb2d.linearVelocity = velocity;
    }

    private void LaunchPuck(float directionY)
    {
        lastLaunchDirection = directionY >= 0f ? 1f : -1f;

        float randomX = Random.Range(-0.6f, 0.6f);

        Vector2 direction =
            new Vector2(randomX, lastLaunchDirection).normalized;

        rb2d.linearVelocity = direction * initialSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayCollisionSound(collision);

        if (collision.rigidbody == null)
            return;

        if (collision.rigidbody.bodyType != RigidbodyType2D.Kinematic)
            return;

        rb2d.linearVelocity *= hitSpeedGain;
    }

    private void PlayCollisionSound(Collision2D collision)
    {
        if (audioSource == null || collisionSound == null)
            return;

        float impact = collision.relativeVelocity.magnitude;

        if (impact < minimumImpactForSound)
            return;

        audioSource.pitch = Mathf.Lerp(
            minPitch,
            maxPitch,
            Mathf.InverseLerp(minimumImpactForSound, maxSpeed, impact)
        );

        audioSource.PlayOneShot(
            collisionSound,
            Mathf.Clamp(impact / maxSpeed, 0.35f, 1f)
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (resetting)
            return;

        if (other.CompareTag("GoalTop"))
        {
            if (scoreManager != null)
            {
                scoreManager.PlayerScored();
            }

            StartCoroutine(ResetPuck(1f));
        }

        else if (other.CompareTag("GoalBottom"))
        {
            if (scoreManager != null)
            {
                scoreManager.AIScored();
            }

            StartCoroutine(ResetPuck(-1f));
        }
    }

    private IEnumerator ResetPuck(float directionY)
    {
        resetting = true;
        stuckTimer = 0f;

        rb2d.linearVelocity = Vector2.zero;
        rb2d.angularVelocity = 0f;

        rb2d.position = initialPosition;

        yield return new WaitForSeconds(resetDelay);

        LaunchPuck(directionY);

        resetting = false;
    }
}
