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

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            const long managerChatId = -611718767;//chatID канала через который управляем ботом
            const string fileName = @"groups.json";
            string data;//строка, в которую будет помещаться информация с файла groups.json

            TelegramGroup group = new TelegramGroup();
            

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;

                if (message.Text != null && message.Text.ToLower() == "/start") //Button "Start" was pressed
                {
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
                    }
                }
                else if (message.Chat.Id == managerChatId && listGroups != null && false)
                {
                    if (System.IO.File.Exists(fileName))//если файл не существует, то создаем его, в него будем записывать список групп
                    {
                        data = System.IO.File.ReadAllText(fileName);
                        if (data != string.Empty) listGroups = JsonConvert.DeserializeObject<List<TelegramGroup>>(data);
                        if (listGroups != null) listGroups[listGroups.Count - 1].TextTemplate = message.Text;
                        data = JsonConvert.SerializeObject(listGroups);//записываем группу в Json файл
                        System.IO.File.WriteAllText(fileName, data);
                    }

                }
                else if (message.Chat.Id == managerChatId && message.Text == "/remind")
                {
                    if (System.IO.File.Exists(fileName))//если файл не существует, то создаем его, в него будем записывать список групп
                    {
                        data = System.IO.File.ReadAllText(fileName);
                        if (data != string.Empty) listGroups = JsonConvert.DeserializeObject<List<TelegramGroup>>(data);
                    }


                    if (listGroups != null)
                    {
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(listGroups[0].Name,listGroups[0].ID.ToString())
                        }
                    });
                        /*await botClient.SendTextMessageAsync(message.Chat,
                            "Вибери групу, якій потрібно відправити нагадування", replyMarkup: keyboard);*/
                        await botClient.SendTextMessageAsync(listGroups[0].ID, $"Нагадування про заняття");
                    }
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
