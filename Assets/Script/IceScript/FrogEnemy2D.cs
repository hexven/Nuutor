using UnityEngine;

public class FrogEnemy2D : MonoBehaviour
{
    public float jumpHeight = 2f;       // ความสูงในการกระโดด
    public float jumpSpeed = 5f;        // ความเร็วในการกระโดด
    public float waitTime = 0.5f;       // เวลาพักระหว่างกระโดด
    public float maxJumpDistance = 3f;  // ระยะกระโดดสูงสุดต่อครั้ง
    public float groundCheckDistance = 0.2f; // ระยะตรวจจับพื้น
    public LayerMask groundLayer;       // เลเยอร์สำหรับพื้น

    private GameObject player;
    private bool isJumping = false;
    private bool isGrounded = false;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    void Start()
    {
        TryFindPlayer();
        timer = waitTime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null) Debug.LogError("ไม่พบ BoxCollider2D ใน FrogEnemy2D");
    }

    void Update()
    {
        // พยายามหา player ถ้ายังไม่พบ
        if (player == null)
        {
            TryFindPlayer();
            return;
        }

        // ตรวจสอบว่าแตะพื้นหรือไม่
        CheckGround();

        if (!isJumping && isGrounded && player != null)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                StartJump();
            }
        }
        else if (isJumping)
        {
            PerformJump();
        }

        // ล็อก rotation ไม่ให้เอียง
        transform.rotation = Quaternion.identity;

        // Flip Sprite ตามทิศทาง
        if (spriteRenderer != null && player != null)
        {
            spriteRenderer.flipX = player.transform.position.x < transform.position.x;
        }
    }

    void TryFindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogWarning("ไม่พบ GameObject ที่มี Tag 'Player' - จะพยายามค้นหาอีกครั้ง");
    }

    void CheckGround()
    {
        // ใช้ Raycast เพื่อตรวจจับพื้น
        Vector2 rayOrigin = (Vector2)transform.position + boxCollider.offset;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, groundLayer);
        
        // วาด raycast เพื่อ debug
        Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, Color.red);
        
        isGrounded = hit.collider != null;

        // ปรับตำแหน่งให้อยู่เหนือพื้นถ้าตกลงไป
        if (hit.collider != null && transform.position.y < hit.point.y)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            isJumping = false; // หยุดการกระโดดถ้าปรับตำแหน่ง
        }
    }

    void StartJump()
    {
        if (!isGrounded || player == null) return;

        isJumping = true;
        startPos = transform.position;

        // คำนวณทิศทางและระยะทางไปยัง player
        Vector2 playerPos = player.transform.position;
        Vector2 direction = (playerPos - startPos).normalized;
        float distanceToPlayer = Vector2.Distance(startPos, playerPos);

        // จำกัดระยะกระโดดไม่ให้เกิน maxJumpDistance
        float jumpDistance = Mathf.Min(distanceToPlayer, maxJumpDistance);
        targetPos = startPos + direction * jumpDistance;

        // ตรวจสอบว่าตำแหน่งเป้าหมายอยู่เหนือพื้น
        RaycastHit2D targetHit = Physics2D.Raycast(targetPos, Vector2.down, groundCheckDistance, groundLayer);
        if (targetHit.collider != null)
        {
            targetPos.y = targetHit.point.y; // ปรับ Y ให้อยู่ที่พื้น
        }

        timer = 0;
    }

    void PerformJump()
    {
        timer += Time.deltaTime * jumpSpeed;

        // เคลื่อนที่แบบโค้ง parabola
        Vector2 newPos = Vector2.Lerp(startPos, targetPos, timer);
        float height = Mathf.Sin(Mathf.PI * timer) * jumpHeight;
        newPos.y += height;

        // ตั้งตำแหน่งใหม่
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

        if (timer >= 1f)
        {
            isJumping = false;
            timer = waitTime;
        }
    }

    // ใช้สำหรับ debug ใน Unity Editor
    void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = Color.red;
            Vector2 rayOrigin = (Vector2)transform.position + boxCollider.offset;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * groundCheckDistance);
        }
    }
}