using System;
using System.IO;
using Lab1_2026.Models;

namespace Lab1_2026.Services
{
    public class StorageService
    {
        private readonly string filePath = Path.Combine("data", "results.txt");

        public void SaveResult(User user, int totalScore, string interpretation)
        {
            Directory.CreateDirectory("data");

            string record = $"Дата тестування: {DateTime.Now}\n" +
                            $"Прізвище та Ім'я: {user.LastName} {user.FirstName}\n" +
                            $"Професія: {user.Profession} | Стать: {user.Gender}\n" +
                            $"Варіант тесту: {user.TestNumber} (Шуберт)\n" +
                            $"Набрані бали: {totalScore}\n" +
                            $"Інтерпретація: {interpretation}\n" +
                            "--------------------------------------------------\n";

            File.AppendAllText(filePath, record);
        }
    }
}