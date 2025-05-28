using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void LoadPlayGameScene()
    {
        SceneManager.LoadScene("SampleScene"); // atau gunakan index: SceneManager.LoadScene(1);
    }
}