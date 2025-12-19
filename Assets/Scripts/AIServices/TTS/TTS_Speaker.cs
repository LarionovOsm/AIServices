using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class TTSSpeaker : MonoBehaviour
{
   [SerializeField] private AudioSource _audioSource;
   [SerializeField] [TextArea(3, 10)] private string _textForTTS;
   [SerializeField] private string _fileName = "tts_output";

   private void Awake()
   {
      AppManager.instance.AIServicesManager.STTService.OnRecordEnd += CreateAndPlayAudio;
   }
   
   public async void CreateAndPlayAudio(string text)
   {
      var ttsService = AppManager.instance.AIServicesManager.TTSService;

      if (!ttsService.IsReady())
      {
         Debug.LogWarning("TTS Service не готов!");
         return;
      }

      AudioClip clip = null;

      if (text.Length < 3000)
      {
         // Для короткого текста
         clip = await ttsService.GenerateAudioAsync(text);
      }
      
      if (clip != null)
         PlayClipWithCallback(clip);
   }

   [ContextMenu("Save TTS to File")]
   public async void SaveTTSToFile()
   {
      if (string.IsNullOrEmpty(_textForTTS))
      {
         Debug.LogWarning("Текст для TTS пустой!");
         return;
      }
        
      var ttsService = AppManager.instance.AIServicesManager.TTSService;

      if (!ttsService.IsReady())
      {
         Debug.LogWarning("TTS Service не готов!");
         return;
      }

      Debug.Log($"Сохранение аудио для текста длиной: {_textForTTS.Length} символов");
        
      try
      {
         bool success = await ttsService.GenerateAndSaveAudioAsync(_textForTTS, _fileName);
            
         if (success)
         {
            Debug.Log("Аудиофайл успешно сохранен!");
         }
         else
         {
            Debug.LogError("Не удалось сохранить аудиофайл");
         }
      }
      catch (Exception e)
      {
         Debug.LogError($"Ошибка при сохранении аудио: {e.Message}");
      }
   }

   private void PlayClipWithCallback(AudioClip clip)
   {
      _audioSource.clip = clip;
      _audioSource.Play();
      StartCoroutine(WaitUntilFinished());
   }

   private IEnumerator WaitUntilFinished()
   {
      yield return new WaitWhile(() => _audioSource.isPlaying);
      OnAudioFinished();
   }
   
   private void OnAudioFinished()
   {
      AppManager.instance.AIServicesManager.STTService.MicrophoneRecord.StartRecord();
   }
   
   private void OnDestroy()
   {
      AppManager.instance.AIServicesManager.STTService.OnRecordEnd -= CreateAndPlayAudio;
      StopAllCoroutines();
   }
}
