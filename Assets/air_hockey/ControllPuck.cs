using UnityEngine;

public class ControllPuck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody2D rb;
    public float maxPuckSpeed = 15f;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocity.magnitude > maxPuckSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxPuckSpeed;
        }
    }
}
