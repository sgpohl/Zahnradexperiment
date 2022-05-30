using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Linq;


public class DataGoNoGO : MonoBehaviour
{
    //VPN nummer soll im gesamten project den Input vom Textfeld VPN bekommen sodass die datei auf diejenige Versuchsperson sich bezieht
    public static string VPN;
    static string fileName;
    public static string filePath;

    int i = 1;

    public static StringBuilder overall = new StringBuilder();
    public static StringBuilder header = new StringBuilder();

    public static List<StringBuilder> results = new List<StringBuilder>();
    
    public static StringBuilder z1 = new StringBuilder();


    int gesamtPunktzahl;

    void Start()
    {

        gesamtPunktzahl = GoNoGo.correctNoClick + GoNoGo.correctClick;
        fileName = "VPN" + VPN + "_goNoGo.csv";
        fileName = checkFilename(fileName);
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        overall.Append("Go-Nogo Task,Gesamtpunktzahl,"+ gesamtPunktzahl +"\n");
        overall.Append(",Hits," + GoNoGo.correctClick + "\n");
        overall.Append(",Misses," + GoNoGo.incorrectNoClick + "\n");
        overall.Append(",Correct Rejections," + GoNoGo.correctNoClick + "\n");
        overall.Append(",False Alarms," + GoNoGo.incorrectClick + "\n\n\n");
        header.Append(",aktuelles NoGo-Tier,praesentiertes Tier, Click(Button), CRESP, RT (in ms)\n");

        results.Add(overall);
        results.Add(header);
        results.Add(z1);
        File.WriteAllText(filePath, ListToString(results));
    }

    public string checkFilename(string fileName)
    {    
        while(File.Exists(Path.Combine(Application.persistentDataPath, fileName)))
        {
            fileName = "VPN" + VPN + "(" + i + ")" + "_goNoGo.csv";
            i++;
        }
        return fileName;
    }
    public static void MeasureSequenz(string currentAnimal, string actualAnimal, int clicked, int CRESP, double reaction)
    {
        z1.AppendFormat(",{0},{1},{2},{3},{4}\n", currentAnimal, actualAnimal, clicked, CRESP, reaction.ToString("0", System.Globalization.CultureInfo.InvariantCulture));
    }

    private string ListToString(List<StringBuilder> results)
    {
        string x = "";
        foreach (var element in results)
        {
            x = x + element.ToString();
        }

        return x;
    }
}
