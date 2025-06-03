using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadPlayGameScene()
    {
        Debug.Log("Button Clicked");
        SceneManager.LoadScene("SampleScene");
    }
}