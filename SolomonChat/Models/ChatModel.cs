using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolomonChat.Model
{
    public class ChatModel
    {
        public ChatModel(string user, string text)
        {
            User = user;
            Text = text;
        }

        public string User { get; }
        public string Text { get; }

        public override string ToString()
        {
            return $"{User}: {Text}";
        }
    }
}