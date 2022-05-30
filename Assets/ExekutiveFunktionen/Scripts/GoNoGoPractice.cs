using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GoNoGoPractice : MonoBehaviour
{

    public GameObject pig;
    public GameObject chicken;
    public GameObject cow;
    public GameObject donkey;
    [SerializeField] Button button;
    GameObject shownAnimal;
    GameObject currentAnimal;

    public TextMesh introText;
    public TextMesh introText2;
    public GameObject introAnimal;
    public GameObject introAnimal2;
    public Button introButton;

    public Button continueButton;
    public Button redoButton;
    public TextMesh continueText;

    public static Stopwatch timer = new Stopwatch();
    public int counter;
    public int trial;
    // Start is called before the first frame update
    void Start()
    { 
        trial = 0;
    }

    void disableIntro()
    {
        trial++;
        introAnimal.SetActive(false);
        introText.gameObject.SetActive(false);
        introAnimal2.SetActive(false);
        introText2.gameObject.SetActive(false);
        introButton.gameObject.SetActive(false);
        button.gameObject.SetActive(true);
    }

   
    void enableIntro()
    {
        introAnimal2.SetActive(true);
        introText2.gameObject.SetActive(true);
        introButton.gameObject.SetActive(true);
        shownAnimal.gameObject.SetActive(false);
        button.gameObject.SetActive(false);
    }

    public void startSequenz()
    {
        counter = 1;
        disableIntro();
        shownAnimal = donkey;
        donkey.SetActive(true);
        //selectAnimal(counter);
        
        timer.Start();
    }

    private void Update()
    {

        if (counter == 11 && trial == 2)
        {
            timer.Stop();
            shownAnimal.gameObject.SetActive(false);
            button.gameObject.SetActive(false);
            continueText.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(true);
            redoButton.gameObject.SetActive(true);

        }
        if (counter == 11 && trial != 2)
        {

            timer.Stop();
            enableIntro();
        }
        
   
        if (timer.Elapsed.TotalSeconds >= 2.0)
        {
            timer.Reset();
            SelectNextAnimal();
        }
    }

    public void StartGoNoGo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void RepeatPractice()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 0);
    }

    void selectAnimal(int counter)
    {
        switch (counter)
        {
            case int n when ((counter % 4) == 0):
                StartCoroutine(showPig());
                break;
            case int n when ((counter % 3) == 0):
                StartCoroutine(showCow());
                break;
            case int n when ((counter % 2) == 0):
                StartCoroutine(showChicken());
                break;
            case int n when ((counter % 1) == 0):
                StartCoroutine(showDonkey());
                break;
        }
    }

    public void clickButton()
    {
        timer.Reset();
        SelectNextAnimal();
        
    }

    private void SelectNextAnimal()
    {
        
        selectAnimal(counter);
        button.enabled = false;
        counter++;
    }

    IEnumerator showDonkey()
    {
        shownAnimal.SetActive(false);
        shownAnimal = donkey;
        yield return new WaitForSeconds(1f);
        shownAnimal.SetActive(true);
        button.enabled = true;
        timer.Start();
    }

    IEnumerator showChicken()
    {
        shownAnimal.SetActive(false);
        shownAnimal = chicken;
        yield return new WaitForSeconds(1f);
        shownAnimal.SetActive(true);
        button.enabled = true;
        timer.Start();
    }

    IEnumerator showCow()
    {
        shownAnimal.SetActive(false);
        shownAnimal = cow;
        yield return new WaitForSeconds(1f);
        shownAnimal.SetActive(true);
        button.enabled = true;
        timer.Start();
    }

    IEnumerator showPig()
    {
        shownAnimal.SetActive(false);
        shownAnimal = pig;
        yield return new WaitForSeconds(1f);
        shownAnimal.SetActive(true);
        button.enabled = true;
        timer.Start();
    }
}
