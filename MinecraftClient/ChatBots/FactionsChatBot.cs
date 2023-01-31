using System;
using System.Text.RegularExpressions;
using MinecraftClient.Protocol.ProfileKey;
using MinecraftClient.Scripting;
using Tomlet.Attributes;

namespace MinecraftClient.ChatBots
{
    public class FactionsChatBot : ChatBot
    {
        public static Configs Config = new();

        [TomlDoNotInlineObject]
        public class Configs
        {
            public bool Enabled = false;

            public void OnSettingUpdate()
            {
                
            }
        }
    }
}
