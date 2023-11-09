
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;


namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for DatabaseSetting.xaml
    /// </summary>
    public partial class DatabaseSetting : UserControl
    {
        public DatabaseSetting()
        {
            InitializeComponent();
            if(strin != "null")
            {
                ConnectionStringSetting.Text = dbsqlclass.Decrypt(SeparateString(strin)[0], SeparateString(strin)[1]);
            } else
            {
                ConnectionStringSetting.Text = "Full Database String";
            }
            Mysqldatabasecheckbox.IsChecked = true;
        }
        string strin = ConfigurationManager.ConnectionStrings["AdminConnect"].ConnectionString;

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
                dbsqlclass cclass = new dbsqlclass();
                if(cclass.IsConnectedToInternet())
                {
                    if(cclass.ConnectionStringTester(ConnectionStringSetting.Text))
                    {
                        MessageBox.Show("The Database Connection was established!");
                    } else
                    {
                        MessageBox.Show("Verify your Connection String and try again!");
                    }
                }         
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            dbsqlclass cclass = new dbsqlclass();
            if(ConnectionStringSetting.Text == "" || ConnectionStringSetting.Text == "Full Database String")
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.ConnectionStrings.ConnectionStrings["AdminConnect"].ConnectionString = "null";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
                return;
            }
            if(cclass.IsConnectedToInternet())
            {
                if(cclass.ConnectionStringTester(ConnectionStringSetting.Text))
                {
                    string rnd = SignUpWindow.GenerateRandomCharacters(16);


                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.ConnectionStrings.ConnectionStrings["AdminConnect"].ConnectionString = CombineStrings(dbsqlclass.Encrypt(ConnectionStringSetting.Text, rnd), rnd);
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("connectionStrings");
                    dbsqlclass.DoesuserstableExist();
                    MessageBox.Show("Your Connection String is saved!");

                } else
                {
                    MessageBox.Show("Verify your Connection String and try again! or Leave it blank to delete it");
                }
            }
        }

        public static List<string> SeparateString(string input)
        {

            string string1 = "";
            string string2 = "";
            List<string> str = new List<string>();

            for(int i = 0; i < input.Length; i++)
            {
                if(i < 32)
                {
                    if(i % 2 == 0)
                    {
                        string1 += input[i];
                    } else
                    {
                        string2 += input[i];
                    }
                } else
                {
                    string1 += input[i];
                }
            }
            str.Add(string1);
            str.Add(string2);
            return str;
        }

        public static List<string> SeperateStringA(string input)
        {

            string string1 = "";
            string string2 = "";
            List<string> str = new List<string>();

            int n = (input.Length - 16) * 2;

            if (input.Length > 32)
            {
                for(int i = 0; i < input.Length; i++)
                {
                    if(i < 32)
                    {
                        if(i % 2 == 0)
                        {
                            string1 += input[i];
                        } else
                        {
                            string2 += input[i];
                        }
                    } else
                    {
                        string1 += input[i];
                    }
                }

            } else
            {
                for(int i = 0; i < input.Length; i++)
                {
                    if(i < n)
                    {
                        if(i % 2 == 0)
                        {
                            string1 += input[i];
                        } else
                        {
                            string2 += input[i];
                        }
                    } else
                    {
                        string2 += input[i];
                    }
                }
            }

           
            str.Add(string1);
            str.Add(string2);
            return str;
        }

        public static string CombineStrings(string string1, string string2)
        {
            string result = "";

            int maxLength =Math.Max(string1.Length, string2.Length);

            for(int i = 0; i < maxLength; i++)
            {
                if(i < string1.Length)
                {
                    result += string1[i];
                }
                if(i < string2.Length)
                {
                    result += string2[i];
                }
            }

            return result;
        }


    }
}
