using UnityEngine;

public class ControllMalletBlue : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
     private Rigidbody2D rb;
 public float minX = -3f;  // borda esquerda da mesa (ajuste pro valor real)
public float maxX = 3f;   // borda direita da mesa (ajuste pro valor real)
public float minY = -5f;   // linha central — não deixa passar pra baixo
public float maxY = 0f;   // topo da mesa (ajuste pro valor real)  



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log($"Mouse world pos: {mousePos}"); // linha temporária pra debug    
        Vector2 targetPos = mousePos;

        targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
        targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
Debug.Log($"Depois do clamp: {targetPos}");
        Vector2 currentPos = rb.position;

        //Calculo de velocidade
        Vector2 desiredVelocity = (targetPos - currentPos) / Time.fixedDeltaTime;

        rb.linearVelocity = desiredVelocity;
    }
}
