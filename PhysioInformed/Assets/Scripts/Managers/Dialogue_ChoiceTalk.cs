using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using DG.Tweening;
using UnityEngine.UI;

public class Dialogue_ChoiceTalk : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject choiceOptionPanel;
    [SerializeField] GameObject focusHistoryPanel;
    [SerializeField] GameObject focusInfo;
    [SerializeField] GameObject recommendationPanel;
    [SerializeField] GameObject treatmentSelectionPanel;


    [Header("Dialogue UI")]
    [SerializeField] TextMeshProUGUI docText;
    [SerializeField] TextMeshProUGUI patientText;
    [SerializeField] GameObject[] choiceOptions;
    [SerializeField] GameObject[] focusHistoryOptions;
    [SerializeField] GameObject continueButton;
    [SerializeField] GameObject[] patientPageButtons;
    Button continueDialogueBut;
    int defaultMaxVisibleChar = 99999;
    bool pageButtonsDisplayed = false;


    [Header("Dialogue")]
    [SerializeField] TextAsset dialogueFile;
    Story currentStory;


    //Choice Option Logic
    int choiceCount = 0;
    TextMeshProUGUI[] choiceOptionTexts = new TextMeshProUGUI[6]; //up to 6 option


    //Focus History Topics Logic
    List<string> focusHistoryTopics = new List<string>
    {
        "_Onset", "_Radiation", "_MedHist", "_ExerRelFac", "_WorkCond", "_WorkOut", "_SocHist"
    };
    Dictionary<string, GameObject> namedFocusHistoryOptions = new Dictionary<string, GameObject>();
    bool takingFocus = false;


    //To show info given on info panel once --> 0: show info, 1: disable info, 2: pass that part
    Dictionary<string ,int> infoGiven = new Dictionary<string, int> 
    { {"choiceInfo", 0 }, {"focusTopicInfo", 0 }, {"recommendationInfo", 0 } };


    //Treatment selection
    List<int> selectedIndexes = new List<int>();


    private void Awake()
    {       
        PrepareChoiceTalk();
    }

    void PrepareChoiceTalk()
    {
        continueDialogueBut = continueButton.GetComponent<Button>();

        //Get texts from choice options to print
        for (int i = 0; i < choiceOptions.Length; i++)
        {
            choiceOptionTexts[i] = choiceOptions[i].GetComponentInChildren<TextMeshProUGUI>();
        }

        //Match focus topic names and options
        for (int i = 0; i < focusHistoryOptions.Length; i++)
        {
            namedFocusHistoryOptions.Add(focusHistoryTopics[i], focusHistoryOptions[i]);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PrepareGameStep();

        FeedDialogue();
    }

    void PrepareGameStep()
    {
        UIManager.GetInstance().PrepareChoiceButtons(true, choiceOptions, (int index) => { ChooseOption(index); });

        continueDialogueBut.onClick.AddListener(() => { ContinueDialogue(); });

        dialoguePanel.SetActive(true);

        continueButton.SetActive(true);
        continueButton.transform.DOScale(1f, 0.5f);

        UIManager.GetInstance().InfoTextUpdate("You can continue dialogues with continue button");
    }

    void FeedDialogue()
    {
        currentStory = new Story(dialogueFile.text);

        //Ink functions to bind
        currentStory.BindExternalFunction("ChangeFocusTopics", () => { FocusHistoryOptionsElimination(); });
        currentStory.BindExternalFunction("GoToGTO", () => { GoToOptionTalk(); });
        currentStory.BindExternalFunction("FinalizeSession", () => { EndSession(); });
        currentStory.BindExternalFunction("RecommendTreatment", () => { OpenTreatmentRecommendation(true); });
        currentStory.ObserveVariable("takingFocusHistory", (string varName, object newValue) => { takingFocus = (bool)newValue; });
        currentStory.BindExternalFunction("TakingFocus", () => { OpenFocusHistoryTopics(true); });

        FocusHistoryOptionsElimination();   ////In start get the topics given in patient history (marked in ink)
        ContinueDialogue();
    }

    //Button listener 
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

        if (speaker == "Patient")
        {
            patientText.text = line;

            if (patientText.GetTextInfo(line).pageCount > 1)
            {
                ShowPageButtons(true);
                pageButtonsDisplayed = true;
            }
            else if (pageButtonsDisplayed) //to not tween everytime pageCount 1
            {
                ShowPageButtons(false);
                pageButtonsDisplayed = false;
            }
        }
        else if (speaker == "Doctor")
        {
            docText.text = line;
            DocTextorChoice(true, false);
        }
    }

    IEnumerator TypeEffect(string line, TextMeshProUGUI text)
    {         
        text.maxVisibleCharacters = 0;
        text.text = line;       

        foreach (char letter in line.ToCharArray())
        {
            text.maxVisibleCharacters++;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                text.maxVisibleCharacters = defaultMaxVisibleChar;
                break;
            }

            yield return null;
        }        
    }

    void ShowPageButtons(bool status)
    {
        float duration = 0.25f;
        float buttonScale = status ? 1 : 0;

        patientPageButtons[0].transform.DOScale(buttonScale, duration);
        patientPageButtons[1].transform.DOScale(buttonScale, duration);
    }

    void DisplayOptions()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        choiceCount = currentChoices.Count;
        
        DocTextorChoice(false, true);
        RandomizeChoices();    //randomize options cause in ink, ordered from worst to best


        int index = 0;
        foreach(Choice choice in currentChoices)
        {
            choiceOptionTexts[index].text = choice.text;

            index++;
        }

        if (infoGiven["choiceInfo"] == 0)
        {
            UIManager.GetInstance().InfoTextUpdate("Choose an option to answer patient");
            infoGiven["choiceInfo"] = 1;
        }        
    }

    void RandomizeChoices()
    {
        List<int> randomIndexes = new List<int>(choiceCount);

        //Create random indexes as choice count
        int maxRange = choiceCount;
        while (randomIndexes.Count < choiceCount)
        {
            int randomIndex = Random.Range(0, maxRange);
            if (!randomIndexes.Contains(randomIndex))
            {
                randomIndexes.Add(randomIndex);
                if (randomIndex == maxRange) maxRange--;  //to reduce finding not yet generated random number in random.range
            }
        }

        //Set choices random indexes
        for(int i = 0; i < choiceCount; i++)
        {
            choiceOptions[randomIndexes[i]].transform.SetSiblingIndex(i);
        }
    }

    //Button listener
    public void ChooseOption(int index)
    {
        currentStory.ChooseChoiceIndex(index);

        if (takingFocus)
        {
            currentStory.Continue();
        }
        ContinueDialogue();

        if (infoGiven["choiceInfo"] == 1)
        {
            UIManager.GetInstance().InfoTextUpdate("");
            infoGiven["choiceInfo"] = 2;
        }

    }

    void DocTextorChoice(bool text, bool choice)
    {
        choiceOptionPanel.SetActive(choice);
        for (int i = 0; i < choiceCount; i++)
        {
            choiceOptions[i].SetActive(choice);
        }          

        docText.gameObject.SetActive(text);

        if (text) //use button to continue texts
        {
            continueButton.transform.DOScale(1f, 0.5f);
        }
        else
        {
            continueButton.transform.DOScale(0f, 0.5f);
        }
    }

    void OpenTreatmentRecommendation(bool status)
    {
        DocTextorChoice(!status, false);

        recommendationPanel.SetActive(status);

        if (infoGiven["recommendationInfo"] == 0)
        {
            UIManager.GetInstance().InfoTextUpdate("Choose a treatment to prescribe");
            infoGiven["recommendationInfo"] = 1;
        }        
    }

    //Button listener
    public void RecommendTreatment(int index)
    {
        currentStory.variablesState["treatmentRecommendationIndex"] = index;  //in ink which treatment's dialogue comes...

        OpenTreatmentRecommendation(false);

        currentStory.ChoosePathString("TreatmentRecommendation");          //...go that treatment in that knot
        ContinueDialogue();

        if (infoGiven["recommendationInfo"] == 1)
        {
            UIManager.GetInstance().InfoTextUpdate("");
            infoGiven["recommendationInfo"] = 2;
        }
    }    

    void OpenFocusHistoryTopics(bool status)
    {
        DocTextorChoice(!status, false);
        focusHistoryPanel.SetActive(status);
        focusInfo.SetActive(status);

        if (infoGiven["focusTopicInfo"] == 0)
        {
            UIManager.GetInstance().InfoTextUpdate("Choose a topic to take information");
            infoGiven["focusTopicInfo"] = 1;
        }        
    }

    //Button listener
    public void FocusTopicButton(Button button)
    {
        Image image = button.gameObject.GetComponent<Image>();
        Outline outline = button.gameObject.GetComponent<Outline>();

        //Mark already chosen topics' buttons
        image.sprite = Resources.Load<Sprite>("BTN_Focus");
        outline.enabled = true;
        button.interactable = false;
    }

    void FocusHistoryOptionsElimination()
    {
        var topicNamesToEliminate = currentStory.variablesState["focusTopicNames"] as Ink.Runtime.InkList;

        //Disable topics given in patient info
        foreach (var item in topicNamesToEliminate)
        {
            namedFocusHistoryOptions[item.Key.itemName].SetActive(false);
        }
    }

    //Button listener
    public void ChooseFocusTopic(string topic)
    {
        currentStory.ChoosePathString(topic);

        OpenFocusHistoryTopics(false);    
        ContinueDialogue();

        if (infoGiven["focusTopicInfo"] == 1)
        {
            UIManager.GetInstance().InfoTextUpdate("");
            infoGiven["focusTopicInfo"] = 2;
        }
    }    

    //Ink function, ends choice talk
    void GoToOptionTalk()
    {
        currentStory.RemoveVariableObserver();

        UIManager.GetInstance().PrepareChoiceButtons(false, choiceOptions);  //remove current listeners
        choiceOptionPanel.SetActive(true);  //will be active in next session, here not to reference again
        OpenTreatmentSelection();    //before next part(GTO) in game

        continueButton.transform.DOScale(0f, 0.5f); 
        continueButton.SetActive(false);
        continueDialogueBut.onClick.RemoveAllListeners();

        GiveFeedback();
    }

    void OpenTreatmentSelection()
    {
        dialoguePanel.SetActive(false);
        treatmentSelectionPanel.SetActive(true);

        TutorialManager.GetInstance().ShowTutorial(5);
    }

    //Button listener
    public void SelectTreatment(Button button)
    {
        //Get image and outline components in the button
        Image image = button.gameObject.GetComponent<Image>();
        Outline outline = button.gameObject.GetComponent<Outline>();

        //Selected or deselected button switch logic
        if (!outline.enabled) //select
        {
            //Eliminated option button view
            image.sprite = Resources.Load<Sprite>("Button_Large_A");
            outline.enabled = true;

            //Add the index of eliminated treatment
            selectedIndexes.Add(button.gameObject.transform.GetSiblingIndex());
        }
        else //deselect
        {
            //Selected option button view
            image.sprite = Resources.Load<Sprite>("BTN_A3");
            outline.enabled = false;

            //Remove the index of reselected treatment
            selectedIndexes.Remove(button.gameObject.transform.GetSiblingIndex());
        }
    }

    //Button listener
    public void TreatmentSelectionOkButton()
    {
        int treatmentCount = selectedIndexes.Count;

        if (treatmentCount < 2)
        {
            UIManager.GetInstance().InfoTextUpdate("You've selected very few treatments!", true);
        }
        else if (treatmentCount > 3)
        {
            UIManager.GetInstance().InfoTextUpdate("You've selected too many treatments!", true);
        }
        else  //if chosen 2 or 3 treatment
        {
            treatmentSelectionPanel.SetActive(false);
            UIManager.GetInstance().InfoTextUpdate("");

            GameManager.GetInstance().selectedTreatmentIndexes = selectedIndexes;
            GameManager.GetInstance().NextStep(1, 0);    //1st(0.) step is over here 
        }
    }

    //Button listener
    public void PatientNextPage()
    {
        int pageNum = patientText.pageToDisplay;
        int pageCount = patientText.textInfo.pageCount;

        if (pageNum < pageCount)
        {
            pageNum++;
            patientText.pageToDisplay = pageNum;
        }
    }

    //Button listener
    public void PatientPreviousPage()
    {
        int pageNum = patientText.pageToDisplay;

        if (pageNum > 1)
        {
            pageNum--;
            patientText.pageToDisplay = pageNum;
        }
    }

    void EndSession()
    {
        GiveFeedback();
        UIManager.GetInstance().InGameAchievements("Fast&Furious");

        dialoguePanel.SetActive(false);

        //Prepare for next session
        continueButton.transform.DOScale(0f, 0.5f);
        continueButton.SetActive(false);
        continueDialogueBut.onClick.RemoveAllListeners();

        currentStory.RemoveVariableObserver();

        //Remove this step's listeners
        UIManager.GetInstance().PrepareChoiceButtons(false, choiceOptions);

        string prescribedTreatment = (string)currentStory.variablesState["prescribedTreatment"];
        GameManager.GetInstance().FinalizeSession(prescribedTreatment);
    }

    void GiveFeedback()
    {
        InkList[] _OSCEPoints = new InkList[4];
        _OSCEPoints[0] = currentStory.variablesState["_IS_OSCE"] as Ink.Runtime.InkList;
        _OSCEPoints[1] = currentStory.variablesState["_IRfC_OSCE"] as Ink.Runtime.InkList;
        _OSCEPoints[2] = currentStory.variablesState["_BR_OSCE"] as Ink.Runtime.InkList;
        _OSCEPoints[3] = currentStory.variablesState["_PfSDM_OSCE"] as Ink.Runtime.InkList;

        FeedbackManager.GetInstance().SetChoiceTalkResults(_OSCEPoints);

        if(currentStory.variablesState["totalStrConsul"] != null)
        {
            int totalStrConsul = (int)currentStory.variablesState["totalStrConsul"];
            int playerStrConsul = (int)currentStory.variablesState["playerStrConsul"];

            FeedbackManager.GetInstance().SetStructuringOSCE(totalStrConsul, playerStrConsul);
        }
    }
}
