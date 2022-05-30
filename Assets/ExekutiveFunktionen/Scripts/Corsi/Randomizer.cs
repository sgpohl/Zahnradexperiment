using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Randomizer : MonoBehaviour
{

    [SerializeField] List<Transform> blocks = new List<Transform>();
    public GameObject fairy;
    public Player player;
    float speed = 1f;
    public int count1, count2 = 0;
    public static int totalTasks = 0;
    public static int totlalAccuracyClicks = 0;
    public static bool reverse;
    public TextMesh increaseText;
    [SerializeField] Button increaseButton;


    public static int countFalseTask = 0;


    public static int clickedBlocks = 0;
    int sequenzBlocks = 1;



    private void Start()
    {  
        Debug.Log(reverse);
        player = FindObjectOfType<Player>();
        fairy.transform.position = new Vector3(-7f, 3f, -1);
        StartCoroutine(SequenzZero(3));
        count1++; 
    }

    public void Update()
    {
        
        if(clickedBlocks == sequenzBlocks)
        {
            player.CleanLists();
            clickedBlocks = 0;
            StartSequenz();
        }
    }

    public void StartSequenz()
    {
        //Debug.Log(DataSaver.filePath.ToString());
        fairy.SetActive(true);
        fairy.transform.position = new Vector3(-7f, 3f, -1);

        

        if (count1 == 7 && count2 != 5)
        {
            count1 = 0;
            count2++;
            countFalseTask = 0;
            sequenzBlocks++;
        } 


        // 3 7 richtig
        if(count1 == 6 && count2 == 5)
        {
            //Finish the whole Sequenz so the OutroScene is loaded
            Debug.Log("FINISH");
            DataSaver.rightTask = player.rightTaskCounter.ToString();
            DataSaver.accuracy = player.accuracyCounter.ToString();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }


        //Zahlen fuer die verschiedenen Trials wurden mithilfe der GetRandom Funktion erstellt

        //Trial 0
        if (count1 == 1 && count2 == 0) StartCoroutine(SequenzZero(3));
        if (count1 == 2 && count2 == 0) StartCoroutine(SequenzZero(4));
        if (count1 == 3 && count2 == 0) StartCoroutine(SequenzZero(5));
        if (count1 == 4 && count2 == 0) StartCoroutine(SequenzZero(6));
        if (count1 == 5 && count2 == 0) StartCoroutine(SequenzZero(7));
        if (count1 == 6 && count2 == 0) increaseWarning();


        //Trial 1 
        if (count1 == 0 && count2 == 1)
        {
            showField();
            StartCoroutine(SequenzOne(5,1));
        }
        if (count1 == 1 && count2 == 1) StartCoroutine(SequenzOne(8, 1));
        if (count1 == 2 && count2 == 1) StartCoroutine(SequenzOne(5, 3));
        if (count1 == 3 && count2 == 1) StartCoroutine(SequenzOne(9, 8));
        if (count1 == 4 && count2 == 1) StartCoroutine(SequenzOne(7, 2));
        if (count1 == 5 && count2 == 1) StartCoroutine(SequenzOne(7, 8));
        if (count1 == 6 && count2 == 1) increaseWarning();

        //Trial 2
        if (count1 == 0 && count2 == 2) 
        {
            showField();
            StartCoroutine(SequenzTwo(1, 5, 9));
        }         
        if (count1 == 1 && count2 == 2) StartCoroutine(SequenzTwo(1, 4, 9));
        if (count1 == 2 && count2 == 2) StartCoroutine(SequenzTwo(2, 8, 6));
        if (count1 == 3 && count2 == 2) StartCoroutine(SequenzTwo(5, 2, 9));
        if (count1 == 4 && count2 == 2) StartCoroutine(SequenzTwo(7, 8, 2));
        if (count1 == 5 && count2 == 2) StartCoroutine(SequenzTwo(5, 2, 8));
        if (count1 == 6 && count2 == 2) increaseWarning();

        //Trial 3
        if (count1 == 0 && count2 == 3)
        {
            showField();
            StartCoroutine(SequenzThree(8, 4, 9, 6));
        }
        
        if (count1 == 1 && count2 == 3) StartCoroutine(SequenzThree(5, 6, 9, 2));
        if (count1 == 2 && count2 == 3) StartCoroutine(SequenzThree(1, 9, 3, 6));         
        if (count1 == 3 && count2 == 3) StartCoroutine(SequenzThree(9, 1, 7, 6));
        if (count1 == 4 && count2 == 3) StartCoroutine(SequenzThree(1, 2, 7, 5));
        if (count1 == 5 && count2 == 3) StartCoroutine(SequenzThree(1, 9, 3, 8));
        if (count1 == 6 && count2 == 3) increaseWarning();

        //Trial 4
        if (count1 == 0 && count2 == 4) 
        {
            showField();
            StartCoroutine(SequenzFour(1, 3, 6, 4, 9));
        }
        if (count1 == 1 && count2 == 4) StartCoroutine(SequenzFour(5, 3, 7, 2, 8));
        if (count1 == 2 && count2 == 4) StartCoroutine(SequenzFour(2, 1, 9, 5, 6));
        if (count1 == 3 && count2 == 4) StartCoroutine(SequenzFour(4, 5, 6, 7 ,8));
        if (count1 == 4 && count2 == 4) StartCoroutine(SequenzFour(1, 2, 7, 5 ,3));
        if (count1 == 5 && count2 == 4) StartCoroutine(SequenzFour(1, 2 ,4 ,7, 5));
        if (count1 == 6 && count2 == 4) increaseWarning();

        //Trial5
        if(count1 == 0 && count2 == 5)
        {
            showField();
            StartCoroutine(SequenzFive(1, 4, 5, 2, 9, 3));
        }
        if (count1 == 1 && count2 == 5) StartCoroutine(SequenzFive(2, 4, 8, 7, 9, 3));
        if (count1 == 2 && count2 == 5) StartCoroutine(SequenzFive(1, 5, 6, 2, 3, 8));
        if (count1 == 3 && count2 == 5) StartCoroutine(SequenzFive(3, 6, 9, 8, 1, 7));
        if (count1 == 4 && count2 == 5) StartCoroutine(SequenzFive(8, 7, 1, 4, 9, 3));
        if (count1 == 5 && count2 == 5) StartCoroutine(SequenzFive(2, 7, 8, 4, 3, 1));
       // if (count1 == 6 && count2 == 5) StartCoroutine(SequenzFive(1, 4, 5, 2, 9, 3));

        count1++;

    }

    void SpawnFairyInBlock(int blockNumber)
    {
        switch (blockNumber)
        {
            case 1:
                fairy.transform.position = new Vector3(blocks[0].transform.position.x, blocks[0].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[0].transform.gameObject);
                break;
            case 2:
                fairy.transform.position = new Vector3(blocks[1].transform.position.x, blocks[1].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[1].transform.gameObject);
                break;
            case 3:
                fairy.transform.position = new Vector3(blocks[2].transform.position.x, blocks[2].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[2].transform.gameObject);
                break;
            case 4:
                fairy.transform.position = new Vector3(blocks[3].transform.position.x, blocks[3].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[3].transform.gameObject);
                break;
            case 5:
                fairy.transform.position = new Vector3(blocks[4].transform.position.x, blocks[4].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[4].transform.gameObject);
                break;
            case 6:
                fairy.transform.position = new Vector3(blocks[5].transform.position.x, blocks[5].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[5].transform.gameObject);
                break;
            case 7:
                fairy.transform.position = new Vector3(blocks[6].transform.position.x, blocks[6].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[6].transform.gameObject);
                break;
            case 8:
                fairy.transform.position = new Vector3(blocks[7].transform.position.x, blocks[7].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[7].transform.gameObject);
                break;
            case 9:
                fairy.transform.position = new Vector3(blocks[8].transform.position.x, blocks[8].transform.position.y, -1);
                player.sequenzBlocks.Add(blocks[8].transform.gameObject);
                break;
        }
    }

    /*IEnumerator um die Fee fliegen zu lassen
     * es wird acht mal 
     * SequenzOne, SequenzTwo, SequenzThree in dieser Reihenfolge oben verwendet
     * 
     * a,b,c,d sind die Bloecke in denen die Fee Spawnt
     * 
     */
    IEnumerator SequenzZero(int a)
    {
        disableField();

        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks++;
        enableField();
        Player.timer.Start();
    }
    IEnumerator SequenzOne(int a, int b)
    {
        disableField();
        
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(b);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks+=2;
        enableField();
        Player.timer.Start();
    }

    IEnumerator SequenzTwo(int a, int b, int c)
    {
        disableField();
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(b);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(c);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks += 3;
        Player.timer.Start();
        enableField();
    }

    IEnumerator SequenzThree(int a, int b, int c , int d)
    {
        disableField();
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(b);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(c);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(d);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks += 4;
        enableField();
        Player.timer.Start();
    }

    IEnumerator SequenzFour(int a, int b, int c, int d, int e)
    {
        disableField();
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(b);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(c);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(d);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(e);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks += 5;
        enableField();
        Player.timer.Start();
    }
    IEnumerator SequenzFive(int a, int b, int c, int d, int e, int f)
    {
        disableField();
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(a);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(b);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(c);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(d);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(e);
        yield return new WaitForSeconds(speed);
        SpawnFairyInBlock(f);
        yield return new WaitForSeconds(speed);
        fairy.SetActive(false);
        totlalAccuracyClicks += 6;
        enableField();
        Player.timer.Start();
    }



    /*
     * enableField und disableField sind funktionen um das gesamte Spielfeld zu Blockieren,
     * sodass waehrend der Animation keine Input auf dem Bildschirm verfuegbar ist
     * 
     * werden in der ueberliegenden Funktionen verwendet (vor einer Sequenz und wenn Sie 
     * durchgelaufen ist) 
     */
    void enableField()
    {
        for(int i = 0; i< blocks.Count;i++)
        {
            blocks[i].GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);   // XXX
            blocks[i].GetComponent<Collider2D>().enabled = true;
        }

        
    }
    public void disableField()   // ggbfs. hier könnte man alle Felder eingräuen
    {        

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].GetComponent<SpriteRenderer>().color = Color.grey;   // XXX
            blocks[i].GetComponent<Collider2D>().enabled = false;
        }

        totalTasks++;
        
    }

    void increaseWarning()
    {
        if(countFalseTask >= 3)
        {
            SkipToFinish();
        }
        else
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].gameObject.SetActive(false);
            }
            increaseText.gameObject.SetActive(true);
            increaseButton.gameObject.SetActive(true);
        }

        
    }
    public void showField()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].gameObject.SetActive(true);
        }
        increaseText.gameObject.SetActive(false);
        increaseButton.gameObject.SetActive(false);
    }
    
    public  void SkipToFinish()
    {
        DataSaver.rightTask = player.rightTaskCounter.ToString();
        DataSaver.accuracy = player.accuracyCounter.ToString();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

/*
 * 
 * Diese drei funktionen koennen verwendet werden wenn man die Fee 
 * randomiesiert fliegen lassen will ohne das ein Block 
 * mehrmals innerhalb einer Sequenz getroffen wird
 * 
 * 
 * 
void GetRandomBlock()
{

    randomBlock = Random.Range(1, 10);

    if (CheckRandomNumber(randomBlock))
    {
        SpawnFairyInBlock();
    }
    else
    {
        GetRandomBlock();
    }
    randomNumbers.Add(randomBlock);
}

void SpawnFairyInBlock()
{
    switch (randomBlock)
    {
        case 1:
            fairy.transform.position = new Vector3(blocks[0].transform.position.x, blocks[0].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[0].transform.gameObject);
            break;
        case 2:
            fairy.transform.position = new Vector3(blocks[1].transform.position.x, blocks[1].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[1].transform.gameObject);
            break;
        case 3:
            fairy.transform.position = new Vector3(blocks[2].transform.position.x, blocks[2].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[2].transform.gameObject);
            break;
        case 4:
            fairy.transform.position = new Vector3(blocks[3].transform.position.x, blocks[3].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[3].transform.gameObject);
            break;
        case 5:
            fairy.transform.position = new Vector3(blocks[4].transform.position.x, blocks[4].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[4].transform.gameObject);
            break;
        case 6:
            fairy.transform.position = new Vector3(blocks[5].transform.position.x, blocks[5].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[5].transform.gameObject);
            break;
        case 7:
            fairy.transform.position = new Vector3(blocks[6].transform.position.x, blocks[6].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[6].transform.gameObject);
            break;
        case 8:
            fairy.transform.position = new Vector3(blocks[7].transform.position.x, blocks[7].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[7].transform.gameObject);
            break;
        case 9:
            fairy.transform.position = new Vector3(blocks[8].transform.position.x, blocks[8].transform.position.y, -1);
            player.sequenzBlocks.Add(blocks[8].transform.gameObject);
            break;
    }
}

bool CheckRandomNumber(int i)
{
    foreach (int x in randomNumbers)
    {
        if (x == i)
        {
            return false;
        }
    }
    return true;
}
*/


/*funktiion welche die nummer des blocks nimmt und dann der fee ueber eine switch case die koordinaten zuweist
 * die z koordinate wurde auf -1 gesetzt damit die fee vor dem block spawnt (verschiedenes layer)
 */

