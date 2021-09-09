using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;

public class StabilityTrial : ITrial, ITrialFunctionality<Bauklotz>
{
    public Vector2 NearestPositionCandidate(Vector2 pos, Bauklotz b)
    {
        Bauklotz overlapping = null;
        for (int i = 0; i < Blocks.Count; ++i)
        {
            if (Blocks[i] == b)
                continue;
            if (b.Overlaps(Blocks[i], pos))
            {
                overlapping = Blocks[i];
                break;
            }
        }
        if (overlapping == null)
            return pos;
        /*
        Vector2 c1 = b.Collider.bounds.center;
        Vector2 c2 = overlapping.Collider.bounds.center;
        Vector2 dir = c2 - c1;
        dir.Normalize();
        dir *= 1000;

        float scale = 1000;
        Vector2 cp1 = b.ClosestPoint(pos, c1+dir* scale) - c1;
        Vector2 cp2 = overlapping.ClosestPoint(overlapping.transform.position, c2-dir* scale) - c2;
        float len = (cp2 + cp1).magnitude;
        */
        var d = b.Collider.Distance(overlapping.Collider);
        Vector2 diff = d.normal * (d.distance < 0 ? d.distance : 0);

        //Debug.Log("moved " + b.gameObject.name + "  " + b.transform.position.ToString("0.0") + " -> " + diff.ToString("0.0") +" * "+ d.distance.ToString("0.00"));

        //return (Vector2)b.Collider.transform.position + diff;
        return pos;
        //return (Vector2)overlapping.transform.position + len*dir;
    }

    public bool PositionIsValid(Vector2 pos, Bauklotz b)
    {
        return true;
        /*        for (int i = 0; i < Blocks.Count; ++i)
                {
                    if (Blocks[i] == b)
                        continue;
                    if (b.Overlaps(Blocks[i], pos))
                        return false;
                }
                return true;*/
    }

    protected List<BauklotzZielgebiet> TargetAreas;
    protected List<Bauklotz> Blocks;
    protected List<Bauklotz> SolutionBlocks;
    public StabilityTrial() : base()
    {
        TargetAreas = new List<BauklotzZielgebiet>();
        Blocks = new List<Bauklotz>();
        SolutionBlocks = new List<Bauklotz>();
    }


    public void Register(Bauklotz b)
    {
        Blocks.Add(b);
        if (!b.IsFixedInPlace)
            SolutionBlocks.Add(b);
    }
    public void Register(BauklotzZielgebiet a)
    {
        TargetAreas.Add(a);
    }

    public bool IsCorrectlyPlaced(Bauklotz b)
    {
        bool correct = false;
        foreach (BauklotzZielgebiet area in TargetAreas)
            if (area.ContainsCompletely(b))
                correct = true;
        return correct;
    }

    public virtual void SolutionBlockSelected(Bauklotz b)
    {
    }

    public virtual void SolutionBlockReleased(Bauklotz b)
    {
    }
}

public class GreenStabilityTrial : StabilityTrial, ISelectorTrial<Auswahlsymbol>
{
    private Auswahlsymbol Selector;
    private const int MaxIterations = 3;
    private const int SimulationDuration = 3000;
    private int Iteration = 0;

    private System.Diagnostics.Stopwatch timer;
    public GreenStabilityTrial() : base()
    {
        timer = new System.Diagnostics.Stopwatch();
    }
    public override void Update(float DeltaTime)
    {
        base.Update(DeltaTime);
        if(timer.ElapsedMilliseconds > SimulationDuration)
        {
            timer.Stop();
            timer.Reset();

            if (Iteration < MaxIterations)
            {
                foreach (var block in Blocks)
                {
                    block.ResetPosition();
                    block.LockMovement();
                    block.Show();
                }
                Selector.Deselect();
            }
        }
    }


    public Auswahlsymbol GetCurrentlySelected()
    {
        return Selector;
    }

    public List<Auswahlsymbol> GetSelectors()
    {
        var l = new List<Auswahlsymbol>();
        l.Add(Selector);
        return l;
    }

    public void Register(Auswahlsymbol s)
    {
        Selector = s;
    }

