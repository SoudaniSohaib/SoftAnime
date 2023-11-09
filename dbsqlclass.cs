using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Configuration;
using System.Windows;
using System.Net.Http;
using System.IO;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;

namespace SoftAnime
{
    internal class dbsqlclass
    {
        private static readonly string AdminUserConnectionString = ConfigurationManager.ConnectionStrings["AdminConnect"].ConnectionString;
        private MySqlConnection? cnn;
        private MySqlCommand? command;
        HttpClient client = new HttpClient();
        List<string> Tagstrings = new List<string>()
            {
                "Action","Comedy","Fantasy","Horror","School","Isekai","Magic","Military","Drama","Music","Ecchi","Romance","Mystery","Sport","Sliceoflife","Adventure","Supernatural","Superpower"
            };

        public void Alltable()
        {

            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
            string sql = "Select * from animestable";
            command = new MySqlCommand(sql, cnn);
            cnn.Open();
            MySqlDataReader reader = command.ExecuteReader();

            while( reader.Read() )
            {
                int k = 0;
                List<string> list = new List<string>();
                MainWindow.Anime an = new MainWindow.Anime();
                an.ATitle = reader.GetValue(0).ToString();
                an.Rating = Convert.ToInt32(reader.GetValue(1));
                an.Seasonsnumber = Convert.ToInt32(reader.GetValue(2));
                an.movienum = Convert.ToInt32(reader.GetValue(3));
                an.Ovanum = Convert.ToInt32(reader.GetValue(4));
                an.Episodes = Convert.ToInt32(reader.GetValue(5));
                an.SMOrder = reader.GetValue(6).ToString();
                an.WatchedTypes = reader.GetValue(7).ToString();
                an.ImagePath = reader.GetValue(8).ToString();

                for( int i = 9; i < 27; i++ )
                {
                    if( reader.GetValue(i).ToString().Contains("yes") )
                    {
                        list.Add(Tagstrings[k]);
                    } else if( reader.GetValue(i).ToString().Contains("no") )
                    {
                        list.Add("none");
                    }
                    k++;
                }
                an.TagList = list;
                foreach( string gte in an.TagList )
                {
                    MessageBox.Show("TAGLIST :   " + gte);
                }
                MainWindow.AnimesCollection.Add(an);
            }
        }

