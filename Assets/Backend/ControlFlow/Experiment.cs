using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Experiment : MonoBehaviour
{
    public static Experiment Instance { get; private set; }
    
    private System.Diagnostics.Stopwatch timer;

    public Measurement measurement;
    private Canvas menue;
    public PlayButton ContinueButton { private get; set; }

    private List<Block> Blocks;
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
        
        timer = new System.Diagnostics.Stopwatch();
        Blocks = new List<Block>();
        
        measurement = gameObject.AddComponent(typeof(Measurement)) as Measurement;

        //ExperimentConfig.Load("config.xml");

        Object[] canvas = GameObject.FindObjectsOfType(typeof(Canvas));
        menue = (Canvas)canvas[0];

        SceneManager.sceneLoaded += InitScene;
    }
    // Start is called before the first frame update
    void Start()
    {
        measurement.Init(0, "default");

        //LoadScene("instructions");
    }

    public void OpenDataFolder()
    {
        //TODO: windows only
        string winPath = Application.persistentDataPath.Replace("/", "\\");
        System.Diagnostics.Process.Start("explorer.exe", winPath);

        Debug.Log(winPath);
    }

    private string trialPrefix = null;
    private int trialNum = 0;
    public void StartBlock(string name)
    {
        trialPrefix = name;
        trialNum = 0;

        Blocks.Add(Block.Instantiate(name));

        LoadScene(name+"_instructions", false);
        menue.enabled = false;
        Debug.Log("start "+name+ "  "+ SceneManager.GetActiveScene().buildIndex.ToString());
        measurement.newBlock(name);
        ContinueButton.Activate();
    }

    // Update is called once per frame
    private bool measuring = false;
    private float FPS = 0;
    void Update()
    {
        FPS = 1.0f / Time.deltaTime;
        if (Input.GetMouseButtonUp(1))
            if(measuring)
            {
                timer.Stop();
                measuring = false;
            }
            else
            {
                timer.Restart();
                measuring = true;
            }
        if (Input.GetKey("escape"))
        {
            measurement.Finish();
            Application.Quit();
        }
        /*if (Input.GetKeyUp("space") && trialPrefix != null)
        {
            NextTrial();
        }*/
        if(CurrentBlock != null)
            CurrentBlock.Update(Time.deltaTime);
    }

    public void NextTrial()
    {
        trialNum++;

        if(TrialIsActive)
            EndTrial();
        if (!IsValidTrial(trialNum))
        {
            //Debug.Log("invalid: " + trialPrefix + trialNum.ToString());
            //trialNum = 1;
            //LoadScene("debriefing");
            EndBlock();
        }
        else
        {
            CurrentBlock.OpenTrial();
            var name = trialPrefix + trialNum.ToString();
            LoadScene(name);
            measurement.newTrial(name);
        }
    }

    private bool IsValidTrial(int idx)
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string x = SceneUtility.GetScenePathByBuildIndex(i);
            if (x.Contains("/"+ trialPrefix + idx.ToString() + ".unity"))
                return true;
        }
        return false;
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
            measurement.VPN_Num = parsedNum;
        Debug.Log("VPN: " + measurement.VPN_Num.ToString());
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
    private void LoadScene(string name, bool withBoard = true)
    {
        //Debug.Log("Load " + name + (withBoard ? " with board" : " without board"));
        //unload board first, if needed
        if (BoardLoaded && !withBoard)
        {
            //Debug.Log("unload board");
            AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            unloading.completed += (operation) =>
            {
                BoardLoaded = false;
                this.LoadScene(name, withBoard);
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
                this.LoadScene(name, withBoard);
            };
            return;
        }

        //load board if needed
        if (withBoard && !BoardLoaded)
        {
            //Debug.Log("load board");
            SceneManager.LoadScene("board", LoadSceneMode.Additive);
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
        if(TrialIsActive)
            CurrentTrial<Trial>().Open();
    }

    public void EndTrial()
    {
        CurrentTrial<Trial>().Close();
    }

    private void EndBlock()
    {
        //Debug.Log("EndBlock: " + trialPrefix);
        LoadScene(null, false);
        menue.enabled = true;
        trialPrefix = null;
        ContinueButton.Deactivate();
    }
}
