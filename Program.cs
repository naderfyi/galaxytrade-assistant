using System;
using System.Linq;
using XCommas.Net;
using XCommas.Net.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

    class Program
    {
        //update with your info
        static string key = "3commas API KEY";
        static string secret = "3commas Secret KEY";
        public static XCommasApi api;
        public static HttpClient hc = new HttpClient();
        public static List<BotInfo> biList = new List<BotInfo>();

        static DiscordSocketClient _client;
        static CommandService _commands;
        static IServiceProvider _services;
        static string token = "Discord Bot Token"; //TODO: Add Discord bot token
        public System.Threading.Timer timer;

        //RUN THE BOT ASSISTANT DISCORD BOT
        static async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();
            _client.Log += _client_Log;
            await RegisterCommandsAsync();

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(60); //Runs Bot assistant ever 1h

            var timer = new System.Threading.Timer(async (e) =>
            {
                await MainAsync();
            }, null, startTimeSpan, periodTimeSpan);

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);


        }

        static void Main() { RunBotAsync().GetAwaiter().GetResult(); }
        public static async Task MainAsync()
        {
        string perp;
        DiscordWrapper dw = new DiscordWrapper(token);
        foreach (BotInfo currentBot in biList) //check if user is trying to add a futures bots to the assistant
        {
            if (currentBot.getFutures() == true && currentBot.getMarket() == "binance")
            {
                perp = "_USDT";
            }
            else if (currentBot.getFutures() == true && currentBot.getMarket() == "ftx")
            {
                perp = "-PERP";
            }
            else
            {
                perp = "";
            }
            api = new XCommasApi(key, secret, default, UserMode.Real);
            var accts = await api.GetAccountsAsync();
            int accountId = currentBot.getAccountID();
            if (accountId != 0) foreach (var acct in accts.Data) if (acct.MarketCode == currentBot.getMarket()) { accountId = acct.Id; break; }

            HashSet<string> pairs = new HashSet<string>();
            var marketPairs = await api.GetMarketPairsAsync(currentBot.getMarket());
            foreach (string p in marketPairs.Data) if (p.StartsWith(currentBot.getBaseType())) pairs.Add(p + perp);
                try
                {
                string discordMessage = "";
                Console.WriteLine("Adding new pairs...");
                discordMessage = discordMessage + "━━━━━━> **New Pairs** :fire:" + "\n";
                List<Bubble500Root> bubbles = await GetJSON<List<Bubble500Root>>("https://cryptobubbles.net/backend/data/currentBubbles500.json");

                    bubbles = bubbles.OrderByDescending(x => x.data.usd.performance.min5).ToList();

                    HashSet<string> pairsToUpdate = new HashSet<string>();
                    int idx = 1;
                    foreach (Bubble500Root bubble in bubbles)
                    {
                        if (pairsToUpdate.Count >= currentBot.getMax()) break;
                        string pair = $"{currentBot.getBaseType()}_{bubble.symbol}{perp}";
                        if (!pairsToUpdate.Contains(pair))
                        {
                            if (pairs.Contains(pair))
                            {
                                pairsToUpdate.Add(pair);
                                Console.WriteLine($"{idx}) Added {pair} on {currentBot.getMarket()}, performance: {bubble.data.usd.performance.min5}");
                                discordMessage = discordMessage + $"{idx}) Added {pair}" + "\n";
                        }
                        }
                        idx++;
                    }

                    //return;

                    var sb = await api.ShowBotAsync(botId: currentBot.getBotId());
                    Bot bot = sb.Data;
                    //pairsToUpdate.Add("USDT_TESTTEST");
                    bot.Pairs = pairsToUpdate.ToArray();
                    var ub = await api.UpdateBotAsync(currentBot.getBotId(), new BotUpdateData(bot));
                if (ub.IsSuccess)
                {
                    Console.WriteLine($"\nSuccessfully updated {bot.Name} with {pairsToUpdate.Count} new pairs..");
                    await dw.SendMessage($"\nSuccessfully updated *{bot.Name}* with {pairsToUpdate.Count} new pairs :smile:" + "\n" + discordMessage); //send to discord

                }

                else
                {
                    //I couldn't find the market code for ftx futures although it probably exists
                    if (ub.Error.Contains("No market data for this pair"))
                    {
                        string[] badPairs = ub.Error.Split(": ").Select(p => p.Substring(0, p.IndexOf('"'))).ToArray();
                        foreach (string badPair in badPairs)
                            if (pairsToUpdate.Contains(badPair))
                            {
                                Console.WriteLine($"Removed {badPair} on {currentBot.getMarket()} because it only exists on spot");
                                pairsToUpdate.Remove(badPair);
                            }
                            else if (badPair.Contains(currentBot.getBaseType())) Console.WriteLine(badPair + " malformed?");
                        bot.Pairs = pairsToUpdate.ToArray();
                        ub = await api.UpdateBotAsync(currentBot.getBotId(), new BotUpdateData(bot));
                        if (ub.IsSuccess)
                        {
                            Console.WriteLine($"\nSuccessfully updated {bot.Name} with {pairsToUpdate.Count} new pairs..");
                            await dw.SendMessage($"\nSuccessfully updated *{bot.Name}* with {pairsToUpdate.Count} new pairs :smile:" + "\n" + discordMessage); //send to discord
                        }
                        else
                        {
                            Console.WriteLine($"ERROR: {ub.Error}");
                            await dw.SendMessage($"ERROR: {ub.Error} \n :poop: "+ $"{bot.Name}"); //send to discord
                        }
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: {ub.Error}");
                        await dw.SendMessage($"ERROR: {ub.Error} \n :poop: "+ $"{bot.Name}"); //send to discord
                    }
                }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                await dw.SendMessage($"ERROR: " + ex.Message + " :poop:");
                api = new XCommasApi(key, secret, default, UserMode.Real);
                    hc = new HttpClient();
                }
            //update every five minutes
            Thread.Sleep(4000);
            
        }
        Console.WriteLine(biList.Count + " Bots Updated:!");
        await dw.SendMessage("**Bots Updated: **" + " **" + biList.Count + "** " + ":smile:");
    }
        private static Task _client_Log(LogMessage arg)
        {
          
            Console.WriteLine(arg);
           

            return Task.CompletedTask;
        }

        static async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private static async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("$", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }

        }


        static async Task<dynamic> GetJSON<T>(string endpoint)
        {
            var res = await hc.GetAsync(endpoint);
            if (res.StatusCode == HttpStatusCode.OK)
            {
                string json = await res.Content.ReadAsStringAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            return null;
        }
    }
