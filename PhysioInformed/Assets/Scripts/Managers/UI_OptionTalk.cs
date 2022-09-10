using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_OptionTalk : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] GameObject choiceOptionsPanel;
    [SerializeField] GameObject treatmentsPanel;
    [SerializeField] GameObject keypointsPanel;
    [SerializeField] Transform preferences;

    [Header("Layout Elements")]
    [SerializeField] GameObject[] treatments;
    [SerializeField] VerticalLayoutGroup optionsLayout;
    RectOffset originalLayoutPadding;
    RectOffset newLayoutPadding;
    float originalLayoutSpacing;
    float newLayoutSpacing;

    //Treatment/keypoint effects(Transform tweening) and buttons
    Dictionary<Button, RectTransform> treatmentTransforms = new Dictionary<Button, RectTransform>();   //dict to reach which transform with but
    List<Button> keypointButtons;
    List<Button> allKeypointButtons;  //backup to renew keypoint buttons
    RectTransform keypointsTransform;           

    bool tutorialShown = false;

    private void Awake()
    {
        PrepareKeypoints();

        originalLayoutPadding = optionsLayout.padding;
        originalLayoutSpacing = optionsLayout.spacing;

        newLayoutSpacing = 40f;
        newLayoutPadding = new RectOffset
            (
                optionsLayout.padding.left,
                optionsLayout.padding.right,
                optionsLayout.padding.top,
                optionsLayout.padding.bottom
            );
        newLayoutPadding.top = 0;
        newLayoutPadding.bottom = 30;
    }

    //Button and transform assignments
    void PrepareKeypoints()
    {
        allKeypointButtons = new List<Button>(keypointsPanel.GetComponentsInChildren<Button>());
        keypointButtons = new List<Button>(allKeypointButtons);
        keypointsTransform = keypointsPanel.GetComponent<RectTransform>();        
    }

    private void Start()
    {
        PrepareTreatments();
        
        ChangeLayoutValues(newLayoutPadding, newLayoutSpacing);

        TutorialManager.GetInstance().ShowTutorial(6, 6, () => { OpenTreatments(); });
    }

    //Adjusting treatments acc. to player choices  + button and transform assignments
    void PrepareTreatments()
    {
        List<int> selectedTreatments = new List<int>(GameManager.GetInstance().selectedTreatmentIndexes);

        foreach (int index in selectedTreatments)
        {
            treatments[index].SetActive(true);

            Button but = treatments[index].GetComponent<Button>();
            RectTransform trform = treatments[index].GetComponent<RectTransform>();
            treatmentTransforms.Add(but, trform);
        }
    }

    void ChangeLayoutValues(RectOffset padding, float spacing)
    {
        optionsLayout.padding = padding;
        optionsLayout.spacing = spacing;
    }

    void OpenDialogue()
    {
        dialoguePanel.SetActive(true);
        choiceOptionsPanel.SetActive(true);
        UIManager.GetInstance().InfoTextUpdate("");
    }

    void OpenTreatments()
    {        
        treatmentsPanel.SetActive(true);
        UIManager.GetInstance().InfoTextUpdate("Choose a treatment");
    }

    public void OpenKeypoints()
    {
        keypointsPanel.SetActive(true);
        UIManager.GetInstance().InfoTextUpdate("Choose a keypoint");
    }

    //Button listener
    public void TreatmentButtons(Button but)
    {
        Color selectedColor = new Color32(29, 227, 8, 255);
        but.targetGraphic.color = selectedColor; //when clicked

        HideTreatments(true, but);  //except this button object

        if (!tutorialShown)
        {
            TutorialManager.GetInstance().ShowTutorial(7, 7, () => { OpenDialogue(); });
            tutorialShown = true;
        }

        but.interactable = false;
    }

    //Called in dialogue
    public void HideTreatments(bool status, Button buttonKey = null)
    {
        float duration = 0.25f;  
        float toWhere = -200f;

        if (!status) toWhere = -20f;

        foreach (Button but in treatmentTransforms.Keys)
        {
            if(but != buttonKey)  //except but given as parameter
            {
                treatmentTransforms[but].DOAnchorPosX(toWhere, duration);
            }            
        }
    }        

    //Button listener
    public void KeypointButtons(Button but)
    {
        Color selectedColor = new Color32(29, 227, 8, 255);
        but.targetGraphic.color = selectedColor; //when clicked

        DeactivateKeypointButtons(true);

        keypointButtons.Remove(but);    //used once in every treatment
    }

    //Called in dialogue
    public void HideKeypoints(bool status)
    {
        float duration = 0.25f;
        if (status)
        {
            keypointsTransform.DOAnchorPosX(180f, duration);
            NormalizeKeypointButView();
        }
        else
        {
            keypointsTransform.DOAnchorPosX(0f, duration);
        }
    }

    //Same keypoints for each treatment, renew them
    void NormalizeKeypointButView()
    {
        keypointButtons = new List<Button>(allKeypointButtons);

        Color normalColor = new Color32(0, 171, 234, 150);
        foreach (Button but in keypointButtons)
        {
            but.targetGraphic.color = normalColor;
            but.interactable = true;
        }
    }

    public void DeactivateKeypointButtons(bool status)
    {
        foreach (Button but in keypointButtons)
        {
            but.interactable = !status;
        }
    }

    public void EndGTOButton(Button but)
    {
        dialoguePanel.SetActive(false);
        choiceOptionsPanel.SetActive(false);
        ChangeLayoutValues(originalLayoutPadding, originalLayoutSpacing);
        treatmentsPanel.SetActive(false);
        keypointsPanel.SetActive(false);

        GameManager.GetInstance().NextStep(2, 1);

        but.gameObject.SetActive(false);
    }
}
