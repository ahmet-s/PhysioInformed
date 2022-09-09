using UnityEngine;
using DG.Tweening;

public class ScaleEffect : MonoBehaviour
{
    [SerializeField] float duration = 0.5f;

    private void OnEnable()
    {
        transform.DOScale(1f, duration);
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.zero;
    }
}
