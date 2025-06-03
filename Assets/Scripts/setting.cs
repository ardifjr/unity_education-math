using UnityEngine;

public class OpenSettingPanel : MonoBehaviour
{
    public GameObject panelSetting; // drag "setting" ke sini lewat Inspector

    public void OpenPanel()
    {
        if (panelSetting != null)
        {
            panelSetting.SetActive(true); // tampilkan panel setting
        }
    }
    public void ClosePanel()
    {
        if (panelSetting != null)
        {
            panelSetting.SetActive(false); // sembunyikan panel setting
        }
    }
}
