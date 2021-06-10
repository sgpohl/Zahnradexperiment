using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trial
{
    protected bool IsLoaded = false;
    public string Name { get; private set; }

    public Trial(string name)
    {
        Name = name;
    }

    public virtual void Update(float DeltaTime)
    {
    }

    public virtual void Open()
    {
        IsLoaded = true;

        Experiment.Measurement.newTrial(Name);
    }
    public virtual void Close()
    {
    }
}

public class CogTrial : Trial
{
    protected List<Zahnrad> Cogs;
    public Spielbrett GameBoard { get; private set; }

    public CogTrial(string name) : base(name)
    {
    }

    public override void Update(float DeltaTime)
    {
        if (!IsLoaded)
            return;

        if (Cogs.Count > 0)
        {
            var cog = Cogs[0];
            if (System.Math.Abs(cog.Speed) > 0)
            {
                cog.Speed = cog.Speed * (1 - DeltaTime);
            }
        }
    }
    public override void Open()
    {
        Cogs = new List<Zahnrad>();

        GameObject go = GameObject.Find("Spielbrett");
        if (go != null)
        {
            GameBoard = go.GetComponent<Spielbrett>();
            //Debug.Log("GameBoard present");
        }
        else
            GameBoard = null;

        base.Open();
    }

    public virtual void RegisterCog(Zahnrad cog)
    {
        Cogs.Add(cog);
        ConnectCog(cog);
        Experiment.Measurement.SaveCogInfo(Cogs.Count - 1, cog.Size);
    }

    public void ConnectCog(Zahnrad cog)
    {
        cog.Disconnect();
        for (int i = 0; i < Cogs.Count; ++i)
        {
            if (Cogs[i] == cog)
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

    public Zahnrad CogAt(Vector2 pos)
    {
        for (int i = 0; i < Cogs.Count; ++i)
        {
            if (Cogs[i].Contains(pos))
                return Cogs[i];
        }
        return null;
    }

    public Vector2 NearestPositionCandidate(Vector2 pos, Zahnrad cog)
    {
        Zahnrad overlapping = null;
        for (int i = 0; i < Cogs.Count; ++i)
        {
            if (Cogs[i] == cog)
                continue;
            if (cog.Overlaps(Cogs[i], pos))
            {
                overlapping = Cogs[i];
                break;
            }
        }
        if (overlapping == null)
            return pos;
        Vector2 diff = pos - (Vector2)overlapping.transform.position;
        diff.Normalize();
        diff *= cog.InnerRadius.radius + overlapping.OuterRadius.radius;

        return (Vector2)overlapping.transform.position + diff;
    }

    public void RotationApplied(Zahnrad cog, float speed)
    {
        int id = Cogs.FindIndex(c => c == cog);
        Experiment.Measurement.MeasureCogRotated((int)speed, id);
    }
    public void PlacementApplied(Zahnrad cog, Vector2 pos)
    {
        int id = Cogs.FindIndex(c => c == cog);
        bool connected = cog.ConnectedCogs.Count > 0;
        Experiment.Measurement.MeasureCogPlaced((int)(pos.x * 10), (int)(pos.y * 10), connected, id);
    }
}

public class SpeedTrial : CogTrial
{
    public SpeedTrial(string name) : base(name)
    {
    }

    public List<Auswahlzahnrad> CogSelectors { get; private set; }
    public override void Open()
    {
        CogSelectors = new List<Auswahlzahnrad>();
        base.Open();
    }

    public override void RegisterCog(Zahnrad cog)
    {
        Cogs.Add(cog);
        ConnectCog(cog);
    }

    public void RegisterCogSelector(Auswahlzahnrad selector)
    {
        CogSelectors.Add(selector);
        Experiment.Measurement.SaveCogInfo(CogSelectors.Count - 1, selector.Size);
    }

    public Auswahlzahnrad SelectedCog { get; private set; }
    public void SelectCog(Auswahlzahnrad selector)
    {
        SelectedCog = selector;
        Experiment.Measurement.MeasureCogSelected(CogSelectors.FindIndex(c => c == selector));
    }
}

public class DirectionTrial : CogTrial
{
    public enum Direction
    {
        CCW = -1,
        CW = 1
    }

    public DirectionTrial(string name) : base(name)
    {
    }

    public Direction SelectedDirection { get; private set; }
    public void SelectDirection(Direction dir)
    {
        SelectedDirection = dir;
        Experiment.Measurement.MeasureDirectionSelected(dir);
    }
}