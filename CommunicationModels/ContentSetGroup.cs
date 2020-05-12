using System.Collections.Generic;

namespace CommunicationModels
{
    public class ContentSetGroup : ContentModel
    {
        public string GroupName { get; set; }
        public List<string> ParticipantsNames { get; set; }
    }
}
