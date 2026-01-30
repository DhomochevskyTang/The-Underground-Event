using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("车辆设置")]
    [Tooltip("上车位置，玩家上车后会移动到这里")]
    public Transform enterPosition;
    [Tooltip("下车位置，玩家下车后会移动到这里")]
    public Transform exitPosition;
    [Tooltip("是否可以上车")]
    public bool canEnter = true;
    [Tooltip("车辆最大移动速度")]
    public float maxMoveSpeed = 10f;
    [Tooltip("车辆加速度")]
    public float acceleration = 5f;
    [Tooltip("车辆减速度")]
    public float deceleration = 3f;
    [Tooltip("车辆旋转角度（每帧旋转的角度）")]
    public float rotationAngle = 2f;
    [Tooltip("旋转平滑度，数值越大旋转越平滑")]
    public float rotationSmoothness = 5f;
    [Tooltip("下车按键")]
    public KeyCode exitKey = KeyCode.F;

    private Player_Control playerControl;
    private bool isPlayerInVehicle = false;
    private float currentSpeed;
    private float currentRotationSpeed;

    void Update()
    {
        if (isPlayerInVehicle)
        {
            HandleVehicleMovement();
            HandleExitVehicle();
        }
    }

    public void EnterVehicle(Player_Control control)
    {
        Debug.Log("玩家上车: " + gameObject.name);
        
        playerControl = control;
        if (playerControl == null)
        {
            Debug.LogError("传入的Player_Control为空！");
            return;
        }
        
        isPlayerInVehicle = true;
        playerControl.CurrentPlayerState = PlayerState.InVehicle;
        
        if (enterPosition != null)
        {
            CharacterController controller = playerControl.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
                playerControl.transform.position = enterPosition.position;
                playerControl.transform.rotation = enterPosition.rotation;
                playerControl.transform.SetParent(enterPosition);
                controller.enabled = true;
                Debug.Log("玩家已移动到上车位置并成为子物体");
            }
            else
            {
                playerControl.transform.position = enterPosition.position;
                playerControl.transform.rotation = enterPosition.rotation;
                playerControl.transform.SetParent(enterPosition);
                Debug.Log("玩家已移动到上车位置并成为子物体");
            }
        }
        
        Debug.Log("玩家状态已切换为: InVehicle");
    }

    private void HandleVehicleMovement()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        
        bool isMoving = Mathf.Abs(vertical) > 0.01f;
        
        if (isMoving)
        {
            float targetSpeed = vertical * maxMoveSpeed;
            
            if (currentSpeed < targetSpeed)
            {
                currentSpeed += acceleration * Time.deltaTime;
                if (currentSpeed > targetSpeed)
                {
                    currentSpeed = targetSpeed;
                }
            }
            else if (currentSpeed > targetSpeed)
            {
                currentSpeed -= deceleration * Time.deltaTime;
                if (currentSpeed < targetSpeed)
                {
                    currentSpeed = targetSpeed;
                }
            }
            
            Vector3 movement = transform.forward * currentSpeed;
            transform.position += movement * Time.deltaTime;
            
            float rotationInput = Input.GetAxis("Horizontal");
            float targetRotationSpeed = 0f;
            
            if (Mathf.Abs(rotationInput) > 0.01f)
            {
                float rotationDirection = rotationInput > 0 ? 1f : -1f;
                targetRotationSpeed = rotationDirection * rotationAngle;
            }
            
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, targetRotationSpeed, rotationSmoothness * Time.deltaTime);
            transform.Rotate(Vector3.up * currentRotationSpeed);
        }
        else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= deceleration * Time.deltaTime;
                if (currentSpeed < 0)
                {
                    currentSpeed = 0;
                }
            }
            else if (currentSpeed < 0)
            {
                currentSpeed += deceleration * Time.deltaTime;
                if (currentSpeed > 0)
                {
                    currentSpeed = 0;
                }
            }
            
            currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, 0f, rotationSmoothness * Time.deltaTime);
            transform.Rotate(Vector3.up * currentRotationSpeed);
        }
    }

    private void HandleExitVehicle()
    {
        if (Input.GetKeyDown(exitKey))
        {
            ExitVehicle();
        }
    }

    public void ExitVehicle()
    {
        Debug.Log("玩家下车: " + gameObject.name);
        
        if (playerControl == null)
        {
            Debug.LogError("Player_Control引用为空，无法下车！");
            return;
        }
        
        isPlayerInVehicle = false;
        playerControl.CurrentPlayerState = PlayerState.OnFoot;
        
        CharacterController controller = playerControl.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        playerControl.transform.SetParent(null);
        
        if (exitPosition != null)
        {
            playerControl.transform.position = exitPosition.position;
            playerControl.transform.rotation = exitPosition.rotation;
            Debug.Log("玩家已移动到下车位置");
        }
        
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        Debug.Log("玩家状态已切换为: OnFoot");
    }
}

