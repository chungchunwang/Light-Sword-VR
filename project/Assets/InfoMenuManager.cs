using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoMenuManager : MonoBehaviour
{
    public void onReturnToGameClicked()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
