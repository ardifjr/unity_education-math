using UnityEngine;

public class CarCollisionSound : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip collisionSound;
    private AudioSource audioSource;
    
    [Header("Collision Control")]
    public float cooldownTime = 1f; // Jeda antar suara (detik)
    private float lastSoundTime = 0f;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            Debug.LogError("❌ TIDAK ADA AUDIO SOURCE!");
        }
        
        if (collisionSound == null)
        {
            Debug.LogError("❌ AUDIO CLIP KOSONG!");
        }
        
        Debug.Log("✅ CarCollisionSound siap!");
    }

    void OnTriggerEnter(Collider other)
    {
        // Cek apakah sudah lewat cooldown time
        if (Time.time - lastSoundTime < cooldownTime)
        {
            Debug.Log("⏰ Masih cooldown, skip suara");
            return;
        }
        
        Debug.Log("🎯 TRIGGER: " + other.name);
        
        // Cek apakah yang ditabrak adalah blok
        if (IsBlock(other.gameObject))
        {
            PlayCollisionSound();
            lastSoundTime = Time.time; // Update waktu terakhir main suara
        }
    }

    // Fungsi untuk cek apakah object adalah blok
    bool IsBlock(GameObject obj)
    {
        string name = obj.name.ToLower();
        return name.Contains("cube") || 
               name.Contains("block") || 
               name.Contains("brick") ||
               name.Contains("box");
    }

    void PlayCollisionSound()
    {
        if (collisionSound != null && audioSource != null)
        {
            Debug.Log("🔊 MAIN SUARA NABRAK!");
            
            // Stop suara sebelumnya jika masih main
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            audioSource.clip = collisionSound;
            audioSource.volume = 1f;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("❌ Audio Source atau Clip tidak ada!");
        }
    }

    // Test dengan keyboard (tekan T)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🎵 TEST SUARA MANUAL!");
            PlayCollisionSound();
        }
    }
}