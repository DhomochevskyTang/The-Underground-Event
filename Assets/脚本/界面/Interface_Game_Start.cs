using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Interface_Game_Start : MonoBehaviour
{
    [Header("按钮效果设置")]
    [Tooltip("鼠标悬停时的缩放比例")]
    public float hoverScale = 1.2f;
    [Tooltip("鼠标悬停时的颜色")]
    public Color hoverColor = Color.yellow;
    [Tooltip("动画速度")]
    public float animationSpeed = 0.15f;

    private ButtonEffect[] buttonEffects;

    void Start()
    {
        FindAllButtons();
    }

    void FindAllButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        buttonEffects = new ButtonEffect[buttons.Length];
        
        Debug.Log($"找到 {buttons.Length} 个按钮");

        for (int i = 0; i < buttons.Length; i++)
        {
            buttonEffects[i] = buttons[i].gameObject.AddComponent<ButtonEffect>();
            buttonEffects[i].Initialize(this, buttons[i]);
            Debug.Log($"已为按钮 {buttons[i].name} 添加效果");
        }
    }

    public void OnStartButtonClicked()
    {
        Debug.Log("开始游戏按钮被点击");
    }

    public void OnContinueButtonClicked()
    {
        Debug.Log("继续游戏按钮被点击");
    }

    public void OnSettingsButtonClicked()
    {
        Debug.Log("设置按钮被点击");
    }

    public void OnExitButtonClicked()
    {
        Debug.Log("退出游戏按钮被点击");
        Application.Quit();
    }
}

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Interface_Game_Start manager;
    private Button button;
    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine currentAnimation;

    public void Initialize(Interface_Game_Start manager, Button button)
    {
        this.manager = manager;
        this.button = button;
        buttonImage = button.GetComponent<Image>();
        originalScale = button.transform.localScale;
        
        // 设置初始颜色为黑色
        originalColor = Color.black;
        buttonImage.color = Color.black;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"鼠标进入按钮: {button.name}");
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateButton(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"鼠标离开按钮: {button.name}");
        
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        currentAnimation = StartCoroutine(AnimateButton(false));
    }

    private System.Collections.IEnumerator AnimateButton(bool isHovering)
    {
        Vector3 startScale = button.transform.localScale;
        Vector3 targetScale = isHovering ? originalScale * manager.hoverScale : originalScale;
        Color startColor = buttonImage.color;
        Color targetColor = isHovering ? manager.hoverColor : originalColor;
        float elapsedTime = 0f;

        Debug.Log($"开始动画: {(isHovering ? "悬停" : "离开")}, 目标颜色: {targetColor}");

        while (elapsedTime < manager.animationSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / manager.animationSpeed;
            button.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            buttonImage.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        button.transform.localScale = targetScale;
        buttonImage.color = targetColor;
        
        Debug.Log($"动画完成: {(isHovering ? "悬停" : "离开")}, 当前颜色: {buttonImage.color}");
    }
}
