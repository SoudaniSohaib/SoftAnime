using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Data.SqlClient;
using System.Data;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls;
using static SoftAnime.MainWindow;
using System.Windows.Shapes;


namespace SoftAnime
{
    /// <summary>
    /// TODO :
    ///  - Email verification
    ///  - User password reset 
    ///  - to change the user password an Email is sent.
    ///  
    ///  - improve the UI 
    ///  - custom WPF if possible
    ///  
    /// </summary>

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            AnimesListBox.ItemsSource = AnimesCollection;
            TypeshowList.ItemsSource = AnimesTypesList;          
            
        }


        public class Anime 
        {
            public int animeId { get; set; }
            public string? ATitle { get; set; }
            public int Rating { get; set; }
            public List<string> TagList { get; set; }
            public int Seasonsnumber { get; set; }
            public int movienum { get; set; }
            public int Ovanum { get; set; }
            public int? Episodes { get; set; }
            public string? SMOrder { get; set; }
            public string? ImagePath { get; set; }
            public string? WatchedTypes { get; set; }


            public Anime()
            {

            }
           
        }

        public class User
        {
            public string? Username { get; set; }
            public string? PreviousUsername {  get; set; }
            public string? UserEmail { get; set; }
            private string? UserPassword { get; set; }
            private string UserImagePath;

            public string GetUserImagePath()
            {

                return strWorkPath + @"\SoftAnime\" + Username;
            }

            public void SetUserImagePath(string value)
            {
                UserImagePath = value;
            }

            public string GetUserPassword()
            {
                return UserPassword;
            }

            public void SetUserPassword(string value)
            {
                UserPassword = value;
            }

            public User()
            {
                UserImagePath = strWorkPath + @"\SoftAnime\" + Username;
            }
        }

        List<string> Tagstrings = new List<string>()
            {
                "Action","Comedy","Fantasy","Horror","School","Isekai","Magic","Military","Drama","Music","Ecchi","Romance","Mystery","Sport","Sliceoflife","Adventure","Supernatural","Superpower"
            };
        public static ObservableCollection<Anime> AnimesCollection = new();
        public static List<string> ExistingAnimesCollection = new();
        public static  List<NewAnimewindow.Item> AnimesTypesList = new List<NewAnimewindow.Item>();
        static readonly string? strWorkPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private bool Connectedtoserver = false;
        public static User? Currentuser = null;
        public static System.Windows.Controls.Image Anm;

        public void SetCurrentuser(User user)
        {
            Currentuser = user;
        }

        public User GetCurrentuser()
        {
            return Currentuser;
        }
        
        private void AddAnime_Click(object sender, RoutedEventArgs e)
        {
            if(Connectedtoserver == true) { 
                     var NewAnime = new NewAnimewindow();
                      bool? result = NewAnime.ShowDialog();

                if (result == true)
                {
                // User finished adding the anime 
                    AutoClosingMessageBox.Show("The anime is added to your list","wait...", 1000);
                }
                else
                {
                    // User cancelled the dialog box
                    AutoClosingMessageBox.Show("Nothing was added","Try again",1000);
                }
            } else
            {
                MessageBox.Show("Connect to your database first to add new animes");
            }
}

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if( Connectedtoserver == true )
            {
                int deleteitem = AnimesListBox.SelectedIndex;
                dbsqlclass del = new dbsqlclass();
                if( deleteitem != -1 )
                {
                    MainWindow.Anime anime = AnimesCollection[deleteitem];
                        AnimePicture.Visibility = Visibility.Hidden;
                        AnimePicture.Source = null;
                        if( !String.IsNullOrEmpty(strWorkPath) )
                        {
                            string[] files = Directory.GetFiles(strWorkPath + @"\SoftAnime\" + Currentuser.Username);
                            string[] spl = anime.ImagePath.Split(".");

                            foreach( string file in files )
                            {
                                string fle = System.IO.Path.GetFileName(file);
                                FileInfo df = new FileInfo(System.IO.Path.Combine(strWorkPath + @"\SoftAnime\" + Currentuser.Username, fle));
                            if( fle.Contains(spl[0].ToLower()) )
                                {
                                (new System.Threading.Thread(() =>
                                {
                                    while(true)
                                    {
                                        try
                                        {                                       
                                            File.Delete(df.FullName);
                                            break;
                                        } catch { }
                                    }
                                })).Start();

                                System.IO.File.Delete(df.FullName);
                                    break;
                                }
                            }

                            
                        }
                        del.Deletesql(anime.ATitle.ToLower());
                        AnimesCollection.RemoveAt(deleteitem);
                        ExistingAnimesCollection.Remove(anime.ATitle);
                    
                }
                AnimePicture.Visibility = Visibility.Visible;

            } else
            {
                MessageBox.Show("Connect to your database first!");
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if( Connectedtoserver == true )
            {
                if( !(AnimesCollection.Count == 0) )
                {

                    int edititem = AnimesListBox.SelectedIndex;
                    if( edititem != -1 )
                    {
                        Anm = AnimePicture;
                        var NewAnime = new NewAnimewindow();
                        NewAnime.Show();
                        AnimePicture.Source = null;
                        NewAnime.EditingAnime(edititem, Currentuser.Username);
                        if(Anm.Source != null)
                        {
                            NewAnime.ima(Anm);
                        }
                        AnimesListBox.UnselectAll();
                    }
                } else
                {
                    MessageBox.Show("Add some animes to your list to edit them later!");
                }
            } else
            {
                MessageBox.Show("Connect to your database first to edit animes");
            }
        }

        private async void DatabaseConnectList_Click(object sender, RoutedEventArgs e)
        {
            dbsqlclass fer = new dbsqlclass();
            WaitingWindow wait = new WaitingWindow();

            if(Currentuser == null)
            {
                MessageBox.Show("You must login first to Connect to your database");
                return;
            }
            
            if(fer.IsConnectedToInternet() == true && Connectedtoserver == false)
            {
                await dbsqlclass.DoesTableExist(Currentuser.Username);
                wait.Show();
                wait.WaitngConnect(Currentuser.Username);
                Connectedtoserver = true;
                DatabaseConnectList.Content = "Connected";
                DatabaseConnectList.FontWeight = FontWeights.Bold;
                DatabaseConnectList.Foreground = Brushes.Green;
                DatabaseConnectList.BorderBrush = Brushes.White;
                DatabaseConnectList.Background = Brushes.White;
                DatabaseName.Content = Currentuser.Username;

            } else if(Connectedtoserver == true)
            {
                Cleaners();
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            WaitingWindow wait = new WaitingWindow();
            if(Connectedtoserver == true)
            {
                wait.Show();
                wait.WaitingFilter(NameSearch.Text);
            } else
            {
                MessageBox.Show("Connect to your database first to search animes");
            }

        }

        public static async void Delay()
        {
            await Task.Delay(2000);

        }

        private async void SelectedAnime_FromList(object sender, RoutedEventArgs e)
        {

            int mov = 1;
            int indx = 1;
            int sea = 1;
            int ova = 1;
            int k = 0;
            int w = 0;
            StringBuilder sb = new StringBuilder();
            int SelectedAnimeIndex = AnimesListBox.SelectedIndex;   
            if (SelectedAnimeIndex != -1)
            {                
                MainWindow.Anime SelectedAnime = MainWindow.AnimesCollection[SelectedAnimeIndex];

                Animetitle.Text = SelectedAnime.ATitle;
                seasonshow.Content = SelectedAnime.Seasonsnumber;
                episodeshow.Content = SelectedAnime.Episodes;
                Ratingshow.Value = SelectedAnime.Rating;
                movieshow.Content = SelectedAnime.movienum;
                ovashow.Content = SelectedAnime.Ovanum;
                char[] Typefilter = (SelectedAnime.SMOrder).ToCharArray();
                char[] Watchedfilter = (SelectedAnime.WatchedTypes).ToCharArray();
                AnimePicture.Source = null;
                AnimesTypesList.Clear();
                for( int i = 0; i < Typefilter.Length; i++ )
                {
                    if( Typefilter[i] == 'S' )
                    {
                        NewAnimewindow.Item m = new NewAnimewindow.Item("Season " + sea, "Episodes :", indx);                     
                        m.Episodes = Convert.ToInt32(Typefilter[i + 1].ToString() + Typefilter[i + 2].ToString() + Typefilter[i + 3].ToString());
                        if( Watchedfilter[w] == 'W' )
                        {
                            m.status = "Watched";
                            m.brush = Brushes.Green;
                        } else if( Watchedfilter[w] == 'X' )
                        {
                            m.status = "Not Watched";
                            m.brush = Brushes.Red;
                        }
                        AnimesTypesList.Add(m);
                        sea++;
                        indx++;
                        i += 3;
                        w++;

                    } else if( Typefilter[i] == 'M' )
                    {
                        NewAnimewindow.Item m = new NewAnimewindow.Item("Movie " + mov, "", indx);
                        if( Watchedfilter[w] == 'W' )
                        {
                            m.status = "Watched";
                            m.brush = Brushes.Green;
                        } else if( Watchedfilter[w] == 'X' )
                        {
                            m.status = "Not Watched";
                            m.brush = Brushes.Red;
                        }
                        m.Episodes = null;
                        AnimesTypesList.Add(m);
                        mov++;
                        indx++;
                        w++;

                    } else if( Typefilter[i] == 'O' )
                    {
                        NewAnimewindow.Item m = new NewAnimewindow.Item("OVA " + ova, "", indx);
                        if( Watchedfilter[w] == 'W' )
                        {
                            m.status = "Watched";
                            m.brush = Brushes.Green;
                        } else if( Watchedfilter[w] == 'X' )
                        {
                            m.status = "Not Watched";
                            m.brush = Brushes.Red;
                        }
                        m.Episodes = null;
                        AnimesTypesList.Add(m);
                        ova++;
                        indx++;
                        w++;
                    }
                }

                CollectionViewSource.GetDefaultView(AnimesTypesList).Refresh();


                foreach( string tag in SelectedAnime.TagList )
                {
                    if( tag == Tagstrings[k] )
                    {
                        sb.Append(Tagstrings[k]+@", ");
                        k++;
                    } else if( tag == "none" )
                    {
                        k++;
                    }
                }
                string Tagstring = sb.ToString();
                string Tagshow = Tagstring.Remove(Tagstring.Length - 2, 2);
                AnimeTags.Text = Tagshow;
                string dir = strWorkPath + @"\SoftAnime\" + Currentuser.Username;
                string Prevdir = strWorkPath + @"\SoftAnime\" + Currentuser.PreviousUsername;
                if (!Directory.Exists(dir))
                {
                    if(!Directory.Exists(Prevdir))
                    {
                        Directory.CreateDirectory(strWorkPath + @"\SoftAnime\" + Currentuser.Username);
                    } else { 
                        Directory.Move(Prevdir, dir);
                    }

                    return;
                }
                string[] files = Directory.GetFiles(dir);
                string[] spl = SelectedAnime.ImagePath.Split(".");
                AnimePicture.Source = null;
                AnimePicture.Source = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src"), "noimage.png")));
                foreach( string file in files )
                {
                    string fle = System.IO.Path.GetFileName(file);
                    if( fle.Contains(spl[0].ToLower()))
                    {
                        AnimePicture.Source = BitmapFromUri(new Uri(file));
                        break;
                    }                    
                }

            } else if(SelectedAnimeIndex == -1)
            {
                Animetitle.Text = "Please, Select an anime from the list or add a new one";
                seasonshow.Content = 0;
                episodeshow.Content = 0;
                Ratingshow.Value = 10;
                movieshow.Content = 0;
                ovashow.Content = 0;
                AnimeTags.Text = string.Empty;
                AnimesTypesList.Clear();
                CollectionViewSource.GetDefaultView(AnimesTypesList).Refresh();
                AnimePicture.Source = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src"), "noimage.png")));
            }

        }

        public static ImageSource BitmapFromUri(Uri source)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = source;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void AdvSearch_Click(object sender, RoutedEventArgs e)
        {       
            if(Connectedtoserver == true )
            {          
                var AdvancedSearch = new AdvancedSearch();                
                AdvancedSearch.ShowDialog();
            } else
            {
                MessageBox.Show("Connect to your database first to search animes");
            }
        }

        private void NameSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            /// removes any special characters from the input  
            char[] one = NameSearch.Text.ToCharArray();
            if( one.Length == 0 )
            {
                return;
            }
            List<Char> two = new List<char>();
            for( int i = 0; i < one.Length; i++ )
            {
                if( Char.IsLetterOrDigit(one[i]) )
                {
                    two.Add(one[i]);
                }
            }

            string goodtext = new string(two.ToArray());
            NameSearch.Text = goodtext;
            NameSearch.Select(NameSearch.Text.Length, 0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            string header = menuItem.Header.ToString();

            switch (header) {
                case "Open":

                    break;

                default:

                    break;
            }
        }

        public void AnimeImageset(string uri)
        {
            if(uri != null || !String.IsNullOrEmpty(uri.ToString()))
            {
                AnimePicture.Source = new BitmapImage(new Uri(uri));
            }
            return;
        }

        private void UserTaskBarLogin_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menusender  = (MenuItem)sender;  
            string header = menusender.Header.ToString();

             switch (header) {

                case "Login":
                AnimesCollection.Clear();
                Connectedtoserver = false;
                LoginWindow login = new LoginWindow();
                login.Show();
                menusender.Header = "Log Off";
                Close();
                break;

                case "Log Off":
                Cleaners();
                LogOff();
                menusender.Header = "Login";

                    
                break;

                case "Settings":
                SettingsWindow settings = new SettingsWindow();
                settings.Show();               
                break;

                default:

                    break;
              }

        }

        private void Cleaners()
        {
            DatabaseName.Content = "Click to connect!";
            DatabaseConnectList.Content = "Disconnected";
            DatabaseConnectList.FontWeight = FontWeights.Bold;
            DatabaseConnectList.Foreground = Brushes.Red;
            DatabaseConnectList.BorderBrush = Brushes.White;
            DatabaseConnectList.Background = Brushes.White;
            Animetitle.Text = "Please, Select an anime from the list or add a new one";
            AnimesCollection.Clear();
            ExistingAnimesCollection.Clear();
            Connectedtoserver = false;
            Currentuser = null;
            seasonshow.Content = 0;
            episodeshow.Content = 0;
            Ratingshow.Value = 10;
            movieshow.Content = 0;
            ovashow.Content = 0;
            AnimeTags.Text = string.Empty;
            AnimesTypesList.Clear();
            CollectionViewSource.GetDefaultView(AnimesTypesList).Refresh();
            AnimePicture.Source = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src"), "noimage.png")));

        }

        private void LogOff()
        {

            AnimesCollection.Clear();
            ExistingAnimesCollection.Clear();
            Connectedtoserver = false;
            DatabaseName.Content = "Click to connect!";
            DatabaseConnectList.Content = "Disconnected";
            DatabaseConnectList.FontWeight = FontWeights.Bold;
            DatabaseConnectList.Foreground = Brushes.Red;
            DatabaseConnectList.BorderBrush = Brushes.White;
            DatabaseConnectList.Background = Brushes.White;
            AnimePicture.Source = new BitmapImage(new Uri(System.IO.Path.Combine(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "src"), "noimage.png")));
            Animetitle.Text = "Please, Select an anime from the list or add a new one";
        }


    }

    /// var watch = new System.Diagnostics.Stopwatch();
    /// watch.Start();
    /// watch.Stop();  
    /// MessageBox.Show($"Execution Time: {watch.ElapsedMilliseconds} ms"); 
    /// a timer to check performance or execution time
    
    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(callback: OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
}

