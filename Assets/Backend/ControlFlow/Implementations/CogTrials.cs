using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;


public class CogTrial : ITrial, ITrialFunctionality<Zahnrad>
{
    protected List<Zahnrad> Cogs;
    public Spielbrett GameBoard { get; private set; }

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
            GameBoard = go.GetComponent<Spielbrett>();
        else
            GameBoard = null;

        base.Open();
    }

    private bool _ispaused = false;
    public virtual bool IsPaused
    {
        get => _ispaused;
        set
        {
            _ispaused = value;
            if (_ispaused)
                Experiment.Measurement.HaltTimer();
            else
                Experiment.Measurement.ContinueTimer();
        }
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
        if (!cog.OnBoard)
            return;
        for (int i = 0; i < Cogs.Count; ++i)
        {
            if (Cogs[i] == cog)
                continue;
            if (!Cogs[i].OnBoard)
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
            if (!Cogs[i].OnBoard)
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
            if (!Cogs[i].OnBoard)
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
        Experiment.Measurement.MeasureCogRotated((int)speed, id, Cogs[id].System.Size);
    }
    public void PlacementApplied(Zahnrad cog, Vector2 pos)
    {
        int id = Cogs.FindIndex(c => c == cog);
        bool connected = cog.ConnectedCogs.Count > 0;
        Experiment.Measurement.MeasureCogPlaced((int)(pos.x * 10), (int)(pos.y * 10), connected, cog.OnBoard, id, cog.System.Size);
    }
}

public class SpeedTrial : CogTrial, ISelectorTrial<Auswahlzahnrad>
{
    public override void Aggregate(Measurement.Trial data)
    {
        results.Add(new StringBuilder("RT-Erstauswahl,RT-LetzteWahl,RT-Gesamt,AnzahlSelektionen,RESP-Typ,CRESP"));

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

        int RESP = -1;
        if (RESP_ID >= 0)
            RESP = data.Zahnraeder[RESP_ID].Zaehne;

        var z2 = new StringBuilder();
        z2.AppendFormat("{0},{1},{2},{3},{4},{5}", RT1, RT2, data.Dauer, clicks, RESP, CRESP);
        results.Add(z2);
    }

    /* *
     * LOGIC
     * */

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

    public Auswahlzahnrad SelectedCog { get; private set; }

    public void SelectAnswer(Auswahlzahnrad selected)
    {
        SelectedCog = selected;
        Experiment.Measurement.MeasureCogSelected(CogSelectors.FindIndex(c => c == selected), selected.IstDieKorrekteLoesung);
    }

    public void Register(Auswahlzahnrad selector)
    {
        CogSelectors.Add(selector);
        Experiment.Measurement.SaveCogInfo(CogSelectors.Count - 1, selector.Size);
    }

    public Auswahlzahnrad GetCurrentlySelected()
    {
        return SelectedCog;
    }

    public List<Auswahlzahnrad> GetSelectors()
    {
        return CogSelectors;
    }
}

public class DirectionTrial : CogTrial, ITrialFunctionality<Richtungssticker>//, ISelectorTrial<Auswahlpfeil>
{
    public List<Richtungssticker> SelectedDirections = null;
    public DirectionTrial() : base()
    {
        SelectedDirections = new List<Richtungssticker>();
    }

