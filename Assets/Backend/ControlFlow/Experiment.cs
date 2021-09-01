using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/**
 * Singleton. Coordinates global data exchange.
 * Attach it once (eg. Main Camera)
 **/
public class Experiment : MonoBehaviour
{
    public static Experiment Instance { get; private set; }

    public Measurement _measurement;
    public static Measurement Measurement { get { return Experiment.Instance._measurement; } }

    private Canvas menue;
    public PlayButton ContinueButton { private get; set; }

    public List<Block> Blocks { get; private set; }
    public static Block CurrentBlock
    {
        get
        {
            if (Experiment.Instance.Blocks.Count == 0)
                return null;
            return Experiment.Instance.Blocks[Experiment.Instance.Blocks.Count - 1];
        }
    }
    public static T CurrentTrial<T>() where T : class
    {
        if(Experiment.CurrentBlock == null)
            throw new System.ArgumentOutOfRangeException("No block loaded");
        return Experiment.CurrentBlock.CurrentTrial as T;
    }
    public static bool TrialIsActive { get { return CurrentBlock != null && CurrentBlock.TrialCount > 0; } }

    public static BaseInput Input
    {
        get
        {
            return EventSystem.current.currentInputModule.input;
        }
    }

    public bool TestMode { get; private set; }

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        if (Instance != null)
        {
            Debug.LogError("There is more than one instance!");
            return;
        }

        Instance = this;

        Blocks = new List<Block>();
        _measurement = gameObject.AddComponent(typeof(Measurement)) as Measurement;

        //ExperimentConfig.Load("config.xml");

        Object[] canvas = GameObject.FindObjectsOfType(typeof(Canvas));
        menue = (Canvas)canvas[0];

        SceneManager.sceneLoaded += InitScene;
    }
    // Start is called before the first frame update
    void Start()
    {
        Measurement.Init(0);
        SetUIStatus(false);
        //LoadScene("instructions");
    }

    public void OpenDataFolder()
    {
        //TODO: windows only
        string winPath = Application.persistentDataPath.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", winPath);

        Debug.Log(winPath);
    }

    public void StartBlock(string name)
    {
        Blocks.Add(Block.Instantiate(name));

        LoadScene(name+"_instructions");
        menue.enabled = false;
        ContinueButton.Activate();

        if (ReplayMode)
            CurrentBlock.OpenFromReplay();
        else
            CurrentBlock.OpenLive();
    }

    // Update is called once per frame
    private float FPS = 0;
    private bool IsLoading = false;
    void Update()
    {
        FPS = 1.0f / Time.deltaTime;
        if (Input.GetButtonDown("CloseApp"))
        {
            Measurement.SaveAll();
            Application.Quit();
        }

        // UNCOMMENT FOR REPLAY FUNCTIONALITY
        if (Input.GetButtonDown("StartReplay"))
        {
        }
        //

        /*if (Input.GetKeyUp("space") && trialPrefix != null)
        {
            NextTrial();
        }*/
        if (CurrentBlock != null && !IsLoading)
            CurrentBlock.Update(Time.deltaTime);
    }

    public void NextTrial()
    {
        if(TrialIsActive)
            EndTrial();
        bool trialValid = CurrentBlock.SetupNextTrial();
        if (trialValid)
            LoadScene(CurrentTrial<ITrial>().Name, CurrentBlock.GameBoardSceneName());
        else
            EndBlock();
    }


    void OnGUI()
    {
        /*
        GUI.Label(new Rect(0, 0, 500, 100), "Escape zum Beenden\nrechter Mausklick für Zeitmessung\nZahnrad anklicken (links) -> Drehung im Uhrzeigersinn");
        GUI.Label(new Rect(0, 200, 300, 100), "Messung: " + timer.ElapsedMilliseconds.ToString() + "ms");
        GUI.Label(new Rect(0, 230, 300, 100), FPS.ToString("0.0") + " FPS -> "+(dtime*1000).ToString("0.0")+"ms/Frame");
        */
    }

    public void OnVPNChanged(string vpn)
    {
        int parsedNum;
        bool valid = int.TryParse(vpn, out parsedNum);
        if (valid)
        {
            Measurement.VPN_Num = parsedNum;
            SetUIStatus(true);
        }
        else
            SetUIStatus(false);
    }

    private void SetUIStatus(bool active)
    {
        GameObject[] lockables = GameObject.FindGameObjectsWithTag("Lockable");
        var lockableButtons = from l in lockables where l.GetComponent<Button>() != null select l;
        foreach (var button in lockableButtons)
        {
            button.GetComponent<Button>().interactable = active;
            var c = button.GetComponent<Image>().color;
            if (active)
                c.a = 1.0f;
            else
                c.a = 0.5f;
            button.GetComponent<Image>().color = c;
        }

    }

    public bool ReplayMode { get; private set; }
    public void SetReplayMode(bool active)
    {
        ReplayMode = active;
    }

    public ReplayInput ActivateReplayInput()
    {
        EventSystem.current.currentInputModule.inputOverride = gameObject.AddComponent(typeof(ReplayInput)) as ReplayInput;
        return EventSystem.current.currentInputModule.inputOverride as ReplayInput;
    }
    public void DectivateReplayInput()
    {
        EventSystem.current.currentInputModule.inputOverride = null;
    }

    /* IDX
     * begin 0
     * board 1
     * scene 2
     * 
     * OR
     * 
     * begin 0
     * scene 1
     */
    bool BoardLoaded = false;
    private void LoadScene(string name, string boardName = null)
    {
        IsLoading = true;
        //Debug.Log("Load " + name + (withBoard ? " with board" : " without board"));
        //unload board first, if needed
        if (BoardLoaded && (boardName == null))
        {
            //Debug.Log("unload board");
            AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            unloading.completed += (operation) =>
            {
                BoardLoaded = false;
                this.LoadScene(name, null);
            };
            return;
        }

        //unload old scene, if needed
        if ((SceneManager.sceneCount == 2 && !BoardLoaded) || 
            (SceneManager.sceneCount == 3 && BoardLoaded))
        {
            //Debug.Log("unload scene");
            AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
            unloading.completed += (operation) =>
            {
                this.LoadScene(name, boardName);
            };
            return;
        }

        //load board if needed
        if ((boardName != null) && !BoardLoaded)
        {
            //Debug.Log("load board");
            SceneManager.LoadScene(boardName, LoadSceneMode.Additive);
            BoardLoaded = true;
        }

        //load scene
        if (name != null)
        {
            //Debug.Log("load scene "+name);
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
        }
    }

    private void InitScene(Scene scene, LoadSceneMode mode)
    {
        if(TrialIsActive && !scene.name.Equals("board"))
            CurrentTrial<ITrial>().Open();
        IsLoading = false;
    }

    public void EndTrial()
    {
        bool blockFinished = CurrentBlock.EndCurrentTrial();
        if (blockFinished)
            EndBlock();
    }

    private void EndBlock()
    {
        //Debug.Log("EndBlock: " + trialPrefix);
        CurrentBlock.Close();
        //LoadScene("debriefing");
        LoadScene(null);
        menue.enabled = true;
        ContinueButton.Deactivate();
    }

    public void SetTestMode(bool value)
    {
        TestMode = value;
    }
}
