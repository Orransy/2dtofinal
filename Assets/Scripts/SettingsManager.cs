using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("Music")]
    public Toggle musicToggle;

    [Header("Sound Effects")]
    public Toggle soundToggle;

    private void Start()
    {
        // Đọc trạng thái đã lưu
        bool musicOn =
            PlayerPrefs.GetInt("MusicOn", 1) == 1;

        bool soundOn =
            PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // Cập nhật Toggle nhưng không gọi OnValueChanged
        musicToggle.SetIsOnWithoutNotify(musicOn);
        soundToggle.SetIsOnWithoutNotify(soundOn);

        // Áp dụng trạng thái âm thanh
        ApplyMusic(musicOn);
        ApplySound(soundOn);
    }

    public void OnMusicChanged(bool isOn)
    {
        Debug.Log("Music Toggle = " + isOn);

        PlayerPrefs.SetInt(
            "MusicOn",
            isOn ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplyMusic(isOn);
    }

    public void OnSoundChanged(bool isOn)
    {
        Debug.Log("Sound Toggle = " + isOn);

        PlayerPrefs.SetInt(
            "SoundOn",
            isOn ? 1 : 0
        );

        PlayerPrefs.Save();

        ApplySound(isOn);
    }

    private void ApplyMusic(bool isOn)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy AudioManager!");
            return;
        }

        AudioManager.Instance.SetMusic(isOn);
    }

    private void ApplySound(bool isOn)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy AudioManager!");
            return;
        }

        if (AudioManager.Instance.sfxSource == null)
        {
            Debug.LogError("SFX Source chưa được gán!");
            return;
        }

        // Bật / tắt toàn bộ Sound Effects
        AudioManager.Instance.sfxSource.mute = !isOn;
    }

    public void GoBack()
    {
        Time.timeScale = 1f;

        SceneManager.UnloadSceneAsync("SettingsScene");
    }

    public void ResetProgress()
    {
        // Reset level đã hoàn thành
        int maxLevel = 5;

        for (int i = 1; i <= maxLevel; i++)
        {
            PlayerPrefs.DeleteKey("LevelCompleted_" + i);
        }

        // Reset level đang chọn
        PlayerPrefs.DeleteKey("SelectedLevel");

        PlayerPrefs.Save();

        Debug.Log("Progress đã được reset!");

        Time.timeScale = 1f;
        SceneManager.LoadScene("HomeScene");
    }
}