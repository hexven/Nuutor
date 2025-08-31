using UnityEngine;

public class FrogEnemy2D : MonoBehaviour
{
    public float jumpHeight = 2f;       // ความสูงในการกระโดด
    public float jumpSpeed = 5f;        // ความเร็วในการกระโดด
    public float waitTime = 0.5f;       // เวลาพักระหว่างกระโดด
    public float jumpStepDistance = 1f; // ระยะกระโดดทีละนิด
    public float spawnOffsetY = 0f;     // Offset to adjust spawn position above terrain

    private GameObject player;
    private bool isJumping = false;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float timer;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.LogError("ไม่พบ GameObject ที่มี Tag 'Player'");
        timer = waitTime;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer component not found");

        // Adjust initial position to align with terrain
        Vector3 initialPos = transform.position;
        float terrainY = Terrain.activeTerrain.SampleHeight(new Vector3(initialPos.x, 0, initialPos.z));
        if (terrainY != initialPos.y)
        {
            transform.position = new Vector3(initialPos.x, terrainY + spawnOffsetY, initialPos.z);
            Debug.Log($"Frog spawned at X: {initialPos.x}, Y: {terrainY + spawnOffsetY}, Z: {initialPos.z}. Terrain detected at Y: {terrainY}");
        }
        else
        {
            Debug.LogWarning($"No valid terrain height at X: {initialPos.x}, Z: {initialPos.z}. Using fallback Y: {initialPos.y}");
        }
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
        Vector3 direction = (player.transform.position - startPos).normalized;

        // กำหนดเป้าหมายทีละนิด (only X/Z-axis movement)
        Vector3 potentialTargetPos = new Vector3(startPos.x + direction.x * jumpStepDistance, startPos.y, startPos.z + direction.z * jumpStepDistance);

        // ใช้ terrain height ที่ targetPos
        float targetGroundY = Terrain.activeTerrain.SampleHeight(new Vector3(potentialTargetPos.x, 0, potentialTargetPos.z));

        // อัปเดต startPos และ targetPos ให้ยึดตามความสูงของ terrain
        startPos = new Vector3(startPos.x, Terrain.activeTerrain.SampleHeight(new Vector3(startPos.x, 0, startPos.z)) + spawnOffsetY, startPos.z);
        targetPos = new Vector3(potentialTargetPos.x, targetGroundY + spawnOffsetY, potentialTargetPos.z);

        timer = 0;
    }

    void PerformJump()
    {
        timer += Time.deltaTime * jumpSpeed;

        // เคลื่อนที่แบบโค้ง parabola
        Vector3 newPos = Vector3.Lerp(startPos, targetPos, timer);
        float height = Mathf.Sin(Mathf.PI * timer) * jumpHeight;
        newPos.y += height;

        // Ensure Y-position doesn’t go below the terrain height at current X/Z
        float currentGroundY = Terrain.activeTerrain.SampleHeight(new Vector3(newPos.x, 0, newPos.z));
        newPos.y = Mathf.Max(newPos.y, currentGroundY + spawnOffsetY);

        // ตั้ง Z คงที่ 0
        transform.position = new Vector3(newPos.x, newPos.y, newPos.z);

        if (timer >= 1f)
        {
            isJumping = false;
            timer = waitTime;
        }
    }
}