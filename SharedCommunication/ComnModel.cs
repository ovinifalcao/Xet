using System;

namespace SharedCommunication
{
    public class ComnModel
    {
        public enum Actions
        {
            SendText = 1,
            SendImage = 2,
            SendTextGroup = 3,
            SendImageGroup = 4,
            SetGroup = 5,
            SendError = 6,
            SendConnectionSuccessful = 7,
            SendConnectionResquest = 8,
            SendNewUSerConneted = 9,
            SendUsersAlreadyLogged = 10,
            SendUserIsDisconnecting = 11
        }

        public Actions ContentAction { get; set; }
        public string Addresee { get; set; }
        public DateTime Moment { get; set; }
        public string Content { get; set; }
    }
}
