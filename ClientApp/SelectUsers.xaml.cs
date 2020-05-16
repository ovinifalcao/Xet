using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClientApp
{
    public partial class SelectUsers : Window
    {
        public SelectUsers(List<ConctactCard> ActiveContactCards)
        {
            InitializeComponent();
            BuildListOfConnectedContacts(ActiveContactCards);
        }

        private void BuildListOfConnectedContacts(List<ConctactCard> ActiveContactCards)
        {
            foreach (ConctactCard Cd in ActiveContactCards)
            {
                if (string.IsNullOrEmpty(Cd.GroupContacts))
                {
                    var ListElement = new SelectContact();
                    ListElement.txbContactName.Text = Cd.txbContactName.Text;
                    pnSelectContacts.Children.Add(ListElement);
                }
            }
        }

        public List<string> GetAllSelected()
        {
            return (from Sc in pnSelectContacts.Children.Cast<SelectContact>().ToList()
                    where Sc.cbContact.IsChecked == true
                    select Sc.txbContactName.Text).ToList();
        }

        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
