using System;

namespace Nikse.SubtitleEdit.Logic.Networking
{
    public class UpdateLogEntry
    {
        public int Id { get; private set; }
        public string UserName { get; private set; }
        public int Index { get; set; }
        public DateTime OccuredAt { get; private set; }
        public string Action { get; private set; }

        public UpdateLogEntry(int id, string userName, int index, string action)
        {
            Id = id;
            UserName = userName;
            Index = index;
            OccuredAt = DateTime.Now;
            Action = action;
        }

        public override string ToString()
        {
            return string.Format("{0:00}:{1:00}:{2:00} {3}: {4}", OccuredAt.Hour, OccuredAt.Minute, OccuredAt.Second, UserName, Action);
        }

    }
}