using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Experiment : MonoBehaviour
{
    public static Experiment Instance   {get;private set;}

    private List<Zahnrad> Cogs;
    private System.Diagnostics.Stopwatch timer;
    
    private Measurement measurement;
    private Canvas menue;

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

        GridSize = 0.25f;
        Cogs = new List<Zahnrad>();
        timer = new System.Diagnostics.Stopwatch();
        
        measurement = gameObject.AddComponent(typeof(Measurement)) as Measurement;

        //ExperimentConfig.Load("config.xml");

        Object[] canvas = GameObject.FindObjectsOfType(typeof(Canvas));
        menue = (Canvas)canvas[0];
    }
    // Start is called before the first frame update
    void Start()
    {
        measurement.Init(0, "noConfig");

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
        LoadScene(name+"_instructions");
        menue.enabled = false;
        Debug.Log("start "+name);
    }

    // Update is called once per frame
    private bool measuring = false;
    private float FPS = 0;
    private float dtime = 0;
    void Update()
    {
        FPS = 1.0f / Time.deltaTime;
        dtime = Time.deltaTime;
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
        if (Input.GetKeyUp("space") && trialPrefix != null)
        {
            trialNum++;
            
            EndTrial();
            if (!IsValidTrial(trialNum))
            {
                Debug.Log("invalid: "+ trialPrefix + trialNum.ToString());
                //trialNum = 1;
                //LoadScene("debriefing");
                EndBlock();
            }
            else 
            {
                var name = trialPrefix + trialNum.ToString();
                LoadScene(name);
                measurement.newTrial(name);
            }
        }
        
        if(Cogs.Count > 0)
        {
            var cog = Cogs[0];
            if(System.Math.Abs(cog.Speed) > 0)
            {
                cog.Speed = cog.Speed*(1-dtime);
            }
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

    private void LoadScene(string name)
    {
        if (SceneManager.sceneCount > 1)
        {
            AsyncOperation unloading = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
            unloading.completed += (operation) =>
            {
                SceneManager.LoadScene(name, LoadSceneMode.Additive);
            };
        }
        else
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    public void EndTrial()
    {
        Debug.Log("EndTrial: " + trialPrefix + trialNum.ToString());
        Cogs.Clear();
    }

    private void EndBlock()
    {
        Debug.Log("EndBlock: " + trialPrefix);
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
        menue.enabled = true;
        trialPrefix = null;
    }

    public void RegisterCog(Zahnrad cog)
    {
        Cogs.Add(cog);
        ConnectCog(cog);
        measurement.SaveCogInfo(Cogs.Count-1, cog.Size);
    }
    
    public void ConnectCog(Zahnrad cog)
    {
        cog.Disconnect();
        for (int i = 0; i<Cogs.Count; ++i)
        {
            if(Cogs[i] == cog)
                continue;
            if (Cogs[i].Intersects(cog))
            {
                Cogs[i].ConnectTo(cog);
                cog.ConnectTo(Cogs[i]);
            }
        }
    }

    public bool PositionIsValid(Vector2 pos, Zahnrad cog)
    {
        for (int i = 0; i < Cogs.Count; ++i)
        {
            if (Cogs[i] == cog)
                continue;
            if (cog.Overlaps(Cogs[i], pos))
                return false;
        }
        return true;
    }
    
    public void RotationApplied(Zahnrad cog, float speed)
    {
        int id = Cogs.FindIndex(c => c == cog);
        measurement.MeasureCogRotated((int)speed, id);
    }
    public void PlacementApplied(Zahnrad cog, Vector2 pos)
    {
        int id = Cogs.FindIndex(c => c == cog);
        bool connected = cog.ConnectedCogs.Count > 0;
        measurement.MeasureCogPlaced((int)(pos.x*10), (int)(pos.y*10), connected, id);
    }

    public float GridSize;
}