        public async Task<bool> AlterTableName(string previous, string username)
        {
            using(MySqlConnection connection = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1])))
            {
                await connection.OpenAsync();

                string query = $"ALTER TABLE {previous} RENAME TO {username};";

                MySqlCommand command = new MySqlCommand(query, connection);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
        }

        public static async Task<string> DoesTableExist(string username)
        {
            using(MySqlConnection connection = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1])))
            {
                await connection.OpenAsync();

                string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{username}';";
                MySqlCommand command = new MySqlCommand(query, connection);

                long count = (long)await command.ExecuteScalarAsync();
                

                if(count == 0)
                {
                    // Table doesn't exist, create it
                    query = $"CREATE TABLE {username} (Name varchar(255) NOT NULL,  Rating int NOT NULL,  Seasons int,Movies int,Ova int,episodes int NOT NULL, SMO varchar(255),Watched varchar(255),ImagePath varchar(255),Action varchar(255) DEFAULT 'no',Comedy varchar(255) DEFAULT 'no',Fantasy varchar(255) DEFAULT 'no',Horror varchar(255) DEFAULT 'no',School varchar(255) DEFAULT 'no',Isekai varchar(255) DEFAULT 'no',Magic varchar(255) DEFAULT 'no',Military varchar(255) DEFAULT 'no', Drama varchar(255) DEFAULT 'no',Music varchar(255) DEFAULT 'no',Ecchi varchar(255) DEFAULT 'no', Romance varchar(255) DEFAULT 'no',Mystery varchar(255) DEFAULT 'no',Sport varchar(255) DEFAULT 'no', Sliceoflife varchar(255) DEFAULT 'no', Adventure varchar(255) DEFAULT 'no',Supernatural varchar(255) DEFAULT 'no', Superpower varchar(255) DEFAULT 'no');";
                    command = new MySqlCommand(query, connection);

                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();

                    return "Created";

                }

                await connection.CloseAsync();

                return "Exists";
            }
        }

        public static async Task<string> DoesuserstableExist()
        {
            using(MySqlConnection connection = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1])))
            {
                await connection.OpenAsync();

                string query = $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'userstable';";
                MySqlCommand command = new MySqlCommand(query, connection);

                long count = (long)await command.ExecuteScalarAsync();


                if(count == 0)
                {
                    // Table doesn't exist, create it
                    query = $"CREATE TABLE userstable ( UserID int NOT NULL AUTO_INCREMENT, Username varchar(255),PreviousUsername varchar(255) DEFAULT 'null',UserEmail varchar(255),UserPass varchar(255),Verification varchar(255) DEFAULT 'False',PRIMARY KEY (UserID));";
                    command = new MySqlCommand(query, connection);


                    await command.ExecuteNonQueryAsync();
                    await connection.CloseAsync();

                    return "Created";

                }

                await connection.CloseAsync();

                return "Exists";
            }
        }

        public async Task AlltableAscending(string username)
            {
            try
            {

                /// SOLVE THE GET VALUE ISSUE 

                string sql = $"Select * from {username} ORDER BY Name ASC;";
                cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
                command = new MySqlCommand(sql, cnn);


                cnn.Open();

                MySqlDataReader reader = command.ExecuteReader();
                MainWindow.AnimesCollection.Clear();
                while( reader.Read() )
                {
                    int k = 0;
                    List<string> list = new List<string>();

                    MainWindow.Anime an = new MainWindow.Anime();

                    string title = reader.GetValue(0).ToString();
                    an.ATitle = char.ToUpper(title[0]) + title.Substring(1).ToLower();

                    MainWindow.ExistingAnimesCollection.Add(reader.GetValue(0).ToString());
                    an.Rating = Convert.ToInt32(reader.GetValue(1));
                    an.Seasonsnumber = Convert.ToInt32(reader.GetValue(2));
                    an.movienum = Convert.ToInt32(reader.GetValue(3));
                    an.Ovanum = Convert.ToInt32(reader.GetValue(4));
                    an.Episodes = Convert.ToInt32(reader.GetValue(5));
                    an.SMOrder = reader.GetValue(6).ToString();
                    an.WatchedTypes = reader.GetValue(7).ToString();
                    an.ImagePath = reader.GetValue(8).ToString();

                    for( int i = 9; i < 27; i++ )
                    {
                        if( reader.GetValue(i).ToString().Contains("yes") )
                        {
                            list.Add(Tagstrings[k]);
                        } else if( reader.GetValue(i).ToString().Contains("no") )
                        {
                            list.Add("none");
                        }
                        k++;
                    }

                    an.TagList = list;
                    MainWindow.AnimesCollection.Add(an);
                }
                cnn.Close();
            } catch(Exception e)
            {
                MessageBox.Show(e.ToString());
                MessageBox.Show("Something went wrong, Check your internet connection");
            }
            await Task.FromResult("");
        }

        public async Task<bool> FilteredRequest(string request,string animetitle)
        {
            /// works good 100% comepleted
            MainWindow.AnimesCollection.Clear();

            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));;
            command = new MySqlCommand(request, cnn);
            command.Parameters.AddWithValue("@Name",animetitle);

            cnn.Open();
            MySqlDataReader reader = command.ExecuteReader();

            while( reader.Read() )
            {
                int k = 0;
                List<string> list = new List<string>();
                MainWindow.Anime an = new MainWindow.Anime();

                string title = reader.GetValue(0).ToString();
                an.ATitle = char.ToUpper(title[0]) + title.Substring(1).ToLower();

                an.Rating = Convert.ToInt32(reader.GetValue(1));
                an.Seasonsnumber = Convert.ToInt32(reader.GetValue(2));
                an.movienum = Convert.ToInt32(reader.GetValue(3));
                an.Ovanum = Convert.ToInt32(reader.GetValue(4));
                an.Episodes = Convert.ToInt32(reader.GetValue(5));
                an.SMOrder = reader.GetValue(6).ToString();
                an.WatchedTypes = reader.GetValue(7).ToString();
                an.ImagePath = reader.GetValue(8).ToString();

                for( int i = 9; i < 27; i++ )
                {
                    if( reader.GetValue(i).ToString().Contains("yes") )
                    {
                        list.Add(Tagstrings[k]);
                    } else if( reader.GetValue(i).ToString().Contains("no") )
                    {
                        list.Add("none");
                    }
                    k++;
                }

                an.TagList = list;
                MainWindow.AnimesCollection.Add(an);
            }
            cnn.Close();
            return await Task.FromResult(true);
        }

        public async Task<bool> FilteredRequest(string animetitle)
        {
            /// works good 100% comepleted
            MainWindow.AnimesCollection.Clear();
            string request = "Select * From "+ MainWindow.Currentuser.Username + " where Name Like '%@Name%' ";
            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1])); ;
            command = new MySqlCommand(request, cnn);
            command.Parameters.AddWithValue("@Name", animetitle);

            cnn.Open();
            MySqlDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                int k = 0;
                List<string> list = new List<string>();
                MainWindow.Anime an = new MainWindow.Anime();

                string title = reader.GetValue(0).ToString();
                an.ATitle = char.ToUpper(title[0]) + title.Substring(1).ToLower();

                an.Rating = Convert.ToInt32(reader.GetValue(1));
                an.Seasonsnumber = Convert.ToInt32(reader.GetValue(2));
                an.movienum = Convert.ToInt32(reader.GetValue(3));
                an.Ovanum = Convert.ToInt32(reader.GetValue(4));
                an.Episodes = Convert.ToInt32(reader.GetValue(5));
                an.SMOrder = reader.GetValue(6).ToString();
                an.WatchedTypes = reader.GetValue(7).ToString();
                an.ImagePath = reader.GetValue(8).ToString();

                for(int i = 9; i < 27; i++)
                {
                    if(reader.GetValue(i).ToString().Contains("yes"))
                    {
                        list.Add(Tagstrings[k]);
                    } else if(reader.GetValue(i).ToString().Contains("no"))
                    {
                        list.Add("none");
                    }
                    k++;
                }

                an.TagList = list;
                MainWindow.AnimesCollection.Add(an);
            }
            cnn.Close();
            return await Task.FromResult(true);
        }

        public async Task<MainWindow.User> LoginCheck(string email, string password)
        {
            if(AdminUserConnectionString != "null")
            {
                cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
            } else
            {
                MessageBox.Show("No connection string detected!");

                return new MainWindow.User { Username = "NoConnectionString", UserEmail = "NoConnectionString", PreviousUsername = "NoConnectionString" };
            }
           
            string sql = "SELECT * FROM userstable WHERE UserEmail = @email;";
            command = new MySqlCommand(sql, cnn);
            command.Parameters.AddWithValue("@email", email);
            cnn.Open();
            MySqlDataReader reader = command.ExecuteReader();
            if(reader.Read())
            {
                MainWindow.User user = new MainWindow.User();

                /// returns the email and pass usr prev strings 
                user.SetUserPassword(reader.GetValue(4).ToString());
                string rnd = DatabaseSetting.SeperateStringA(user.GetUserPassword())[1];
                string decryptedPassword = dbsqlclass.Decrypt(DatabaseSetting.SeperateStringA(user.GetUserPassword())[0], rnd);
                if(decryptedPassword == password)
                {
                    user.Username = reader.GetValue(1).ToString();
                    user.UserEmail = reader.GetValue(3).ToString();

                    user.PreviousUsername = reader.GetValue(2).ToString();

                    return await Task.FromResult(user);

                } else
                {
                    return new MainWindow.User { Username = "wrong", UserEmail = "wrong", PreviousUsername = "wrong"};
                }

            } else
            {
                return null;
            }
        }

        public async Task<string?> Signup(string username,string previoususername, string useremail,string userpass)
        {
            if(!IsConnectedToInternet()) 
            { 
                return null; 
            }

            if(AdminUserConnectionString != "null")
            {
                cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
            } else
            {
                MessageBox.Show("No connection string detected!");

                return "NoConnectionString";
            }

            string signupcheck = "SELECT * FROM userstable WHERE Username = @username;";
            MySqlCommand commandCheck = new MySqlCommand(signupcheck, cnn);
            commandCheck.Parameters.AddWithValue("@username", username);


            string sql = "INSERT INTO userstable (Username, PreviousUsername, UserEmail, UserPass) VALUES (@Username, @PreviousUsername, @UserEmail, @UserPass);";
            command = new MySqlCommand(sql, cnn);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PreviousUsername", previoususername ?? "null");
            command.Parameters.AddWithValue("@UserEmail", useremail);
            command.Parameters.AddWithValue("@UserPass", userpass);

            cnn.Open();
            MySqlDataReader reader = commandCheck.ExecuteReader();
            if(reader.Read())
            {
                reader.Close();
                cnn.Close();
                return "exists";

            } else
            {
                reader.Close();
                commandCheck.Dispose();
                int rows = command.ExecuteNonQuery();
                if(rows > 0)
                {
                    cnn.Close();
                    return "created";
                } else
                {
                    cnn.Close();
                    return await Task.FromResult("Error");
                }
            }
        }

        public bool IsConnectedToInternet()
        {
            bool result = false;

            string pingtest = "1.1.1.1";

            Ping p = new Ping();

            try
            {
                PingReply reply = p.Send(pingtest, 1000);
                if( reply.Status == IPStatus.Success )
                {
                    result = true;

                } else result = false;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("You have no internet! \nTry again later : "+ ex );
            }
            return result;
        }

        public bool ConnectionStringTester(string Connectionstring)
        {
            try
            {
                using(MySqlConnection conction = new MySqlConnection(Connectionstring))
                {

                    conction.Open();
                    conction.Close();
                    return true;
                }
            } catch (Exception e)
            {
                e.ToString();
                return false;
            }
        }

        public async void Deletesql(string animename)
        {
            /// DONE AND WORKING!!!!
            string IDdelete = "Delete from "+ MainWindow.Currentuser.Username + $" where Name='{animename}';";
            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
            cnn.Open();
            command = new MySqlCommand(IDdelete, cnn);
            command.ExecuteNonQuery();
            cnn.Close();
            MainWindow.ExistingAnimesCollection.Remove(animename);
            await Task.FromResult("");
        }

        public async void InsertMysql(MainWindow.Anime Newanime)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("INSERT INTO " + MainWindow.Currentuser.Username + " VALUES (");
                sql.Append(@"'" + Newanime.ATitle.ToLower() + @"',");
                sql.Append(Newanime.Rating + @",");
                sql.Append(Newanime.Seasonsnumber + @",");
                sql.Append(Newanime.movienum + @",");
                sql.Append(Newanime.Ovanum + @",");
                sql.Append(Newanime.Episodes + @",");
                sql.Append(@"'" + Newanime.SMOrder + @"',");
                sql.Append(@"'" + Newanime.WatchedTypes + @"',");
                sql.Append(@"'"+ Newanime.ImagePath.ToLower() + "'");
                int i = 0;
                foreach(string tag in Newanime.TagList)
                {
                    if(tag == Tagstrings[i])
                    {
                        sql.Append(@",'yes'");
                        i++;
                    } else if(tag == "none")
                    {
                        sql.Append(@",'no'");
                        i++;
                    }
                }
                sql.Append(@");");
                cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
                cnn.Open();

                command = new MySqlCommand(sql.ToString(), cnn);


                int rows = command.ExecuteNonQuery();

                cnn.Close();

                await Task.FromResult("");
            } catch(Exception t)
            {
                
            }

        }

        public Task UpdateMysql(MainWindow.Anime UpdatedAnime)
        {

            /// DONE AND WORKING !!!!!
            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));
            
            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE "+ MainWindow.Currentuser.Username +" SET ");
            sql.Append("Rating=" + UpdatedAnime.Rating + @",");
            sql.Append("Seasons=" + UpdatedAnime.Seasonsnumber + @",");
            sql.Append("Movies=" + UpdatedAnime.movienum + @",");
            sql.Append("Ova=" + UpdatedAnime.Ovanum + @",");
            sql.Append("episodes=" + UpdatedAnime.Episodes + @",");
            sql.Append("SMO=" + @"'" + UpdatedAnime.SMOrder + @"',");
            sql.Append("Watched=" + @"'" + UpdatedAnime.WatchedTypes + @"',");
            sql.Append("ImagePath=" + @"'"+ UpdatedAnime.ImagePath +@"'");
            int i = 0;
            foreach( string tag in UpdatedAnime.TagList )
            {
                if( tag == Tagstrings[i] )
                {
                    sql.Append(@"," + Tagstrings[i] + @"='yes'");
                    i++;
                } else if( tag == "none" )
                {
                    sql.Append(@"," + Tagstrings[i] + @"='no'");
                    i++;
                }
            }
            sql.Append($" WHERE Name='{UpdatedAnime.ATitle}';");
            cnn.Open();
            command = new MySqlCommand(sql.ToString(), cnn);

            int rows = command.ExecuteNonQuery();
            cnn.Close();
            return Task.CompletedTask;
        }

        public static string Encrypt(string plainText, string seed)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] seedBytes = Encoding.UTF8.GetBytes(seed);

        byte[] encryptedBytes;
        using(Aes aes = Aes.Create())
        {
            aes.Key = seedBytes;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using(MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(aes.IV, 0, aes.IV.Length);
                using(CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }
                encryptedBytes = memoryStream.ToArray();
            }
        }

        return Convert.ToBase64String(encryptedBytes);
    }

        public static string Decrypt(string encryptedText, string seed)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] seedBytes = Encoding.UTF8.GetBytes(seed);

        byte[] decryptedBytes;
        using(Aes aes = Aes.Create())
        {
            aes.Key = seedBytes;
            aes.Mode = CipherMode.CBC;

            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using(MemoryStream memoryStream = new MemoryStream())
            {
                using(CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length);
                    cryptoStream.FlushFinalBlock();
                }
                decryptedBytes = memoryStream.ToArray();
            }
        }
           return Encoding.UTF8.GetString(decryptedBytes);
    }    

        public async Task<int> GetUserID (string useremail)
        {
            if(!IsConnectedToInternet())
            {
                return 0;
            }
            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));

            string sql = "SELECT UserID from userstable WHERE UserEmail = @email";

            command = new MySqlCommand(sql, cnn);

            command.Parameters.AddWithValue("@email", useremail);

            cnn.Open();

            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {

                return Convert.ToInt32(reader.GetValue(0));

            }

            return await Task.FromResult(-1);
        }

        public async Task<string> UserUpdateSQL(UserSetting.UserSettingItem settingItem, string rnd, int id)
        {
            if(!IsConnectedToInternet())
            {
                return null;
            }

            cnn = new MySqlConnection(Decrypt(DatabaseSetting.SeparateString(AdminUserConnectionString)[0], DatabaseSetting.SeparateString(AdminUserConnectionString)[1]));

            if(settingItem.Username == settingItem.PreviousUsername)
            {
                string Userupdate = "UPDATE userstable SET Username = @Username, UserEmail = @UserEmail, UserPass = @UserPass WHERE UserID = @UserID;";
                command = new MySqlCommand(Userupdate, cnn);

            } else
            {
                string Userupdate = "UPDATE userstable SET Username = @Username, PreviousUsername = @PreviousUsername, UserEmail = @UserEmail, UserPass = @UserPass WHERE UserID = @UserID;";
                command = new MySqlCommand(Userupdate, cnn);
                command.Parameters.AddWithValue("@PreviousUsername", settingItem.PreviousUsername ?? "null");
            }

                 command.Parameters.AddWithValue("@Username", settingItem.Username);
                 command.Parameters.AddWithValue("@UserEmail", settingItem.GetUserSettingEmail());
                 command.Parameters.AddWithValue("@UserPass", DatabaseSetting.CombineStrings(dbsqlclass.Encrypt(settingItem.GetUserSettingPassword(), rnd), rnd));
                 command.Parameters.AddWithValue("@UserID", id);

                 cnn.Open();

                int rows = command.ExecuteNonQuery();
                if(rows > 0)
                {
                    cnn.Close();
                    MessageBox.Show("Changes were saved!");
                    return "changed";
                } else
                {
                    cnn.Close();
                    return await Task.FromResult("Error");
                }
            
        }
    }
}
