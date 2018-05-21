using UnityEngine.UI;
using UnityEngine;
namespace Mkey
{
    public class SettingsMenuController : PopUpsController
    {
        public Button musicButton;
        public Sprite musicOnSprite;
        public Sprite musicOffSprite;
        public Image[] volume;

        public void SoundPlusButton_Click()
        {
            SoundMasterController.Instance.Volume+=0.1f;
            SetSoundButtVolume(SoundMasterController.Instance.Volume);
        }

        public void SoundMinusButton_Click()
        {
            SoundMasterController.Instance.Volume -= 0.1f;
            SetSoundButtVolume(SoundMasterController.Instance.Volume);
        }

        public void MusikButton_Click()
        {
            SoundMasterController.Instance.MusicOn = !SoundMasterController.Instance.MusicOn;
            SetMusicButtSprite(SoundMasterController.Instance.MusicOn);
        }
    
        public void FacebookButton_Click()
        {

        }

        private void SetSoundButtVolume(float soundVolume)
        {
           if(volume!=null && volume.Length > 0)
            {
                int length = volume.Length;
                float vpl = 1.0f / (float)length;
                int count =Mathf.RoundToInt(soundVolume / vpl);
                Debug.Log("soundVol: " + soundVolume + " ; count: " + count + " ;s/vpl: " + soundVolume / vpl);
                SetVolume(count);
            }
        }

        private void SetVolume(int count)
        {
            for (int i = 0; i < volume.Length; i++)
            {
                 volume[i].gameObject.SetActive(i < count);
            }
        }

        private void SetMusicButtSprite(bool musicOn)
        {
                musicButton.image.sprite = (musicOn)? musicOnSprite : musicOffSprite;
        }

        public override void RefreshWindow()
        {
            SetSoundButtVolume(SoundMasterController.Instance.Volume);
            SetMusicButtSprite(SoundMasterController.Instance.MusicOn);
            base.RefreshWindow();
        }
    }
}