using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

/**
 * DAS IST EIN TESTKOMMENTAR FUER DUMME LEUTE
 * 
 */


public class Measurement : MonoBehaviour
{
    private Messdaten data;
    private System.Diagnostics.Stopwatch timer;

    public int VPN_Num
    {
        get => data.VPN;
        set
        {
            data.VPN = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        timer = new System.Diagnostics.Stopwatch();
    }
    
    public void Init(int VPN)
    {
        data = new Messdaten();
        data.VPN = VPN;
    }
    public void Finish()
    {
        SaveData(data);
    }
    
    public void newBlock(string type)
    {
        var b = new Block {Typ = type};
        data.Blocks.Add(b);
    }
    
    public void newTrial(ITrial trial)
    {
        var t = new Trial{ Name = trial.Name };
        if(data.Blocks.Count == 0)
            newBlock("default");
        data.Blocks[data.Blocks.Count -1].Trials.Add(t);
        
        timer.Restart();
    }
    
    private Trial CurrentTrial()
    {
        var block = data.Blocks[data.Blocks.Count -1];
        return block.Trials[block.Trials.Count-1];
    }
    
    public void SaveCogInfo(int id, int size)
    {
        var trial = CurrentTrial();
        if(trial.Zahnraeder == null)
            trial.Zahnraeder = new List<ZahnradInfo>();
        trial.Zahnraeder.Add(new ZahnradInfo {ID = id, Zaehne = size});
        
    }
    public void MeasureCogPlaced(int x, int y, bool connected, bool onBoard, int id)
    {
        CurrentTrial().Interaktionen.Add(new Platzierung{x = x, y=y, ZahnradID=id, AufBrett = onBoard, Zeitpunkt = timer.ElapsedMilliseconds, verbunden = connected});
    }
    public void MeasureCogRotated(int dir, int id)
    {
        CurrentTrial().Interaktionen.Add(new Drehung{Richtung=dir, ZahnradID=id, Zeitpunkt=timer.ElapsedMilliseconds});
    }
    public void MeasureCogSelected(int id, bool correct)
    {
        CurrentTrial().Interaktionen.Add(new Zahnradauswahl { ZahnradID = id, CRESP = correct, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasureDirectionSelected(DirectionTrial.Direction dir, bool correct)
    {
        CurrentTrial().Interaktionen.Add(new Richtungsauswahl { Richtung = dir, CRESP = correct, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasurePropellerAttached(int id)
    {
        CurrentTrial().Interaktionen.Add(new PropellerAngefuegt { ZahnradID = id, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasurePropellerDetached(int id)
    {
        CurrentTrial().Interaktionen.Add(new PropellerEntfernt { ZahnradID = id, Zeitpunkt = timer.ElapsedMilliseconds });
    }

    void SaveData(Messdaten data)
    {
        string xmlPath = Path.Combine(Application.persistentDataPath, "VPN"+data.VPN.ToString()+".xml"); 
        var serializer = new XmlSerializer(typeof(Messdaten));

        using (var stream = new FileStream(xmlPath, FileMode.Create))
        {
            serializer.Serialize(stream, data);
        }
        
        string csvPath = Path.Combine(Application.persistentDataPath, "VPN"+data.VPN.ToString()+".csv");

        using (var stream = new StreamWriter(new FileStream(csvPath, FileMode.Create)))
        {
            stream.WriteLine("VPN,"+data.VPN.ToString()+"\n");
            for(int blockIdx = 0; blockIdx < data.Blocks.Count; ++blockIdx)
            {
                var data_block = data.Blocks[blockIdx];
                var logic_block = Experiment.Instance.Blocks[blockIdx];
                logic_block.Aggregate(data_block, stream);

                for (int trialIdx = 0; trialIdx < data_block.Trials.Count; ++trialIdx)
                {
                    var data_trial = data_block.Trials[trialIdx];
                    var logic_trial = logic_block.Trials[trialIdx];

                    logic_trial.Aggregate(data_trial, stream);
                }

                stream.WriteLine("");
                /*
                stream.WriteLine("Block,"+block.Typ);
                stream.WriteLine("Name,Drehungen,Platzierungen,RT1,RT2,RT3,CRESP");
                foreach(var trial in block.Trials)
                {
                    long RT1 = 0;
                    if(trial.Interaktionen.Count>0)
                        RT1 = trial.Interaktionen[0].Zeitpunkt;
                    long RT3 = 0;
                    for(int i = 1; i<trial.Interaktionen.Count; ++i)
                        RT3 += trial.Interaktionen[i].Zeitpunkt;
                    int rotationCount = 0;
                    int placementCount = 0;
                    for(int i = 0; i<trial.Interaktionen.Count; ++i)
                    {
                        if(trial.Interaktionen[i] is Platzierung)
                            placementCount++;
                        if(trial.Interaktionen[i] is Drehung)
                            rotationCount++;
                    }
                    
                    
                    stream.WriteLine(trial.Name+","+rotationCount.ToString()+","+placementCount.ToString()+","+RT1.ToString()+",na,"+RT3.ToString()+",na");
                }
                */
            }
            stream.Flush();
        }
    }
    
    public class Messdaten
    {
        public int VPN;
        public List<Block> Blocks;
        public Messdaten()
        {
            Blocks = new List<Block>();
        }
    }
    
    public class Block
    {
        [XmlAttribute]
        public string Typ;
        public List<Trial> Trials;
        public Block()
        {
            Trials = new List<Trial>();
        }
    }
    public class Trial
    {
        [XmlAttribute]
        public string Name;
        public List<ZahnradInfo> Zahnraeder;
        public List<Interaktion> Interaktionen;
        public Trial()
        {
            Interaktionen = new List<Interaktion>();
            Zahnraeder = null;
        }
    }
    
    public class ZahnradInfo
    {
        [XmlAttribute]
        public int ID;
        [XmlAttribute]
        public int Zaehne;
    }
    
    [   
        XmlInclude(typeof(Platzierung)), 
        XmlInclude(typeof(Drehung)), 
        XmlInclude(typeof(Richtungsauswahl)), 
        XmlInclude(typeof(Zahnradauswahl)),
        XmlInclude(typeof(PropellerAngefuegt)),
        XmlInclude(typeof(PropellerEntfernt))
    ]
    public abstract class Interaktion
    {
        [XmlAttribute]
        public long Zeitpunkt;
    }
    public class Platzierung : Interaktion
    {
        [XmlAttribute]
        public int ZahnradID;
        [XmlAttribute]
        public int x;
        [XmlAttribute]
        public int y;
        [XmlAttribute]
        public bool verbunden;
        [XmlAttribute]
        public bool AufBrett;
    }
    public class Drehung : Interaktion
    {
        [XmlAttribute]
        public int ZahnradID;
        [XmlAttribute]
        public int Richtung;
    }
    public class Richtungsauswahl : Interaktion
    {
        [XmlAttribute]
        public DirectionTrial.Direction Richtung;
        [XmlAttribute]
        public bool CRESP;
    }
    public class Zahnradauswahl : Interaktion
    {
        [XmlAttribute]
        public int ZahnradID;
        [XmlAttribute]
        public bool CRESP;
    }
    public class PropellerAngefuegt : Interaktion
    {
        [XmlAttribute]
        public int ZahnradID;
    }
    public class PropellerEntfernt : Interaktion
    {
        [XmlAttribute]
        public int ZahnradID;
    }
}
