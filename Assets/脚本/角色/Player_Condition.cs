using UnityEngine;

public class Player_Condition : MonoBehaviour
{
    public static Player_Condition Instance;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRecoveryRate = 15f;
    public float staminaDepletedRecoveryDelay = 5f;

    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2.5f;
    public float standingHeight = 2f;
    public float crouchHeight = 1f;

    public float CurrentStamina { get; private set; }
    public bool IsStaminaDepleted { get; private set; }
    public bool CanSprint { get; private set; }
    public bool IsCrouching { get; private set; }

    private float recoveryDelayTimer;
    private bool isRecovering;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CurrentStamina = maxStamina;
        IsStaminaDepleted = false;
        CanSprint = true;
        recoveryDelayTimer = 0f;
        isRecovering = false;
    }

    void Update()
    {
        HandleStaminaRecovery();
    }

    public void ConsumeStamina(float amount)
    {
        if (CurrentStamina > 0)
        {
            CurrentStamina = Mathf.Max(0, CurrentStamina - amount);
            
            if (CurrentStamina <= 0)
            {
                IsStaminaDepleted = true;
                CanSprint = false;
                recoveryDelayTimer = staminaDepletedRecoveryDelay;
                isRecovering = false;
            }
        }
    }

    public void RecoverStamina(float amount)
    {
        if (!IsStaminaDepleted && !isRecovering)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + amount);
            
            if (CurrentStamina > 0)
            {
                CanSprint = true;
            }
        }
    }

    private void HandleStaminaRecovery()
    {
        if (IsStaminaDepleted)
        {
            recoveryDelayTimer -= Time.deltaTime;
            
            if (recoveryDelayTimer <= 0)
            {
                isRecovering = true;
                IsStaminaDepleted = false;
            }
        }
        
        if (isRecovering)
        {
            CurrentStamina += staminaRecoveryRate * Time.deltaTime;
            
            if (CurrentStamina >= maxStamina)
            {
                CurrentStamina = maxStamina;
                isRecovering = false;
                CanSprint = true;
            }
        }
    }

    public void StartSprinting()
    {
        if (CanSprint && !IsStaminaDepleted)
        {
            ConsumeStamina(staminaDrainRate * Time.deltaTime);
        }
    }

    public void StopSprinting()
    {
        if (!IsStaminaDepleted)
        {
            RecoverStamina(staminaRecoveryRate * Time.deltaTime);
        }
    }

    public float GetStaminaPercentage()
    {
        return CurrentStamina / maxStamina;
    }
    
    public void SetCrouch(bool isCrouching)
    {
        IsCrouching = isCrouching;
    }
}