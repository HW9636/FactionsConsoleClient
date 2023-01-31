using System;
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

            public void OnSettingUpdate()
            {
                // Delay cannot be less than 100ms
                reconnect_delay = Math.Max(0.1D, reconnect_delay);
                // Cannot have negative magnitude
                reconnect_randomness = Math.Abs(reconnect_randomness);

                command_interval = Math.Max(0.5D, command_interval);
                command_interval_randomness = Math.Abs(command_interval_randomness);

                home_command = home_command.Trim();
            }
        }

        private static readonly Random random = new();

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

        public override void OnDeath()
        {
            //TODO: Doesn't work
            if (!Config.Enabled) return;
            LogDebugToConsole(Translations.bot_factionsAfk_respawning);
            System.Threading.Thread.Sleep(200 + (int)(random.NextDouble() * 500));
            Respawn();
            SendReconnectCommands();
            Home(true);
        }

        public override void AfterGameJoined()
        {
            if (!Config.Enabled) return;
            SendReconnectCommands();
            Home(true);
        }

        public override bool OnDisconnect(DisconnectReason reason, string message)
        {
            if (!Config.Enabled) return false;
            if (reason == DisconnectReason.UserLogout)
            {
                LogDebugToConsole(Translations.bot_autoRelog_ignore_user_logout);
            }
            else 
            {
                message = GetVerbatim(message);
                string comp = message.ToLower();

                LogDebugToConsole(string.Format(Translations.bot_factionsAfk_reconnecting, message));

                Reconnect();
                LogDebugToConsole(Translations.bot_autoRelog_reconnect_ignore);
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

