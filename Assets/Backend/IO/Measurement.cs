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
    public void SaveAll()
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
        var t = new Trial{ Name = trial.Name, Dauer = -1 };
        if(data.Blocks.Count == 0)
            newBlock("default");
        data.Blocks[data.Blocks.Count -1].Trials.Add(t);
        
        timer.Restart();
    }


    public Block CurrentBlock    {  get  {  return data.Blocks[data.Blocks.Count - 1];  }  }
    public Trial CurrentTrial    {  get  {  return CurrentBlock.Trials[CurrentBlock.Trials.Count - 1];   }    }
    
    public void SaveCogInfo(int id, int size)
    {
        var trial = CurrentTrial;
        if(trial.Zahnraeder == null)
            trial.Zahnraeder = new List<ZahnradInfo>();
        trial.Zahnraeder.Add(new ZahnradInfo {ID = id, Zaehne = size});
        
    }
    public void MeasureCogPlaced(int x, int y, bool connected, bool onBoard, int id)
    {
        CurrentTrial.Interaktionen.Add(new Platzierung{x = x, y=y, ZahnradID=id, AufBrett = onBoard, Zeitpunkt = timer.ElapsedMilliseconds, verbunden = connected});
    }
    public void MeasureCogRotated(int dir, int id, int systemSize)
    {
        CurrentTrial.Interaktionen.Add(new Drehung{Richtung=dir, ZahnradID=id, Kettenlaenge=systemSize, Zeitpunkt=timer.ElapsedMilliseconds});
    }
    public void MeasureCogSelected(int id, bool correct)
    {
        CurrentTrial.Interaktionen.Add(new Zahnradauswahl { ZahnradID = id, CRESP = correct, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasureDirectionSelected(DirectionTrial.Direction dir, int id, bool correct)
    {
        CurrentTrial.Interaktionen.Add(new Richtungsauswahl { Richtung = dir, CRESP = correct, ZahnradID = id, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasurePropellerAttached(int id)
    {
        CurrentTrial.Interaktionen.Add(new PropellerAngefuegt { ZahnradID = id, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasurePropellerDetached(int id)
    {
        CurrentTrial.Interaktionen.Add(new PropellerEntfernt { ZahnradID = id, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasureSelection(int selectorNumber, bool correct)
    {
        CurrentTrial.Interaktionen.Add(new Optionsauswahl { Nummer = selectorNumber, CRESP = correct, Zeitpunkt = timer.ElapsedMilliseconds });
    }
    public void MeasureTrialFinished()
    {
        CurrentTrial.Dauer = timer.ElapsedMilliseconds;
    }

    public void SaveCurrentBlock()
    {
        int blockIdx = data.Blocks.Count - 1;
        var data_block = data.Blocks[blockIdx];
        var logic_block = Experiment.Instance.Blocks[blockIdx];

        string csvPath = Path.Combine(Application.persistentDataPath, logic_block.FileName+".csv");

        using (var stream = new StreamWriter(new FileStream(csvPath, FileMode.Create)))
        {
            stream.WriteLine(logic_block.Aggregate(data_block));
            stream.Flush();
        }
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

                stream.WriteLine(logic_block.Aggregate(data_block));
                stream.WriteLine("");
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
        [XmlAttribute]
        public long Dauer;
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
        [XmlAttribute]
        public int Kettenlaenge;
    }
    public class Richtungsauswahl : Interaktion
    {
        [XmlAttribute]
        public DirectionTrial.Direction Richtung;
        [XmlAttribute]
        public bool CRESP;
        [XmlAttribute]
        public int ZahnradID;
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
    public class Optionsauswahl : Interaktion
    {
        [XmlAttribute]
        public int Nummer;
        [XmlAttribute]
        public bool CRESP;
    }
}
