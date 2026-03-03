using UnityEngine;

public class SoundManager : MonoBehaviour
{
  public static SoundManager Instance { get; private set; }

  [Header("Sound Effects")]
  [SerializeField] private AudioClip bottleUp;
  [SerializeField] private AudioClip bottleDown;
  [SerializeField] private AudioClip bottleFull;
  [SerializeField] private AudioClip bottleClose;
  [SerializeField] private AudioClip pouringWater;

  [Header("Settings")]
  [Range(0f, 1f)] [SerializeField] private float sfxVolume    = 1f;
  [Range(0f, 1f)] [SerializeField] private float pouringVolume = 0.7f;

  private AudioSource sfxSource;
  private AudioSource loopSource;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
    DontDestroyOnLoad(gameObject);

    sfxSource  = gameObject.AddComponent<AudioSource>();
    loopSource = gameObject.AddComponent<AudioSource>();

    sfxSource.playOnAwake = false;
    loopSource.playOnAwake = false;
    loopSource.loop = true;
  }

  public void PlayBottleUp()    => PlaySFX(bottleUp);
  public void PlayBottleDown()  => PlaySFX(bottleDown);
  public void PlayBottleFull()  => PlaySFX(bottleFull);
  public void PlayBottleClose() => PlaySFX(bottleClose);

  public void StartPouring()
  {
    if (pouringWater == null || loopSource.isPlaying) return;
    loopSource.clip   = pouringWater;
    loopSource.volume = pouringVolume;
    loopSource.Play();
  }

  public void StopPouring()
  {
    if (loopSource.isPlaying)
      loopSource.Stop();
  }

  private void PlaySFX(AudioClip clip)
  {
    if (clip == null) return;
    sfxSource.PlayOneShot(clip, sfxVolume);
  }
}
