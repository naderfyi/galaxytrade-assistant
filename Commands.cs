using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

    public class Commands : ModuleBase<SocketCommandContext>
    {
    [Command("run")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task Run()
        {
            await ReplyAsync("Refreshing bot assistant ...");
            Program.MainAsync().GetAwaiter().GetResult();
        }
  [Command("addBA")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task AddBA(params string[] args)
    {
        if (args.Length == 0 || args.Length < 6)
        {
            await ReplyAsync("Input Full Bot Details");
            return;
        }
        int accountId = int.Parse(args[0]);
        int botId = int.Parse(args[1]);
        int max = int.Parse(args[2]);
        string market = args[3];
        string basetype = args[4];
        bool futures = bool.Parse(args[5]);

        Program.biList.Add(new BotInfo(accountId, botId, max, market, basetype, futures));

        await ReplyAsync("Your Bot: " + botId + " **successfully Added** :sunglasses:");

        Program.MainAsync().GetAwaiter().GetResult();
    }

    [Command("help")]
        public async Task help()
        {
            await ReplyAsync(":exclamation: **Useful Commands** :exclamation: \n\n" +
                "> • $addBA: *Adds a new bot to Bot Assistant*: \n" +
                "> **$addBA {AccountID} {BotID} {max_new_pairs} {market: binance/ftx} {baseType: ETH/USDT/USD} {futures true or false}** \n \n" + 
                "> • $removeBA: *Removes a bot fom Bot Assistant*: **$removeBA {BotID}** \n \n" +
                "> • $clear: *Clears discord channel chat*: **$clear {number of messages to delete}** \n \n " +
                "> • $runningBots: *Output all bot IDs that are running*: **$runningBots** \n \n" +
                "> • $run: *Refresh Bot Assistant*: **$run** ");
        }


   [Command("removeBA")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task removeBA(params string[] args)
        {
            if (args.Length == 0 || !int.TryParse(args[0], out int BotID))
            {
                await ReplyAsync("Input Bot ID");
                return;
            }
            int index=0;
            for(int i=0; i < Program.biList.Count; i++)
            {
                if (Program.biList[i].getBotId()==int.Parse(args[0]))
                {
                    index = i;
                }
            }

            Program.biList.RemoveAt(index);
            await ReplyAsync($"Your Bot: {BotID}" + " **successfully removed** :sunglasses:");
            Program.MainAsync().GetAwaiter().GetResult();
        }

        [Command("clear", RunMode = RunMode.Async)]
        [Summary("Deletes the specified amount of messages.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task PurgeChat(uint amount)
        {
            IEnumerable<IMessage> messages = await Context.Channel.GetMessagesAsync((int)(amount + 1)).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            const int delay = 3000;
            IUserMessage m = await ReplyAsync($"I have deleted {amount} messages for you :smile:");
            await Task.Delay(delay);
            await m.DeleteAsync();
        }

    [Command("runningBots")]
    public async Task runningBots()
    {
        if (Program.biList.Count <= 0)
        {
            await ReplyAsync("No bots running at the moment");
        }
        else
        {
            for (int i = 0; i < Program.biList.Count; i++)
            {
                int id = Program.biList[i].getBotId();
                await ReplyAsync(id.ToString() + ": is Running!");

            }
        }
    }



}