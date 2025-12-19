using UnityEngine;
using System;
using System.IO;

public static class WavUtility
{
    /// <summary>
    /// Преобразует WAV-байты в AudioClip.
    /// Поддерживает PCM 16-bit WAV с любым количеством каналов.
    /// </summary>
    /// <param name="wavFile">Массив байт WAV</param>
    /// <param name="offsetSamples">Пропустить N сэмплов с начала</param>
    /// <param name="name">Имя AudioClip</param>
    /// <returns>AudioClip или null</returns>
    public static AudioClip ToAudioClip(byte[] wavFile, int offsetSamples = 0, string name = "wav")
    {
        if (wavFile == null || wavFile.Length == 0)
            return null;

        using (var ms = new MemoryStream(wavFile))
        using (var reader = new BinaryReader(ms))
        {
            try
            {
                // Проверяем RIFF заголовок
                string riff = new string(reader.ReadChars(4));
                if (riff != "RIFF")
                    throw new Exception("Not a valid WAV file (missing RIFF).");

                reader.ReadInt32(); // размер файла
                string wave = new string(reader.ReadChars(4));
                if (wave != "WAVE")
                    throw new Exception("Not a valid WAV file (missing WAVE).");

                // Находим "fmt " chunk
                string fmt = new string(reader.ReadChars(4));
                while (fmt != "fmt ")
                {
                    int chunkSize = reader.ReadInt32();
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    fmt = new string(reader.ReadChars(4));
                }

                int fmtSize = reader.ReadInt32();
                int audioFormat = reader.ReadInt16(); // 1 = PCM
                int channels = reader.ReadInt16();
                int sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byte rate
                reader.ReadInt16(); // block align
                int bitsPerSample = reader.ReadInt16();
                reader.BaseStream.Seek(fmtSize - 16, SeekOrigin.Current); // пропускаем оставшееся

                // Находим "data" chunk
                string dataChunk = new string(reader.ReadChars(4));
                while (dataChunk != "data")
                {
                    int chunkSize = reader.ReadInt32();
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                    dataChunk = new string(reader.ReadChars(4));
                }

                int dataSize = reader.ReadInt32();
                int sampleCount = dataSize / (bitsPerSample / 8);

                float[] data = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    if (bitsPerSample == 16)
                    {
                        if (reader.BaseStream.Position + 1 >= reader.BaseStream.Length)
                            break;
                        short sample = reader.ReadInt16();
                        data[i] = sample / 32768f;
                    }
                    else
                    {
                        // Если вдруг 8-bit PCM
                        byte sample = reader.ReadByte();
                        data[i] = (sample - 128) / 128f;
                    }
                }

                AudioClip audioClip = AudioClip.Create(name, sampleCount / channels, channels, sampleRate, false);
                audioClip.SetData(data, 0);
                return audioClip;
            }
            catch (Exception ex)
            {
                Debug.LogError($"WavUtility: Ошибка парсинга WAV - {ex.Message}");
                return null;
            }
        }
    }
}
