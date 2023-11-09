using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static SoftAnime.UserSetting;

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for UserSetting.xaml
    /// </summary>
    public partial class UserSetting : UserControl
    {
        public UserSetting()
        {
            InitializeComponent();
        }

        public class UserSettingItem
        {
            public string Username { get; set; }
            public string PreviousUsername { get; set; }
            private string Email { get; set; }
            private string Password { get; set; }
            public bool EmailVerify { get; set; }
            
            public UserSettingItem (string username, string prevuser,string email, string password)
            {
                Username = username;   
                PreviousUsername = prevuser;
                Email = email;
                Password = password;
            }
            public string GetUserSettingPassword()
            {
                return Password;
            }

            public string GetUserSettingEmail()
            {
                return Email;
            }

        }
        public UserSettingItem Settingholder(string PreviousUsername)       
        {
            if(UserFormChecker(UsernameSetting.Text, EmailSetting.Text, SecureStringToString(PasswordSetting.SecurePassword), SecureStringToString(PasswordConfirmSetting.SecurePassword)))
            {
                if(DatabaseSetting.SeperateStringA(MainWindow.Currentuser.GetUserPassword())[0] == SecureStringToString(PasswordSetting.SecurePassword) || SecureStringToString(PasswordSetting.SecurePassword) == "")
                {

                    UserSettingItem userSettingItem = new UserSettingItem(UsernameSetting.Text, PreviousUsername, EmailSetting.Text, DatabaseSetting.SeperateStringA(MainWindow.Currentuser.GetUserPassword())[0]);
                    return userSettingItem;
                } else
                {
                    UserSettingItem userSettingItem = new UserSettingItem(UsernameSetting.Text, PreviousUsername, EmailSetting.Text, SecureStringToString(PasswordSetting.SecurePassword));
                    return userSettingItem;
                }
            } else
            {
                return null;
            }          
        }

        private bool UserFormChecker(string username, string email, string pass, string passconf)
        {
            Regex PassRegex = new Regex(@"^(?=.*[A-Z])(?=.*[!@#$\-*/_+#\\.,])(?=.*\d)(?=.*[a-z]).{8,}$", RegexOptions.IgnoreCase);
            Match PassMatch = PassRegex.Match(pass);

            EmailAddressAttribute emailValidator = new EmailAddressAttribute();

            if(username.Length == 0)
            {
                UserSettingsErrorMessage.Content = "Add a username for your account!";
                return false;
            }

            if(!emailValidator.IsValid(email))
            {
                UserSettingsErrorMessage.Content = "This is not a valid Email address!";
                return false;
            }

            if(email.Length > 120)
            {
                UserSettingsErrorMessage.Content = "Email address is too long!";
                return false;
            }

            if(pass.Length > 0)
            {
                if(pass.Length > 50)
                {
                    UserSettingsErrorMessage.Content = "Password too long!";
                    return false;
                }

                if(!PassMatch.Success)
                {
                    UserSettingsErrorMessage.Content = "That's not a secure password!";
                    return false;
                }

                if(passconf != pass)
                {
                    UserSettingsErrorMessage.Content = "The passwords don't match up!";
                    return false;
                }
            }

            UserSettingsErrorMessage.Content = "";
            return true;
        }

        public void SettingCurrentUserSetting(MainWindow.User user)
        {
            if (user != null)
            {
                UsernameSetting.Text = user.Username;
                EmailSetting.Text = user.UserEmail;               
            }
        }

        String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            } finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
