using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMouse : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private Camera mainCamera;

    [Header("Movimento")]
    [SerializeField] private float speed = 26f;
    [SerializeField] private float responsiveness = 0.55f;

    [Header("Impacto")]
    [SerializeField] private float hitTransfer = 0.85f;
    [SerializeField] private float minHitSpeed = 1.5f;
    [SerializeField] private float maxHitSpeed = 18f;

    [Header("Limites")]
    [SerializeField] private float minX = -2.2f;
    [SerializeField] private float maxX = 2.2f;
    [SerializeField] private float minY = -4.3f;
    [SerializeField] private float maxY = -0.3f;

    private Vector2 targetPosition;
    private Vector2 currentVelocity;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        targetPosition = rb2d.position;
    }

    void Update()
    {
        if (mainCamera == null)
            return;

        Vector3 screenPosition = Input.mousePosition;

        if (Input.touchCount > 0)
        {
            screenPosition = Input.GetTouch(0).position;
        }

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        targetPosition = new Vector2(
            Mathf.Clamp(worldPosition.x, minX, maxX),
            Mathf.Clamp(worldPosition.y, minY, maxY)
        );
    }

    void FixedUpdate()
    {
        Vector2 previousPosition = rb2d.position;

        Vector2 desiredPosition = Vector2.Lerp(
            previousPosition,
            targetPosition,
            responsiveness
        );

        Vector2 newPosition = Vector2.MoveTowards(
            previousPosition,
            desiredPosition,
            speed * Time.fixedDeltaTime
        );

        currentVelocity =
            (newPosition - previousPosition) / Time.fixedDeltaTime;

        rb2d.MovePosition(newPosition);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ApplyImpact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        ApplyImpact(collision);
    }

    private void ApplyImpact(Collision2D collision)
    {
        Rigidbody2D other = collision.rigidbody;

        if (other == null)
            return;

        if (other.bodyType != RigidbodyType2D.Dynamic)
            return;

        float impact = Mathf.Min(currentVelocity.magnitude, maxHitSpeed);

        if (impact < minHitSpeed)
            return;

        Vector2 direction =
            (other.position - rb2d.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = currentVelocity.normalized;

        other.linearVelocity += direction * impact * hitTransfer;
    }
}
