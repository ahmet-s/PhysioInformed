using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;

public class Dialogue_OptionTalk : MonoBehaviour
{
    private static UI_OptionTalk _UI;

    [Header("Dialogue UI")]
    [SerializeField] TextMeshProUGUI docText;
    [SerializeField] TextMeshProUGUI patientText;
    [SerializeField] GameObject[] choiceOptions;
    [SerializeField] GameObject endDialogueButton;
    
    [Header("Dialogue")]
    [SerializeField] TextAsset dialogueFile;
    Story currentStory;
    int defaultMaxVisibleChar = 99999;
    string inkTreatmentKnot = "";
    string inkKeypointKnot = "";

    //Choice Option Logic
    TextMeshProUGUI[] choiceOptionTexts = new TextMeshProUGUI[2]; //2 option
    bool secondOption = false;

    //Treatment Logic
    int explainedTreatments = 0;
    int totalTreatment;     //start 0, total 3

    bool tutorialShown = false;


    private void Awake()
    {
        _UI = GetComponent<UI_OptionTalk>();

        PrepareChoiceTexts();
    }

    private void Start()
    {
        FeedDialogue();
        PrepareOptionTalk();
    }

    void PrepareOptionTalk()
    {
        UIManager.GetInstance().PrepareChoiceButtons(true, choiceOptions, (int index) => { ChooseOption(index); });

        totalTreatment = GameManager.GetInstance().selectedTreatmentIndexes.Count;

        currentStory.variablesState["treatmentCount"] = totalTreatment;

        docText.gameObject.SetActive(true);   //always active in this part
    }

    void PrepareChoiceTexts()
    {
        //Choice texts' assignment
        for (int i = 0; i < choiceOptions.Length; i++)
        {
            choiceOptionTexts[i] = choiceOptions[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void FeedDialogue()
    {
        currentStory = new Story(dialogueFile.text);


        currentStory.onError += (errorMessage, errorType) =>
        {
            if (errorType == Ink.ErrorType.Warning)
                Debug.LogWarning(errorMessage);
            else
                Debug.LogError(errorMessage);
        };

        //Ink functions binding and observers
        currentStory.BindExternalFunction("StartKeypoints", () => { StartKeypoints(); }, true);
        currentStory.BindExternalFunction("ActivateKeypoints", () => { _UI.DeactivateKeypointButtons(false); }, true);
        currentStory.BindExternalFunction("DocTextView", (bool parameter) => { DocTextView(parameter) ; }, true);
        currentStory.ObserveVariable("secondOption", (string varName, object newValue) => { secondOption = true; });
        currentStory.ObserveVariable("keypointsCompleted", (string varName, object newValue) => { if ((bool)newValue == true) KeypointsCompleted(); });
        currentStory.ObserveVariable("nextStep", (string varName, object newValue) => { explainedTreatments = totalTreatment; KeypointsCompleted(); });

    }

    public void ContinueDialogue()
    {
        if (currentStory.canContinue)
        {
            string line = currentStory.Continue();
            DisplayDialogue(line);
        }
        else if (currentStory.currentChoices.Count > 0)  //giving choices to player
        {
            DisplayOptions();
        }
    }    

    void DisplayDialogue(string line)
    {
        string speaker = currentStory.variablesState["speaker"].ToString();

        if (speaker == "Doctor")
        {
            docText.text = line;           
        }
        else if (speaker == "Patient")
        {
            patientText.text = line;         
        }

        ContinueDialogue();
    }

    void DisplayOptions()
    {
        OpenDocChoices(true);
        RandomizeOptions();

        List<Choice> currentChoices = currentStory.currentChoices;

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choiceOptionTexts[index].text = choice.text;

            index++;
        }
    }

    //Button listener
    public void ChooseOption(int index)
    {
        currentStory.ChooseChoiceIndex(index);

        OpenDocChoices(false);

        ContinueDialogue();
    }

    void RandomizeOptions()
    {
        //Since there are only 2 options in this section
        int randomIndex = Random.Range(0, 2);
        choiceOptions[0].transform.SetSiblingIndex(randomIndex);
    }

    void OpenDocChoices(bool status)
    {
        foreach (GameObject option in choiceOptions)
        {
            option.SetActive(status);
        }

        if (secondOption)
        {
            choiceOptions[1].SetActive(false);
        }
    }

    //Button listener
    public void TreatmentButtons(string treatmentKnot)
    {
        inkTreatmentKnot = treatmentKnot;
        inkKeypointKnot = "Exp";
        string knotToGo = $"D_{inkTreatmentKnot}_T_{inkKeypointKnot}";

        currentStory.ChoosePathString(knotToGo);
        ContinueDialogue();

        explainedTreatments++;
    }

    public void KeypointButtons(string keypointKnot)
    {
        if (!tutorialShown)
        {
            UIManager.GetInstance().InfoTextUpdate("");
            tutorialShown = true;
        }

        inkKeypointKnot = keypointKnot;
        string knotToGo = $"D_{inkTreatmentKnot}_T_{inkKeypointKnot}";

        currentStory.ChoosePathString(knotToGo);
        ContinueDialogue();
    }

    //Ink function, start keypoints after initial explanation
    void StartKeypoints()
    {
        if (tutorialShown)
        {
            _UI.HideKeypoints(false);
        }
        else
        {
            TutorialManager.GetInstance().ShowTutorial(8, 8, () => { _UI.OpenKeypoints(); });
        }
    }

    //Ink function
    void KeypointsCompleted()
    {   
        if (explainedTreatments == totalTreatment)  //explained all treatments
        {
            UIManager.GetInstance().PrepareChoiceButtons(false, choiceOptions);  //remove listeners from choices, new listeners next section

            currentStory.ChoosePathString("EndGTO");
            ContinueDialogue();

            UIManager.GetInstance().InfoTextUpdate("<b>Press button to continue to next step<b>");
            endDialogueButton.SetActive(true);      //Listener in _UI      
            
            currentStory.RemoveVariableObserver();

            GiveFeedback();
        }
        else
        {
            _UI.HideKeypoints(true);
            _UI.HideTreatments(false);
        }        
    }

    void DocTextView(bool status)
    {
        docText.gameObject.SetActive(status);
    }

    void GiveFeedback()
    {
        int[] totalPoints = new int[3];
        int[] finalPoints = new int[3];

        totalPoints[0] = (int)currentStory.variablesState["totalAvoidJar"];
        totalPoints[1] = (int)currentStory.variablesState["totalSufInfo"];
        totalPoints[2] = (int)currentStory.variablesState["totalApplyICE"];

        finalPoints[0] = (int)currentStory.variablesState["finalAvoidJar"];
        finalPoints[1] = (int)currentStory.variablesState["finalSufInfo"];
        finalPoints[2] = (int)currentStory.variablesState["finalApplyICE"];

        FeedbackManager.GetInstance().SetOptionTalkResults(totalPoints, finalPoints);

        if (currentStory.variablesState["totalStrConsul"] != null)
        {
            int totalStrConsul = (int)currentStory.variablesState["totalStrConsul"];
            int playerStrConsul = (int)currentStory.variablesState["playerStrConsul"];

            FeedbackManager.GetInstance().SetStructuringOSCE(totalStrConsul, playerStrConsul);
        }
    }
}
