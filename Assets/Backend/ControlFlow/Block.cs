using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block
{
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

    public Block()
    {
        _trials = new List<Trial>();
    }

    protected abstract Trial InstantiateTrial();
    public void OpenTrial()
    {
        _trials.Add(InstantiateTrial());
    }

    public static Block Instantiate(string name)
    {
        switch(name)
        {
            case "carousel":
                return new CogBlock();
            case "propeller":
                return new CogBlock();
            case "speed":
                return new SpeedBlock();
            case "training":
                return new CogBlock();
            case "direction":
                return new DirectionBlock();
            default:
                throw new System.ArgumentException("Tried to instantiate a block with an unknown type: '" + name + "'");
        }
    }

    public void Update(float DeltaTime)
    {
        if(TrialCount > 0)
            CurrentTrial.Update(DeltaTime);
    }
}

public class CogBlock : Block
{
    protected override Trial InstantiateTrial()
    {
        return new CogTrial();
    }
}

public class SpeedBlock : Block
{
    protected override Trial InstantiateTrial()
    {
        return new SpeedTrial();
    }
}

public class DirectionBlock : Block
{
    protected override Trial InstantiateTrial()
    {
        return new DirectionTrial();
    }
}