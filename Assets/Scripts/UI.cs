using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class UI : MonoBehaviour
{
    public void Enter_Game_Scene()
    {
        SceneManager.LoadScene("Game");
    }
    public void Info_Key()
    {
        SceneManager.LoadScene("Info Key");
    }
}
