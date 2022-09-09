using UnityEngine.EventSystems;
using UnityEngine;
using DG.Tweening;

public class SlideMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] float duration;
    [SerializeField] float toPositionX;

    RectTransform rectTransform;
    Vector2 startPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.DOAnchorPosX(toPositionX, duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.DOAnchorPosX(startPos.x, duration);
    }
}
