using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    //**ID and Password
    [SerializeField] GameObject loginMenu;
    [SerializeField] GameObject loginWarningMessage;

    bool isIDCorrect = false;
    bool isPasswordCorrect = false;

    //Temporary login info
    string ID = "Ahmet";
    string password = "123456";

    //**Main Menu
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject mainMenuLink;
    [SerializeField] GameObject overview;
    [SerializeField] GameObject afterGameMenu;

    //**Patients Menu
    [SerializeField] GameObject patientMenu;
    [SerializeField] GameObject[] patients;

    //**Settings Menu
    [SerializeField] GameObject settingsMenu;

    GameObject currentMenu;
    GameObject lastMenu;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("played"))
        {
            PlayerPrefs.SetInt("played", 0);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        if(PlayerPrefs.GetInt("played") == 0)
        {
            mainMenu.SetActive(true);            
        }
        else
        {
            afterGameMenu.SetActive(true);
            mainMenuLink.SetActive(true);
        }
        currentMenu = mainMenu;
    }

    public void IDCheck(TMP_InputField input)
    {
        if (input.text.Equals(ID))
        {
            isIDCorrect = true;
        }
        else
        {
            isIDCorrect = false;
        }
    }

    public void PasswordCheck(TMP_InputField input)
    {
        if (input.text.Equals(password))
        {
            isPasswordCorrect = true;
        }
        else
        {
            isPasswordCorrect = false;
        }
    }

    public void LoginButton()
    {
        if (isIDCorrect && isPasswordCorrect)
        {
            loginMenu.SetActive(false);
            mainMenu.SetActive(true);
            currentMenu = mainMenu;
        }
        else
        {
            loginWarningMessage.SetActive(true);
            StartCoroutine(WaitForMessageDisappear());
        }
    }

    IEnumerator WaitForMessageDisappear()
    {
        yield return new WaitForSeconds(5f);

        loginWarningMessage.SetActive(false);
    }

    public void Play()
    {
        lastMenu = currentMenu;
        currentMenu.SetActive(false);

        overview.SetActive(true);
        currentMenu = patientMenu;
    }

    public void CloseOverview()
    {
        overview.SetActive(false);
        currentMenu.SetActive(true);
    }

    public void CloseAfterGame()
    {
        afterGameMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void ChoosePatient(int patientNumber)
    {
        if (patientNumber == 0)
        {
            LevelLoader.GetInstance().LoadNextLevel("DocRoom");
        }        
    }

    public void Back()
    {
        currentMenu.SetActive(false);
        lastMenu.SetActive(true);

        currentMenu = lastMenu;
    }
}
