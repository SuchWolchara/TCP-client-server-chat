using System.ComponentModel;

namespace ServerApplication.ChatServer
{
    public enum ChatCommands
    {
        [Description("Bye")]
        Bye,
        [Description("Hello, world!")]
        HelloWorld,
        [Description("How are you?")]
        HowAreYou,
        [Description("What is your name?")]
        WhatIsYourName,
        [Description("You are breathtaking!")]
        YouAreBreathtaking
    }

    public class ChatConstants
    {
        public const string Server = "Server";
        public const string Client = "Client";

        public const string Connected = "connected";
        public const string Disconnected = "disconnected";
        public const string Ok = "ok";

        public const char Separator = '|';
    }
}
