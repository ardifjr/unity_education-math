using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CarSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button selectButton;
    public Button startButton;
    
    [Header("Car Container")]
    public GameObject carContainer; // Objek "Car" yang berisi semua mobil
    
    private GameObject[] cars; // Array untuk menyimpan semua mobil
    private int currentCarIndex = 0; // Index mobil yang sedang aktif
    private int selectedCarIndex = -1; // Index mobil yang sudah dipilih (-1 = belum ada yang dipilih)
    
    void Start()
    {
        InitializeCars();
        SetupButtons();
        UpdateCarDisplay();
        UpdateSelectButtonVisibility(); // Update tombol select saat start
    }
    
    void InitializeCars()
    {
        // Ambil semua child object dari Car container
        cars = new GameObject[carContainer.transform.childCount];
        for (int i = 0; i < carContainer.transform.childCount; i++)
        {
            cars[i] = carContainer.transform.GetChild(i).gameObject;
        }
        
        // Cari mobil mana yang sedang aktif sebagai starting point
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].activeInHierarchy)
            {
                currentCarIndex = i;
                break;
            }
        }
    }
    
    void SetupButtons()
    {
        // Setup button listeners
        leftArrowButton.onClick.AddListener(PreviousCar);
        rightArrowButton.onClick.AddListener(NextCar);
        selectButton.onClick.AddListener(SelectCar);
        startButton.onClick.AddListener(StartGame);
    }
    
    void PreviousCar()
    {
        // Tombol panah tetap bisa diklik setelah select
        currentCarIndex--;
        if (currentCarIndex < 0)
        {
            currentCarIndex = cars.Length - 1; // Loop ke mobil terakhir
        }
        
        UpdateCarDisplay();
        UpdateSelectButtonVisibility(); // Update tombol select setiap ganti mobil
    }
    
    void NextCar()
    {
        // Tombol panah tetap bisa diklik setelah select
        currentCarIndex++;
        if (currentCarIndex >= cars.Length)
        {
            currentCarIndex = 0; // Loop ke mobil pertama
        }
        
        UpdateCarDisplay();
        UpdateSelectButtonVisibility(); // Update tombol select setiap ganti mobil
    }
    
    void UpdateCarDisplay()
    {
        // Matikan semua mobil
        for (int i = 0; i < cars.Length; i++)
        {
            cars[i].SetActive(false);
        }
        
        // Aktifkan mobil yang sedang dipilih
        cars[currentCarIndex].SetActive(true);
    }
    
    void UpdateSelectButtonVisibility()
    {
        // Tombol Select hanya muncul jika mobil yang ditampilkan BUKAN mobil yang dipilih
        if (currentCarIndex == selectedCarIndex)
        {
            selectButton.gameObject.SetActive(false); // Sembunyikan jika ini mobil yang dipilih
        }
        else
        {
            selectButton.gameObject.SetActive(true); // Tampilkan jika ini bukan mobil yang dipilih
        }
    }
    
    void SelectCar()
    {
        selectedCarIndex = currentCarIndex; // Simpan index mobil yang dipilih
        
        // Simpan pilihan mobil ke PlayerPrefs
        PlayerPrefs.SetInt("SelectedCarIndex", selectedCarIndex);
        PlayerPrefs.Save();
        
        // Update visibility tombol select
        UpdateSelectButtonVisibility();
        
        Debug.Log("Mobil dipilih: " + cars[selectedCarIndex].name);
    }
    
    void StartGame()
    {
        // Pastikan ada mobil yang sudah dipilih
        if (selectedCarIndex == -1)
        {
            Debug.Log("Pilih mobil terlebih dahulu!");
            return;
        }
        
        // Load scene SampleScene
        SceneManager.LoadScene("SampleScene");
    }
}