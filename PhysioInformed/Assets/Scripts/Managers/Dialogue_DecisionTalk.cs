using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class Dialogue_DecisionTalk : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject treatmentsPanel;
    [SerializeField] TextMeshProUGUI docText;
    [SerializeField] TextMeshProUGUI patientText;
    [SerializeField] GameObject choiceOptionsPanel;
    [SerializeField] GameObject continueButton;
    [SerializeField] GameObject[] treatments;
    [SerializeField] GameObject[] choiceOptions;

    [Header("Dialogue")]
    [SerializeField] TextAsset dialogueFile;
    Story currentStory;
    Button continueDialogueBut;
    int defaultMaxVisibleChar = 99999;

    //Treatments adjustment
    List<int> selectedTreatments;

    //Choice logic
    TextMeshProUGUI[] choiceOptionTexts = new TextMeshProUGUI[3]; //at most
    int choiceCount = 0;
    Dictionary<string, int> treatmentChoiceCounts = new Dictionary<string, int>
    {
        {"Inj", 3}, {"Phy", 3}, {"Sur", 3}, {"WaS", 3}, {"AS", 3}, {"NSA", 3}, {"IaP", 3}, {"SaP", 3}
    };

    //Treatment buttons logic
    bool assignedFirstTreatmentToInk = false;
    Button currentTreatmentBut;
    List<Button> treatmentButtons = new List<Button>();

    int eliminableTreatments;
    int treatmentsToNegotiate;

    private void Awake()
    {        
        PrepareUI();
    }

    void PrepareUI()
    {
        continueDialogueBut = continueButton.GetComponent<Button>();

        for (int i = 0; i < choiceOptions.Length; i++)
        {
            choiceOptionTexts[i] = choiceOptions[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PrepareGameStep();        

        OpenTreatments(true);

        FeedDialogue();
    }

    void PrepareGameStep()                                                                      
    {
        UIManager.GetInstance().PrepareChoiceButtons(true, choiceOptions, (int index) => { ChooseOption(index); });

        selectedTreatments = new List<int>(GameManager.GetInstance().selectedTreatmentIndexes);
        treatmentsToNegotiate = selectedTreatments.Count;
        eliminableTreatments = selectedTreatments.Count;

        foreach (int index in selectedTreatments)
        {
            treatments[index].SetActive(true);
            treatmentButtons.Add(treatments[index].GetComponent<Button>());
        }

        dialoguePanel.SetActive(true);

        continueDialogueBut.onClick.AddListener(() => { ContinueDialogue(); });
        continueButton.SetActive(true);
    }

    public void OpenTreatments(bool status)                                       
    {
        treatmentsPanel.SetActive(status);

        if (status)
        {
            UIManager.GetInstance().InfoTextUpdate("Choose a treatment to start negotiating");

            TutorialManager.GetInstance().ShowTutorial(10);
        }
        else
        {
            UIManager.GetInstance().InfoTextUpdate("");
        }
    }

    void FeedDialogue()                                                                      
    {
        //Load dialogue
        currentStory = new Story(dialogueFile.text);

        //Functions and observers
        currentStory.variablesState["treatmentsToNegotiate"] = treatmentsToNegotiate;
        currentStory.ObserveVariable("treatmentsToNegotiate", (string VarName, object newValue) => { NegotiatedTreatment(); });
        currentStory.BindExternalFunction("EndConsultation", () => { EndConsultation(); });

        //Treatments to ink
        var newList = new Ink.Runtime.InkList("treatments", currentStory);
        foreach (Button but in treatmentButtons)
        {
            newList.AddItem(but.name);
        }
        currentStory.variablesState["currentTreatments"] = newList;
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
            DocShowChoices(false);
            DocShowText(true);
        }
        else if (speaker == "Patient")
        {
            patientText.text = line;
        }
    }

    void DisplayOptions()
    {
        List<Choice> currentChoices = currentStory.currentChoices;
        choiceCount = currentChoices.Count;
               
        DocShowText(false);
        DocShowChoices(true);

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choiceOptionTexts[index].text = choice.text;

            index++;
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

    //Button listener
    public void ChooseOption(int index)
    {
        currentStory.ChooseChoiceIndex(index);
        ContinueDialogue();

        if(currentTreatmentBut != null) treatmentChoiceCounts[currentTreatmentBut.name]--;
    }

    void DocShowChoices(bool status)
    {
        choiceOptionsPanel.SetActive(status);
        for (int i = 0; i < choiceCount; i++)
        {
            choiceOptions[i].SetActive(status);
        }
    }

    void DocShowText(bool status)
    {
        docText.gameObject.SetActive(status);

        if (status) //use button to continue texts
        {
            continueButton.transform.DOScale(1f, 0.5f);
        }
        else
        {
            continueButton.transform.DOScale(0f, 0.5f);
        }
    }       

    //Button listener
    public void TreatmentButtons(Button but)
    {
        if (!assignedFirstTreatmentToInk)
        {
            currentStory.variablesState["lastTreatment"] = but.name;
            assignedFirstTreatmentToInk = true;
        }

        ChoiceCountAdjustment(but);  //for jumping between treatments

        string knotToGo = $"D_{but.name}";        
        currentStory.ChoosePathString(knotToGo);
        ContinueDialogue();
        ContinueDialogue();  //why two continue don't know, ink doesn't get directly choices but says story.canContinue when jumps in a knot
                
        //To jump between buttons
        if(treatmentButtons.Count != 1) //if not only this treatment left to negotiate
            ActivateButtons();           

        currentTreatmentBut = but;
        but.interactable = false;
    }

    void ChoiceCountAdjustment(Button but)
    {
        //Choice options decrease as you go through story but if you jump between treatments without finishing negotiating,
        //the previous treatment's last chosen option doesn't get deactivated. However, choice count decreases ink,
        //since choice is made. So, when turning back a treatment jump from, deactivate the options already chosen
        //So change choice count for treatments and apply that when jumping(in TreatmentButtons)
        for (int i = treatmentChoiceCounts[but.name]; i < choiceOptions.Length; i++)
        {
            choiceOptions[i].SetActive(false);
        }
    }

    void ActivateButtons()
    {
        foreach (Button but in treatmentButtons)
        {
            if (!but.IsInteractable())
                but.interactable = true;
        }
    }

    //Button listener
    public void EliminateButton(Button treatmentButton)
    {
        treatmentsToNegotiate--;
        currentStory.variablesState["treatmentsToNegotiate"] = treatmentsToNegotiate;
        eliminableTreatments--;

        if (eliminableTreatments > 0)
        {
            string knotToGo = $"D_{treatmentButton.name}_Eliminate";

            currentStory.ChoosePathString(knotToGo);
            ContinueDialogue();

            //Remove treatment from all logic
            treatmentButton.gameObject.SetActive(false);
            if(treatmentButtons.Contains(treatmentButton))  //if not removed by completing negotiating
                treatmentButtons.Remove(treatmentButton);

            //if not eliminated in the very beginning and a treatment other than current being negotiated one
            if(currentTreatmentBut != null && currentTreatmentBut != treatmentButton)
            {
                currentTreatmentBut.interactable = true;
            }            
        }
        else
        {
            UIManager.GetInstance().InfoTextUpdate("You can't eliminate, that's the only option left!", true);
        }
    }

    //Button listener
    public void PrescribeButton(string treatmentKnot)
    {
        string knotToGo = $"D_{treatmentKnot}_Prescribe";
        currentStory.ChoosePathString(knotToGo);
        ContinueDialogue();

        OpenTreatments(false);
    }    

    void NegotiatedTreatment()
    {
        //if called from ink, not when elimination changed ink variable
        if (treatmentsToNegotiate != (int)currentStory.variablesState["treatmentsToNegotiate"])  
        {
            //Negotiated view
            Color negotiatedColor = new Color32(0, 0, 0, 186);
            currentTreatmentBut.targetGraphic.color = negotiatedColor; //when clicked

            treatmentsToNegotiate--;

            //to not activate again when choose another treatment to negotiate
            treatmentButtons.Remove(currentTreatmentBut);  

            if (treatmentsToNegotiate <= 0)
            {
                UIManager.GetInstance().InfoTextUpdate("You've negotiated all the treatments! Choose one to prescribe", true);
            }
        }        
    }

    void EndConsultation()
    {
        GiveFeedback();

        dialoguePanel.SetActive(false);
        currentStory.RemoveVariableObserver();

        string prescribedTreatment = (string)currentStory.variablesState["prescribedTreatment"];
        GameManager.GetInstance().FinalizeSession(prescribedTreatment);
    }

    void GiveFeedback()
    {
        InkList playerASDMP = currentStory.variablesState["_ADMP_OSCEPoints"] as Ink.Runtime.InkList;
        InkList playerCC = currentStory.variablesState["_CC_OSCEPoints"] as Ink.Runtime.InkList;

        FeedbackManager.GetInstance().SetDecisionTalkResults(playerASDMP, playerCC);

        if (currentStory.variablesState["totalStrConsul"] != null)
        {
            int totalStrConsul = (int)currentStory.variablesState["totalStrConsul"];
            int playerStrConsul = (int)currentStory.variablesState["playerStrConsul"];

            FeedbackManager.GetInstance().SetStructuringOSCE(totalStrConsul, playerStrConsul);
        }
    }
}
