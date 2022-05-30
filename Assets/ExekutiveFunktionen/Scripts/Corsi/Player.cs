using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Player : MonoBehaviour
{
    public List<GameObject> clickedBlocks = new List<GameObject>();
    public List<GameObject> sequenzBlocks = new List<GameObject>();

    public int rightTaskCounter;
    public int accuracyCounter;
    public int totalClicksCounter;
    public static int currentSequenzCounter;

    public static Stopwatch timer = new Stopwatch();

    int listCompareVar;

    /*
     * Start() Funktion wird beim Programmstart aufgerufen um die Variablen zu bereinigen und alles auf default 0 zu setzten
     */
    void Start()
    {
        DataSaver.totalTime = 0.0d;
        currentSequenzCounter = 0;
        rightTaskCounter = 0;
        totalClicksCounter = 0;
        accuracyCounter = 0;
        clickedBlocks.Clear();
    }


    /*
     * Update() Funktion wird benutzt damit die Testperson nicht mehr Klicks als erfordelich ausfuehren kann bzw. die Klicks
     * nicht abgespeichert werden
     */
    private void Update()
    {

        if(clickedBlocks.Count > sequenzBlocks.Count)
        {
            clickedBlocks.RemoveAt(clickedBlocks.Count - 1);
            totalClicksCounter--;
        }
    }

    /*
     * Ruft CompareLists auf und leert anschliessend die Listen der Sequenz und der geklickten Bloecke
     */
    public void CleanLists()
    {
        if(Randomizer.reverse == true)
        {
            
            CompareListsReverse();
        }
        else
        {
            CompareLists();
        }
        
        clickedBlocks.Clear();
        sequenzBlocks.Clear();
    }

    public void increaseClick()
    {
        totalClicksCounter++;
    }



    /*
     * Der Timer wird beim Aufruf der CompareList Funktion gestoppt und anschliessen wird die Zeit zur Gesamten Zeit addiert
     *      * 
     * x und y sind jeweils die laengen von der geklickten Liste und der Sequenz Liste. y ist z.B. maximal 4
     * 
     * der Array clicks wird mit einer laenge von 4 erstellt mit einer -1 an jeder Stelle
     * Falls garkein Klick erfolgt wird in die CSV eine -1 eingefuegt an der stelle 0 = falsch und 1 = richtiger Klick
     * deswegen werden alle Klicks am Anfang automatisch auf -1 gesetzt
     * 
     * listCompareVar ist eine boolean variable welche fuer die Aktuelle Sequenz belegt ob Sie True oder False ist
     * Diese Variable wird auch anschliessend ind die CSV mit uebernommen, um zu sehen welche Sequenz NR richtig oder Falsch ist
     * 
     * Am Ende der Funktion wird geprueft ob die Boolean variable (listCompareVar) richtig oder falsch ist und anschliessend die Informationen
     * mithilfe der WriteInDatasaver() Funktion abgespeichert 
     * 
     * Da y = die laenge der aktuellen Sequenz  ist wird in der WriteInDataSaver bestimmt ob 2,3 oder vier Klicks erwartet wurden
     * 
     */
    public bool CompareLists()
    {
        timer.Stop();
        DataSaver.totalTime += timer.Elapsed.TotalMilliseconds;
        currentSequenzCounter++;
        int x = clickedBlocks.Count;
        int y = sequenzBlocks.Count;
        int[] clicks = {-1,-1,-1,-1,-1, -1};
        listCompareVar = 1;

       
        if (x != y)
        {
            
            listCompareVar = 0;
        }

        for (int i = 0; i < x; i++)
        {
            if (clickedBlocks[i] == sequenzBlocks[i])
            {
                clicks[i] = 1;
                accuracyCounter++;                
            }
            

            if (clickedBlocks[i] != sequenzBlocks[i])
            {
                clicks[i] = 0;
                listCompareVar = 0;
            }
        }

        if (listCompareVar == 1)
        {
            
            WriteInDatasaver(listCompareVar, clicks[0], clicks[1], clicks[2], clicks[3], clicks[4], clicks[5], y);
            rightTaskCounter++;
            timer.Reset();
            return true;
        }
        WriteInDatasaver(listCompareVar, clicks[0], clicks[1], clicks[2], clicks[3], clicks[4], clicks[5], y);
        timer.Reset();
        Randomizer.countFalseTask++;
        return false;  
    }

    public bool CompareListsReverse()
    {
        timer.Stop();
        DataSaver.totalTime += timer.Elapsed.TotalMilliseconds;
        currentSequenzCounter++;
        int x = clickedBlocks.Count;
        int y = sequenzBlocks.Count;
        int[] clicks = { -1, -1, -1, -1, -1 , -1};
        listCompareVar = 1;
        int j = sequenzBlocks.Count - 1;
        if (x != y)
        {
            listCompareVar = 0;
        }

        for (int i = 0; i < x; i++)
        {
            if (clickedBlocks[i] == sequenzBlocks[j])
            {
                clicks[i] = 1;
                accuracyCounter++;
            }


            if (clickedBlocks[i] != sequenzBlocks[j])
            {
                clicks[i] = 0;
                listCompareVar = 0;
            }
            j--;
        }

        if (listCompareVar == 1)
        {
            WriteInDatasaver(listCompareVar, clicks[0], clicks[1], clicks[2], clicks[3], clicks[4], clicks[5], y);
            rightTaskCounter++;
            timer.Reset();
            return true;
        }
        WriteInDatasaver(listCompareVar, clicks[0], clicks[1], clicks[2], clicks[3], clicks[4], clicks[5], y);
        timer.Reset();
        Randomizer.countFalseTask++;
        return false;
    }


    private void WriteInDatasaver (int listCompareVar, int click1, int click2, int click3, int click4, int click5, int click6, int sequenzLength)
    {
        if (sequenzLength == 1) DataSaver.MeasureSequenzZero(listCompareVar, timer.Elapsed.TotalMilliseconds, click1);
        if (sequenzLength == 2) DataSaver.MeasureSequenzOne(listCompareVar, timer.Elapsed.TotalMilliseconds,click1,click2);
        if (sequenzLength == 3) DataSaver.MeasureSequenzTwo(listCompareVar,timer.Elapsed.TotalMilliseconds, click1,click2,click3);
        if (sequenzLength == 4) DataSaver.MeasureSequenzThree(listCompareVar, timer.Elapsed.TotalMilliseconds, click1, click2, click3,click4);
        if (sequenzLength == 5) DataSaver.MeasureSequenzFour(listCompareVar, timer.Elapsed.TotalMilliseconds, click1, click2, click3, click4, click5);
        if (sequenzLength == 6) DataSaver.MeasureSequenzFive(listCompareVar, timer.Elapsed.TotalMilliseconds, click1, click2, click3, click4, click5, click6);
    }
}
