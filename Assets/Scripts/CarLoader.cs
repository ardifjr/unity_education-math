using UnityEngine;

public class CarLoader : MonoBehaviour
{
    [Header("Car Container")]
    public GameObject carContainer; // Objek "Car" di SampleScene
    
    private GameObject[] cars; // Array untuk menyimpan semua mobil
    
    void Start()
    {
        LoadSelectedCar();
    }
    
    void LoadSelectedCar()
    {
        // Ambil semua child object dari Car container
        cars = new GameObject[carContainer.transform.childCount];
        for (int i = 0; i < carContainer.transform.childCount; i++)
        {
            cars[i] = carContainer.transform.GetChild(i).gameObject;
        }
        
        // Ambil index mobil yang dipilih dari PlayerPrefs
        int selectedCarIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        
        // Pastikan index valid
        if (selectedCarIndex >= cars.Length)
        {
            selectedCarIndex = 0;
        }
        
        // Matikan semua mobil
        for (int i = 0; i < cars.Length; i++)
        {
            cars[i].SetActive(false);
        }
        
        // Aktifkan mobil yang dipilih
        cars[selectedCarIndex].SetActive(true);
        
        Debug.Log("Mobil dimuat: " + cars[selectedCarIndex].name);
    }
}