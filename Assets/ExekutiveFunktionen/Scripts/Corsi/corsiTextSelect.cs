using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class corsiTextSelect : MonoBehaviour
{
    public TextMesh corsi;
    public TextMesh corsiReverse;
    // Start is called before the first frame update
    void Start()
    {
        if (Randomizer.reverse)
        {
            corsiReverse.gameObject.SetActive(true);
            corsi.gameObject.SetActive(false);
        }
        else
        {
            corsiReverse.gameObject.SetActive(false);
            corsi.gameObject.SetActive(true);
        }
    }
}
