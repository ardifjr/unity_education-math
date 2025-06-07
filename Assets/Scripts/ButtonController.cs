using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public void OnMulaiClicked()
    {
        // Ganti "Home" dengan nama scene kamu
        SceneManager.LoadScene("Home");
    }

    public void OnKeluarClicked()
    {
        // Jika di editor, stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Jika di build (Android/Windows/etc), keluar aplikasi
        Application.Quit();
#endif
    }
}
