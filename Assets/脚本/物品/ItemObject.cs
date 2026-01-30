using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [Header("物品属性")]
    public string itemName;
    [TextArea]
    public Sprite itemIcon;
    public bool isPickupable = true;
}
