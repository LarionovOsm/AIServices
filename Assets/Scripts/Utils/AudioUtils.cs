using UnityEngine;
using System;

public static class AudioUtils
{
    /// <summary>
    /// Создаёт AudioClip из WAV или PCM байтов.
    /// </summary>
    /// <param name="audioData">Массив байт аудио (WAV)</param>
    /// <param name="name">Имя AudioClip</param>
    /// <returns>AudioClip</returns>
    public static AudioClip ToAudioClip(byte[] audioData, string name = "TTS_Audio")
    {
        if (audioData == null || audioData.Length == 0)
            return null;

        return WavUtility.ToAudioClip(audioData, 0, name); // используем готовую WavUtility
    }

    /// <summary>
    /// Воспроизводит AudioClip на указанном AudioSource.
    /// </summary>
    /// <param name="source">AudioSource для воспроизведения</param>
    /// <param name="clip">AudioClip</param>
    public static void PlayClip(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
            return;

        source.clip = clip;
        source.Play();
    }

    /// <summary>
    /// Создаёт AudioClip из байтов и сразу воспроизводит через AudioSource.
    /// </summary>
    /// <param name="source">AudioSource</param>
    /// <param name="audioData">Массив байт аудио</param>
    public static void PlayFromBytes(AudioSource source, byte[] audioData)
    {
        var clip = ToAudioClip(audioData);
        PlayClip(source, clip);
    }
}

