using App.Services.Context;
using App.Services.Models;
using Discord;

namespace App.Services.Repository
{
    internal class MessageRepository
    {
        private ulong _id;
        private ulong _authorId;
        private DateTime _date;
        private DateTime? _editDate;
        private ulong _channelId;
        private string _message;

        public MessageRepository(
            ulong id, ulong authorId, ulong channelId,
            DateTime date, DateTime editDate, string messageContent)
        {
            _id = id;
            _authorId = authorId;
            _channelId = channelId;
            _date = date;
            _editDate = editDate;
            _message = messageContent;
        }

        public static bool CheckIfExists(ulong id)
        {
            using var context = new MessageBackupContext();
            return context.Messages.Any(m => m.Id == id);
        }

        public void SaveToDatabase(List<IMessage> messagesToSave)
        {
            using var context = new MessageBackupContext();

            foreach (var theMessage in messagesToSave)
            {
                context.Messages.Add(
                    new Message()
                    {

                    });
            }

        }


    }
}
