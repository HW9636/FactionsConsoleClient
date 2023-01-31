using System;
using System.Text.RegularExpressions;
using MinecraftClient.Commands;
using MinecraftClient.Scripting;
using Tomlet.Attributes;

namespace MinecraftClient.ChatBots
{
    public class FactionsCannonBot : ChatBot
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
