using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for SignUpWindow.xaml
    /// </summary>
    public partial class SignUpWindow : Window
    {
        public SignUpWindow()
        {
            InitializeComponent();
        }

        static readonly string? strWorkPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private static readonly Random random = new Random();

        private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/";

        private void Signup_Click(object sender, RoutedEventArgs e)
        {
            string usern = UsernameTextbox.Text;
            SecureString pass = passwordBox1.SecurePassword;
            SecureString confpass = passwordBoxConfirm.SecurePassword;
           
            string dir = strWorkPath + @"\SoftAnime";

            if( FormChecker(usern,textBoxEmail.Text, SecureStringToString(pass), SecureStringToString(confpass)) )
            {


                    string rnd = GenerateRandomCharacters(16);
                    string Email = textBoxEmail.Text;
                    string EncryptedPass = DatabaseSetting.CombineStrings(dbsqlclass.Encrypt(SecureStringToString(pass), rnd),rnd);
                
                    WaitingWindow waitlogin = new WaitingWindow();
                    waitlogin.Show();
                    Task<string?> task = waitlogin.WaitingSignup("Please wait for your account to be created", usern, Email, EncryptedPass);
                
                if(task.Result == null)
                {
                    MessageBox.Show("Check your internet connection!");
                    return;
                }
                if(task.Result == "exists") {

                    MessageBox.Show("This User already exists!");

                } else if(task.Result == "NoConnectionString")
                {
                    SettingsWindow settings = new SettingsWindow();
                    settings.Show();
                    settings.SettingsWindowChanger("Database");

                } else if(task.Result == "created") {

                    try
                    {

                        if(!Directory.Exists(dir + @"\" + usern))
                        {
                            Directory.CreateDirectory(dir + @"\" + usern);
                        }
                        // Open the file for writing
                        using(StreamWriter writer = new StreamWriter(dir + @"\usersdata.txt",true))
                        {
                            // Write the content to the file
                            writer.WriteLine(usern);
                            writer.WriteLine(dbsqlclass.Encrypt(textBoxEmail.Text, rnd));
                            writer.WriteLine(EncryptedPass);
                            writer.WriteLine("null");
                        }


                    } catch(Exception ex)
                    {
                        MessageBox.Show("Something went wrong, Try again!" + ex.Message);
                        return;
                    }
                    MessageBox.Show("Your account is successfully created, Click to Login!");
                    LoginWindow login = new LoginWindow();
                    login.Show();
                    Close();
                } else { 
                    MessageBox.Show("Something went wrong! Try Again");
                }               

                

            }
        }

        public bool FormChecker(string username, string email,string psw, string confpsw)
        {
            Regex Mailregex = new Regex("[a-z0-9+_]+(?:\\.[a-z0-9+_]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", RegexOptions.IgnoreCase);
            Match Mailmatch = Mailregex.Match(email);
            Regex PassRegex = new Regex(@"^(?=.*[A-Z])(?=.*[!@#$\-*/_+#\\.,])(?=.*\d)(?=.*[a-z]).{8,}$", RegexOptions.IgnoreCase);
            Match PassMatch = PassRegex.Match(psw);

            if( username.Length == 0 )
            {
                UsernameWarning.Text = "Add a username for your account!";
                return false;
            } else UsernameWarning.Text = "";

            if( !Mailmatch.Success )
            {
                EmailWarning.Text = "This is not a valid Email address!";
                return false;
            } else EmailWarning.Text = "";

            if(email.Length > 120)
            {
                EmailWarning.Text = "Email address is too long!";
                return false;
            } else EmailWarning.Text = "";

            if(!PassMatch.Success)
            {
                PswWarning.Text = "Thats not a secure password!";
                return false;
            } else { 
                PswWarning.Text = ""; 
            }

            if(psw.Length > 50 )
            {
                PswWarning.Text = "Password too long!";
                return false;
            } else
            {
                PswWarning.Text = "";
            }

            if( confpsw != psw )
            {
                ConfirmpassWarning.Text = @"The passwords doesn't match up!";
                return false;
            } else ConfirmpassWarning.Text = "";

            return true;
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

        public static string GenerateRandomCharacters(int length)
        {
            StringBuilder builder = new StringBuilder(length);

            for(int i = 0; i < length; i++)
            {
                builder.Append(chars[random.Next(chars.Length)]);
            }

            return builder.ToString();
        }


        private void UsernameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] one = UsernameTextbox.Text.ToCharArray();
            if( one.Length == 0 || one.Length >= 30 )
            {
                return;
            }
            List<Char> two = new List<char>();
            for( int i = 0; i < one.Length; i++ )
            {
                if( one[i] == '_' )
                {
                    two.Add('_');
                    continue;
                } else if( Char.IsLetterOrDigit(one[i]) || one[i] == '_')
                {
                    two.Add(one[i]);
                }
            }
            string goodtext = new string(two.ToArray());
            UsernameTextbox.Text = goodtext;
            UsernameTextbox.Select(UsernameTextbox.SelectionStart, 0);
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow login = new LoginWindow();
            login.Show();
            Close();
        }

        private void ResetForm_Click(object sender, RoutedEventArgs e)
        {
            textBoxEmail.Clear();
            UsernameTextbox.Clear();
            passwordBox1.Clear();
            passwordBoxConfirm.Clear();
            UsernameWarning.Text = "";
            EmailWarning.Text = "";
            PswWarning.Text = "";
            ConfirmpassWarning.Text = "";
        }

        
        public static string ReadFromFile(string filePath)
        {
            try
            {
                // Open the file for reading
                using(StreamReader reader = new StreamReader(filePath))
                {
                    // Read the content from the file
                    string content = reader.ReadLine();
                    return content;
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Error occurred while reading from the file: " + ex.Message);
                return null;
            }
        }




    }
}
