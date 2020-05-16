using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp
{
    /// <summary>
    /// Interação lógica para ControlMessage.xam
    /// </summary>
    public partial class ControlMessage : UserControl
    {
        public ControlMessage()
        {
            InitializeComponent();
        }

        public static List<Tuple<string, string>> RepresentacaoTextualDosEmojis = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>(":D", "emj_smile"),
            new Tuple<string, string>(":(", "emj_sad"),
            new Tuple<string, string>(":0", "emj_surprise"),
            new Tuple<string, string>(":)", "emj_msmile"),
            new Tuple<string, string>("X0", "emj_dead"),
            new Tuple<string, string>(":'(", "emj_cry"),
            new Tuple<string, string>(":'0", "emj_scry"),
            new Tuple<string, string>("(<3", "emj_love"),
            new Tuple<string, string>("<>)", "emj_shsmile"),
            new Tuple<string, string>("':(", "emj_angry"),
            new Tuple<string, string>("<<star>>", "emj_star"),
            new Tuple<string, string>("<<like>>", "emj_like"),
            new Tuple<string, string>("<<heart>>", "emj_heart")
        };
}
}
