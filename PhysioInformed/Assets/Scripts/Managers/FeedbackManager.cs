using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class FeedbackManager : MonoBehaviour
{
    private static FeedbackManager instance;

    //Choice Talk Variables with weights in OSCE
    InkList[] choiceTalkOSCEs = new InkList[4];
    List<string> _OSCE_Steps = new List<string> { "IS", "IRfC", "BR", "PfSDM", "GTO", "ADMP", "CC", "StrConsul", "Process" };
    [SerializeField] Image[] _OSCE_StepsAchievements;
    
    int totalStrConsulPoints;
    int missingOSCESteps = 0;


    ////Scores
    float scoreStrConsul = 0.0f;
    int totalAchievementsTaken = 0;

    //Additional Game Achievements
    List<string> additionalAchievementsCodes = new List<string> { "Fast&Furious", "Tidsoptomist", "FastnotFurious"};
    [SerializeField] Image[] additionalAchievements;

    //Achievements
    Dictionary<string, Image> achievementsList = new Dictionary<string, Image>();

    //Achievements menu
    [SerializeField] GameObject[] contents;
    Image lastTab;
    Shadow lastTabShadow;



    public static FeedbackManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;

        PrepareAchievements();
    }

    void PrepareAchievements()
    {
        List<List<string>> _OSCE_Lists = new List<List<string>>
        { _OSCE_Steps, additionalAchievementsCodes };

        List<Image[]> _OSCE_Achievements = new List<Image[]>
        { _OSCE_StepsAchievements, additionalAchievements };

        //Prepare achievements dict
        int index = 0;
        foreach (List<string> item in _OSCE_Lists)
        {
            for (int i = 0; i < item.Count; i++)
            {
                achievementsList.Add(item[i], _OSCE_Achievements[index][i]);
            }

            index++;
        }
    }

    public void SetChoiceTalkResults(InkList[] playerOSCEPoints)
    {
        choiceTalkOSCEs = playerOSCEPoints;

        ChoiceTalkCalculations(0, /*_IS_OSCE,*/ 2);
        ChoiceTalkCalculations(1, /*_IRfC_OSCE,*/ 2);
        ChoiceTalkCalculations(2, /*_BR_OSCE,*/ 2);
        ChoiceTalkCalculations(3, /*_PfSDM_OSCE,*/ 3);
    }

    void ChoiceTalkCalculations(int choiceTalkIndex, /*List<string> _OSCEStep,*/ int stepPoints)
    {
        InkList playerOSCEPoints = choiceTalkOSCEs[choiceTalkIndex];

        if (playerOSCEPoints.Count == stepPoints)
        {
            AchievementStatus(_OSCE_Steps[choiceTalkIndex]);
        }
        else if(playerOSCEPoints.Count == 0)
        {
            missingOSCESteps++; 
        }
    }    

    public void SetOptionTalkResults(int[] totalOSCEPoints, int[] playerOSCEPoints)
    {
        int count = 0;
        int index = 0;
        foreach(int points in playerOSCEPoints)
        {
            if (points >= Mathf.FloorToInt(totalOSCEPoints[index]/2f))
            {
                count++;                
            }

            index++;
        }

        if (count == 3)
        {
            AchievementStatus("GTO");
        }
    }

    public void SetDecisionTalkResults(InkList playerADMPPoints, InkList playerCCPoints)
    {
        int countADMPPoints = playerADMPPoints.Count;

        if (countADMPPoints == 3)
        {
            
            AchievementStatus("ADMP");
        }
        else if(countADMPPoints == 0)
        {
            missingOSCESteps++;
        }
        
        //Because doctor thanks anyway in the current game, may change in other games
        if(playerCCPoints.Count == 2)
        {
            AchievementStatus("CC");
        }
    }

    //Can be called multiple times during game, so just take scores
    public void SetStructuringOSCE(int totalPoints, int playerPoints)
    {
        totalStrConsulPoints += totalPoints;
        scoreStrConsul += playerPoints;
    }

    public void AdditionalOSCECalculations()
    {
        if(scoreStrConsul == totalStrConsulPoints)
        {            
            AchievementStatus("StrConsul");
        }

        if (missingOSCESteps == 0)
        {
            AchievementStatus("Process");
        }
    }

    public void AchievementStatus(string achievementCode, bool earned = true, bool refresh = false)
    {
        Color tempColor = achievementsList[achievementCode].color;
        tempColor.a = 1f;
        achievementsList[achievementCode].color = tempColor;

        if (earned)
        {
            string spritePath = $"Achievements/Earned/{achievementCode}";
            achievementsList[achievementCode].transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(spritePath);
            totalAchievementsTaken++;
        }
        else if (refresh)
        {
            string spritePath = $"Achievements/{achievementCode}";
            achievementsList[achievementCode].transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(achievementCode);
            if (totalAchievementsTaken > 0) totalAchievementsTaken--;
        }
    }

    public int achievementsCount
    {
        get { return totalAchievementsTaken; }
    }

    public void AchievementstTabsView(Image image)
    {
        if (lastTabShadow != null)
        {
            Color lastMenuColor = lastTab.color;
            lastMenuColor.a = 0.45f;
            lastTab.color = lastMenuColor;

            lastTabShadow.enabled = false;
        }

        Color currentMenuColor = image.color;
        currentMenuColor.a = 1f;
        image.color = currentMenuColor;

        Shadow shadow = image.GetComponent<Shadow>();
        shadow.enabled = true;

        lastTab = image;
        lastTabShadow = shadow;
    }

    public void ConsultationAchieve()
    {
        contents[0].SetActive(true);
        contents[1].SetActive(false);
    }

    public void GameAchieve()
    {
        contents[0].SetActive(false);
        contents[1].SetActive(true);
    }
}
