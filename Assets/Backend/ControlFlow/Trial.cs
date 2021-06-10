using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public abstract class ITrial
{
    protected bool IsLoaded = false;
    public string Name { get; private set; }

    public ITrial(string name)
    {
        Name = name;
    }

    public virtual void Update(float DeltaTime)
    {
    }

    public virtual void Open()
    {
        IsLoaded = true;

        Experiment.Measurement.newTrial(this);
    }
    public virtual void Close()
    {
    }

    public virtual void Aggregate(Measurement.Trial data, StreamWriter stream)
    {

    }
}

public class CogTrial : ITrial
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
        Experiment.Measurement.MeasureCogPlaced((int)(pos.x * 10), (int)(pos.y * 10), connected, cog.OnBoard, id);
    }
}
//verschiebungen
public class SpeedTrial : CogTrial
{
    public override void Aggregate(Measurement.Trial data, StreamWriter stream)
    {
        stream.WriteLine("RT-Erstauswahl,RT-LetzteWahl,CRESP");

        long RT1 = 0;
        long RT2 = 0;
        int CRESP = -1;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Zahnradauswahl)
            {
                if (RT1 == 0)
                    RT1 = data.Interaktionen[i].Zeitpunkt;
                RT2 = data.Interaktionen[i].Zeitpunkt;
                CRESP = (data.Interaktionen[i] as Measurement.Zahnradauswahl).CRESP ? 1 : 0;
            }
        }

        stream.WriteLine(RT1.ToString() + ","+RT2.ToString()+"," + CRESP.ToString());
    }

    /* *
     * LOGIC
     * */
    
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
        Experiment.Measurement.MeasureCogSelected(CogSelectors.FindIndex(c => c == selector), selector.IstDieKorrekteLoesung);
    }
}

public class DirectionTrial : CogTrial
{
    public override void Aggregate(Measurement.Trial data, StreamWriter stream)
    {
        stream.WriteLine("RT-Erstauswahl,RT-LetzteWahl,CRESP");

        long RT1 = 0;
        long RT2 = 0;
        Direction RESP = Direction.INVALID;
        int CRESP = -1;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Richtungsauswahl)
            {
                if (RT1 == 0)
                    RT1 = data.Interaktionen[i].Zeitpunkt;
                RT2 = data.Interaktionen[i].Zeitpunkt;
                CRESP = (data.Interaktionen[i] as Measurement.Richtungsauswahl).CRESP ? 1 : 0;
            }
        }

        stream.WriteLine(RT1.ToString() + "," + RT2.ToString() + "," + CRESP.ToString());
    }

    /* *
     * LOGIC
     * */
    
    public enum Direction
    {
        CCW = -1,
        CW = 1,
        INVALID = 0
    }

    public DirectionTrial(string name) : base(name)
    {
    }

    public Direction SelectedDirection { get; private set; }
    public void SelectDirection(Direction dir, bool correct)
    {
        SelectedDirection = dir;
        Experiment.Measurement.MeasureDirectionSelected(dir, correct);
    }
}

public class PropellerTrial : CogTrial
{
    public override void Aggregate(Measurement.Trial data, StreamWriter stream)
    {
        stream.WriteLine("RT-Erstplatzierung,RT-Propeller1,RT-Propeller2,Platzierungen,Drehungen,CRESP");

        long RT1 = 0; //erstes mal reingezogen
        long RT2 = 0; //erster propeller
        long RT3 = 0; //erstes mal 2 propeller
        int placements = 0;
        int rotations = 0;
        int propeller_placed = 0;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Platzierung)
            {
                placements++;
                var p = data.Interaktionen[i] as Measurement.Platzierung;
                if (RT1 == 0 && p.AufBrett)
                    RT1 = p.Zeitpunkt;
            }
            if (data.Interaktionen[i] is Measurement.Drehung)
            {
                rotations++;
            }
            if (data.Interaktionen[i] is Measurement.PropellerAngefuegt)
            {
                propeller_placed++;
                var p = data.Interaktionen[i] as Measurement.PropellerAngefuegt;
                if (RT2 == 0)
                    RT2 = p.Zeitpunkt;
                if(RT3 == 0 && propeller_placed == 2)
                    RT3 = p.Zeitpunkt;
            }
            if (data.Interaktionen[i] is Measurement.PropellerEntfernt)
            {
                propeller_placed--;
            }
        }
        int CRESP = -1; //TODO

        stream.WriteLine(RT1.ToString() + "," + RT2.ToString() + "," + RT3.ToString() + "," + placements.ToString() + "," + rotations.ToString() + "," + CRESP.ToString());
    }

    /* *
     * LOGIC
     * */
    public PropellerTrial(string name) : base(name)
    {
    }

    public void AttachPropeller(Zahnrad AttachedTo)
    {
        if (AttachedTo == null)
            return;
        AttachedTo.IsTarget = true;
        Experiment.Measurement.MeasurePropellerAttached(Cogs.FindIndex(c => c == AttachedTo));
    }
    public void DetachPropeller(Zahnrad DetachedFrom)
    {
        if (DetachedFrom == null)
            return;
        DetachedFrom.IsTarget = false;
        Experiment.Measurement.MeasurePropellerDetached(Cogs.FindIndex(c => c == DetachedFrom));
    }
}