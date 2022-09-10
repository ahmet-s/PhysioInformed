using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;

    [SerializeField] GameObject tutorialOption;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] GameObject[] progressButtons;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] TextAsset tutorialData;

    int showTutorials = -1; //-1: beginning, ask if want tutorial / 1: wants tutorial / 0: not want's tutorial
    
    //For data from json
    [System.Serializable]
    public class Information
    {
        public string text;
        public Vector3 position;
    }

    //For data from json
    [System.Serializable]
    public class Informations
    {
        public Information[] informations;
    }

    Informations info = new Informations();

    int index = 0;
    int startIndex = 0;
    int totalIndex = 0;
    int filler = -1;
    Action fillerAction = null;

    Image[] attentionImages = null;

    private void Awake()
    {
        instance = this;
        info = JsonUtility.FromJson<Informations>(tutorialData.text);
    }

    public static TutorialManager GetInstance()
    {
        return instance;
    }

     public void ShowTutorial(int untilJsonIndex, int stepForAction = -1, Action x = null, Image[] attentionList = null)
    {
        if (totalIndex != untilJsonIndex)  //if not initialized anything yet
        {
            filler = stepForAction;
            fillerAction = x;
            startIndex = index;
            totalIndex = untilJsonIndex;
            attentionImages = attentionList;
        }        

        //Tutorial type
        if (showTutorials == -1)  // chosing if wants tutorial
        {
            tutorialOption.SetActive(true);
        }
        else if (showTutorials == 1)  //wants tutorial
        {
            tutorialText.text = info.informations[index].text;
            tutorialPanel.transform.localPosition = info.informations[index].position;

            tutorialPanel.SetActive(true);

            if (index == stepForAction) x.Invoke();

            //If doesn't have previous page
            if (index == startIndex)
            {
                progressButtons[0].SetActive(false);
            }

            //If doesn't have next page
            if (index == totalIndex)
            {
                progressButtons[1].SetActive(false);
            }

            //Put attention to specific parts in game
            int attentionIndex = index - startIndex;
            bool attend = attentionList != null && attentionList.Length > attentionIndex ? true : false;
            if (attend && attentionList[attentionIndex] != null)
            {
                GetAttention(attentionList[attentionIndex]);                
            }

        }
        else //0, doesnt want tutorial
        {
            //invoke the action at index
            if (index == stepForAction) x.Invoke();

            //Move automatically
            index++;
            if (index <= totalIndex) 
            {
                ShowTutorial(totalIndex, filler, fillerAction, attentionImages);
            }
        }
    }

    void GetAttention(Image image)
    {
        Color startColor = image.color;
        Color warningColor = new Color(0.45f, 0.45f, 0.45f);
        float duration = 0.4f;

        DOTween.Sequence().Append(image.DOColor(warningColor, duration)).
            Append(image.DOColor(startColor, duration)).
            Append(image.DOColor(warningColor, duration)).
            Append(image.DOColor(startColor, duration));
    }

    //Button listener
    public void NextTut()
    {
        tutorialPanel.SetActive(false);

        index++;
        if (index <= totalIndex)
        {
            ShowTutorial(totalIndex, filler, fillerAction, attentionImages);
        }

        //When moved next, activate previous 
        if (!progressButtons[0].activeSelf) 
        {
            progressButtons[0].SetActive(true);
        }
    }

    //Button listener
    public void PreviousTut()
    {
        tutorialPanel.SetActive(false);

        index--;
        if (index >= startIndex)
        {            
            ShowTutorial(totalIndex, filler, fillerAction, attentionImages);
        }

        //When moved previous, activate next
        if (!progressButtons[1].activeSelf) //if moved previous
        {
            progressButtons[1].SetActive(true);
        }

    }

    //Button listener for to close the tutorial panel
    public void CloseButton()
    {
        tutorialPanel.SetActive(false);

        if(fillerAction != null) //if has any action to do
        {
            fillerAction.Invoke();
        }

        index = totalIndex + 1;  
    }

    //Button listener for yes and no in asking if wants tutorial
    public void ChooseTutorial(int status)
    {
        showTutorials = status; // either 1 or 0 for yes and no

        tutorialOption.SetActive(false);

        ShowTutorial(totalIndex, filler, fillerAction, attentionImages);
    }
}
