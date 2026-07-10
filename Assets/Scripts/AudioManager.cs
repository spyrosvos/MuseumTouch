using UnityEngine;

public class AudioManager : MonoBehaviour
{
    AudioSource ac;

    public void Start() {
        ac = GetComponent<AudioSource>();
    }
    public void PlayAudio(string name) {
        ac.clip = Resources.Load("sounds/" + name) as AudioClip;
        ac.Play();
    }
}
