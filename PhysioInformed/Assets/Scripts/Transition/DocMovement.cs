using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class DocMovement : MonoBehaviour
{   
    float walkDuration = 4f;
    float gazeDuration = 0.75f;
    float rotateDuration = 0.6f;
    float uiDuration = 0.4f;
    float fadeDuration = 500f;

    [SerializeField] Transform[] waypointSigns;
    Vector3[] waypoints = new Vector3[2];
    Vector3[] waypoints2 = new Vector3[2];

    [SerializeField] GameObject speechPanel;
    [SerializeField] TextMeshProUGUI recepText;
    [SerializeField] TextMeshProUGUI docText;

    private void Awake()
    {
        DOTween.Init(true, false, LogBehaviour.Verbose);        
    }

    // Start is called before the first frame update
    void Start()
    {
        waypoints.SetValue(waypointSigns[1].position, 0);
        waypoints.SetValue(waypointSigns[2].position, 1);

        waypoints2.SetValue(waypointSigns[3].position, 0);
        waypoints2.SetValue(waypointSigns[4].position, 1);

        DOTween.Sequence()
            .Append(transform.DOMove(waypointSigns[0].position, walkDuration).SetSpeedBased())
            .Join(transform.DORotate(Vector3.up * 80, gazeDuration))
            .Insert(gazeDuration, transform.DORotate(Vector3.up * -80, gazeDuration * 2))
            .Insert(3 * gazeDuration, transform.DORotate(Vector3.zero, gazeDuration))
            .OnComplete(() => StartCoroutine(DialogueAtReception()));   
    }

    IEnumerator DialogueAtReception()
    {
        DOTween.Sequence()
            .Append(speechPanel.transform.DOScaleX(1f, uiDuration))
            .Append(recepText.DOFade(255f, fadeDuration));

        yield return new WaitForSeconds(3f);

        docText.DOFade(255f, fadeDuration);

        yield return new WaitForSeconds(3f);

        speechPanel.transform.DOScaleX(0f, uiDuration).OnComplete(() => speechPanel.SetActive(false));

        yield return new WaitForSeconds(0.9f);

        DOTween.Sequence()
                .Append(transform.DOPath(waypoints, walkDuration))
                .Join(transform.DOLookAt(waypointSigns[2].position, rotateDuration))
                .OnComplete(DocRoomAnim);
    }

    void DocRoomAnim()
    {
        DOTween.Sequence()
                .Append(transform.DOPath(waypoints2, walkDuration))
                .Join(transform.DOLookAt(waypointSigns[4].position, rotateDuration))
                .Insert(walkDuration / 2, transform.DORotate(Vector3.up * 180, rotateDuration))
                .Append(transform.DOMove(waypointSigns[5].position, walkDuration / 2))
                .Join(transform.DORotate(Vector3.up * 90, gazeDuration))
                .OnComplete(() => LevelLoader.GetInstance().LoadNextLevel("Hospital"));
    }
}
