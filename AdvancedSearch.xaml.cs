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
    /// Interaction logic for AdvancedSearch.xaml
    /// </summary>
    public partial class AdvancedSearch : Window
    {
        public AdvancedSearch()
        {
            InitializeComponent();
        }
        bool minSeasons = true;
        bool maxSeasons = false;
        bool minEpisodes = true;
        bool maxEpisodes = false;
        bool minMovies = true;
        bool maxMovies = false;
        bool minOva = true;
        bool maxOva = false;


        private async void Searchfilter_Click(object sender, RoutedEventArgs e)
        {          
            StringBuilder SqlRequest = new StringBuilder();
            WaitingWindow wait = new WaitingWindow();
            if(DataVerify() == false)
            {
                return;
            }
            
            SqlRequest.Append("Select * From "+ MainWindow.Currentuser.Username + " where " + FormCollector());
            wait.Show();
            DialogResult = true;
            wait.WaitingFilter(SqlRequest.ToString(),SearchTitle.Text);
        }

        private bool DataVerify()
        {
            List<TextBox> textboxes = new List<TextBox>()
            {
               SearchSeason, SearchEpisode, SearchMovies, SearchOva
            };
            List<bool> minmaxbool = new List<bool>()
            {
                maxSeasons,maxEpisodes,maxMovies,maxOva
            };
            List<Label> labels = new List<Label>()
            {
                Seasonlabel,Episodelabel,Movieslabel,Ovalabel
            };
            int i = 0;
            bool Vrf = true;
            foreach(TextBox tct in textboxes)
            {               
                if( String.Equals(tct.Text,"0") && minmaxbool[i] )
                {
                    MessageBox.Show("Enter a valid value or change Min/Max setting!");                  
                    Vrf = false;
                    Delay(tct, labels[i]);
                    return Vrf;                   
                } else
                {
                    Vrf = true;
                }
                i++;
            }
            return Vrf;
        }

        private string FormCollector()
        {
            StringBuilder RequestString = new StringBuilder();
            bool first = true;
            List<TextBox> textboxes = new List<TextBox>()
            {
                SearchTitle, SearchSeason, SearchEpisode, SearchMovies, SearchOva,SearchRating
            };
            List<bool> minmaxbool = new List<bool>()
            {
                minSeasons,maxSeasons,minEpisodes,maxEpisodes,minMovies,maxMovies,minOva,maxOva,true,true
            };
            List<string> colomuns = new List<string>()
            {
                "Seasons","episodes","Movies","Ova","Rating"
            };
            int i = 0;
            int k = 0;
            int t = 0;
            foreach(TextBox txt in textboxes)
            {
                
                    if( t == 0 )
                    {
                         if(!String.IsNullOrWhiteSpace(txt.Text) )
                         {
                             RequestString.Append(@" Name LIKE '%@Name%'");
                             first = false;
                             t++;
                         } else
                          {

                            t++;
                          }
                    } else
                    {
                         if(!String.IsNullOrWhiteSpace(txt.Text) )
                         {
                            RequestString.Append(Easy(txt, minmaxbool[k], minmaxbool[k+1], colomuns[i], first));
                            first = false;
                         i++;
                         k += 2;
                         }
                    }
            }
            if(SearchWatched.IsChecked == true)
            {
                RequestString.Append(" AND Watched LIKE '%X%'");
            }
            RequestString.Append(TagSearch());
            
            RequestString.Append(';');

            return RequestString.ToString();
        }

        private string Easy(TextBox txtbox,bool minstatus,bool maxstatus,string ColumnName,bool andstatus)
        {          
            StringBuilder RS = new StringBuilder();
            if(andstatus == false)
            {
                RS.Append(@" AND");
            }
            if(!String.IsNullOrWhiteSpace(txtbox.Text)) {
                if( minstatus && !maxstatus)
                {
                    RS.Append(@" " + ColumnName + @" >= " + txtbox.Text);
                } else if( maxstatus && !minstatus)
                {
                    RS.Append(@" " + ColumnName + @" <= " + txtbox.Text);
                } else if (minstatus && maxstatus )
                {
                    
                    RS.Append(@" " + ColumnName + @" = " + txtbox.Text);
                }
            } else
            {
                return "";
            }
            return RS.ToString();
        }

        public static async void Delay(TextBox box, Label label)
        {
            box.BorderBrush = Brushes.Red;
            label.Foreground = Brushes.Red;
            await Task.Delay(5000);
            box.BorderBrush = Brushes.Gray;
            label.Foreground = Brushes.Black;
        }

        private void SearchSeason_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TxT = sender as TextBox;
            char[] one = TxT.Text.ToCharArray();
            if( one.Length == 0 )
            {
                return;
            }
            List<Char> two = new List<char>();
            for( int i = 0; i < one.Length; i++ )
            {
                if( Char.IsDigit(one[i]) )
                {
                    two.Add(one[i]);
                }
            }
            string goodtext = new string(two.ToArray());
            TxT.Text = goodtext;
            TxT.Select(TxT.Text.Length, 0);
        }

        private void SearchRating_TextChanged(object sender, TextChangedEventArgs e)
        {
            if( int.TryParse(SearchRating.Text, out int result) )
            {
                if( result > 10 )
                {
                    SearchRating.Text = "10";
                    SearchRating.SelectAll();
                    SearchRating.Focus();
                } else if( result < 0 )
                {
                    SearchRating.Text = "0";
                    SearchRating.SelectAll();
                    SearchRating.Focus();
                } else
                {
                    StarSearch.Value = result;

                }
            } else
            {
                SearchRating.Text = "0";
                SearchRating.SelectAll();
                SearchRating.Focus();
            }
        }

        private void SearchTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            char[] one = SearchTitle.Text.ToCharArray();
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
            SearchTitle.Text = goodtext;
            SearchTitle.Select(SearchTitle.Text.Length, 0);
        }

        private void AvancedSearchbox_KeyDown(object sender, KeyEventArgs e)
        {
            CheckBox c1 = sender as CheckBox;
            if( e.Key == System.Windows.Input.Key.Enter )
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

        private void MinMax_Click(object sender, RoutedEventArgs e)
        {
            Button minmax = sender as Button;           
            switch( minmax.Name )
            {
                case "MinSeason":
                minSeasons = true;
                maxSeasons = false;
                minmax.Background = Brushes.LimeGreen;
                Button mis = (Button)FindName("MaxSeason");
                mis.Background = Brushes.LightGray;
                break;

                case "MaxSeason":
                minSeasons = false;
                maxSeasons = true;
                minmax.Background = Brushes.LimeGreen;
                Button mas = (Button)FindName("MinSeason");
                mas.Background = Brushes.LightGray;
                break;

                case "MinEpisodes":
                minEpisodes = true;
                maxEpisodes = false;
                minmax.Background = Brushes.LimeGreen;
                Button mie = (Button)FindName("MaxEpisodes");
                mie.Background = Brushes.LightGray;
                break;

                case "MaxEpisodes":
                minEpisodes = false;
                maxEpisodes = true;
                minmax.Background = Brushes.LimeGreen;
                Button mae = (Button)FindName("MinEpisodes");
                mae.Background = Brushes.LightGray;
                break;

                case "MinMovies":
                minMovies = true;
                maxMovies = false;
                minmax.Background = Brushes.LimeGreen;
                Button mim = (Button)FindName("MaxMovies");
                mim.Background = Brushes.LightGray;
                break;

                case "MaxMovies":
                minMovies = false;
                maxMovies = true;
                minmax.Background = Brushes.LimeGreen;
                Button mam = (Button)FindName("MinMovies");
                mam.Background = Brushes.LightGray;
                break;

                case "MinOva":
                minOva = true;
                maxOva = false;
                minmax.Background = Brushes.LimeGreen;
                Button mao = (Button)FindName("MaxOva");
                mao.Background = Brushes.LightGray;
                break;

                case "MaxOva":
                minOva = false;
                maxOva = true;
                minmax.Background = Brushes.LimeGreen;
                Button mio = (Button)FindName("MinOva");
                mio.Background = Brushes.LightGray;
                break;

                default:
                MessageBox.Show("Something went wrong!");
                break;
            }
        }
    
        private string TagSearch()
        {
            List<CheckBox> checkBoxes = new List<CheckBox>()
            {
              ActionSearch, ComedySearch, FantasySearch, HorrorSearch, SchoolSearch, IsekaiSearch, MagicSearch, MilitarySearch, DramaSearch, MusicSearch, EcchiSearch, RomanceSearch, MysterySearch, SportsSearch, SliceoflifeSearch, adventureSearch, SupernaturalSearch, superpowerSearch
            };
            List<string> Tagstrings = new List<string>()
            {
                "Action","Comedy","Fantasy","Horror","School","Isekai","Magic","Military","Drama","Music","Ecchi","Romance","Mystery","Sport","Sliceoflife","Adventure","Supernatural","Superpower"
            };
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < checkBoxes.Count; i++)
            {
                CheckBox checkBox = checkBoxes[i];
                if(checkBox.IsChecked == true)
                {
                    sb.Append(@" AND " + Tagstrings[i] + @"='yes'");
                }
            }

            return sb.ToString();
        }

        private async void ResetToggle_Click(object sender, RoutedEventArgs e)
        {
            dbsqlclass AdvFilter = new dbsqlclass();
            WaitingWindow wait = new WaitingWindow();   
            DialogResult = true;
            wait.Show();
            wait.WaitngConnect(MainWindow.Currentuser.Username);
        }
    }
}
