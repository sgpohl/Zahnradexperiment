using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeBlock : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (gameObject.CompareTag("Block"))
        {
            StartCoroutine(ClickTimeAnimation());
        }
    }

    IEnumerator ClickTimeAnimation()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.grey;
        yield return new WaitForSeconds(.2f);
        gameObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        CorsiPractice.clickedBlocks++;
    }
}