    public override void Aggregate(Measurement.Trial data)
    {
        results.Add(new StringBuilder("RT-Gesamt,Zahnrad,RT-Erstauswahl,RT-LetzteWahl,AnzahlSelektionen,RESP,CRESP"));

        List<int> indices = new List<int>();
        for(int i = 0; i<Cogs.Count; ++i)
        {
            if (Cogs[i].IsStart)
                continue;
            indices.Add(i);
        }

        long[] RT1 = new long[indices.Count];
        long[] RT2 = new long[indices.Count];
        int[] CRESP = new int[indices.Count];
        int[] clicks = new int[indices.Count];
        Direction[] RESP = new Direction[indices.Count];

        for(int i = 0; i<indices.Count; ++i)
        {
            RT1[i] = 0;
            RT2[i] = 0;
            CRESP[i] = -1;
            clicks[i] = 0;
            RESP[i] = Direction.INVALID;
        }

        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Richtungsauswahl)
            {
                var d = data.Interaktionen[i] as Measurement.Richtungsauswahl;
                Debug.Log(d.ZahnradID.ToString() + " -> " + indices.IndexOf(d.ZahnradID));
                int cidx = indices.IndexOf(d.ZahnradID);
                if (cidx < 0)
                    continue;

                clicks[cidx]++;
                if (RT1[cidx] == 0)
                    RT1[cidx] = d.Zeitpunkt;
                RT2[cidx] = d.Zeitpunkt;
                CRESP[cidx] = d.CRESP ? 1 : 0;
                RESP[cidx] = d.Richtung;
            }
        }

        var z2 = new StringBuilder();
        z2.AppendFormat("{0},{1},{2},{3},{4},{5},{6}", data.Dauer, Cogs[indices[0]].Beschreibung, RT1[0], RT2[0], clicks[0], RESP[0], CRESP[0]);
        results.Add(z2);
        for (int i = 1; i<indices.Count; ++i)
        {
            var zi = new StringBuilder();
            zi.AppendFormat(",{0},{1},{2},{3},{4},{5}", Cogs[indices[i]].Beschreibung, RT1[i], RT2[i], clicks[i], RESP[i], CRESP[i]);
            results.Add(zi);
        }
    }

    public bool PositionIsValid(Vector2 pos, Richtungssticker cog)
    {
        return true;
    }

    public Vector2 NearestPositionCandidate(Vector2 pos, Richtungssticker cog)
    {
        return pos;
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

    public override void RegisterCog(Zahnrad cog)
    {
        SelectedDirections.Add(null);
        base.RegisterCog(cog);
    }

    public void AttachSticker(Richtungssticker sticker, Zahnrad cog)
    {
        int idx = Cogs.IndexOf(cog);
        if (idx < 0)
            return;
        if (SelectedDirections[idx] != null)
            GameObject.Destroy(SelectedDirections[idx].gameObject);
        SelectedDirections[idx] = sticker;
        Experiment.Measurement.MeasureDirectionSelected(sticker.Direction, Cogs.IndexOf(cog), sticker.Direction == cog.Direction);
    }
    /*
    public void SelectDirection(Direction dir, bool correct)
    {
        SelectedDirection = dir;
        Experiment.Measurement.MeasureDirectionSelected(dir, correct);
    }
    */

    /*
    private Auswahlpfeil CurrentSelector = null;
    public void SelectAnswer(Auswahlpfeil selected)
    {
        CurrentSelector = selected;
        SelectedDirection = selected.Richtung;
        Experiment.Measurement.MeasureDirectionSelected(SelectedDirection, selected.IstDieKorrekteLoesung);
    }

    public void Register(Auswahlpfeil s)
    {
    }

    public Auswahlpfeil GetCurrentlySelected()
    {
        return CurrentSelector;
    }

    public List<Auswahlpfeil> GetSelectors()
    {
        return new List<Auswahlpfeil>();
    }
    */
}

