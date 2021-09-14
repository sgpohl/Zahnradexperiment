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
                b = new GenericBlock<GreenStabilityTrial>("RESP,CRESP,AnzahlVersuche,Antwortkoordinaten,RT1,RT2,RT3,RT-Gesamt");
                break;
            case "stabilityRed":
                b = new RedStabilityBlock(2);
                break;
            case "matrix":
                b = new MatrixBlock(7, 3);
                break;
            case "vocabulary":
                b = new VocabularyBlock(13);
                break;
            default:
                throw new System.ArgumentException("Tried to instantiate a block with an unknown type: '" + name + "'");
        }
        b.Config = name;
        return b;
    }

    public bool IsOpen { private set; get; }
    bool replayMode = false;
    public void OpenLive()
    {
        IsOpen = true;
        replayMode = false;
        Replay.StartRecording();
    }

    public string FileName {    get {   return "VPN" + Experiment.Measurement.VPN_Num.ToString()+"_"+ Config;   }   }
    public void OpenFromReplay()
    {
        IsOpen = true;
        replayMode = true;

        var replayInput = Experiment.Instance.ActivateReplayInput();
        replayInput.Init(FileName + ".replay");
    }

    public void Close()
    {
        IsOpen = false;
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
        return this.Aggregate(data, null);
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
        return this.Aggregate(data, MeasurementHeader);
    }
}


public class CogBlock<T> : GenericBlock<T> where T : ITrial, new()
{
    public override string GameBoardSceneName() { return "board"; }
}

public class RedStabilityBlock : GenericBlock<RedStabilityTrial>
{
    int MaxTrainingLevels;
    int CurrentTrainingLevel;
    public RedStabilityBlock(int TrainingLevels) : base("RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP,CRESP")
    {
        MaxTrainingLevels = TrainingLevels;
        CurrentTrainingLevel = 1;
    }

    protected override string GetNextTrialPostfix()
    {
        if (CurrentTrainingLevel <= MaxTrainingLevels)
            return "U" + (CurrentTrainingLevel).ToString();
        return base.GetNextTrialPostfix();
    }

    public override bool EndCurrentTrial()
    {
        if (CurrentTrainingLevel <= MaxTrainingLevels)
        {
            CurrentTrainingLevel++;
            CurrentTrial.Close();
            return false;
        }
        return base.EndCurrentTrial();
    }
}

public class PretestBlock : GenericBlock<SelectionTrial>
{
    protected int StartLevel;
    protected int CurrentLevel;
    protected int points;

    public PretestBlock(int StartLevel) : base("Trial,RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP,CRESP")
    {
        this.StartLevel = StartLevel;
        CurrentLevel = StartLevel;
        points = StartLevel -1;
    }

    public override bool EndCurrentTrial()
    {
        var trial = CurrentTrial as SelectionTrial;

        if (trial.Answer == ITrial.AnswerType.CORRECT && CurrentLevel >= StartLevel)
            points++;

        if (trial.Answer != ITrial.AnswerType.CORRECT && CurrentLevel < StartLevel)
            points--;

        //1st iteration
        if (CurrentLevel == StartLevel)
            CurrentLevel++;
        //2nd iteration
        else if (CurrentLevel == StartLevel + 1)
        {
            var prevTrial = Trials[Trials.Count - 2] as SelectionTrial;
            if (trial.Answer == ITrial.AnswerType.CORRECT && prevTrial.Answer == ITrial.AnswerType.CORRECT)
                CurrentLevel = StartLevel + 2;
            else
                CurrentLevel = StartLevel - 1;
        }
        //begin backwards movement
        else if (CurrentLevel == StartLevel - 1)
        {
            CurrentLevel--;
        }
        //continue backwards movement
        else if (CurrentLevel < StartLevel - 1)
        {
            var prevTrial = Trials[Trials.Count - 2] as SelectionTrial;
            if (trial.Answer == ITrial.AnswerType.CORRECT && prevTrial.Answer == ITrial.AnswerType.CORRECT)
                CurrentLevel = StartLevel + 2;
            else
                CurrentLevel--;
        }
        //forward movement
        else
        {
            var trial2 = Trials[Trials.Count - 2] as SelectionTrial;
            var trial3 = Trials[Trials.Count - 3] as SelectionTrial;
            if (trial.Answer != ITrial.AnswerType.CORRECT && trial2.Answer != ITrial.AnswerType.CORRECT && trial3.Answer != ITrial.AnswerType.CORRECT)
            {
                CurrentTrial.Close();
                return true;
            }
            else
                CurrentLevel++;
        }
        return base.EndCurrentTrial();
    }

    protected override string GetNextTrialPostfix()
    {
        return (CurrentLevel).ToString();
    }

    protected override string Aggregate(Measurement.Block data, string header)
    {
        var returnString = new StringBuilder(data.Typ);
        returnString.AppendFormat(",Gesamtpunktzahl:,{0}", points);
        if (header != null)
            returnString.AppendFormat("\n,{0}", header);
        returnString.Append("\n");
        foreach (var trial in Trials)
            returnString.Append(trial.ToString(","));
        return returnString.ToString();
    }
}

public class VocabularyBlock : PretestBlock
{
    public VocabularyBlock(int start) : base(start)
    {
    }
}

public class MatrixBlock : PretestBlock
{
    int MaxTrainingLevels;
    int CurrentTrainingLevel;
    public MatrixBlock(int start, int TrainingLevels) : base(start)
    {
        MaxTrainingLevels = TrainingLevels;
        CurrentTrainingLevel = 1;
    }

    protected override string GetNextTrialPostfix()
    {
        if (CurrentTrainingLevel <= MaxTrainingLevels)
            return "U" + (CurrentTrainingLevel).ToString();
        return base.GetNextTrialPostfix();
    }

    public override bool EndCurrentTrial()
    {
        if (CurrentTrainingLevel <= MaxTrainingLevels)
        {
            CurrentTrainingLevel++;
            CurrentTrial.Close();
            return false;
        }
        return base.EndCurrentTrial();
    }
}