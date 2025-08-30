using UnityEngine;

public class RandomButtonSound : MonoBehaviour
{
    public AudioSource[] sounds;
    private AudioSource currentSound; // เก็บเสียงที่เล่นอยู่
    private int currentIndex = 0;     // เก็บตำแหน่งเสียงปัจจุบัน

    public void PlaySequentialSound()
    {
        // ถ้ามีเสียงเก่าเล่นอยู่ → หยุดก่อน
        if (currentSound != null && currentSound.isPlaying)
        {
            currentSound.Stop();
        }

        // เล่นเสียงตามลำดับ
        currentSound = sounds[currentIndex];
        currentSound.Play();

        // ไปตำแหน่งถัดไป (ถ้าเกินให้วนกลับไปเริ่มที่ 0)
        currentIndex = (currentIndex + 1) % sounds.Length;
    }
}
