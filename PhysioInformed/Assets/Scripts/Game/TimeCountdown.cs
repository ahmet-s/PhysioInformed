using UnityEngine;
using TMPro;
using DG.Tweening;

public class TimeCountdown : MonoBehaviour
{
    float timer = 60 * 10; //10 minutes
    int minutes;
    int seconds;

    TextMeshProUGUI timerText;

    //For ShakeEffect
    [SerializeField] RectTransform container;
    bool shaked = false;    
    float shakeDuration = 5f;
    Vector3 shakeStrength = new Vector3(5f, 5f, 0f);
    int vibrato = 20;
    float randomness = 90f;

    bool feedbackGiven = false;
    bool timeInfoSent = false;
    bool extraTime = false;

    // Start is called before the first frame update
    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.GetInstance().gameStarted)
        {
            TimeCount();

            //Shake timer when time < 2 min
            if (timer < 120f && !shaked)
            {
                container.DOShakePosition(shakeDuration, shakeStrength, vibrato, randomness, true, false);
                shaked = true;
            }
        }
        
        if (GameManager.GetInstance().gameEnded && !timeInfoSent)
        {
            GameManager.GetInstance().completionTime = timerText.text;
            GiveOnTimeFeedback();

            timeInfoSent = true;
        }

        if (extraTime)
        {
            if (minutes == -5)
            {
                GameManager.GetInstance().FinalizeSession("None");
                extraTime = false;
            }           
        }
    }

    void TimeCount()
    {
        timer -= Time.deltaTime;

        minutes = (int)(timer / 60f);
        seconds = (int)(timer - minutes  * 60f);

        if (timer >= 0)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            timerText.text = string.Format("<color=#C80000>-{0:00}:{1:00}", -minutes, -seconds);

            if (!feedbackGiven)
            {
                extraTime = true;
                UIManager.GetInstance().InGameAchievements("Tidsoptomist");

                feedbackGiven = true;
            }
        }
         
    }

    void GiveOnTimeFeedback()
    {
        if (timer >= 0f)
        {
            UIManager.GetInstance().InGameAchievements("FastnotFurious");
        }
    }

}
