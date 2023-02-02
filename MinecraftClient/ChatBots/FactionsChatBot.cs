using System;
using System.Text.RegularExpressions;
using DSharpPlus.EventArgs;
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


        public override void Initialize()
        {
            
        }

        public override void GetText(string text)
        {
            text = GetVerbatim(text);

            FactionSender sender = new();
            string message = "";

            if (IsFactionMessage(text, ref sender, ref message))
            {

            }
        }
    }
}
