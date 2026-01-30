using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image itemIcon;
    public Sprite defaultSlotSprite;
    
    [Header("Hover Effect Settings")]
    public float hoverScale = 1.2f;
    public float animationSpeed = 0.15f;
    
    private ItemData currentItemData;
    private Vector3 originalScale;
    private Coroutine currentAnimation;
    
    void Start()
    {
        if (itemIcon == null)
        {
            itemIcon = GetComponent<Image>();
        }
        
        originalScale = transform.localScale;
        
        if (defaultSlotSprite == null && itemIcon.sprite != null)
        {
            defaultSlotSprite = itemIcon.sprite;
        }
    }
    
    public void SetItem(ItemData itemData)
    {
        currentItemData = itemData;
        
        if (itemIcon == null)
        {
            Debug.LogError("itemIcon 为空！请在Inspector中拖拽Image组件");
            return;
        }
        
        if (itemData != null && itemData.icon != null)
        {
            itemIcon.sprite = itemData.icon;
            itemIcon.enabled = true;
        }
        else
        {
            ClearSlot();
        }
    }
    
    public void ClearSlot()
    {
        currentItemData = null;
        
        if (itemIcon != null)
        {
            if (defaultSlotSprite != null)
            {
                itemIcon.sprite = defaultSlotSprite;
            }
            else
            {
                itemIcon.sprite = null;
            }
            itemIcon.enabled = true;
        }
    }
    
    public ItemData GetItemData()
    {
        return currentItemData;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItemData != null)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
            }
            currentAnimation = StartCoroutine(ScaleAnimation(originalScale * hoverScale));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(ScaleAnimation(originalScale));
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItemData != null && ItemPreviewManager.Instance != null)
        {
            ItemPreviewManager.Instance.ShowItemPreview(currentItemData);
        }
    }
    
    private System.Collections.IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationSpeed;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}
