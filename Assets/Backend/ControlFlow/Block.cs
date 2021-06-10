using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block
{
    public string Config { get; private set; }
    private List<Trial> _trials;
    public Trial CurrentTrial
    { 
        get
        {
            if (_trials.Count == 0)
                throw new System.ArgumentOutOfRangeException("No trial loaded");
            return _trials[_trials.Count - 1];
        }
    }
    public int TrialCount { get { return _trials.Count; } }
    public string NextTrialName
    {
        get { return Config + (TrialCount + 1).ToString(); }
    }
    

    public Block()
    {
        _trials = new List<Trial>();
    }

    protected abstract Trial InstantiateTrial(string name);
    public void OpenTrial(string name)
    {
        _trials.Add(InstantiateTrial(name));
    }

    public static Block Instantiate(string name)
    {
        Experiment.Measurement.newBlock(name);
        Block b;
        switch (name)
        {
            case "carousel":
                b = new CogBlock();
                break;
            case "propeller":
                b = new CogBlock();
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

    public void Update(float DeltaTime)
    {
        if(TrialCount > 0)
            CurrentTrial.Update(DeltaTime);
    }
}

public class CogBlock : Block
{
    protected override Trial InstantiateTrial(string name)
    {
        return new CogTrial(name);
    }
}

public class SpeedBlock : Block
{
    protected override Trial InstantiateTrial(string name)
    {
        return new SpeedTrial(name);
    }
}

public class DirectionBlock : Block
{
    protected override Trial InstantiateTrial(string name)
    {
        return new DirectionTrial(name);
    }
}