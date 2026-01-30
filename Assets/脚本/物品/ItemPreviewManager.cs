using UnityEngine;
using TMPro;

public class ItemPreviewManager : MonoBehaviour
{
    public static ItemPreviewManager Instance;

    [Header("Preview Settings")]
    public Transform previewPosition;
    public float previewDistance = 2f;
    public Vector3 positionOffset = Vector3.zero;
    public float rotationSpeed = 100f;
    public float minScale = 0.5f;
    public float maxScale = 2f;
    public float scaleSpeed = 5f;
    
    [Header("UI Settings")]
    public GameObject backButton;
    public TMP_Text itemNameText;
    
    private GameObject currentPreviewModel;
    private bool isPreviewActive = false;
    private bool isRotating = false;
    private Vector3 rotationVelocity;
    private float currentScale = 1f;
    private float targetScale = 1f;

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
        if (previewPosition == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                previewPosition = mainCamera.transform;
            }
            else
            {
                Debug.LogError("无法找到主相机！请确保场景中有MainCamera或在Inspector中指定Preview Position");
            }
        }
        
        if (backButton != null)
        {
            backButton.SetActive(false);
        }
        
        if (itemNameText != null)
        {
            itemNameText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isPreviewActive && currentPreviewModel != null)
        {
            UpdatePreviewPosition();
            HandleRotation();
            HandleZoom();
            UpdateScale();
        }
    }

    void UpdatePreviewPosition()
    {
        if (previewPosition != null)
        {
            Vector3 targetPosition = previewPosition.position + previewPosition.forward * previewDistance + positionOffset;
            currentPreviewModel.transform.position = targetPosition;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isRotating = true;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
        
        if (isRotating && previewPosition != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            Vector3 cameraRight = previewPosition.right;
            Vector3 cameraUp = previewPosition.up;
            
            currentPreviewModel.transform.Rotate(cameraUp, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
            currentPreviewModel.transform.Rotate(cameraRight, mouseY * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
    
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetScale += scroll * scaleSpeed * 0.1f;
            targetScale = Mathf.Clamp(targetScale, minScale, maxScale);
        }
    }
    
    void UpdateScale()
    {
        currentScale = Mathf.Lerp(currentScale, targetScale, scaleSpeed * Time.deltaTime);
        currentPreviewModel.transform.localScale = Vector3.one * currentScale;
    }

    public void ShowItemPreview(ItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemData为空，无法显示预览");
            return;
        }

        if (itemData.modelPrefab == null)
        {
            Debug.LogWarning("物品 " + itemData.itemName + " 没有设置3D模型预制体");
            return;
        }

        ClearPreview();

        if (previewPosition != null)
        {
            Vector3 spawnPosition = previewPosition.position + previewPosition.forward * previewDistance;
            currentPreviewModel = Instantiate(itemData.modelPrefab, spawnPosition, Quaternion.identity);
            
            currentPreviewModel.transform.LookAt(previewPosition);
            currentScale = minScale;
            targetScale = minScale;
            currentPreviewModel.transform.localScale = Vector3.one * minScale;
            isPreviewActive = true;
            
            if (itemNameText != null)
            {
                itemNameText.text = itemData.itemName;
                itemNameText.gameObject.SetActive(true);
            }
            
            if (backButton != null)
            {
                backButton.SetActive(true);
            }
        }
    }

    public void ClearPreview()
    {
        if (currentPreviewModel != null)
        {
            Destroy(currentPreviewModel);
            currentPreviewModel = null;
        }
        isPreviewActive = false;
        isRotating = false;
        
        if (itemNameText != null)
        {
            itemNameText.gameObject.SetActive(false);
        }
        
        if (backButton != null)
        {
            backButton.SetActive(false);
        }
    }
    
    public bool IsPreviewActive()
    {
        return isPreviewActive;
    }
}