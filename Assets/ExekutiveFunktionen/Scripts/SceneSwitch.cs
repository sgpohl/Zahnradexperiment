using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{

    /*
     * Easy Scene Switcher index = 0 is the intro, 1 = the play scene, 2 = Outroscene
     * 
     */

    private string inputVPN = "";
    public static bool reverse = false;
    public void StartGame()
    {
        DataSaver.z0.Clear();
        DataSaver.z1.Clear();
        DataSaver.z2.Clear();
        DataSaver.z3.Clear();
        DataSaver.z4.Clear();
        DataSaver.z5.Clear();
        DataSaver.z6.Clear();
        DataSaver.results.Clear();
        Randomizer.countFalseTask = 0;
        Randomizer.totalTasks = 0;
        DataSaver.VPN = inputVPN;
        Randomizer.reverse = reverse;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 123);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void BackStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 126);
    }
    public void GoNoGoBackStart()
    {
        //GoNoGo.counter = 0;
        //GoNoGo.trial = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 131);
    }

    public void ReadInput(string s)
    {
        inputVPN = s;
    }

    public void SetReverse()
    {
        reverse = true;
    }

    public void StartGoNoGO()
    {
        
        DataGoNoGO.overall.Clear();
        DataGoNoGO.results.Clear();
        DataGoNoGO.header.Clear();
        DataGoNoGO.z1.Clear();
        DataGoNoGO.VPN = inputVPN;
        
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 127);
    }
}
