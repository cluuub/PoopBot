using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace PoopBot
{
    class Program
    {
        private const int DiscordMessageCacheSize = 5120;
        private ConcurrentBag<ulong> _monitoredChannels;
        private IEnumerable<Emote> _emoteList;
        private Random _rand;
        //private ConcurrentDictionary<ulong, MessageInformation> _messageStore;
        
        private DiscordSocketClient _client;

        static async Task Main(string[] args)
            => await new Program().MainAsync();


        public async Task MainAsync()
        {
            _rand = new Random();
            //create a client and register callbacks
            var config = new DiscordSocketConfig
            {
                MessageCacheSize = DiscordMessageCacheSize, AlwaysDownloadUsers = true
            };
            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.MessageDeleted += MessageDeleted;
            _client.MessageReceived += MessageReceived;
            //initialize messagestore and monitoredchannels
            //_messageStore = new ConcurrentDictionary<ulong, MessageInformation>(4, DiscordMessageCacheSize);
            _monitoredChannels = new ConcurrentBag<ulong>();
            //log into the discord API and start listening
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            
            await _client.StartAsync();
            //wait forever so the main thread doesn't end
            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private void ParseEmotes()
        {
            _emoteList = new List<Emote>
            {
                //add desired emotes here.
                //acquire them by typing out the emote in your server, then adding a backslash infront of it.
                Emote.Parse("<:NotSureDank:720454064042672188>")
            };
        }


        private async Task MessageReceived(SocketMessage msg)
        {
            if (msg.Content == "!monitor")
            {
                if (!_monitoredChannels.Contains(msg.Channel.Id))
                {
                    _monitoredChannels.Add(msg.Channel.Id);
                    await msg.Channel.SendMessageAsync(
                        $"I am now monitoring this channel for otherworldly ghost activity... {_emoteList.ElementAt(_rand.Next(0, _emoteList.Count()))}");
                }
            }
        }

        private async Task MessageDeleted(Cacheable<IMessage, ulong> cacheObject, ISocketMessageChannel channel)
        {
            if (_monitoredChannels.Contains(channel.Id) && cacheObject.HasValue)
            {
                var message = cacheObject.Value;
                var mentionedUsers = message.MentionedUserIds;
                var mentionedRoles = message.MentionedRoleIds;
                if (!mentionedRoles.Any() && !mentionedUsers.Any())
                    return;
                var author = message.Author;
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(
                    $"I can feel a ghost in here... {_emoteList.ElementAt(_rand.Next(0, _emoteList.Count()))}" +
                    $"\n{author.Mention} just tried to ghostping ");
                
                if (mentionedUsers.Any())
                {
                    stringBuilder.Append("these people:");
                    foreach (var userId in mentionedUsers)
                    {
                        stringBuilder.Append(" ");
                        var user = await channel.GetUserAsync(userId);
                        stringBuilder.Append(user.Username);
                    }
                }
                if (mentionedRoles.Any())
                {
                    if (mentionedUsers.Any())
                    {
                        stringBuilder.Append("\n");
                        stringBuilder.Append("and also these roles:");
                    }
                    else
                    {
                        stringBuilder.Append("these roles:");
                    }

                    foreach (var roleId in mentionedRoles)
                    {
                        stringBuilder.Append(" ");
                        var role = ((IGuildChannel) message.Channel).Guild.GetRole(roleId);
                        stringBuilder.Append(role.Name);
                    }
                }
                await channel.SendMessageAsync(stringBuilder.ToString());
            }
        }
    }
}