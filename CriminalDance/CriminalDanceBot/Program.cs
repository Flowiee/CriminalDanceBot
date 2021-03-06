﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Runtime.Caching;
using CriminalDanceBot.Handlers;
using CriminalDanceBot.Models;
using System.Threading;
using ConsoleTables;
using System.Xml.Linq;
using System.IO;
using TelegramBotTranslations;
using Telegram.Bot.Types.Enums;

namespace CriminalDanceBot
{
    class Program
    {
        public static BotTranslationManager Translations;
        public static readonly MemoryCache AdminCache = new MemoryCache("GroupAdmins");
        public static bool MaintMode = false;
        public static DateTime Startup;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            var token = Constants.GetBotToken("BotToken");

            Bot.Api = new TelegramBotClient(token);
            Bot.Me = Bot.Api.GetMeAsync().Result;

            Bot.Gm = new GameManager();

            Translations = new BotTranslationManager(token, Constants.GetLangDirectory(false), Constants.GetLangDirectory(true), 
                "English", TelegramBotTranslations.Models.ParseMode.Html, false);

            token = null;

            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            Bot.Send(Constants.LogGroupId, $"Bot started! Version: {version.ToString()}");

            Console.Title = $"CriminalDanceBot - Connected to {Bot.Me.FirstName} (@{Bot.Me.Username} | {Bot.Me.Id}) - Version {version.ToString()}";

            foreach (var m in typeof(Commands).GetMethods())
            {
                foreach (var a in m.GetCustomAttributes(true))
                {
                    if (a is Attributes.Command cmd)
                    {
                        var method = m.CreateDelegate(typeof(Bot.CommandMethod)) as Bot.CommandMethod;
                        Bot.Commands.Add(new Models.Command(cmd.Trigger, cmd.AdminOnly, cmd.DevOnly, cmd.GroupOnly, method));
                    }
                    
                }
            }

            Bot.Api.GetUpdatesAsync(-1).Wait();
            Handler.HandleUpdates(Bot.Api);
            Bot.Api.StartReceiving();
            Startup = DateTime.Now;
            new Thread(UpdateConsole).Start();
            Console.ReadLine();
            Bot.Api.StopReceiving();
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exc = (Exception)e.ExceptionObject;
            string message = Environment.NewLine + Environment.NewLine + exc.Message + Environment.NewLine + Environment.NewLine;
            string trace = exc.StackTrace;

            do
            {
                exc = exc.InnerException;
                if (exc == null) break;
                message += exc.Message + Environment.NewLine + Environment.NewLine;
            }
            while (true);

            message += trace;
            Bot.Send(Constants.LogGroupId, "<b>UNHANDELED EXCEPTION! BOT IS PROBABLY CRASHING!</b>" + message.FormatHTML());
            Thread.Sleep(5000); // Give the message time to be sent
        }

        private static void UpdateConsole()
        {
            while (true)
            {
                Console.Clear();
                var Uptime = DateTime.Now - Startup;
                string msg = $"Startup Time: {Startup.ToString()}";
                msg += Environment.NewLine + $"Uptime: {Uptime.ToString()}";
                var games = Bot.Gm.Games;
                int gameCount = games.Count();

                msg += Environment.NewLine + $"Number of Games: {gameCount.ToString()}";
                Console.WriteLine(msg);

                var table = new ConsoleTable("Game GUID",　"ChatId", "Phase", "# of Players", "InGame Action");
                foreach (CriminalDance game in games)
                {
                    table.AddRow(game.Id.ToString(), game.ChatId.ToString(), game.Phase.ToString(), game.Players.Count().ToString(), game.Phase == CriminalDance.GamePhase.InGame ? game.NowAction.ToString() : "------");
                    if (game.Phase == CriminalDance.GamePhase.InGame && game.PlayerQueue.Count > 0)
                    {
                        var p = game.PlayerQueue.Peek();
                        table.AddRow("--->", "Current Player:", p.Name, "Player/Card Choice",
                            (p.PlayerChoice1 != null || p.CardChoice1 != null) ? (p.PlayerChoice1 != null ? p.PlayerChoice1.ToString() : p.CardChoice1.ToString()) : "-----");

                    }
                }
                table.Write(Format.Alternative);
                
                Thread.Sleep(2000);
            }
        }
    }
}
