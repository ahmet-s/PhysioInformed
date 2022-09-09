using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField] List<GameObject> _SDMSteps;  // 3 SDM steps = 3 game steps

    bool sessionStarted = false;
    bool sessionOver = false;

    int sessionCount = 0;
    string patient = "Thomas Johnson";
    string treatment = "";
    List<int> selectedTreatments;    
    List<string> preferences = new List<string>() { "PIAT", "RTfT", "PEDT" }; //Assignment for debug, will take data for patients later
    //int score = 0;
    string timeCompleted;

    public static GameManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
        //sessionCount = PlayerPrefs.GetInt("SessionCount");
        sessionCount++;
        //PlayerPrefs.SetInt("sessionCount", sessionCount);
        //PlayerPrefs.Save();
    }

    public bool gameStarted
    {
        get { return sessionStarted; }

        set { 
            sessionStarted = true;
            NextStep(0);
            }
    }

    public bool gameEnded
    {
        get { return sessionOver; }
    }

    public List<int> selectedTreatmentIndexes
    {
        get { return selectedTreatments; }
        set { selectedTreatments = new List<int>(value); }
    }

    public List<string> patientPreferences
    {
        get { return preferences; }
        set { preferences = new List<string>(value); }
    }

    public string completionTime
    {
        get { return timeCompleted; }
        set { timeCompleted = value; }
    }

    //public int totalScore
    //{
    //    get { return score; }
    //    set { score = value; }
    //}

    //There are 3 steps in game in orderly
    public void NextStep(int nextSDMStep, int currentSDMStep = -1)
    {
        if (currentSDMStep >= 0)   //at start no step to disable
        {
            _SDMSteps[currentSDMStep].SetActive(false);
        }

        _SDMSteps[nextSDMStep].SetActive(true);
    }    

    public void FinalizeSession(string prescribedTreatment)
    {
        if(prescribedTreatment != "None") FeedbackManager.GetInstance().AdditionalOSCECalculations();
        sessionStarted = false;
        sessionOver = true;

        //if (sessionCount == 1) UIManager.GetInstance().InGameAchievements("FirstTrilogy");

        treatment = prescribedTreatment;
        //score = FeedbackManager.GetInstance().TotalScore();

        StartCoroutine(WaitToUpdateGameInfo());

        PlayerPrefs.SetInt("played", 1);
        PlayerPrefs.Save();
    }

    IEnumerator WaitToUpdateGameInfo()
    {
        yield return new WaitUntil(() => timeCompleted != "");

        UIManager.GetInstance().FinalizeSession(patient, treatment, completionTime);
    }

}
