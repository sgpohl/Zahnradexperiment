using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class ExperimentConfig
{
    private static void LoadNode(XmlNode node)
    {
        string logstring = node.Name;
        if(node.Attributes != null)
            logstring += " = "+node.Attributes.ToString();
        Debug.Log(logstring);
        foreach(XmlNode child in node.ChildNodes)
            LoadNode(child);
        /*
        switch(node.Name)
        {
            case "Trial":
                break;
            case "Block":
                break;
            case "Serial":
                break;
            case "Counterbalance":
                break;
            case "Random":
                break;
            default:
                break;
        }*/
    }
    
    public static void Load(string filename)
    {
        string xmlpath = Path.Combine(Application.persistentDataPath, filename);
        
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlpath);
        
        LoadNode(doc);
    }
    
    /*
    public class Config
    {
        
    }
    
    public class Execution
    {
        string scene;
    }
    
    public abstract class Element
    {
        Element next {get;set}
        
        public void instantiate(List<Element>);
    }
    
    public class Block : Element
    {
        void instantiate(List<Element>)
        {
        }
    }
    */
}
