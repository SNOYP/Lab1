using System;
using System.IO;
using Lab1_2026.Models;

namespace Lab1_2026.Services
{
    public class StorageService
    {
        public void SaveResult(User user, int totalScore, string interpretation)
        {
            string directory = "data";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filePath = Path.Combine(directory, "results.txt");

            // Красивый многострочный формат с Шубертом
            string dataLine = "--------------------------------------------------\n" +
                              $"Дата тестування: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n" +
                              $"Прізвище та Ім'я: {user.LastName} {user.FirstName}\n" +
                              $"Професія: {user.Profession} | Стать: {user.Gender}\n" +
                              $"Варіант тесту: 11 (Шуберт)\n" +
                              $"Набрані бали: {totalScore}\n" +
                              $"Інтерпретація: {interpretation}\n" +
                              "--------------------------------------------------\n";

            File.AppendAllText(filePath, dataLine);
        }
    }
}