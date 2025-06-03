using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Tambahkan ini kalau pakai TextMeshPro

public class VolumeManager : MonoBehaviour
{
    public AudioSource Backgroundmusic;
    public TextMeshProUGUI volumeText; // Ganti ke TextMeshProUGUI

    public void KetikaSliderDiubah(float nilaiSlider)
    {
        Backgroundmusic.volume = nilaiSlider;
        volumeText.text = nilaiSlider.ToString("F2");
    }
}
