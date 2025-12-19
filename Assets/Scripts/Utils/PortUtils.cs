using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class PortUtils
{
    public static async UniTask FreePortAsync(int port)
    {
        try
        {
            await UniTask.RunOnThreadPool(() =>
            {
                // Запуск netstat
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano -p tcp",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Ищем строку с нужным портом
                var regex = new Regex($@"\s*TCP\s+\S+:{port}\s+\S+\s+\S+\s+(\d+)");
                var match = regex.Match(output);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int pid))
                {
                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        UnityEngine.Debug.Log($"Порт {port} занят процессом {proc.ProcessName} (PID {pid}). Завершаем...");
                        proc.Kill();
                        proc.WaitForExit();
                        UnityEngine.Debug.Log("Процесс успешно завершен.");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.Log($"Не удалось завершить процесс на порту {port}: {ex.Message}");
                    }
                }
                else
                {
                    UnityEngine.Debug.Log($"Порт {port} свободен.");
                }
            });
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log($"Ошибка при проверке порта: {ex.Message}");
        }
    }
}
