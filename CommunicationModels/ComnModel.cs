using System;

namespace CommunicationModels
{
    public class ComnModel
    {
        public enum Actions
        {
            SendText = 1,
            SendImage = 2,
            SetGroup = 3
        }

        public Actions ContentAction { get; set; }
        public string Addresee { get; set; }
        public DateTime Moment { get; set; }
        public ContentModel Content { get; set; }
    }
}
