using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_InventorySystem : MonoBehaviour
{
    public static Player_InventorySystem Instance;

    [Header("Inventory Settings")]
    public int maxInventorySize = 8;
    public List<ItemData> inventoryItems = new List<ItemData>();

    [Header("UI Settings")]
    public GameObject inventoryUI;
    public InventorySlot[] inventorySlots;
    public Player_Camera_Control cameraControl;
    public GraphicRaycaster graphicRaycaster;

    private bool isInventoryOpen = false;

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
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }
        
        InitializeInventorySlots();
        
        if (cameraControl == null)
        {
            Debug.LogWarning("Camera Control 未赋值！请在Inspector中拖拽Character_Camera_Control脚本");
        }
        
        if (inventoryUI == null)
        {
            Debug.LogWarning("Inventory UI 未赋值！请在Inspector中拖拽背包UI对象");
        }
    }
    
    void InitializeInventorySlots()
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogWarning("Inventory Slots 未赋值！请在Inspector中拖拽槽位对象");
            return;
        }
        
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    void Update()
    {
        HandleInventoryToggle();
        HandleMouseClick();
    }

    void HandleInventoryToggle()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
           // Debug.Log("E键被按下");
            ToggleInventory();
        }
    }

    void HandleMouseClick()
    {
    }

    public bool AddItem(ItemData item)
    {
        if (inventoryItems.Count < maxInventorySize)
        {
            inventoryItems.Add(item);
            UpdateInventoryUI();
            return true;
        }
        else
        {
            Debug.LogError("背包已满！无法添加物品: " + item.itemName);
            return false;
        }
    }
    
    public bool AddItemFromObject(ItemObject itemObject, GameObject modelPrefab = null)
    {
        if (itemObject == null)
        {
            Debug.LogError("ItemObject为空！");
            return false;
        }
        
        if (!itemObject.isPickupable)
        {
            return false;
        }
        
        ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
        itemData.itemName = itemObject.itemName;
        itemData.icon = itemObject.itemIcon;
        itemData.modelPrefab = modelPrefab;
        
        return AddItem(itemData);
    }

    public bool RemoveItem(ItemData item)
    {
        if (inventoryItems.Contains(item))
        {
            inventoryItems.Remove(item);
            UpdateInventoryUI();
            Debug.Log("物品已从背包移除: " + item.itemName + " (" + inventoryItems.Count + "/" + maxInventorySize + ")");
            return true;
        }
        return false;
    }
    
    void UpdateInventoryUI()
    {
        Debug.Log("UpdateInventoryUI 被调用，物品数量: " + inventoryItems.Count);
        
        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            Debug.LogError("inventorySlots 为空或长度为0！请在Inspector中拖拽槽位对象");
            return;
        }
        
        Debug.Log("槽位数量: " + inventorySlots.Length);
        
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventoryItems.Count)
            {
                Debug.Log("槽位 " + i + " 设置物品: " + inventoryItems[i].itemName);
                inventorySlots[i].SetItem(inventoryItems[i]);
            }
            else
            {
                Debug.Log("槽位 " + i + " 清空");
                inventorySlots[i].ClearSlot();
            }
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (cameraControl != null)
            {
                cameraControl.IsCameraEnabled = false;
                cameraControl.IsRaycastEnabled = false;
            }
            
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (cameraControl != null)
            {
                cameraControl.IsCameraEnabled = true;
                cameraControl.IsRaycastEnabled = true;
            }
            
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = true;
            }
            
            if (ItemPreviewManager.Instance != null)
            {
                ItemPreviewManager.Instance.ClearPreview();
            }
        }
    }

    void CloseInventory()
    {
        isInventoryOpen = false;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraControl != null)
        {
            cameraControl.IsCameraEnabled = true;
        }
        
        if (ItemPreviewManager.Instance != null)
        {
            ItemPreviewManager.Instance.ClearPreview();
        }
    }

    bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null && 
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