    private Bauklotz Solution
    {
        get
        {
            foreach (var block in SolutionBlocks)
                if (IsCorrectlyPlaced(block))
                    return block;
            return null;
        }
    }
    private bool IsSolved { get { return Solution != null; } }

    public void SelectAnswer(Auswahlsymbol selected)
    {
        if (selected != Selector)
            return;

        Experiment.Measurement.MeasureSelection(0, IsSolved);

        if (!IsSolved)
        {
            foreach (var block in Blocks)
                block.UnlockMovement();
            timer.Start();
        }
        Iteration++;
    }

    public override void SolutionBlockSelected(Bauklotz b)
    {
        b.Show();
        foreach (var block in SolutionBlocks)
            if (block != b)
            {
                block.ResetPosition();
                block.Hide();
            }
    }
    public override void SolutionBlockReleased(Bauklotz b)
    {
        foreach (var block in SolutionBlocks)
            if (block != b)
            {
                block.ResetPosition();
                block.Show();
            }
    }

    public override void Aggregate(Measurement.Trial data)
    {
        long[] RT = new long[MaxIterations];
        long RT_total = 0;
        int attempts = Iteration;

        if (!IsSolved)
            attempts = 4;


        Vector2 coords = new Vector2();
        if (Solution != null)
            coords = Solution.transform.position;

        int clicks = 0;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Optionsauswahl)
            {
                if (clicks > Iteration)
                    throw new Exception("Collected more Selections than Iterations in a GreenStabilityTrial");

                RT[clicks] = data.Interaktionen[i].Zeitpunkt - SimulationDuration*clicks;
                RT_total = RT[clicks];

                clicks++;
            }
        }

        for(int i = Iteration - 1; i>0; --i)
            RT[i] -= RT[i - 1];


        var z2 = new StringBuilder();
        var info = System.Globalization.CultureInfo.InvariantCulture;
        z2.AppendFormat("{0},({1};{2}),{3},{4},{5},{6}", attempts, coords.x.ToString("#0.00", info), coords.y.ToString("#0.00", info), RT[0], RT[1], RT[2], RT_total);
        results.Add(z2);
    }
}

public class RedStabilityTrial : StabilityTrial, ISelectorTrial<Auswahlsymbol>
{
    private AnswerType Answer = AnswerType.UNDEFINED;
    private List<Auswahlsymbol> Selectors;

    public RedStabilityTrial() : base()
    {
        Selectors = new List<Auswahlsymbol>();
    }

    private Auswahlsymbol CurrentlySelected = null;
    public void SelectAnswer(Auswahlsymbol selector)
    {
        CurrentlySelected = selector;
        if (selector == null)
            Answer = AnswerType.UNDEFINED;
        else
            Answer = selector.IstDieKorrekteLoesung;
        Experiment.Measurement.MeasureSelection(selector.Nummer, Answer == AnswerType.CORRECT);
    }

    public void Register(Auswahlsymbol selector)
    {
        Selectors.Add(selector);
    }

    public Auswahlsymbol GetCurrentlySelected()
    {
        return CurrentlySelected;
    }

    public List<Auswahlsymbol> GetSelectors()
    {
        return Selectors;
    }

    public override void Aggregate(Measurement.Trial data)
    {
        long RT1 = 0;
        long RT2 = 0;
        int CRESP = -1;
        int clicks = 0;
        int RESP = -1;
        for (int i = 0; i < data.Interaktionen.Count; ++i)
        {
            if (data.Interaktionen[i] is Measurement.Optionsauswahl)
            {
                clicks++;
                if (RT1 == 0)
                    RT1 = data.Interaktionen[i].Zeitpunkt;
                RT2 = data.Interaktionen[i].Zeitpunkt;
                CRESP = (data.Interaktionen[i] as Measurement.Optionsauswahl).CRESP ? 1 : 0;
                RESP = (data.Interaktionen[i] as Measurement.Optionsauswahl).Nummer;
            }
        }

        var z2 = new StringBuilder();
        z2.AppendFormat("{0},{1},{2},{3},{4},{5}", RT1, RT2, data.Dauer, clicks, RESP, CRESP);
        results.Add(z2);
    }
}
