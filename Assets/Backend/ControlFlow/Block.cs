using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEngine.SceneManagement;

///Organizes data exchange on the block (inter-trial) level.
///Manages Trial lifetime.
public abstract class Block
{
    public string Config { get; private set; }
    public List<ITrial> Trials { get; private set; }
    public ITrial CurrentTrial
    {
        get
        {
            if (Trials.Count == 0)
                throw new System.ArgumentOutOfRangeException("No trial loaded");
            return Trials[Trials.Count - 1];
        }
    }
    public int TrialCount { get { return Trials.Count; } }
    protected virtual string GetNextTrialPostfix() { return (TrialCount + 1).ToString(); }
    public string NextTrialName
    {
        get { return Config + GetNextTrialPostfix().ToString(); }
    }
    public virtual string GameBoardSceneName() { return null; }

    private ReplayInput.Data Replay = null;
    public Block()
    {
        Trials = new List<ITrial>();
        Replay = new ReplayInput.Data();
    }

    protected abstract ITrial InstantiateTrial(string name);
    public void OpenTrial(string name)
    {
        Trials.Add(InstantiateTrial(name));
    }
    public virtual bool EndCurrentTrial()
    {
        CurrentTrial.Close();
        return false;
    }

    private bool IsValidTrial(string name)
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string x = SceneUtility.GetScenePathByBuildIndex(i);
            if (x.Contains("/" + name + ".unity"))
                return true;
        }
        return false;
    }
    public bool SetupNextTrial()
    {
        if (!IsValidTrial(NextTrialName))
            return false;

        OpenTrial(NextTrialName);
        return true;
    }


    public static Block Instantiate(string name)
    {
        Experiment.Measurement.newBlock(name);
        Block b;
        switch (name)
        {
            case "carousel":
                b = new CogBlock<CarouselTrial>();
                break;
            case "propeller":
                b = new CogBlock<PropellerTrial>();
                break;
            case "speed":
                b = new CogBlock<SpeedTrial>();
                break;
            case "training":
                b = new CogBlock<CogTrial>();
                break;
            case "direction":
                b = new CogBlock<DirectionTrial>();
                break;
            case "trainingStability":
                b = new GenericBlock<StabilityTrial>();
                break;
            case "stabilityGreen":
                b = new GenericBlock<GreenStabilityTrial>("AnzahlVersuche, Antwortkoordinaten, RT1, RT2, RT3, RT-Gesamt");
                break;
            case "stabilityRed":
                b = new GenericBlock<RedStabilityTrial>("RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP,CRESP");
                break;
            case "matrix":
                b = new GenericBlock<SelectionTrial>();
                break;
            case "vocabulary":
                b = new VocabularyBlock();
                break;
            default:
                throw new System.ArgumentException("Tried to instantiate a block with an unknown type: '" + name + "'");
        }
        b.Config = name;
        return b;
    }

    bool replayMode = false;
    public void OpenLive()
    {
        replayMode = false;
        Replay.StartRecording();
    }

    public string FileName {    get {   return "VPN" + Experiment.Measurement.VPN_Num.ToString()+"_"+ Config;   }   }
    public void OpenFromReplay()
    {
        replayMode = true;

        var replayInput = Experiment.Instance.ActivateReplayInput();
        replayInput.Init(FileName + ".replay");
    }

    public void Close()
    {
        if (replayMode)
        {
            Experiment.Instance.DectivateReplayInput();
            return;
        }

        Experiment.Measurement.SaveCurrentBlock();
        Replay.Save(FileName + ".replay");
    }

    public void Update(float DeltaTime)
    {
        if (Replay.IsReady)
            Replay.WriteCurrentState();
        if (TrialCount > 0)
            CurrentTrial.Update(DeltaTime);
    }

    protected virtual string Aggregate(Measurement.Block data, string header)
    {
        var returnString = new StringBuilder(data.Typ);
        if(header != null)
            returnString.AppendFormat("\n,{0}", header);
        returnString.Append("\n");
        foreach (var trial in Trials)
            returnString.Append(trial.ToString(","));
        return returnString.ToString();
    }
    public virtual string Aggregate(Measurement.Block data)
    {
        return Aggregate(data, null);
    }
}

public class GenericBlock<T> : Block where T : ITrial, new()
{
    protected string MeasurementHeader = null;
    public GenericBlock()
    {
    }
    public GenericBlock(string header)
    {
        MeasurementHeader = header;
    }

    protected override ITrial InstantiateTrial(string name)
    {
        T t = new T();
        t.Name = name;
        return t;
    }

    public override string Aggregate(Measurement.Block data)
    {
        return base.Aggregate(data, MeasurementHeader);
    }
}

public class CogBlock<T> : GenericBlock<T> where T : ITrial, new()
{
    public override string GameBoardSceneName() { return "board"; }
}

public class SelectionBlock : GenericBlock<SelectionTrial>
{
    public SelectionBlock() : base("RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP,CRESP")
    {
    }
}

public class VocabularyBlock : SelectionBlock
{
    private const int StartLevel = 13;

    private int CurrentLevel = StartLevel;
    private int points = StartLevel-1;
    protected override string GetNextTrialPostfix()
    {
        return (CurrentLevel+1).ToString();
    }

    public override bool EndCurrentTrial()
    {
        var trial = CurrentTrial as SelectionTrial;

        if (trial.Answer == ITrial.AnswerType.CORRECT && CurrentLevel >= StartLevel)
            points++;

        if (trial.Answer != ITrial.AnswerType.CORRECT && CurrentLevel < StartLevel)
            points--;

        if (CurrentLevel == StartLevel)
            CurrentLevel++;
        else if(CurrentLevel == StartLevel+1)
        {
            if (trial.Answer == ITrial.AnswerType.CORRECT)
                CurrentLevel = StartLevel+2;
            else
                CurrentLevel = StartLevel-1;
        }
        else if(CurrentLevel == StartLevel-1)
        {
            CurrentLevel--;
        }
        else if (CurrentLevel < StartLevel-1)
        {
            var prevTrial = Trials[Trials.Count - 2] as SelectionTrial;
            if (trial.Answer == ITrial.AnswerType.CORRECT && prevTrial.Answer == ITrial.AnswerType.CORRECT)
                CurrentLevel = StartLevel + 2;
            else
                CurrentLevel--;
        }
        else
        {
            var trial2 = Trials[Trials.Count - 2] as SelectionTrial;
            var trial3 = Trials[Trials.Count - 3] as SelectionTrial;
            if (trial.Answer != ITrial.AnswerType.CORRECT && trial2.Answer != ITrial.AnswerType.CORRECT && trial3.Answer != ITrial.AnswerType.CORRECT)
                return true;
            else
                CurrentLevel++;
        }
        return base.EndCurrentTrial();
    }
}
