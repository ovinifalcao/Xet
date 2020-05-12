using System;

namespace ServerSide
{
    public class StatusChangedEventArgs : EventArgs
    {
        public string Mensage { get; set; }

        public StatusChangedEventArgs(string mensage)
        {
            this.Mensage = mensage;
        }
    }
}
