using System;
using System.Text.RegularExpressions;
using MinecraftClient.Scripting;
using Tomlet.Attributes;

namespace MinecraftClient.ChatBots
{
	public class FactionsAFKBot : ChatBot
	{
        public static Configs Config = new();

        [TomlDoNotInlineObject]
        public class Configs
        {
            public bool Enabled = true;

            [TomlInlineComment("Delay before the bot tries to reconnect, in seconds")]
            public double reconnect_delay = 3.0D;

            [TomlInlineComment("Introduces randomness in connection timings, in seconds")]
            public double reconnect_randomness = 1.0D;

            [TomlPrecedingComment("Commands that will be run when reconnected, logged in, and with an interval. Commands will be ran in order")]
            public String[] reconnect_commands = { "joinqueue factions-pirate" };

            [TomlInlineComment("Interval between commands, in seconds")]
            public double command_interval = 1.0D;

            [TomlInlineComment("Randomness in interval between commands, in seconds")]
            public double command_interval_randomness = 0.1D;

            [TomlInlineComment("Command that is executed when bot dies, joins, and reconnects from server, leave empty for no command. Example: 'home afk'")]
            public string home_command = "";

            [TomlInlineComment("Automatically respawns when dead")]
            public bool auto_respawn = true;

            [TomlInlineComment("Chat messages that can be used to detect when bot is killed")]
            public string[] respawn_messages = { "^Your power is now ([0-9]+) \\/ [0-9]+$" };

            [TomlInlineComment("Interval to run reconnect_commands, then home command, in seconds. Will be reset on reconnect and respawn")]
            public int autorun_cmd_interval = 120;

            public void OnSettingUpdate()
            {
                // Delay cannot be less than 100ms
                reconnect_delay = Math.Max(0.1D, reconnect_delay);
                // Cannot have negative magnitude
                reconnect_randomness = Math.Abs(reconnect_randomness);

                command_interval = Math.Max(0.5D, command_interval);
                command_interval_randomness = Math.Abs(command_interval_randomness);

                home_command = home_command.Trim();

                Respawn_regex = new Regex[respawn_messages.Length];
                for (int i = 0;i < respawn_messages.Length;i++)
                {
                    try
                    {
                        Respawn_regex[i] = new Regex(respawn_messages[i].Trim());
                    }
                    catch (ArgumentException) {
                        Respawn_regex = new Regex[0];
                        ConsoleIO.WriteLine(string.Format(Translations.bot_factionsAfk_invalid_regex, respawn_messages[i].Trim()));
                        return;
                    }
                }
            }
        }

        private static readonly Random random = new();

        private int Counter = 0;

        private static Regex[] Respawn_regex = { };

        public FactionsAFKBot()
		{
            LogDebugToConsole(Translations.bot_factionsAfk_launch);
        }

        public override void Initialize()
        {
            _Initialize();
        }

        private void _Initialize()
        {
            
        }

        public override void Update()
        {
            Counter++;

            if (Counter > Config.autorun_cmd_interval * 10) // 10 Ticks per second
            {
                Counter = 0;

                SendReconnectCommands();
                Home(true);
            }
        }

        public override void OnDeath()
        {
            _OnDeath();
        }

        public override void GetText(string text)
        {
            text = GetVerbatim(text);
            foreach (Regex regex in Respawn_regex)
            {
                if (regex.IsMatch(text))
                {
                    LogToConsole("Death detected in message: " + text);
                    _OnDeath(false);
                    return;
                }
            }
        }

        private void _OnDeath(bool respawn = true)
        {
            if (!Config.Enabled) return;
            LogToConsole(Translations.bot_factionsAfk_respawning);

            Counter = 0;
            System.Threading.Thread.Sleep(200 + (int)(random.NextDouble() * 500));
            if (respawn) Respawn();
            SendReconnectCommands();
            Home(true);
        }

        public override void AfterGameJoined()
        {
            if (!Config.Enabled) return;
            SendReconnectCommands();
            Home(true);
            Counter = 0;
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            if (!Config.Enabled) return false;
            if (reason == DisconnectReason.UserLogout)
            {
                LogDebugToConsole(Translations.bot_factionsAfk_ignored_user_logout);
            }
            else 
            {
                message = GetVerbatim(message);
                string comp = message.ToLower();

                LogDebugToConsole(string.Format(Translations.bot_factionsAfk_reconnecting, message));

                Reconnect();
                return true;
            }

            return false;
        }

        private void Home(bool immediate = false)
        {
            if (Config.home_command.Length != 0)
            {
                if (!immediate) System.Threading.Thread.Sleep((int)(random.NextDouble() * 500) + 200);
                LogDebugToConsole(string.Format(Translations.bot_factionsAfk_going_home, Config.home_command));
                SendText("/" + Config.home_command);
            }
        }

        private void Reconnect()
        {
            double delay = (random.NextDouble() * 2 * Config.reconnect_randomness - Config.reconnect_randomness) + Config.reconnect_delay;
            LogToConsole(string.Format(Translations.bot_factionsAfk_reconnect_delay, delay));
            System.Threading.Thread.Sleep((int)Math.Floor(delay * 1000));

            ReconnectToTheServer(int.MaxValue, 0, false);
        }

        private void SendReconnectCommands()
        {
            double interval;
            foreach (string cmd in Config.reconnect_commands)
            {
                interval = (random.NextDouble() * 2 * Config.command_interval_randomness - Config.command_interval_randomness) + Config.command_interval;
                SendText("/" + cmd);
                LogDebugToConsole(string.Format(Translations.bot_factionsAfk_command_interval, interval));
                System.Threading.Thread.Sleep((int)Math.Floor(interval * 1000));
            }
        }

        public static bool OnDisconnectStatic(DisconnectReason reason, string message)
        {
            if (Config.Enabled)
            {
                FactionsAFKBot bot = new();
                bot.Initialize();
                return bot.OnDisconnect(reason, message);
            }
            return false;
        }
    }
}

