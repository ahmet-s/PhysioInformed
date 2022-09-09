using UnityEngine;
using DG.Tweening;

public class SlideAuto : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] Vector2 toPosition;

    RectTransform rectTransform;
    Vector2 startPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        Expand();
    }

    private void OnDisable()
    {
        rectTransform.anchoredPosition = startPos;
    }

    public void Expand()
    {
        rectTransform.DOAnchorPos(toPosition, duration);
    }

    public void Collapse()
    {
        rectTransform.DOAnchorPos(startPos, duration);
    }
}
