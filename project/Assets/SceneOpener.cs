using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneOpener : MonoBehaviour
{
    [SerializeField] string scene;
    public void open()
    {
        SceneManager.LoadScene(scene);
    }
}
