using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ContactReminderBot
{
    internal class Program
    {
        static TelegramBotClient bot = new TelegramBotClient("5421750660:AAEfthKF3TMo-9Uw6Ey-LI1NsXdpQ1Qk3SI");
        static List<TelegramGroup> listGroups = new List<TelegramGroup>();
        static bool newGroup = false;
        static bool waitingNumbersForRemind = false; //ожидаем что пользователь введёт числа групп которым будем напоминать
        static bool waitingNumberGroupForTemplate = false; //ожидаем, что пользователь введёт номер группы, шаблон которой он хочет просмотреть/изменить 

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            const long managerChatId = -611718767;//chatID канала через который управляем ботом
            const string fileName = @"groups.json";
            string data;//строка, в которую будет помещаться информация с файла groups.json
            const string numbers = "01234567890,";
          

            TelegramGroup group = new TelegramGroup();
            

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text != null && message.Text.ToLower() == "/start") //Button "Start" was pressed
                {
                    waitingNumberGroupForTemplate = false; //ждём от пользователя номер числа для просмотра шаблона, но он вводит /start
                    waitingNumbersForRemind = false; ////ждём от пользователя номера чисел для напоминаний, но он вводит /start
                    if (message.Chat.Title != null)
                    {

                        if (!System.IO.File.Exists(fileName))//если файл не существует, то создаем его, в него будем записывать список групп
                        {
                            System.IO.File.WriteAllText(fileName, string.Empty);
                        }

                        //сделать метод для записи в файл или класс Controller
                        data = System.IO.File.ReadAllText(fileName);
                        listGroups = (data != string.Empty) ? JsonConvert.DeserializeObject<List<TelegramGroup>>(data) : new List<TelegramGroup>();
                        group = new TelegramGroup(message.Chat.Id, message.Chat.Title);
                        listGroups.Add(group);//добавляем группу в List
                        data = JsonConvert.SerializeObject(listGroups);//записываем группу в Json файл
                        System.IO.File.WriteAllText(fileName, data);
                        await botClient.SendTextMessageAsync(managerChatId, $"Група \"{message.Chat.Title}\" успішно додана ✅ \n\nНадішли шаблон повідомлення, використовуючин наступні коди:\n\n[smile] – смайлик\n[greeting] – привітання");
                        newGroup = true;
                    }
                }
                else if (message.Chat.Id == managerChatId && newGroup)
                {
                    if (System.IO.File.Exists(fileName))//если файл не существует, то создаем его, в него будем записывать список групп
                    {
                        data = System.IO.File.ReadAllText(fileName);
                        if (data != string.Empty) listGroups = JsonConvert.DeserializeObject<List<TelegramGroup>>(data);
                        if (listGroups != null) listGroups[listGroups.Count - 1].TextTemplate = message.Text;
                        data = JsonConvert.SerializeObject(listGroups);//записываем группу в Json файл
                        System.IO.File.WriteAllText(fileName, data);
                    }
                    newGroup = false;

                }
                else if (message.Chat.Id == managerChatId && message.Text == "/remind")
                {
                    if (System.IO.File.Exists(fileName))//если файл не существует, то создаем его, в него будем записывать список групп
                    {
                        data = System.IO.File.ReadAllText(fileName);
                        if (data != string.Empty) listGroups = JsonConvert.DeserializeObject<List<TelegramGroup>>(data);
                    }


                    string outputMessage = "Введи номери груп через кому (1, 2, 3), яким необхідно відправити нагадування про заняття\n\n";
                    if (listGroups != null)
                    {
                        foreach (var item in listGroups)
                        {
                            outputMessage += $"{listGroups.IndexOf(item) + 1} – {item.Name}\n";
                        }
                        await botClient.SendTextMessageAsync(managerChatId, outputMessage);
                    }
                    waitingNumbersForRemind = true;
                }
                else if (message.Chat.Id == managerChatId && waitingNumbersForRemind)
                {
                    bool inputMessageIsCorrect = true;
                    string inputMessage = message.Text;//строка с номерами групп, которым будем напоминать "1,2,3,4,5"
                    for (int  i = 0;  i < inputMessage.Length/2;  i++)//удаляем лишние пробелы
                    {
                        inputMessage = inputMessage.Replace(" ", "");
                    }

                    foreach (var symbol in inputMessage)
                    {
                        if (!numbers.Contains(symbol))
                        {
                            await botClient.SendTextMessageAsync(managerChatId, $"Повторіть введення");
                            inputMessageIsCorrect = false;
                            break;
                        }
                        
                    }

                    if (inputMessageIsCorrect)
                    {
                        var arrayNumbers = inputMessage.Split(',');
                        data = System.IO.File.ReadAllText(fileName);
                        if (data != string.Empty) listGroups = JsonConvert.DeserializeObject<List<TelegramGroup>>(data);


                        for (int i = 0; i < arrayNumbers.Length; i++)
                        {
                            group = listGroups[int.Parse(arrayNumbers[i]) - 1];
                            await botClient.SendTextMessageAsync(managerChatId, $"Повідомлення в групу \"{group.Name}\" успішно відправлено");
                            await botClient.SendTextMessageAsync(group.ID, $"Нагадування");

                        }
                        waitingNumbersForRemind = false;
                    }
                }
                else if (message.Chat.Id == managerChatId && message.Text=="/template")
                {
                    await botClient.SendTextMessageAsync(managerChatId, $"Обери номер групи шаблон якої ти хочеш переглянути/змінити");
                    waitingNumberGroupForTemplate = true;
                }
                else if (message.Chat.Id == managerChatId && waitingNumberGroupForTemplate)
                {
                    await botClient.SendTextMessageAsync(managerChatId, $"Шаблон цієї групи");
                    //await botClient.SendTextMessageAsync(managerChatId, $"{}");
                    waitingNumberGroupForTemplate = false;
                }
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions();
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}
