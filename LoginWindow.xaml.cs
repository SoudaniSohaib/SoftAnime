using Org.BouncyCastle.Asn1.X500;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SoftAnime.MainWindow;

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        static readonly string? strWorkPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string dir = strWorkPath + @"\SoftAnime";

        private void Loginbutton_Click(object sender, RoutedEventArgs e)
        {
            string email = Emailtextbox.Text;
            SecureString pass = PText.SecurePassword;

            if(FormChecker(email, pass))
            {
                if(!File.Exists(dir + @"\usersdata.txt"))
                {
                    using(File.Create(dir + @"\usersdata.txt"))
                    {

                    }
                    OnlineLogin(email, pass);
                    return;

                }

                    if(!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    using(File.Create(dir + @"\usersdata.txt"))
                    {

                    }
                        OnlineLogin(email, pass);
                        return;
                    }
                    try
                    {
                        using(StreamReader reader = new StreamReader(dir + @"\usersdata.txt"))
                        {
                            string content = reader.ReadToEnd(); // Read the entire content of the file
                            if(string.IsNullOrEmpty(content))
                            {
                                reader.Dispose();
                                OnlineLogin(email,pass);
                                return;
                            }
                            // Split the content into lines
                            string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                            for(int i = 0; i < lines.Length; i += 4)
                            {
                                List<string> userdata = new List<string>
                                {
                                  lines[i],
                                  lines[i + 1],
                                  lines[i + 2],
                                  lines[i + 3]
                                };

                                string rnd = DatabaseSetting.SeperateStringA(userdata[2])[1];

                                if(dbsqlclass.Decrypt(userdata[1], rnd) == email)
                                {

                                string decryptedPassword = dbsqlclass.Decrypt(DatabaseSetting.SeperateStringA(userdata[2])[0], rnd);

                                if(decryptedPassword == SecureStringToString(pass))
                                    {
                                        MainWindow main = new MainWindow();
                                        MainWindow.User usr = new MainWindow.User();
                                        usr.UserEmail = dbsqlclass.Decrypt(userdata[1], rnd);
                                        usr.Username = userdata[0];
                                        usr.PreviousUsername = userdata[3];
                                        usr.SetUserPassword(DatabaseSetting.CombineStrings(decryptedPassword,rnd));
                                        usr.SetUserImagePath(dir + @"\" + userdata[0]);
                                        main.Show();
                                        Currentuser = usr;
                                        Close();
                                        return;
                                    }
                                }
                            }
                        }
                    OnlineLogin(email, pass);
                    } catch(Exception ex)
                    {
                        Console.WriteLine("Error occurred while reading from the file: " + ex.Message);
                    }

                
            }   
        }

        public static String SecureStringToString(SecureString value)
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

        public async void OnlineLogin(string email, SecureString pass)
        {
            WaitingWindow waitlogin = new WaitingWindow();
            waitlogin.Topmost = true;
            waitlogin.Show();
            
            Task<MainWindow.User> task = waitlogin.WaitingLogin("Please wait for your account to login", email, SecureStringToString(pass));
            if(task.Result != null)
            {
                if (task.Result.Username == "wrong" && task.Result.UserEmail == "wrong" && task.Result.PreviousUsername == "wrong")
                {
                    MessageBox.Show("Email or password is Wrong! Try again.");
                    return;
                } else if(task.Result.Username == "NoConnectionString")
                {
                    SettingsWindow settings = new SettingsWindow();
                    settings.Show();
                    settings.SettingsWindowChanger("Database");

                } else 
                {
                    MainWindow main = new MainWindow();
                    MainWindow.User user = task.Result;

                    string rnd = DatabaseSetting.SeperateStringA(user.GetUserPassword())[1];

                    user.SetUserPassword(DatabaseSetting.CombineStrings(dbsqlclass.Decrypt(DatabaseSetting.SeperateStringA(user.GetUserPassword())[0],rnd),rnd));
                    user.SetUserImagePath(dir + @"\" + user.Username);

                    if(!Directory.Exists(dir + @"\" + user.Username))
                    {
                        Directory.CreateDirectory(dir + @"\" + user.Username);
                    }
                    // Open the file for writing
                    using(StreamWriter writer = new StreamWriter(dir + @"\usersdata.txt",true))
                    {
                        // Write the content to the file
                        writer.WriteLine(user.Username);
                        writer.WriteLine(dbsqlclass.Encrypt(user.UserEmail, rnd));
                        writer.WriteLine(DatabaseSetting.CombineStrings(dbsqlclass.Encrypt(DatabaseSetting.SeperateStringA(user.GetUserPassword())[0], rnd), rnd)); ;
                        writer.WriteLine(user.PreviousUsername);
                    }
                    main.SetCurrentuser(user);
                    main.Show();
                    main.UserTaskBarMenu.Header = "Log Off";
                    Close();
                }
            } else if (task.Result == null)
            {
                MessageBox.Show("This User is not signed up!");
            } 
        }

        private bool FormChecker(string email, SecureString psw)
        {
            Regex regex = new Regex("[a-z0-9+_]+(?:\\.[a-z0-9+_]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase);
            Match match = regex.Match(email);
            if( email.Length == 0 || SecureStringToString(psw).Length == 0)
            {
                errormessage.Text = "One of the fields is empty!";
                return false;
            } else errormessage.Text = "";

            if( !match.Success )
            {
                errormessage.Text = "This is not a valid Email address!";
                return false;
            } else errormessage.Text = "";

            if ( SecureStringToString(psw).Length > 50 ) {
                errormessage.Text = "password too long!";
                return false;

            } else errormessage.Text = "";

            if(email.Length > 120)
            {
                errormessage.Text = "email too long!";
                return false;

            } else errormessage.Text = "";

            return true;
        }

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            SignUpWindow signup = new SignUpWindow();
            signup.Show();
            Close();
        }

        private void Emailtextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] one = Emailtextbox.Text.ToCharArray();
            if(one.Length == 0 || one.Length >= 120)
            {
                return;
            }
            List<Char> two = new List<char>();
            for(int i = 0; i < one.Length; i++)
            {
                if(one[i] == '@')
                {
                    two.Add('@');
                    continue;
                } else if(one[i] == '.')
                {
                    two.Add('.');
                    continue;
                }
                if(Char.IsLetterOrDigit(one[i]))
                {
                    two.Add(one[i]);
                }
            }

            string goodtext = new string(two.ToArray());
            Emailtextbox.Text = goodtext;
            Emailtextbox.Select(Emailtextbox.SelectionStart, 0);
        }
    }
}
