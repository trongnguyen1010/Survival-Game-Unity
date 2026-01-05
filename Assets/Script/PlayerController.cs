using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    
    [Header("Movement Settings")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed;
    
    // --- THÊM PHẦN NÀY CHO MOBILE ---
    [Header("Mobile Controls")]
    [SerializeField] private Joystick joystick; // Kéo Joystick từ Canvas vào đây
    // --------------------------------

    public Vector3 playerMoveDirection;
    
    [Header("Player Stats")]
    public float playerMaxHealth;
    public float playerHealth;

    public int experience;      // Dùng để tính Level Up (sẽ bị trừ khi lên cấp)
    public int totalScore;      // DÙNG ĐỂ GỬI LEADERBOARD (Chỉ cộng dồn, không bao giờ trừ)

    public int currentLevel;
    public int maxLevel;
    public List<int> playerLevels;

    public Weapon activeWeapon;

    private bool isImmune;
    [SerializeField] private float immunityDuration;
    [SerializeField] private float immunityTimer;    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        for (int i = playerLevels.Count; i < maxLevel; i++)
        {
            playerLevels.Add(Mathf.CeilToInt(playerLevels[playerLevels.Count - 1]* 1.1f + 15));
        }
        playerHealth = playerMaxHealth;

        // Reset điểm tổng về 0 khi bắt đầu game mới
        totalScore = 0;
        
        // Kiểm tra null để tránh lỗi nếu chưa gán UI
        if(UIController.Instance != null)
        {
            UIController.Instance.UpdateHealthSlider();
            UIController.Instance.UpdateExperienceSlider();
        }

        // Tự động ẩn Joystick nếu chơi trên PC (Optional)
        if (joystick != null)
        {
            if (!Application.isMobilePlatform)
            {
                // joystick.gameObject.SetActive(false); // Bỏ comment dòng này nếu muốn ẩn joystick trên PC
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Lấy input từ bàn phím (PC)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // 2. Lấy input từ Joystick (Mobile) và cộng dồn vào
        // Nếu joystick chưa được gán thì bỏ qua để tránh lỗi NullReference
        if (joystick != null) 
        {
            inputX += joystick.Horizontal;
            inputY += joystick.Vertical;
        }

        // 3. Xử lý hướng di chuyển
        // .normalized giúp đi chéo không bị nhanh hơn đi thẳng
        // Dùng Mathf.Clamp để đảm bảo giá trị không vượt quá 1 (phòng trường hợp vừa bấm phím vừa kéo joystick)
        playerMoveDirection = new Vector3(inputX, inputY).normalized;

        // 4. Xử lý Animation
        animator.SetFloat("moveX", playerMoveDirection.x);
        animator.SetFloat("moveY", playerMoveDirection.y);

        // Kiểm tra xem có đang di chuyển không (magnitude > 0)
        if(playerMoveDirection.magnitude == 0)
        {
            animator.SetBool("moving", false);
        }
        else
        {
            animator.SetBool("moving", true); 
        }

        // 5. Xử lý miễn nhiễm sát thương
        if (immunityTimer > 0)
        {
            immunityTimer -= Time.deltaTime;
        }
        else
        {
            isImmune = false;
        }
    }

    void FixedUpdate()
    {
        // Unity 6 dùng linearVelocity, các bản cũ dùng velocity
        // Code này giữ nguyên linearVelocity như bạn cung cấp
        rb.linearVelocity = new Vector3(playerMoveDirection.x * moveSpeed, 
                                        playerMoveDirection.y * moveSpeed);
    } 

    public void TakeDamge(float damge)
    {
        if (!isImmune)
        {
            isImmune = true;
            immunityTimer = immunityDuration;
            playerHealth -= damge;
            
            if(UIController.Instance != null)
                UIController.Instance.UpdateHealthSlider();
            
            if(playerHealth <= 0)
            {
                // --- PHẦN TÍCH HỢP CLOUD & DOCKER ---
                // Khi chết -> Tìm bộ quản lý Leaderboard -> Gửi điểm lên Server
                LeaderboardManager leaderboard = FindObjectOfType<LeaderboardManager>();
                if (leaderboard != null)
                {
                    // Gửi tên là "Player" (hoặc có thể cho nhập tên) và điểm 
                    Debug.Log("Đang gửi điểm lên Docker Server..." + + totalScore);
                    leaderboard.SendScore("Player", totalScore);
                }
                // ------------------------------------
                gameObject.SetActive(false);
                if(GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
        }
    }

    public void GetExperience(int experienceToGet)
    {
        experience += experienceToGet;
        
        //Cộng dồn vào điểm tổng
        totalScore += experienceToGet;

        if(UIController.Instance != null)
            UIController.Instance.UpdateExperienceSlider();
            
        if (currentLevel < playerLevels.Count && experience >= playerLevels[currentLevel - 1])
        {
            LevelUp();
        }
    }

    public void LevelUp()
    {
        // trừ experience đi để reset thanh level, totalScore vẫn giữ nguyên
        experience -= playerLevels[currentLevel -1];
        currentLevel++;
        
        if(UIController.Instance != null)
        {
            UIController.Instance.UpdateExperienceSlider();
            if(UIController.Instance.levelUpButtons.Length > 0)
                 UIController.Instance.levelUpButtons[0].ActivatedButton(activeWeapon);
            UIController.Instance.LevelUpPanelOpen();
        }
    }
}