public class PropellerTrial : CogTrial
{
    public override void Aggregate(Measurement.Trial data)
    {
        results.Add(new StringBuilder("RT-Erstplatzierung,RT-Propeller1,RT-Propeller2,RT-Gesamt,Platzierungen,Drehungen,Propeller1,Propeller2,Geschwindigkeit,PropellerKontakt"));

        //TODO: platzierungen nur wenn sie mit brett interagieren
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
                if (RT3 == 0 && propeller_placed == 2)
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

        for (int i = 0; i < target.Count; ++i)
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

        var _z2 = new StringBuilder();
        _z2.AppendFormat("{0},{1},{2},{3},{4},{5}", RT1, RT2, RT3, data.Dauer, placements, rotations);
        _z2.AppendFormat(",{0},{1},{2},{3}", P[0], P[1], speed, propeller_intersect);
        results.Add(_z2);
        results.Add(new StringBuilder());
        results.Add(new StringBuilder("Interaktionen"));

        StringBuilder z1 = new StringBuilder("Typ");
        StringBuilder z2 = new StringBuilder("RT");
        StringBuilder z3 = new StringBuilder("Start-Ziel");
        StringBuilder z4 = new StringBuilder("Kettenlaenge");
        StringBuilder z5 = new StringBuilder("Zahnradgroesse");
        StringBuilder z6 = new StringBuilder("Mit Propeller");

        bool[] PreviouslyOnBoard = new bool[data.Zahnraeder.Count];
        int P1_idx = -1;
        int P2_idx = -1;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.PropellerEntfernt)
            {
                int cogID = (data.Interaktionen[i] as Measurement.PropellerEntfernt).ZahnradID;
                int cogSize = Experiment.Measurement.CurrentTrial.Zahnraeder[cogID].Zaehne;
                if (P1_idx == cogID)
                    P1_idx = -1;
                if (P2_idx == cogID)
                    P2_idx = -1;

                z1.Append(",Propeller");
                z2.AppendFormat(",{0}", data.Interaktionen[i].Zeitpunkt);
                z3.AppendFormat(",{0}", "entfernt");
                z4.Append(",");
                z5.AppendFormat(",{0}", cogSize);
                z6.Append(",");
            }

            if (data.Interaktionen[i] is Measurement.PropellerAngefuegt)
            {
                int cogID = (data.Interaktionen[i] as Measurement.PropellerAngefuegt).ZahnradID;
                int cogSize = Experiment.Measurement.CurrentTrial.Zahnraeder[cogID].Zaehne;
                if (P1_idx == -1)
                    P1_idx = cogID;
                else if (P2_idx == -1)
                    P2_idx = cogID;

                z1.Append(",Propeller");
                z2.AppendFormat(",{0}", data.Interaktionen[i].Zeitpunkt);
                z3.AppendFormat(",{0}", "angelegt");
                z4.Append(",");
                z5.AppendFormat(",{0}", cogSize);
                z6.Append(",");
            }

