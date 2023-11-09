using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftAnime
{
    /// <summary>
    /// Interaction logic for NewAnimewindow.xaml
    /// </summary>
    public partial class NewAnimewindow : Window
    {
        public NewAnimewindow()
        {
            InitializeComponent();
            SeasonsListBox.ItemsSource = TypesCollection;

        }

        private bool editstatus = false;
        private int editanimeindex;
        private int? episodesnumber = 0;
        private int Seasonsnumber = 0;
        private int Movienumber = 0;
        private int Ovanumber = 0;

        private string filepath = string.Empty;
        private string imageaccessfile = string.Empty;
        private string destfolder = string.Empty;
        private bool ImageselectStatus = false;

        List<string> Tagstrings = new List<string>()
            {
                "Action","Comedy","Fantasy","Horror","School","Isekai","Magic","Military","Drama","Music","Ecchi","Romance","Mystery","Sport","Sliceoflife","Adventure","Supernatural","Superpower"
            };
        readonly string? strWorkPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public Image image; 


        private async void Closingnewanime_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Anime anim = new MainWindow.Anime();
            dbsqlclass sqlconnection = new dbsqlclass();
            WaitingWindow wait = new WaitingWindow();

            if (FormsChecker() == true )
            {
                anim.ATitle = titlebox.Text;
                anim.Rating = Convert.ToInt32(ratingbox.Text);
                anim.TagList = TagListMaker();
                anim.ImagePath = ImageSaving();
                anim.SMOrder = SeasonsString();
                anim.WatchedTypes = WatchedString();
                anim.Episodes = episodesnumber;
                anim.Seasonsnumber = Seasonsnumber;
                anim.movienum = Movienumber;
                anim.Ovanum = Ovanumber;

                if(editstatus == false )
                {
                    MainWindow.AnimesCollection.Add(anim);
                    MainWindow.ExistingAnimesCollection.Add(anim.ATitle);
                    sqlconnection.InsertMysql(anim);
                    Cleaners();
                    DialogResult = true;
                } else if (editstatus == true )
                {
                    wait.Show();
                    wait.WaitingUpdate(anim,"Please wait for your anime to be edited...");
                    MainWindow.AnimesCollection.RemoveAt(editanimeindex);
                    MainWindow.AnimesCollection.Add(anim);
                    Cleaners();
                    Close();
                }
            }
        }

        private string WatchedString()
        {
            StringBuilder wt = new StringBuilder();
            foreach(Item typ in TypesCollection)
            {
                if (typ.status == "Watched")
                {
                    wt.Append("W");
                   
                } else if (typ.status == "Not Watched")
                {
                    wt.Append("X");
                }

            }
            return wt.ToString();
        }



        private void ImageRemove_Click(object sender, RoutedEventArgs e)
        {
            AnimeImage.Source = null;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Cleaners();
        }

        public void ima(Image img)
        {
          img.Source = AnimeImage.Source;
        }


        public void EditingAnime(int editindex,string Username)
        {
            mov = 1;
            indx = 1;
            sea = 1;
            ova = 1;
            int k = 0;
            int w = 0;
            List<CheckBox> Tagboxes = new List<CheckBox>()
            {
                Actionbox, Comedybox, Fantasybox, Horrorbox, Schoolbox, Isekaibox, Magicbox, Militarybox, Dramabox, Musicbox, Ecchibox, Romancebox, Mysterybox, Sportbox, Sliceoflifebox, adventurebox, Supernaturalbox, superpowerbox
            };

            MainWindow.Anime Editanime = MainWindow.AnimesCollection.ElementAt(editindex);
            editanimeindex = editindex;
            titlebox.Text = Editanime.ATitle;
            titlebox.IsReadOnly = true;
            ratingbox.Text = Editanime.Rating.ToString();
            ratings0.Value = Editanime.Rating;

            if(Editanime.ImagePath != null)
            {
                string[] files = Directory.GetFiles(strWorkPath + @"\SoftAnime\" + Username);
                string[] spl = Editanime.ImagePath.Split(".");

                foreach( string file in files )
                {
                    string fle = Path.GetFileName(file);
                    if( fle.Contains(spl[0].ToLower()) )
                    {
                        AnimeImage.Source = new BitmapImage(new Uri(file));
                        break;
                    }
                }
                imageaccessfile = Editanime.ImagePath;
            }

            char[] Typefilter = (Editanime.SMOrder).ToCharArray();
            char[] Watchedfilter = (Editanime.WatchedTypes).ToCharArray();
            CollectionViewSource.GetDefaultView(TypesCollection).Refresh();
            foreach( string tag in Editanime.TagList )
            {
                if( tag == Tagstrings[k] )
                {
                    Tagboxes[k].IsChecked = true;
                    k++;
                } else if( tag == "none" )
                {
                    Tagboxes[k].IsChecked = false;
                    k++;
                }
            }
            TypesCollection.Clear();
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
                    TypesCollection.Add(m);
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
                    TypesCollection.Add(m);
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
                    TypesCollection.Add(m);
                    ova++;
                    indx++;
                    w++;
                }
            }
            CollectionViewSource.GetDefaultView(TypesCollection).Refresh();
            editstatus = true;
        }

        public bool ErrorM(System.Windows.Controls.TextBox Box, System.Windows.Controls.Label label, string role)
        {
            bool Check;
            int num;
            if (Box.Text.Length != 0)
            {
                num = Convert.ToInt32(Box.Text);
                if (num > 0)
                {
                    Check = false;
                }
                else
                {
                    Box.BorderBrush = Brushes.Red;
                    label.Foreground = Brushes.Red;
                    Check = true;
                    AutoClosingMessageBox.Show("Number of " + role + " can't be negative!","Error",1500);
                    Delay(Box, label);
                }

            }
            else
            {
                Box.BorderBrush = Brushes.Red;
                label.Foreground = Brushes.Red;
                Check = true;
                AutoClosingMessageBox.Show("Please, enter a value!", "Error", 1500);
                Delay(Box, label);
            }

            return Check;
        }

        private bool FormsChecker()
        {
            dbsqlclass test = new dbsqlclass();
            int tags = 0;

            foreach( string tag in TagListMaker() )
            {
                if( tag != "none" )
                {
                    tags++;
                }
            }

            if( String.IsNullOrEmpty(titlebox.Text) || titlebox.Text == "title" )
            {
                titlebox.BorderBrush = Brushes.Red;
                AutoClosingMessageBox.Show("Name your anime!", "No Title", 1500);
                ColorDelay(titlebox);
                return false;
            } else
            {

                if( MainWindow.ExistingAnimesCollection.Contains(titlebox.Text) && editstatus == false )
                {
                    titlebox.BorderBrush = Brushes.Red;
                    AutoClosingMessageBox.Show("This anime already exists!", "Already Exists", 1500);
                    ColorDelay(titlebox);
                    return false;

                }
            }

            if( imageaccessfile == string.Empty )
            {
                AutoClosingMessageBox.Show("Add a picture to your anime!", "No Image", 1500);
                return false;
            }


            if( ratingbox.Text == "" )
            {
                ratingbox.BorderBrush = Brushes.Red;
                AutoClosingMessageBox.Show("Rate your anime!", "No Rating", 1500);
                ColorDelay(ratingbox);
                return false;

            }

            if( SeasonsListBox.HasItems == false )
            {
                EpNumber.BorderBrush = Brushes.Red;
                AutoClosingMessageBox.Show("Add some Seasons", "No Seasons", 1500);
                ColorDelay(EpNumber);
                return false;
            }

            if( tags == 0 )
            {
                AutoClosingMessageBox.Show("Add some Tags to your anime!", "No Tag", 1500);
                return false;
            }

            if( test.IsConnectedToInternet() == true )
            {
                return true;
            } else
            {
                AutoClosingMessageBox.Show("Check your internet connection and try again!", "No Internet!", 1500);
                return false;
            }
        }

        private void Cleaners()
        {
            TypesCollection.Clear();
            CollectionViewSource.GetDefaultView(TypesCollection).Refresh();
            imageaccessfile = string.Empty;
            episodesnumber = 0;
            Seasonsnumber = 0;
            Movienumber = 0;
            Ovanumber = 0;
            mov = 1;
            indx = 1;
            sea = 1;
            ova = 1;
            editstatus = false;
            ImageselectStatus = false;
        }

        private void ImageSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.jfif|" +
              "JPEG (*.jpg;*.jpeg;*.jfif)|*.jpg;*.jpeg;|" +
              "Portable Network Graphic (*.png)|*.png";
            try
            {
                if((bool)op.ShowDialog())
                {
                    if( ValidFile(op.FileName, 1024000, 2000, 3000) )
                    {                       
                        filepath = op.FileName;
                        if( strWorkPath != null ) destfolder = Path.Combine(strWorkPath + @"\Animepicture", MainWindow.Currentuser.Username);
                        if( !Directory.Exists(destfolder) )
                        {
                            System.IO.Directory.CreateDirectory(destfolder);
                        }
                        imageaccessfile = Path.Combine(destfolder, op.SafeFileName);
                        AnimeImage.Source = MainWindow.BitmapFromUri(new Uri(op.FileName));
                        ImageselectStatus = true;

                    } else
                    {
                        MessageBox.Show("Image is too big (Max 1MB) or larger than 2000x3000 \nThe best ratio is: 500x700");
                    }
                }
            } catch
            {
                MessageBox.Show("Something went wrong!");
            }
        }

        private bool ValidFile(string filename, long limitInBytes, int limitWidth, int limitHeight)
        {
            var fileSizeInBytes = new FileInfo(filename).Length;
            if( fileSizeInBytes > limitInBytes ) return false;

            using( var img = new System.Drawing.Bitmap(filename) )
            {
                if( img.Width > limitWidth || img.Height > limitHeight ) return false;
            }

            return true;
        }

        private string ImageSaving()
        {
            string imgname = "";
            if( ImageselectStatus )
            {
                

                System.IO.FileInfo fi1 = new System.IO.FileInfo(filepath);           
                if( strWorkPath != null )
                {
                    System.IO.FileInfo renamed = new System.IO.FileInfo(strWorkPath + @"\SoftAnime\" + MainWindow.Currentuser.Username + @"\" + titlebox.Text.ToLower() + fi1.Extension);

                    string[] files = Directory.GetFiles(strWorkPath + @"\SoftAnime\" + MainWindow.Currentuser.Username);

                    foreach( string file in files )
                    {
                        string fle = Path.GetFileName(file);
                        if(fle.Contains(titlebox.Text.ToLower()) )
                        {
                            DeleteFile(file);
                            break;
                        }
                    }                  
                    if(!renamed.Exists)
                    {
                        fi1.CopyTo(renamed.FullName);
                    }
                    
                    imgname = titlebox.Text.ToLower() + fi1.Extension;                  
                } 
            } else
            {
                          
                imgname = imageaccessfile;

            }
            return imgname;
        }

        public static void DeleteFile(string path)
        {
            if( !File.Exists(path) )
            {
                return;
            }

            bool isDeleted = false;
            while( !isDeleted )
            {
                try
                {
                    System.GC.Collect();
                    System.GC.WaitForPendingFinalizers();
                    System.IO.File.Delete(path);
                    isDeleted = true;
                } catch(Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                }
                Thread.Sleep(50);
            }
        }

        private List<string> TagListMaker()
        {
            List<string> animetags = new List<string>();

            List<CheckBox> Tagboxes = new List<CheckBox>()
            {
                Actionbox, Comedybox, Fantasybox, Horrorbox, Schoolbox, Isekaibox, Magicbox, Militarybox, Dramabox, Musicbox, Ecchibox, Romancebox, Mysterybox, Sportbox, Sliceoflifebox, adventurebox, Supernaturalbox, superpowerbox
            };
            List<string> Tagstrings = new List<string>()
            {
                "Action","Comedy","Fantasy","Horror","School","Isekai","Magic","Military","Drama","Music","Ecchi","Romance","Mystery","Sport","Sliceoflife","Adventure","Supernatural","Superpower"
            };
            int i = 0;
            foreach( CheckBox Tagbox in Tagboxes )
            {

                if( Tagbox.IsChecked == true )
                {
                    animetags.Add(Tagstrings[i]);
                    i++;
                } else
                {
                    animetags.Add("none");
                    i++;
                }
            }

            return animetags;
        }


        public static async void Delay(TextBox box,Label label)
        {
            await Task.Delay(5000);
            box.BorderBrush = Brushes.Black;
            label.Foreground = Brushes.Black;
        }
        

        public static async void ColorDelay(TextBox box)
        {
            await Task.Delay(5000);
            box.BorderBrush = Brushes.Black;
        }

        public class Item
        {
            public string? Type { get; set; }
            public int? Episodes { get; set; }
            public int Index { get; set; }
            public string? epnum { get; set; }
            public string? status { get; set; }

            public Brush? brush { get; set; }

            public Item(string? type, string? epnu, int index)
            {
                Type = type;
                epnum = epnu;
                Index = index;
            }

        }

        public static ObservableCollection<Item> TypesCollection = new();

        int indx = 1;
        int sea = 1;
        int ova = 1;
        int mov = 1;
        

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            Button plus = (Button)sender;
            string name = plus.Name;
            if (name == "Splus")
            {
                if (ErrorM(EpNumber, Epbox,"Episodes")== false) {
                    Item m = new Item("Season " + sea, "Episodes :",indx);
                    m.Episodes = Convert.ToInt32(EpNumber.Text);
                    m.status = "Not Watched";
                    m.brush = Brushes.Red;
                    TypesCollection.Add(m);
                    sea++;
                    indx++;
                }

            } else if (name == "Mplus")
            {
                Item m = new Item("Movie "+mov,"", indx);
                m.status = "Not Watched";
                m.Episodes = null;
                m.brush = Brushes.Red;
                TypesCollection.Add(m);
                mov++;
                indx++;

            } else if (name == "Ovaplus")
            {
                Item m = new Item("OVA " + ova,"",indx);
                m.status = "Not Watched";
                m.Episodes = null;
                m.brush = Brushes.Red;
                TypesCollection.Add(m);
                ova++;
                indx++;
            }
            CollectionViewSource.GetDefaultView(TypesCollection).Refresh();
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            Button clicker = (Button)sender;
            
                if (clicker.Name == "Sminus")
                {
                if (sea > 1)
                {
                    Deletefrombelow("Season ", TypesCollection);
                    sea--;
                } else return;

                } else if (clicker.Name == "Mminus")
                {
                if (mov > 1)
                {
                    Deletefrombelow("Movie ", TypesCollection);
                    mov--;
                }
                else return;
                } else if (clicker.Name == "Ovaminus")
                {
                if (ova > 1)
                {
                    Deletefrombelow("OVA ", TypesCollection);
                    ova--;
                } else return;
                } else { MessageBox.Show("You can't delete this "); }
            }

        public void Deletefrombelow(string type,ObservableCollection<Item> collection)
        {
            if (collection.Count() > 0)
            {
                for (int i = collection.Count(); i >= 0; i--)
                {
                    Item m = collection[i - 1];
                    if (m.Type.Contains(type) == true)
                    {
                        TypesCollection.RemoveAt(i - 1);
                        indx--;
                        break;
                    }
                }
                for (int t = 1; t <= collection.Count(); t++)
                {
                    Item im = collection[t - 1];
                    im.Index = t;

                }
                CollectionViewSource.GetDefaultView(collection).Refresh();
            }
        }

        private string SeasonsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Item it in TypesCollection)
            {
                if (it.Type.Contains("Season ") == true)
                {
                    if (it.Episodes < 100)
                    {
                        sb.Append("S" + "0" +it.Episodes);
                        episodesnumber += it.Episodes;
                        Seasonsnumber++;
                    } else {
                        sb.Append("S" + it.Episodes);
                        episodesnumber += it.Episodes;
                        Seasonsnumber++;
                    }
                }else if (it.Type.Contains("Movie ") == true)
                {
                    sb.Append("M");
                    Movienumber++;
                } else if (it.Type.Contains("OVA ") == true)
                {
                    sb.Append("O");
                    Ovanumber++;
                }              
            }
            return sb.ToString();
        }


        private void Watchedbox_Checked(object sender, RoutedEventArgs e)
        {
                foreach (Item i in SeasonsListBox.SelectedItems)
                {
                    if (i.status == "Watched")
                    {
                        i.status = "Not Watched";
                        i.brush = Brushes.Red;
                    }
                    else if (i.status == "Not Watched")
                    {
                        i.status = "Watched";
                        i.brush = Brushes.Green;

                    }
                    
                    CollectionViewSource.GetDefaultView(TypesCollection).Refresh();
                }
                SeasonsListBox.UnselectAll();
        }


        private void Checkbox_Enter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            CheckBox c1 = sender as CheckBox;
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                if( c1.IsChecked == true )
                {
                    c1.IsChecked = false;

                } else if( c1.IsChecked == false )
                {
                    c1.IsChecked = true;

                }
            }
        }

        private void titlebox_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] one = titlebox.Text.ToCharArray();
            if( one.Length == 0 || one.Length >= 120)
            {
                return;
            }
            List<Char> two = new List<char>();          
            for( int i = 0; i < one.Length; i++ )
            {
                if( one[i] == ' ' ) { 
                    two.Add(' ');
                    continue;
                } 
                if( Char.IsLetterOrDigit(one[i]))
                {
                    two.Add(one[i]);                  
                }
            }
            
            string goodtext = new string(two.ToArray());
            titlebox.Text = goodtext;
            titlebox.Select(titlebox.Text.Length, 0);
        }

        private void Ratingbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if( int.TryParse(ratingbox.Text, out int result) )
            {
                if( result > 10 )
                {
                    ratingbox.Text = "10";
                    ratingbox.SelectAll();
                    ratingbox.Focus();
                } else if( result <= 0 )
                {
                    ratingbox.Text = "1";
                    ratingbox.SelectAll();
                    ratingbox.Focus();
                } else
                {
                    ratings0.Value = result;

                }
            } else
            {
                ratingbox.Text = "1";
                ratingbox.SelectAll();
                ratingbox.Focus();
            }
        }

        private void EpNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] one = EpNumber.Text.ToCharArray();
            if( one.Length == 0 )
            {
                return;
            }
            List<Char> two = new List<char>();
            for( int i = 0; i < one.Length; i++ )
            {
                if(Char.IsDigit(one[i]))
                {
                    two.Add(one[i]);
                }
            }
            string goodtext = new string(two.ToArray());
            EpNumber.Text = goodtext;
            EpNumber.Select(EpNumber.Text.Length, 0);
        }
    }
}

