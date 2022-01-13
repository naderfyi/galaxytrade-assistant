using System;
using System.Text;
using System.Collections.Generic;
using XCommas.Net.Objects;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
class DiscordWrapper
{
    string _token, _msg;
    DiscordSocketClient _client;
    public DiscordWrapper(string token) { _token = token; }
    public async Task SendMessage(string msg)
    {
        _msg = msg;
        _client = new DiscordSocketClient();
        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        _client.Ready += _client_Ready;
        //await Task.Delay(-1);
    }
    private async Task _client_Ready()
    {
        var guild = _client.GetGuild(826842684196416); //TODO: guild id Discord SERVER ID
        if (guild != null)
        {
            var channel = guild.GetTextChannel(26545426823542685); //TODO: Discord CHANNEL ID
            await channel.SendMessageAsync(_msg);
        }
        _client.Dispose();
        //Environment.Exit(0);
    }
}