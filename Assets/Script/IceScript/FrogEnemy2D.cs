using UnityEngine;

public class FrogEnemy2D : MonoBehaviour
{
    public float jumpHeight = 2f;       // ความสูงในการกระโดด
    public float jumpSpeed = 5f;        // ความเร็วในการกระโดด
    public float waitTime = 0.5f;       // เวลาพักระหว่างกระโดด
    public float jumpStepDistance = 1f; // ระยะกระโดดทีละนิด
    public float groundLevel = 0f;      // NEW: Define the ground level (adjust in Inspector if needed)

    private GameObject player;
    private bool isJumping = false;
    private Vector2 startPos;
    private Vector2 targetPos;
    private float timer;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("ไม่พบ GameObject ที่มี Tag 'Player'");
        timer = waitTime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        // Set ground level to the initial Y-position if not specified
        if (groundLevel == 0f) groundLevel = transform.position.y;
    }

    void Update()
    {
        if (player == null) return;

        if (!isJumping)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                StartJump();
            }
        }
        else
        {
            PerformJump();
        }

        // ล็อก rotation ไม่ให้เอียง
        transform.rotation = Quaternion.identity;

        // Flip Sprite ตามทิศทาง
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = player.transform.position.x < transform.position.x;
        }
    }

    void StartJump()
    {
        isJumping = true;
        startPos = transform.position;

        // คำนวณทิศทางไปยัง player
        Vector2 direction = ((Vector2)player.transform.position - startPos).normalized;

        // กำหนดเป้าหมายทีละนิด (only X-axis movement, keep Y at ground level)
        targetPos = new Vector2(startPos.x + direction.x * jumpStepDistance, groundLevel);

        timer = 0;
    }

    void PerformJump()
    {
        timer += Time.deltaTime * jumpSpeed;

        // เคลื่อนที่แบบโค้ง parabola
        Vector2 newPos = Vector2.Lerp(startPos, targetPos, timer);
        float height = Mathf.Sin(Mathf.PI * timer) * jumpHeight;
        newPos.y += height;

        // Ensure Y-position doesn’t go below ground level
        newPos.y = Mathf.Max(newPos.y, groundLevel);

        // ตั้ง Z คงที่ 0
        transform.position = new Vector3(newPos.x, newPos.y, 0f);

        if (timer >= 1f)
        {
            isJumping = false;
            timer = waitTime;
        }
    }
}