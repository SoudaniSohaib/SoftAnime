using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            Settings.ItemsSource = Settingsstring;
            SettingsContent.Content = new GeneralSettings();         
        }

        public static ObservableCollection<string> Settingsstring = new ObservableCollection<string>()
        {
            "General","User","Database"
        };

        private UserSetting userSetting;
        private DatabaseSetting databaseSetting;
        private GeneralSettings generalSettings;

        static readonly string? strWorkPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string dir = strWorkPath + @"\SoftAnime";

        private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz/";

        private void Settings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItem = (string)Settings.SelectedItem;

            // Clear the existing UI content
            SettingsContent.Content = null;
            object CurrentContent = SettingsContent.Content;    

            // Create and set the appropriate UI based on the selected item
            switch(selectedItem)
            {
                case "General":
                    if (CurrentContent is not GeneralSettings)
                {
                    SettingsContent.Content = new GeneralSettings();

                }
                break;

                case "Database":
                if(CurrentContent is not DatabaseSetting)
                {
                    SettingsContent.Content = new DatabaseSetting();                 
                }
                break;

                case "User":
                if(CurrentContent is not UserSetting)
                {
                    UserSetting usr = new UserSetting();
                    SettingsContent.Content = usr;
                    userSetting = usr;
                    usr.SettingCurrentUserSetting(MainWindow.Currentuser);
                }
                break;              
            }
        }

        public void SettingsWindowChanger(string windowname)
        {
            SettingsContent.Content = null;
            object CurrentContent = SettingsContent.Content;

            switch(windowname)
            {
                case "General":
                if(CurrentContent is not GeneralSettings)
                {
                    SettingsContent.Content = new GeneralSettings();

                }
                break;

                case "Database":
                if(CurrentContent is not DatabaseSetting)
                {
                    SettingsContent.Content = new DatabaseSetting();
                }
                break;

                case "User":
                if(CurrentContent is not UserSetting)
                {
                    UserSetting usr = new UserSetting();
                    SettingsContent.Content = usr;
                    userSetting = usr;
                    usr.SettingCurrentUserSetting(MainWindow.Currentuser);

                }
                break;
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {

            if(MainWindow.Currentuser != null)
            {
                dbsqlclass dbs = new dbsqlclass();
                int idtask = await dbs.GetUserID(MainWindow.Currentuser.UserEmail);

                if(idtask == -1)
                {
                    MessageBox.Show("Something Went wrong! this user must signup again");
                    return;
                }
                UserSetting.UserSettingItem settingItem = userSetting.Settingholder(MainWindow.Currentuser.Username);
                if(settingItem != null)
                {
                    WaitingWindow waitUpdate = new WaitingWindow();

                    string rnd = SignUpWindow.GenerateRandomCharacters(16);
                    waitUpdate.Show();
                    string task = await waitUpdate.WaitUserUpdate("Your settings are being saved!", settingItem, rnd, idtask);                  
                    if(task != null)
                    {
                        if(task == "changed")
                        {

                            string prvusername = UserDataChange(settingItem, rnd);

                            dbs.AlterTableName(prvusername,settingItem.Username);

                            RenameFolderAsync(settingItem.Username, settingItem.PreviousUsername);

                            MainWindow.Currentuser.Username = settingItem.Username;
                            MainWindow.Currentuser.SetUserPassword(DatabaseSetting.CombineStrings(settingItem.GetUserSettingPassword(),rnd));
                            MainWindow.Currentuser.UserEmail = settingItem.GetUserSettingEmail();
                            if(!String.IsNullOrEmpty(prvusername))
                            {
                                MainWindow.Currentuser.PreviousUsername = settingItem.PreviousUsername;
                                MainWindow.Currentuser.SetUserImagePath(settingItem.PreviousUsername);
                            }
                            Close();
                            return;
                        } else if(task == "Error")
                        {
                            MessageBox.Show("Something went wrong! Try again");
                            return;
                        }
                    }
                    return;
                }

            }
        }

        public string UserDataChange(UserSetting.UserSettingItem usedata, string rnd)
        {
            string tempFilePath = System.IO.Path.GetTempFileName(); // Create a temporary file to write the modified content
            string filePath = dir + @"\usersdata.txt";
            string prviousname ="";
            try
            {
                using(StreamReader reader = new StreamReader(filePath))
                using(StreamWriter writer = new StreamWriter(tempFilePath))
                {
                    string line1, line2, line3, line4;

                    while((line1 = reader.ReadLine()) != null &&
                           (line2 = reader.ReadLine()) != null &&
                           (line3 = reader.ReadLine()) != null &&
                           (line4 = reader.ReadLine()) != null)
                    {
                        // Perform checks on the four lines
                        bool shouldReplace = CheckLines(line1,usedata.PreviousUsername, usedata.Username);

                        if(shouldReplace)
                        {
                            // Replace the lines without leaving them blank

                                ReplaceLines(ref line1, ref line2, ref line3, ref line4, usedata, rnd);
                                prviousname = line4;
                            

                        }

                        // Write the lines to the temporary file
                        writer.WriteLine(line1);
                        writer.WriteLine(line2);
                        writer.WriteLine(line3);
                        writer.WriteLine(line4);

                    }
                }

                // Replace the original file with the modified content
                File.Delete(filePath);
                File.Move(tempFilePath, filePath);
            } catch(Exception ex)
            {
                // Handle any exceptions that may occur during the process
                Console.WriteLine("An error occurred: " + ex.Message);
            } finally
            {
                // Clean up the temporary file if it exists
                if(File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            return prviousname;
        }

        private void ExitSettings_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public async Task RenameFolderAsync(string NewFileName, string PreviousFileName)
        {
            if(NewFileName == PreviousFileName)
            {
                return;
            }
            try
            {
                string previousFolderPath = System.IO.Path.Combine(dir, PreviousFileName);
                string newFolderPath = System.IO.Path.Combine(dir, NewFileName);


                if(Directory.Exists(previousFolderPath))
                {
                    await Task.Run(() => Directory.Move(previousFolderPath, newFolderPath));
                } else
                {
                    Console.WriteLine("The specified folder does not exist.");
                }
            } catch(Exception ex)
            {
                Console.WriteLine($"An error occurred while renaming the folder: {ex.Message}");
            }
        }


        private bool CheckLines(string line1, string previoususername, string username)
    {
        // Perform your checks on the four lines here
        // Return true if the checks are true, otherwise return false
        // Example checks: line1.Contains("abc"), line2.StartsWith("123"), etc.

        if (line1 == previoususername || line1 == username)
            {
                return true;
            }


        return false;
    }

    private void ReplaceLines(ref string line1, ref string line2, ref string line3, ref string line4, UserSetting.UserSettingItem usi, string rnd)
    {
            // Replace the lines here without leaving them blank
            // Example replacement: line1 = "new line 1", line2 = "new line 2", etc.
            line1 = usi.Username;
            line2 = dbsqlclass.Encrypt(usi.GetUserSettingEmail(),rnd);
            line3 = DatabaseSetting.CombineStrings(dbsqlclass.Encrypt(usi.GetUserSettingPassword(), rnd), rnd);
            if(usi.Username != usi.PreviousUsername)
            {
                line4 = usi.PreviousUsername;
            }
    }

}
}
