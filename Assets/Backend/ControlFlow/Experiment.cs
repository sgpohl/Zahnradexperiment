using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        LoadScene(name+"_instructions", false);
        menue.enabled = false;
        ContinueButton.Activate();
        //Debug.Log("start "+name+ "  "+ SceneManager.GetActiveScene().buildIndex.ToString());
    }

    // Update is called once per frame
    private float FPS = 0;
    private bool IsLoading = false;
    void Update()
    {
        FPS = 1.0f / Time.deltaTime;
        if (Input.GetKey("escape"))
        {
            Measurement.Finish();
            Application.Quit();
        }
        /*if (Input.GetKeyUp("space") && trialPrefix != null)
        {
            NextTrial();
        }*/
        if(CurrentBlock != null && !IsLoading)
            CurrentBlock.Update(Time.deltaTime);
    }

    public void NextTrial()
    {
        if(TrialIsActive)
            EndTrial();
        if (!IsValidTrial( CurrentBlock.NextTrialName ))
        {
            //LoadScene("debriefing");
            EndBlock();
        }
        else
        {
            string name = CurrentBlock.NextTrialName;
            CurrentBlock.OpenTrial( name );
            LoadScene( name );
        }
    }

    private bool IsValidTrial(string name)
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string x = SceneUtility.GetScenePathByBuildIndex(i);
            if (x.Contains("/"+ name + ".unity"))
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
            Measurement.VPN_Num = parsedNum;
        Debug.Log("VPN: " + Measurement.VPN_Num.ToString());
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
        IsLoading = true;
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
        if(TrialIsActive && !scene.name.Equals("board"))
            CurrentTrial<ITrial>().Open();
        IsLoading = false;
    }

    public void EndTrial()
    {
        CurrentTrial<ITrial>().Close();
    }

    private void EndBlock()
    {
        //Debug.Log("EndBlock: " + trialPrefix);
        LoadScene(null, false);
        menue.enabled = true;
        ContinueButton.Deactivate();
    }
}
