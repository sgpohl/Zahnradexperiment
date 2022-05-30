using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockClick : MonoBehaviour
{

    /*BlockClick Script for the ClickAnimation (mainly)
     * 
     * also adding the clicked Block into the clickedBlock List 
     * its needed for the compareLists function in the Player Script
     */

    public Player player;

    

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    public void OnMouseDown()
    {
        
        player.increaseClick();
        if ( gameObject.CompareTag("Block"))
        {
            player.clickedBlocks.Add(gameObject);
            StartCoroutine(ClickTimeAnimation());
        }       
    }


    //Block der geklickt wird, ist fuer 0.2 sekunden grau und wird anschliessend wieder weiss
    IEnumerator ClickTimeAnimation()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        yield return new WaitForSeconds(.2f);
        gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        Randomizer.clickedBlocks++;
    }
}
