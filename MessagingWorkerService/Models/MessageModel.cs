using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingWorkerService.Models
{
    public class MessageModel
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public MessageMetaModel Meta { get; set; }
    }

    public partial class MessageMetaModel
    {
        public string Stuff { get; set; }
        public string Things { get; set; }

    }
}
