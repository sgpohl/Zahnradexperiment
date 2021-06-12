using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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
    public string NextTrialName
    {
        get { return Config + (TrialCount + 1).ToString(); }
    }
    

    public Block()
    {
        Trials = new List<ITrial>();
    }

    protected abstract ITrial InstantiateTrial(string name);
    public void OpenTrial(string name)
    {
        Trials.Add(InstantiateTrial(name));
    }

    public static Block Instantiate(string name)
    {
        Experiment.Measurement.newBlock(name);
        Block b;
        switch (name)
        {
            case "carousel":
                b = new CarouselBlock();
                break;
            case "propeller":
                b = new PropellerBlock();
                break;
            case "speed":
                b = new SpeedBlock();
                break;
            case "training":
                b = new CogBlock();
                break;
            case "direction":
                b = new DirectionBlock();
                break;
            default:
                throw new System.ArgumentException("Tried to instantiate a block with an unknown type: '" + name + "'");
        }
        b.Config = name;
        return b;
    }

    string resultsString = "";
    public void Close()
    {
        resultsString = Aggregate(Experiment.Measurement.CurrentBlock);
    }

    public void Update(float DeltaTime)
    {
        if(TrialCount > 0)
            CurrentTrial.Update(DeltaTime);
    }

    public virtual string Aggregate(Measurement.Block data)
    {
        string returnString = data.Typ;
        foreach (var trial in Trials)
            returnString += "\n" + trial.resultsString;
        return returnString;
    }
}

public class CogBlock : Block
{
    protected override ITrial InstantiateTrial(string name)
    {
        return new CogTrial(name);
    }
}

public class SpeedBlock : Block
{
    protected override ITrial InstantiateTrial(string name)
    {
        return new SpeedTrial(name);
    }
}

public class DirectionBlock : Block
{
    protected override ITrial InstantiateTrial(string name)
    {
        return new DirectionTrial(name);
    }
}

public class PropellerBlock : Block
{
    protected override ITrial InstantiateTrial(string name)
    {
        return new PropellerTrial(name);
    }
}

public class CarouselBlock : Block
{
    protected override ITrial InstantiateTrial(string name)
    {
        return new CarouselTrial(name);
    }
}
