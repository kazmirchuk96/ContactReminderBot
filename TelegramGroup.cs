using System;
using System.Collections.Generic;
using System.Text;

namespace ContactReminderBot
{
    internal class TelegramGroup
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string TextTemplate { get; set; }
        //public bool IsParentsGroup { get; set; }

        public TelegramGroup (long iD, string name)
        {
            ID = iD;
            Name = name;
            TextTemplate = String.Empty;
        }

        public TelegramGroup() : this (-1, "Noname")
        { }
    }
}
