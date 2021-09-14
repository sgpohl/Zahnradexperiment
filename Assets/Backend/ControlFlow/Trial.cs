using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text;

/// Organizes data exchange on the trial level, i.e. for object interactions, selections
/// Implements data->string conversion for the results.csv
public abstract class ITrial
{
    public enum AnswerType
    {
        INCORRECT = -1,
        CORRECT = 1,
        UNDEFINED = 0
    }

    protected bool IsLoaded = false;
    public string Name { get; set; }

    public ITrial()
    {
        Name = "UNDEFINED";
        results = new List<StringBuilder>();
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
        Aggregate(Experiment.Measurement.CurrentTrial);
    }

    public List<StringBuilder> results { get; protected set; }
    /// Implements data->string conversion for the results.csv
    public virtual void Aggregate(Measurement.Trial data)
    {
    }

    public string ToString(string prefix)
    {
        var all = new StringBuilder();
        foreach(var line in results)
            all.AppendFormat("{0}{1}\n", prefix, line);
        return all.ToString();
    }
    public override string ToString()
    {
        return ToString("");
    }
}

public interface ITrialFunctionality<T> where T : MonoBehaviour
{
    bool PositionIsValid(Vector2 pos, T cog);
    Vector2 NearestPositionCandidate(Vector2 pos, T cog);
}
public interface ISelectorTrial<T> where T : Selector
{
    void SelectAnswer(T selected);
    void Register(T s);

    T GetCurrentlySelected();
    List<T> GetSelectors();
}

/// Simple trial, that requires the selection of an item (type Auswahlsymbol)
public class SelectionTrial : ITrial, ISelectorTrial<Auswahlsymbol>
{
    public AnswerType Answer { get;  private set; }
    private List<Auswahlsymbol> Selectors;

    public SelectionTrial()
    {
        Selectors = new List<Auswahlsymbol>();
        Answer = AnswerType.UNDEFINED;
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
        z2.Append(data.Name);
        z2.AppendFormat(",{0},{1},{2},{3},{4},{5}", RT1, RT2, data.Dauer, clicks, RESP, CRESP);
        results.Add(z2);
    }
}

