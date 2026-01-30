using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MouseButtonState
{
    None,
    Press,
    LongPress
}

public class Player_Camera_Control : MonoBehaviour
{
    [Header("摄像机设置")]
    [Tooltip("鼠标灵敏度，控制摄像机旋转的速度")]
    [SerializeField] private float mouseSensitivity = 2f;
    [Tooltip("摄像机最大垂直角度，限制上下旋转范围")]
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("摄像机晃动开关")]
    [Tooltip("是否启用摄像机晃动效果")]
    [SerializeField] private bool enableCameraShake = true;
    [Header("摄像机晃动设置")]
    [Tooltip("走路时的晃动强度")]
    [SerializeField] private float walkShakeIntensity = 0.05f;
    [Tooltip("走路时的晃动频率")]
    [SerializeField] private float walkShakeFrequency = 5f;
    [Tooltip("跑步时的晃动强度")]
    [SerializeField] private float sprintShakeIntensity = 0.1f;
    [Tooltip("跑步时的晃动频率")]
    [SerializeField] private float sprintShakeFrequency = 7f;
    [Tooltip("下蹲时的晃动强度")]
    [SerializeField] private float crouchShakeIntensity = 0.03f;
    [Tooltip("下蹲时的晃动频率")]
    [SerializeField] private float crouchShakeFrequency = 4f;
    [Tooltip("晃动平滑度，数值越大晃动越平滑")]
    [SerializeField] private float shakeSmoothing = 10f;
    [Tooltip("头部倾斜强度，控制转头时的倾斜程度")]
    [SerializeField] private float headTiltIntensity = 3f;
    [Tooltip("头部倾斜平滑度，数值越大倾斜越平滑")]
    [SerializeField] private float headTiltSmoothing = 5f;

    [Header("射线检测设置")]
    [Tooltip("射线检测距离，控制能检测到物品的最远距离")]
    [SerializeField] private float rayDistance = 10f;

    [Header("鼠标按钮设置")]
    [Tooltip("长按判定时间，超过此时间视为长按")]
    [SerializeField] private float longPressDuration;

    public bool IsCameraEnabled { get; set; } = true;
    public bool IsRaycastEnabled { get; set; } = true;
    public MouseButtonState CurrentMouseButtonState { get; private set; }
    public PlayerState CurrentPlayerState { get; private set; } = PlayerState.OnFoot;
    
    private Transform cameraTransform;
    private float verticalRotation;
    private float pressStartTime;
    private bool isPressing;
    private MouseButtonState mouseButtonState;
    private GameObject targetItem;
    private GameObject targetVehicle;
    private Player_Condition playerCondition;
    private CharacterController playerController;
    private Player_Control playerControl;
    private Vector3 shakeOffset;
    private float shakeTimer;
    private float currentHeadTilt;
    private float targetHeadTilt;
    private float lastMouseX;
    
    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
        playerCondition = GetComponent<Player_Condition>();
        playerController = GetComponent<CharacterController>();
        playerControl = GetComponent<Player_Control>();
        
        if (cameraTransform == null)
        {
            Debug.LogError("找不到摄像机！");
        }
    }
    
    private void Update()
    {
        HandleMouseButtonState();
        
        if (IsCameraEnabled)
        {
            HandleCameraRotation();
            HandleCameraShake();
        }
        
        HandleRaycast();
    }
    
    private void HandleMouseButtonState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isPressing = true;
            pressStartTime = Time.time;
            mouseButtonState = MouseButtonState.Press;
            CurrentMouseButtonState = MouseButtonState.Press;
            
            if (targetItem != null)
            {
                Debug.Log("鼠标按下，记录目标物品: " + targetItem.name);
            }
            else if (targetVehicle != null)
            {
                Debug.Log("鼠标按下，记录目标车辆: " + targetVehicle.name);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isPressing = false;
            mouseButtonState = MouseButtonState.None;
            CurrentMouseButtonState = MouseButtonState.None;
            
            if (targetItem != null)
            {
                Debug.Log("鼠标释放，拾取物品: " + targetItem.name);
                PickupItem(targetItem);
                targetItem = null;
            }
            else if (targetVehicle != null)
            {
                Debug.Log("鼠标释放，上车: " + targetVehicle.name);
                EnterVehicle(targetVehicle);
                targetVehicle = null;
            }
        }
        
        if (isPressing)
        {
            float pressDuration = Time.time - pressStartTime;
            
            if (pressDuration >= longPressDuration)
            {
                mouseButtonState = MouseButtonState.LongPress;
                CurrentMouseButtonState = MouseButtonState.LongPress;
            }
            else
            {
                mouseButtonState = MouseButtonState.Press;
                CurrentMouseButtonState = MouseButtonState.Press;
            }
        }
        else
        {
            mouseButtonState = MouseButtonState.None;
            CurrentMouseButtonState = MouseButtonState.None;
        }
    }
    
    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 左右旋转（玩家对象Y轴）
        transform.Rotate(Vector3.up * mouseX);
        
        // 上下旋转（摄像机X轴）
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxVerticalAngle, maxVerticalAngle);
        
        // 计算头部倾斜（鼠标移动方向决定倾斜方向）
        // 鼠标向左移动(mouseX为负) -> Z轴为负数
        // 鼠标向右移动(mouseX为正) -> Z轴为正数
        if (enableCameraShake)
        {
            targetHeadTilt = mouseX * headTiltIntensity;
        }
        else
        {
            targetHeadTilt = 0;
        }
    }
    
    private void HandleCameraShake()
    {
        if (!enableCameraShake || cameraTransform == null || playerCondition == null || playerController == null)
        {
            return;
        }
        
        bool isInVehicle = playerControl != null && playerControl.CurrentPlayerState == PlayerState.InVehicle;
        
        if (isInVehicle)
        {
            shakeOffset = Vector3.zero;
            currentHeadTilt = 0;
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            return;
        }
        
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool isMoving = moveDirection.magnitude > 0.1f;
        
        if (!isMoving)
        {
            shakeOffset = Vector3.zero;
        }
        else
        {
            float intensity;
            float frequency;
            
            if (playerCondition.IsCrouching)
            {
                intensity = crouchShakeIntensity;
                frequency = crouchShakeFrequency;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && playerCondition.CanSprint)
            {
                intensity = sprintShakeIntensity;
                frequency = sprintShakeFrequency;
            }
            else
            {
                intensity = walkShakeIntensity;
                frequency = walkShakeFrequency;
            }
            
            shakeTimer += Time.deltaTime * frequency;
            
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && playerCondition.CanSprint;
            
            Vector3 targetShake;
            if (isSprinting)
            {
                // 跑步时：既有左右晃动又有上下颠簸
                targetShake = new Vector3(
                    Mathf.Sin(shakeTimer) * intensity,
                    0,
                    Mathf.Cos(shakeTimer * 2f) * intensity * 0.5f
                );
            }
            else
            {
                // 走路和下蹲时：只有左右晃动
                targetShake = new Vector3(
                    0,
                    0,
                    Mathf.Sin(shakeTimer) * intensity
                );
            }
            
            shakeOffset = Vector3.Lerp(shakeOffset, targetShake, Time.deltaTime * shakeSmoothing);
        }
        
        // 平滑头部倾斜
        if (enableCameraShake)
        {
            currentHeadTilt = Mathf.Lerp(currentHeadTilt, targetHeadTilt, Time.deltaTime * headTiltSmoothing);
        }
        else
        {
            currentHeadTilt = 0;
        }
        
        // 应用旋转（基础旋转 + 晃动 + 头部倾斜）
        cameraTransform.localRotation = Quaternion.Euler(
            verticalRotation + shakeOffset.x, 
            0, 
            shakeOffset.z + currentHeadTilt
        );
    }
    
    private void HandleRaycast()
    {
        if (!IsRaycastEnabled)
        {
            targetItem = null;
            targetVehicle = null;
            return;
        }
        
        Vector3 rayOrigin = cameraTransform.position;
        Vector3 rayDirection = cameraTransform.forward;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
        {
           Debug.DrawRay(rayOrigin, rayDirection * hit.distance, Color.red);
            
            if (hit.collider != null && hit.collider.CompareTag("Item"))
            {
                targetItem = hit.collider.gameObject;
                targetVehicle = null;
            }
            else if (hit.collider != null && hit.collider.CompareTag("Car_Door"))
            {
                targetVehicle = hit.collider.gameObject;
                targetItem = null;
            }
            else
            {
                targetItem = null;
                targetVehicle = null;
            }
        }
        else
        {
            targetItem = null;
            targetVehicle = null;
        }
    }
    
    private void PickupItem(GameObject item)
    {
        Debug.Log("PickupItem被调用: " + item.name);
        
        PickupItem pickupItem = item.GetComponent<PickupItem>();
        
        if (pickupItem == null)
        {
            Debug.LogError("物体上没有PickupItem组件！");
            return;
        }
        
        if (pickupItem.itemObject == null)
        {
            Debug.LogError("ItemObject未赋值！请在Inspector中拖拽ItemObject组件");
            return;
        }
        
        if (Player_InventorySystem.Instance == null)
        {
            Debug.LogError("Player_InventorySystem实例不存在！");
            return;
        }
        
        Debug.Log("准备添加物品: " + pickupItem.itemObject.itemName);
        bool success = Player_InventorySystem.Instance.AddItemFromObject(pickupItem.itemObject, pickupItem.modelPrefab);
        
        Debug.Log("AddItem返回: " + success);
        
        if (success)
        {
            Debug.Log("销毁物体: " + item.name);
            Destroy(item);
        }
    }
    
    private void EnterVehicle(GameObject vehicle)
    {
        Debug.Log("EnterVehicle被调用: " + vehicle.name);
        
        // 先在当前物体上查找Vehicle组件
        Vehicle vehicleScript = vehicle.GetComponent<Vehicle>();
        
        // 如果当前物体上没有，尝试在父物体上查找
        if (vehicleScript == null && vehicle.transform.parent != null)
        {
            vehicleScript = vehicle.transform.parent.GetComponent<Vehicle>();
            Debug.Log("在父物体上找到Vehicle组件: " + vehicle.transform.parent.name);
        }
        
        if (vehicleScript == null)
        {
            Debug.LogError("物体及其父物体上都没有Vehicle组件！");
            return;
        }
        
        if (!vehicleScript.canEnter)
        {
            Debug.LogError("此车辆不可上车！");
            return;
        }
        
        if (vehicleScript.enterPosition == null)
        {
            Debug.LogError("车辆未设置上车位置！请在Inspector中设置EnterPosition");
            return;
        }
        
        Debug.Log($"当前玩家位置: {transform.position}");
        Debug.Log($"目标上车位置: {vehicleScript.enterPosition.position}");
        
        // 移动玩家到车辆的上车位置
        // 如果有CharacterController，需要先禁用它才能直接修改位置
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = vehicleScript.enterPosition.position;
            controller.enabled = true;
            Debug.Log("使用CharacterController移动玩家");
        }
        else
        {
            transform.position = vehicleScript.enterPosition.position;
            Debug.Log("直接修改玩家位置");
        }
        
        Debug.Log($"移动后玩家位置: {transform.position}");
        
        // 调用车辆的上车方法，传递Player_Control引用
        vehicleScript.EnterVehicle(playerControl);
        
        // 清除目标车辆
        targetVehicle = null;
    }


}


