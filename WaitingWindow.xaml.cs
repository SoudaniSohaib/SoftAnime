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

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for WaitingWindow.xaml
    /// </summary>
    public partial class WaitingWindow : Window
    {
        public WaitingWindow()
        {
            InitializeComponent();
        }

        public async void WaitingFilter(string request,string animename)
        {
            dbsqlclass AdvFilter = new dbsqlclass();
            DisplayMessage.Content = "Your list is being filtered, this might take a while...";
            bool result = await AdvFilter.FilteredRequest(request,animename);
            Close();
        }

        public async void WaitingFilter(string animename)
        {
            dbsqlclass AdvFilter = new dbsqlclass();
            DisplayMessage.Content = "Your list is being filtered, this might take a while...";
            bool result = await AdvFilter.FilteredRequest(animename);
            Close();
        }

        public async void WaitingUpdate(MainWindow.Anime anim, string Message)
        {
            dbsqlclass AdvFilter = new dbsqlclass();
            DisplayMessage.Content = Message;
            await AdvFilter.UpdateMysql(anim);
            Close();
        }

        public async Task<MainWindow.User> WaitingLogin(string Message, string mail, string pass)
        {
            dbsqlclass login = new dbsqlclass();
            DisplayMessage.Content = Message;
            
            MainWindow.User result = await login.LoginCheck(mail, pass);
            Close();
            return result;
        }

        public async Task<string?> WaitingSignup(string Message,string username, string mail, string pass)
        {
            dbsqlclass signup = new dbsqlclass();
            DisplayMessage.Content = Message;
            string result = await signup.Signup(username, "null", mail, pass);      
            Close();
            return result;
        }

        public async Task<string> WaitUserUpdate(string Message, UserSetting.UserSettingItem userSetting, string rnd, int id)
        {
            dbsqlclass userupdate = new dbsqlclass();
            DisplayMessage.Content = Message;
            string result = await userupdate.UserUpdateSQL(userSetting,rnd,id);
            Close();
            return result;
        }

        public async void WaitngConnect(string username)
        {
            DisplayMessage.Content = "Your connection is being established... Please Wait";
            dbsqlclass DatabaseConnect = new dbsqlclass();
            await DatabaseConnect.AlltableAscending(username);
            Close();
        }
    }
}
