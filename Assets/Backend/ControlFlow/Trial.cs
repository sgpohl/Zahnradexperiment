using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

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
    public void Close()
    {
        Experiment.Measurement.MeasureTrialFinished();
        resultsString = Aggregate(Experiment.Measurement.CurrentTrial);
    }

    public string resultsString { get; private set; }
    public virtual string Aggregate(Measurement.Trial data)
    {
        return "";
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
        diff *= cog.InnerRadius + overlapping.OuterRadius;

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

public class SpeedTrial : CogTrial
{
    public override string Aggregate(Measurement.Trial data)
    {
        string returnString = "RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP-Typ,CRESP\n";

        long RT1 = 0;
        long RT2 = 0;
        int CRESP = -1;
        int RESP_ID = -1;
        int clicks = 0;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Zahnradauswahl)
            {
                clicks++;
                if (RT1 == 0)
                    RT1 = data.Interaktionen[i].Zeitpunkt;
                RT2 = data.Interaktionen[i].Zeitpunkt;
                CRESP = (data.Interaktionen[i] as Measurement.Zahnradauswahl).CRESP ? 1 : 0;
                RESP_ID = (data.Interaktionen[i] as Measurement.Zahnradauswahl).ZahnradID;
            }
        }
        int RESP = data.Zahnraeder[RESP_ID].Zaehne;

        returnString += RT1.ToString() + "," + RT2.ToString() + "," + data.Dauer.ToString() + "," + clicks.ToString() + "," + RESP.ToString() + "," + CRESP.ToString();
        return returnString;
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
    public override string Aggregate(Measurement.Trial data)
    {
        string returnString = "RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP,CRESP\n";

        long RT1 = 0;
        long RT2 = 0;
        int CRESP = -1;
        int clicks = 0;
        Direction RESP = Direction.INVALID;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Richtungsauswahl)
            {
                clicks++;
                if (RT1 == 0)
                    RT1 = data.Interaktionen[i].Zeitpunkt;
                RT2 = data.Interaktionen[i].Zeitpunkt;
                CRESP = (data.Interaktionen[i] as Measurement.Richtungsauswahl).CRESP ? 1 : 0;
                RESP = (data.Interaktionen[i] as Measurement.Richtungsauswahl).Richtung;
            }
        }

        returnString += RT1.ToString() + "," + RT2.ToString() + "," + data.Dauer.ToString() + "," + clicks.ToString()+ "," + RESP.ToString() + "," + CRESP.ToString();
        return returnString;
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
    public override string Aggregate(Measurement.Trial data)
    {
        string returnString = "RT-Erstplatzierung,RT-Propeller1,RT-Propeller2,RT-Gesamt,Platzierungen,Drehungen,Propeller1,Propeller2,Geschwindigkeit,PropellerKontakt\n";

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
        
        Zahnrad start = Cogs.FindLast(cog => cog.IsStart);
        List<Zahnrad> target = Cogs.FindAll(cog => cog.IsTarget);

        int[] P = new int[] { 0, 0 };

        for(int i = 0; i<target.Count; ++i)
        {
            P[i]++;
            int T_idx = Cogs.IndexOf(target[i]);
            if (target[i].OnBoard)
                P[i]++;
            if (start.System.Contains(target[i]))
                P[i]++;
        }
        int speed = 0;
        int propeller_intersect = -1;
        if (target.Count == 2)
        {
            int smallest = Cogs.Min(cog => cog.Size);
            int largest = Cogs.Max(cog => cog.Size);

            if (target[0].Size == target[1].Size)
                speed = 1;
            else
                speed = 2;
            if ((target[0].Size == smallest && target[1].Size == largest) ||
                (target[0].Size == largest && target[1].Size == smallest))
                speed = 3;

            if (PropellerSet.Count == 2)
            {
                if ((P[0] + P[1]) < 6)
                    propeller_intersect = 0;
                else if (PropellerSet[0].Intersects(PropellerSet[1]))
                    propeller_intersect = 1;
                else
                    propeller_intersect = 2;
            }
        }
        else
            propeller_intersect = 0;

        returnString += RT1.ToString() + "," + RT2.ToString() + "," + RT3.ToString() + "," + data.Dauer.ToString() + "," + placements.ToString() + "," + rotations.ToString();
        returnString += "," + P[0].ToString() + "," + P[1].ToString() + "," + speed.ToString() + "," + propeller_intersect.ToString() ;
        return returnString;
    }

    /* *
     * LOGIC
     * */
    private List<Propeller> PropellerSet;
    public PropellerTrial(string name) : base(name)
    {
        PropellerSet = new List<Propeller>();
    }

    public void RegisterPropeller(Propeller p)
    {
        PropellerSet.Add(p);
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


public class CarouselTrial : CogTrial
{
    public override string Aggregate(Measurement.Trial data)
    {
        string returnString = "RT-Erstplatzierung,RT-Gesamt,Platzierungen,Drehungen,RESP,RESP-Distanz\n";

        long RT1 = 0; //erstes mal reingezogen
        int placements = 0;
        int rotations = 0;
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
        }
        Zahnrad start = Cogs.FindLast(cog => cog.IsStart);
        Zahnrad target = Cogs.FindLast(cog => cog.IsTarget);

        int RESP;
        bool connected = start.System.Contains(target);
        int distance = 0;
        if(connected)
        {
            distance = Zahnrad.ConnectedComponent.Distance(start, target);
            bool CRESP = (distance % 2) == ((start.Direction == target.Direction)?1:0);
            if (CRESP && start.System.CanRotate)
                RESP = 5;
            else if (!CRESP && start.System.CanRotate)
                RESP = 4;
            else
                RESP = 3;
        }
        else
        {
            if (start.System.Size > 1 && target.System.Size > 1)
                RESP = 2;
            else if (start.System.Size > 1 || target.System.Size > 1)
                RESP = 1;
            else
                RESP = 0;
        }


        returnString += RT1.ToString() + "," + data.Dauer.ToString() + "," + placements.ToString() + "," + rotations.ToString() + "," + RESP.ToString()+","+distance.ToString();
        return returnString;
    }

    /* *
     * LOGIC
     * */
    public CarouselTrial(string name) : base(name)
    {
    }
}