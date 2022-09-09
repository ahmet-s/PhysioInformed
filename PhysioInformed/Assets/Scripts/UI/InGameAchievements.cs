using UnityEngine;
using DG.Tweening;

public class InGameAchievements : MonoBehaviour
{
    [SerializeField] float duration;
    [SerializeField] float toPositionX;
    [SerializeField] float interval;

    RectTransform rectTransform;
    float startPosX;

    Sequence sequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosX = rectTransform.anchoredPosition.x;

        sequence = DOTween.Sequence();        
    }

    private void OnEnable()
    {
        sequence.Append(rectTransform.DOAnchorPosX(toPositionX, duration))
            .AppendInterval(interval)
            .Append(rectTransform.DOAnchorPosX(startPosX, duration)).OnComplete(() => this.gameObject.SetActive(false));
    }
}
