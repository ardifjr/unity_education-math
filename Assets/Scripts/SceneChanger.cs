using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadPlayGameScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
    
    public void Back()
    {
        SceneManager.LoadScene("landingPage");
    }
}