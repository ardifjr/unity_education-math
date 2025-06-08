using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI countdownText;
    public GameObject countdownPanel;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip countdownSound; // Sound untuk 3, 2, 1
    public AudioClip startSound;     // Sound untuk "MULAI!!"
    public AudioSource musicBacksound; // Background music yang akan di-off saat countdown
    
    [Header("Game References")]
    public GameObject carParent;              // Parent object yang berisi semua jenis mobil
    public Animator roadAnimator;             // Animator untuk jalan
    public MathQuizManager mathQuizManager;   // Reference ke MathQuizManager
    
    [Header("Countdown Settings")]
    public float countdownDuration = 1f; // Durasi setiap angka
    [Header("Debug Settings")]
    public bool autoStartCountdown = true; // Auto start countdown untuk testing
    public float autoStartDelay = 2f; // Delay sebelum auto start
    
    private bool isCountdownActive = false;
    
    void Start()
    {
        Debug.Log("=== COUNTDOWN MANAGER START ===");
        DebugReferences();
        
        // Setup initial state
        SetupInitialState();
        
        // Auto start countdown jika diaktifkan
        if (autoStartCountdown)
        {
            Debug.Log($"Auto starting countdown in {autoStartDelay} seconds...");
            StartCoroutine(AutoStartCountdownCoroutine());
        }
    }
    private Car GetActiveCar()
    {
        if (carParent == null) return null;
        
        // Cari semua Car component di children carParent
        Car[] allCars = carParent.GetComponentsInChildren<Car>();
        
        // Cari yang aktif dan enabled
        foreach (Car car in allCars)
        {
            if (car.gameObject.activeInHierarchy && car.enabled)
            {
                return car;
            }
        }
        
        // Jika tidak ada yang enabled, cari yang gameObjectnya aktif
        foreach (Car car in allCars)
        {
            if (car.gameObject.activeInHierarchy)
            {
                return car;
            }
        }
        
        Debug.LogWarning("No active car found in carParent!");
        return null;
    }
    void DebugReferences()
    {
        Debug.Log($"Countdown Text: {(countdownText != null ? "OK" : "MISSING")}");
        Debug.Log($"Countdown Panel: {(countdownPanel != null ? "OK" : "MISSING")}");
        Debug.Log($"Audio Source: {(audioSource != null ? "OK" : "MISSING")}");
        
        // Debug active car instead of taxi
        Car activeCar = GetActiveCar();
        Debug.Log($"Active Car: {(activeCar != null ? $"OK ({activeCar.gameObject.name})" : "MISSING")}");
        
        Debug.Log($"Car Parent: {(carParent != null ? "OK" : "MISSING")}");
        Debug.Log($"Road Animator: {(roadAnimator != null ? "OK" : "MISSING")}");
        Debug.Log($"Math Quiz Manager: {(mathQuizManager != null ? "OK" : "MISSING")}");
        Debug.Log($"Music Backsound: {(musicBacksound != null ? "OK" : "MISSING")}");
    }
    
    IEnumerator AutoStartCountdownCoroutine()
    {
        yield return new WaitForSeconds(autoStartDelay);
        Debug.Log("Auto starting countdown now!");
        StartCountdown();
    }
    
    void SetupInitialState()
    {
        Debug.Log("=== SETUP INITIAL STATE ===");
        
        // Nonaktifkan pergerakan semua mobil
        Car activeCar = GetActiveCar();
        if (activeCar != null)
        {
            activeCar.enabled = false;
            Debug.Log($"Car script disabled on: {activeCar.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No active car found to disable!");
        }
        
        // Aktifkan carParent tapi nonaktifkan kontrol (jangan di-SetActive(false))
        if (carParent != null)
        {
            carParent.SetActive(true); // Tetap aktif agar terlihat
            Debug.Log("Car Parent kept active");
        }
        else
        {
            Debug.LogWarning("Car Parent reference is missing!");
        }
        
        // Pause animasi jalan
        if (roadAnimator != null)
        {
            roadAnimator.speed = 0f;
            Debug.Log("Road animator paused");
        }
        else
        {
            Debug.LogWarning("Road Animator reference is missing!");
        }
        
        // Pause MathQuizManager jika ada
        if (mathQuizManager != null)
        {
            mathQuizManager.enabled = false;
            Debug.Log("MathQuizManager disabled");
        }
        else
        {
            Debug.LogWarning("MathQuizManager reference is missing!");
        }
        
        // Setup background music
        if (musicBacksound != null)
        {
            if (musicBacksound.isPlaying)
            {
                musicBacksound.Stop();
                Debug.Log("Background music stopped");
            }
        }
        else
        {
            Debug.LogWarning("Music Backsound reference is missing!");
        }
        
        // Setup countdown panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
            Debug.Log("Countdown panel hidden");
        }
        else
        {
            Debug.LogWarning("Countdown Panel reference is missing!");
        }
    }
    
    public void StartCountdown()
    {
        Debug.Log("=== START COUNTDOWN CALLED ===");
        if (!isCountdownActive)
        {
            Debug.Log("Starting countdown coroutine...");
            StartCoroutine(CountdownCoroutine());
        }
        else
        {
            Debug.Log("Countdown already active!");
        }
    }
    
    IEnumerator CountdownCoroutine()
    {
        isCountdownActive = true;
        Debug.Log("=== COUNTDOWN COROUTINE STARTED ===");
        
        // Tampilkan countdown panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
            Debug.Log("Countdown panel shown");
        }
        else
        {
            Debug.LogError("Cannot show countdown panel - reference is missing!");
        }
        
        // Countdown 3
        Debug.Log("Showing countdown: 3");
        yield return ShowCountdownNumber("3");
        
        // Countdown 2
        Debug.Log("Showing countdown: 2");
        yield return ShowCountdownNumber("2");
        
        // Countdown 1
        Debug.Log("Showing countdown: 1");
        yield return ShowCountdownNumber("1");
        
        // MULAI!!
        Debug.Log("Showing: MULAI!!");
        yield return ShowStartMessage();
        
        // Mulai permainan
        Debug.Log("Starting game...");
        StartGame();
        
        isCountdownActive = false;
        Debug.Log("=== COUNTDOWN COROUTINE FINISHED ===");
    }
    
    IEnumerator ShowCountdownNumber(string number)
    {
        Debug.Log($"Displaying number: {number}");
        
        // Tampilkan angka
        if (countdownText != null)
        {
            countdownText.text = number;
            countdownText.fontSize = 120;
            countdownText.color = Color.white;
            Debug.Log($"Text set to: {number}");
        }
        else
        {
            Debug.LogError("CountdownText is missing!");
        }
        
        // Putar sound countdown
        if (audioSource != null && countdownSound != null)
        {
            audioSource.PlayOneShot(countdownSound);
            Debug.Log("Countdown sound played");
        }
        else
        {
            Debug.LogWarning("Audio source or countdown sound missing");
        }
        
        // Animasi scale (opsional)
        if (countdownText != null)
        {
            StartCoroutine(AnimateCountdownText());
        }
        
        // Tunggu sesuai durasi
        Debug.Log($"Waiting for {countdownDuration} seconds...");
        yield return new WaitForSeconds(countdownDuration);
    }
    
    IEnumerator ShowStartMessage()
    {
        Debug.Log("Displaying: MULAI!!");
        
        // Tampilkan "MULAI!!"
        if (countdownText != null)
        {
            countdownText.text = "MULAI!!";
            countdownText.fontSize = 100;
            countdownText.color = Color.green;
            Debug.Log("Text set to: MULAI!!");
        }
        
        // Putar sound start
        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
            Debug.Log("Start sound played");
        }
        
        // Animasi scale untuk "MULAI!!"
        if (countdownText != null)
        {
            StartCoroutine(AnimateStartText());
        }
        
        // Tunggu sebentar
        yield return new WaitForSeconds(countdownDuration);
    }
    
    IEnumerator AnimateCountdownText()
    {
        if (countdownText == null) yield break;
        
        Vector3 originalScale = countdownText.transform.localScale;
        
        // Scale up
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Bounce effect
            float scale = 1f + (Mathf.Sin(progress * Mathf.PI) * 0.5f);
            countdownText.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        countdownText.transform.localScale = originalScale;
    }
    
    IEnumerator AnimateStartText()
    {
        if (countdownText == null) yield break;
        
        Vector3 originalScale = countdownText.transform.localScale;
        
        // Pulse effect untuk "MULAI!!"
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            float scale = 1f + (Mathf.Sin(progress * Mathf.PI * 2) * 0.3f);
            countdownText.transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        countdownText.transform.localScale = originalScale;
    }
    
    void StartGame()
    {
        Debug.Log("=== START GAME ===");
        
        // Sembunyikan countdown panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
            Debug.Log("Countdown panel hidden");
        }
        
        // Aktifkan kontrol mobil yang sedang aktif
        Car activeCar = GetActiveCar();
        if (activeCar != null)
        {
            activeCar.enabled = true;
            Debug.Log($"Car script enabled on: {activeCar.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No active car found to enable!");
        }
        
        // PERBAIKAN: Mulai animasi jalan dengan speed yang benar
        if (roadAnimator != null)
        {
            // Set speed terlebih dahulu
            if (mathQuizManager != null)
            {
                roadAnimator.SetFloat(mathQuizManager.speedParameterName, mathQuizManager.baseSpeed);
                Debug.Log($"Road animator SetFloat called with: {mathQuizManager.speedParameterName} = {mathQuizManager.baseSpeed}");
            }
            else
            {
                // Fallback jika mathQuizManager null
                roadAnimator.SetFloat("Speed", 0.3f);
                Debug.Log("Road animator SetFloat called with: Speed = 0.3f (fallback)");
            }
            
            // TAMBAHAN: Set speed property juga untuk memastikan
            roadAnimator.speed = 1f;
            Debug.Log($"Road animator speed property set to: 1f");
            
            // TAMBAHAN: Force update animator
            roadAnimator.Update(0f);
            Debug.Log("Road animator force updated");
        }
        
        // Nyalakan background music saat game dimulai
        if (musicBacksound != null && !musicBacksound.isPlaying)
        {
            musicBacksound.Play();
            Debug.Log("Background music started");
        }
        
        // Aktifkan MathQuizManager
        if (mathQuizManager != null)
        {
            mathQuizManager.enabled = true;
            // Panggil StartGame() method di MathQuizManager
            mathQuizManager.StartGame();
            Debug.Log("MathQuizManager enabled and started");
        }
        
        Debug.Log("=== GAME STARTED SUCCESSFULLY ===");
    }
    
    // Method untuk dipanggil dari luar (misalnya dari button)
    public void InitiateGameStart()
    {
        Debug.Log("InitiateGameStart called from external source");
        StartCountdown();
    }
    
    // Method untuk testing di Inspector
    [ContextMenu("Test Start Countdown")]
    public void TestStartCountdown()
    {
        StartCountdown();
    }
    
    [ContextMenu("Debug References")]
    public void TestDebugReferences()
    {
        DebugReferences();
    }
}