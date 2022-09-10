using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    [Header("Panels")]
    [SerializeField] GameObject vignettePanel;
    [SerializeField] Image infoPanel;  //image for color warning effect when info changes
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] GameObject achievementsPanel;
    [SerializeField] GameObject[] inGameAchievements;

    [Header("Game Summary")]
    [SerializeField] GameObject[] completeAnim;
    [SerializeField] TextMeshProUGUI patientName;
    [SerializeField] TextMeshProUGUI diagnosis;
    [SerializeField] TextMeshProUGUI prescription;
    [SerializeField] TextMeshProUGUI completionTime;
    [SerializeField] TextMeshProUGUI achievementCount;

    [SerializeField] Image[] tutorialUIElements;
    bool gameStarted = false;

    private void Awake()
    {
        instance = this;
    }

    public static UIManager GetInstance()
    {
        return instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        Image[] tutorialAttentionElements = new Image[]{null, infoPanel, tutorialUIElements[0], tutorialUIElements[1] };
        TutorialManager.GetInstance().ShowTutorial(4, 4, () => { VignetteButton(); }, tutorialAttentionElements);
    }

    public void InfoTextUpdate(string info, bool warning = false)
    {
        infoText.text = info;

        //Warning effect
        if (warning)
        {
            Color warningColor = new Color(0.8f, 0.1f, 0.1f);
            WarningEffect(infoPanel, warningColor);
        }
    }

    void WarningEffect(Image image, Color warningColor)
    {
        Color startColor = image.color;
        float duration = 0.4f;

        DOTween.Sequence().Append(image.DOColor(warningColor, duration)).
            Append(image.DOColor(startColor, duration)).
            Append(image.DOColor(warningColor, duration)).
            Append(image.DOColor(startColor, duration));
    }

    //Adding different functions to choicebuttons
    public void PrepareChoiceButtons(bool addListener, GameObject[] choiceOptions, Action<int> chooseChoice = null)
    {
        if (addListener)
        {
            for (int i = 0; i < choiceOptions.Length; i++)
            {
                int index = i;
                choiceOptions[i].GetComponent<Button>().onClick.AddListener(() => chooseChoice.Invoke(index));
            }
        }
        else
        {
            for (int i = 0; i < choiceOptions.Length; i++)
            {
                choiceOptions[i].GetComponent<Button>().onClick.RemoveAllListeners();
            }
        }
    }

    public void VignetteButton()
    {
        vignettePanel.SetActive(true);
    }

    public void VignetteOkButton()
    {
        vignettePanel.SetActive(false);

        if (!gameStarted)   //game starts after player reads vignette panel
        {
            GameManager.GetInstance().gameStarted = true;
            gameStarted = true;
        }
    }

    public void InGameAchievements(string achievementName)
    {
        foreach (GameObject achievement in inGameAchievements)
        {
            if (achievement.name == achievementName)
            {
                achievement.SetActive(true);
            }
        }

        FeedbackManager.GetInstance().AchievementStatus(achievementName);
    }

    public void FinalizeSession(string patient, string treatment, string time)
    {
        patientName.text = patient;
        prescription.text = treatment;
        completionTime.text = time;
        achievementCount.text = FeedbackManager.GetInstance().achievementsCount.ToString() + " / 12";

        if(treatment == "None")
        {
            completeAnim[0].SetActive(false);
            completeAnim[1].SetActive(true);
        }

        achievementsPanel.SetActive(true);
        infoPanel.gameObject.SetActive(false);
        tutorialUIElements[0].gameObject.SetActive(false);
        tutorialUIElements[1].gameObject.SetActive(false);
    }

    public void CloseAchievements()
    {
        LevelLoader.GetInstance().LoadNextLevel("Menu");
    }
}
