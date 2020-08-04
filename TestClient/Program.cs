using System;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TestClient
{
    class Program
    {
        static string data = "";  //переменная проверяемой строки
        static string url = "http://localhost:56769/IsPalindromeService.svc";  //url сервера
        [STAThread]
        static void Main(string[] args)
        {
            string command;
            Console.WriteLine($"Welcome! {ShowCommands()}");
            do
            {
                Console.Write("\n>>");
                command = Console.ReadLine();
                Console.WriteLine();

                switch (command.ToLower())
                {
                    case "select":
                        using (OpenFileDialog dialog = new OpenFileDialog())
                        {
                            dialog.Filter = "Текстовые файлы|*.txt";
                            if (dialog.ShowDialog() == DialogResult.OK)
                                data = File.ReadAllText(dialog.FileName);

                            Console.WriteLine($"Selected text: '{data}'");
                        }
                        break;

                    case "check":
                        {
                            _ = CheckPalindrome();
                        }
                        break;

                    case "info":
                        {
                            if (data != "")
                                Console.WriteLine($"Selected text: '{data}'");
                            else
                                Console.WriteLine("The file is not selected or the text is empty!");
                            Console.WriteLine($"Server url: {url}");
                            Console.WriteLine(GetStatus());
                        }
                        break;

                    case "seturl":
                        url = Console.ReadLine();
                        break;

                    case "exit":
                        break;

                    default:
                        Console.WriteLine($"Wrong Command!\n{ShowCommands()}");
                        break;
                }
            }
            while (command != "exit");
        }
        
        //Список команд клиента
        private static string ShowCommands() => "\nCommands:\n" +
                "1)'select' - select a text file with a palindrome;\n" +
                "2)'check' - send text from the selected file for verification\n" +
                "3)'info' - show selected text and info about server\n" +
                "4)'seturl' - change the server url\n" +
                "5)'exit' - close application.";

        //Асинхронная функция отправки POST-запроса на сервер с целью проверки строки на палиндром
        private static async Task CheckPalindrome()
        {
            HttpClient client = new HttpClient();
            //удаление знаков препинания из строки и ее форматирование в JSON
            var content = new StringContent($"{{\"text\":\"{Regex.Replace(data.ToLower(), @"[.,:;?'!-()""\s\n]", "")}\"}}", 
                Encoding.UTF8, "application/json");
            var result = await client.PostAsync($"{url}/Check", content);
            var resultBody = await result.Content.ReadAsStringAsync();
            dynamic resultJson = JsonConvert.DeserializeObject(resultBody);
            MessageBox.Show($"'{data}' is palindrome : {resultJson.CheckPalindromeResult}");  
        }

        //Функция получения информации о сервере
        private static string GetStatus()
        {
            string info = "";
            using (var clientGet = new WebClient())
            {
                try 
                {
                    var xdoc = XDocument.Parse(clientGet.DownloadString($"{url}/Info"));
                    info = $"Connection succeed. {xdoc.Root.Value}.";
                }
                catch { info = "There is no connection with server."; };
            }
            return info;
        }
    }
}