            if (data.Interaktionen[i] is Measurement.Platzierung)
            {
                var p = data.Interaktionen[i] as Measurement.Platzierung;
                if (!PreviouslyOnBoard[p.ZahnradID] && !p.AufBrett)
                    continue;

                z1.Append(",Platzierung");
                z2.AppendFormat(",{0}", p.Zeitpunkt);

                if (!PreviouslyOnBoard[p.ZahnradID] && p.AufBrett)
                {
                    z3.Append(",rein");
                    z4.AppendFormat(",{0}", p.Kettenlaenge);
                }
                else if (PreviouslyOnBoard[p.ZahnradID] && p.AufBrett)
                {
                    z3.Append(",innerhalb");
                    z4.AppendFormat(",{0}", p.Kettenlaenge);
                }
                else
                {
                    z3.Append(",raus");
                    z4.Append(",");
                }

                int cogSize = Experiment.Measurement.CurrentTrial.Zahnraeder[p.ZahnradID].Zaehne;
                z5.AppendFormat(",{0}", cogSize);


                if (P1_idx == p.ZahnradID || P2_idx == p.ZahnradID)
                {
                    z1.Append(",Propeller");
                    z2.AppendFormat(",{0}", p.Zeitpunkt);
                    z3.Append(",bewegt");
                    z4.Append(",");
                    z5.AppendFormat(",{0}", cogSize);
                    z6.Append(",1,");
                }
                else
                    z6.Append(",0");

                PreviouslyOnBoard[p.ZahnradID] = p.AufBrett;
            }
            if (data.Interaktionen[i] is Measurement.Drehung)
            {
                var d = data.Interaktionen[i] as Measurement.Drehung;

                z1.Append(",Drehung");
                z2.AppendFormat(",{0}", d.Zeitpunkt);
                z3.Append(",");
                z4.AppendFormat(",{0}", d.Kettenlaenge);
                z5.AppendFormat(",{0}", Experiment.Measurement.CurrentTrial.Zahnraeder[d.ZahnradID].Zaehne);
                z6.Append(",");
            }
        }
        results.Add(z1);
        results.Add(z2);
        results.Add(z3);
        results.Add(z4);
        results.Add(z5);
        results.Add(z6);
    }

    /* *
     * LOGIC
     * */
    private List<Propeller> PropellerSet;
    public PropellerTrial()
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
    public override void Aggregate(Measurement.Trial data)
    {
        results.Add(new StringBuilder("RT-Erstplatzierung,RT-Gesamt,Platzierungen,Drehungen,RESP,RESP-Distanz"));

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
        if (connected)
        {
            distance = Zahnrad.ConnectedComponent.Distance(start, target);
            bool CRESP = (distance % 2) == ((start.Direction == target.Direction) ? 1 : 0);
            /*if (CRESP && start.System.CanRotate)
                RESP = 5;
            else if (!CRESP && start.System.CanRotate)
                RESP = 4;
            else
                RESP = 3;
                */
            if (CRESP && start.System.CanRotate)
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

        var _z2 = new StringBuilder();
        _z2.AppendFormat("{0},{1},{2},{3},{4},{5}", RT1, data.Dauer, placements, rotations, RESP, distance);
        results.Add(_z2);
        results.Add(new StringBuilder());
        results.Add(new StringBuilder("Interaktionen"));

        StringBuilder z1 = new StringBuilder("Typ");
        StringBuilder z2 = new StringBuilder("RT");
        StringBuilder z3 = new StringBuilder("Start-Ziel");
        StringBuilder z4 = new StringBuilder("Kettenlaenge");
        StringBuilder z5 = new StringBuilder("Zahnradgroesse");

        bool[] PreviouslyOnBoard = new bool[data.Zahnraeder.Count];
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Platzierung)
            {
                var p = data.Interaktionen[i] as Measurement.Platzierung;
                if (!PreviouslyOnBoard[p.ZahnradID] && !p.AufBrett)
                    continue;

                int cogSize = Experiment.Measurement.CurrentTrial.Zahnraeder[p.ZahnradID].Zaehne;

                z1.Append(",Platzierung");
                z2.AppendFormat(",{0}", p.Zeitpunkt);

                if (!PreviouslyOnBoard[p.ZahnradID] && p.AufBrett)
                { 
                    z3.Append(",rein");
                    z4.AppendFormat(",{0}", p.Kettenlaenge);
                }
                else if (PreviouslyOnBoard[p.ZahnradID] && p.AufBrett)
                {
                    z3.Append(",innerhalb");
                    z4.AppendFormat(",{0}", p.Kettenlaenge);
                }
                else
                {
                    z3.Append(",raus");
                    z4.Append(",");
                }

                z5.AppendFormat(",{0}", cogSize);

                PreviouslyOnBoard[p.ZahnradID] = p.AufBrett;
            }
            if (data.Interaktionen[i] is Measurement.Drehung)
            {
                var d = data.Interaktionen[i] as Measurement.Drehung;
                int cogSize = Experiment.Measurement.CurrentTrial.Zahnraeder[d.ZahnradID].Zaehne;

                z1.Append(",Drehung");
                z2.AppendFormat(",{0}", d.Zeitpunkt);
                z3.Append(",");
                z4.AppendFormat(",{0}", d.Kettenlaenge);
                z5.AppendFormat(",{0}", cogSize);
            }
        }
        results.Add(z1);
        results.Add(z2);
        results.Add(z3);
        results.Add(z4);
        results.Add(z5);
    }

    /* *
     * LOGIC
     * */
}