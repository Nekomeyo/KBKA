// Copyright 2016 Google Inc.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace KBKA
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string userName = null;
        public string userSub = null;
      

        // client configuration
        const string clientID = "894692614544-22gu7nau3ledrn8km38t27sbn957n06e.apps.googleusercontent.com";
        const string clientSecret = "BYsABFPcHy_uiT9DWUwa0LqZ";
        // scoopes
        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        

        public MainWindow()
        {
            InitializeComponent();
            chosendate.Content = DateTime.Now.ToShortDateString();
          

        }
       /// <summary>
       /// Turns on interface
       /// </summary>
        public void ActivateInterface()
        {
            AddToDoo.IsEnabled = true;
            EditToDo.IsEnabled = true;
            DeleteToDo.IsEnabled = true;
            TexBoxToDo.IsEnabled = true;
            ToDoNo.IsEnabled = true;
            Todo.IsEnabled = true;
            AddInProgress.IsEnabled = true;
            EditInProgress.IsEnabled = true;
            DeleteInProgress.IsEnabled = true;
            TextBoxInProgress.IsEnabled = true;
            InProgressNo.IsEnabled = true;
            Inprogress.IsEnabled = true;
            AddDone.IsEnabled = true;
            EditDone.IsEnabled = true;
            DeleteDone.IsEnabled = true;
            TextBoxDone.IsEnabled = true;
            DoneNo.IsEnabled = true;
            Done.IsEnabled = true;
            Calendar.IsEnabled = true;
        }


        // ref http://stackoverflow.com/a/3978040
        /// <summary>
        /// Finds "random" unused port which is used for loopback
        /// </summary>
        /// <returns></returns>
        public static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
        /// <summary>
        /// Initializes log in operation after triggering button with clicking it(<param name="sender"></param>)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void LogInbutton_Click(object sender, RoutedEventArgs e)
        {
            // Generates state and Proof Key for Code Exchange values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
            //output("redirect URI: " + redirectURI);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            //output("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings this app back to the foreground.
            this.Activate();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com' charset='utf-8 '></head><body> <div style='text-align:center'><img src='https://i.imgur.com/d3jNKUj.png'> <br>Logged in succesfully.<br> Get back to application </div></body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                output("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }
            //output("Authorization code: " + code);

            // Starts the code exchange at the Token Endpoint.
            performCodeExchange(code, code_verifier, redirectURI);

        
        }
        /// <summary>
        /// Exchanges code gained in log in operation made in LogInbutton_Click function for tokens needed in gaining information from Google servers
        /// </summary>
        /// <param name="code"></param>
        /// <param name="code_verifier"></param>
        /// <param name="redirectURI"></param>
        async void performCodeExchange(string code, string code_verifier, string redirectURI)
        {
            //output("Exchanging code for tokens...");

            // builds the  request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier,
                clientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    //output(responseText);

                    // converts to dictionary
                    Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                    string access_token = tokenEndpointDecoded["access_token"];
                    userinfoCall(access_token);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        //output("HTTP: " + response.StatusCode);
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                          
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Uses (<param name="access_token"></param>)  gained in performCodeExchange function to acces user infromation needed to extract his Avatar, Name, and also to get id needed in database
        /// </summary>
        /// <param name="access_token"></param>
        async void userinfoCall(string access_token)
        {   
            // sends the request
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
            {
                // reads response body
                string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();

                char[] delimiterChars = { '"' };

                string[] words = userinfoResponseText.Split(delimiterChars);
              
                // gets needed informations
                int i = 0;
                foreach (var word in words)
                {

                    i++;
                    if (i == 4)
                    {
                        userSub = word;
                    }
                    if (i == 12)
                    {
                        userName = word;
                        Witaj.Visibility = Visibility.Visible;
                        Witaj.Content = "Hello there, " + userName;
                        LogIn.Visibility = Visibility.Collapsed;
                     
                    }
                    if (i == 20)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(word, UriKind.Absolute);
                        bitmap.EndInit();
                        avatar.ImageSource = bitmap;
                        ActivateInterface();
                        Instructions.Text = "If you want to operate with tasks you have to use buttons bellow textboxes: “Add”, “Edit” and “Delete”. As it goes “Add” button let’s you to add task to table of your choose, if you want to edit content of the task input correct number in text box labeled “No.” and write correction to textbox and click “Edit”. If you want to delete content of table input correct number in text box labeled “No.” and click delete, it will disappear.";


                    }

                }


                
             

              

                
                SQLiteDA.CreateTables(userSub);
             
               
                Todo.ItemsSource = SQLiteDA.GetData(userSub + "td", "todo", chosendate.Content.ToString()).DefaultView;
                Inprogress.ItemsSource = SQLiteDA.GetData(userSub + "ip", "inprogress", chosendate.Content.ToString()).DefaultView;
                Done.ItemsSource = SQLiteDA.GetData(userSub + "d", "done", chosendate.Content.ToString()).DefaultView;
              

            }

        }


        /// <summary>
        /// Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void output(string output)
        {
            // textBoxOutput.Text = textBoxOutput.Text + output + Environment.NewLine;
            Console.WriteLine(output);

        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static byte[] sha256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        /// <summary>
        /// Shows chosen date form calendar in chosendate label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Calendar_SelectedDatesChanged(object sender,SelectionChangedEventArgs e)
        {
            // ... Get reference.
            var calendar = sender as Calendar;

            // ... See if a date is selected.
            if (calendar.SelectedDate.HasValue)
            {
                // ... Display SelectedDate in Title.
                DateTime date = calendar.SelectedDate.Value;
                chosendate.Content = date.ToShortDateString();
               
            }

            Todo.ItemsSource = SQLiteDA.GetData(userSub + "td", "ToDo", chosendate.Content.ToString()).DefaultView;
            Inprogress.ItemsSource = SQLiteDA.GetData(userSub + "ip", "InProgress", chosendate.Content.ToString()).DefaultView;
            Done.ItemsSource = SQLiteDA.GetData(userSub + "d", "Done", chosendate.Content.ToString()).DefaultView;


        }
        /// <summary>
        /// Adds inserted by user text to database
        /// Every Add button works the same with slight diffrence of proper tabels for each button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToDo_Click(object sender, RoutedEventArgs e)
        {
            SQLiteDA.Add(userSub + "td", "ToDo", chosendate.Content.ToString(), TexBoxToDo.Text.ToString(),ToDoNo.Text.ToString());
            Todo.ItemsSource = SQLiteDA.GetData(userSub + "td", "ToDo", chosendate.Content.ToString()).DefaultView;
        }
      
        private void AddInProgress_Click(object sender, RoutedEventArgs e)
        {

            SQLiteDA.Add(userSub + "ip", "InProgress", chosendate.Content.ToString(), TextBoxInProgress.Text.ToString(), InProgressNo.Text.ToString());
            Inprogress.ItemsSource = SQLiteDA.GetData(userSub + "ip", "InProgress", chosendate.Content.ToString()).DefaultView;
        }
        
        private void AddDone_Click(object sender, RoutedEventArgs e)
        {
            SQLiteDA.Add(userSub + "d", "Done", chosendate.Content.ToString(), TextBoxDone.Text.ToString(), DoneNo.Text.ToString());
            Done.ItemsSource = SQLiteDA.GetData(userSub + "d", "Done", chosendate.Content.ToString()).DefaultView;
        }

        /// <summary>
        /// Edits existing in database text, chooses row based on entered by user number
        /// Every Edit button works the same with slight diffrence of proper tabels for each button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditToDo_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else { 
            SQLiteDA.Edit(userSub + "td", "ToDo", chosendate.Content.ToString(), TexBoxToDo.Text.ToString(),ToDoNo.Text.ToString());
            Todo.ItemsSource = SQLiteDA.GetData(userSub + "td", "ToDo", chosendate.Content.ToString()).DefaultView;
            }
        }
        
        private void EditInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (InProgressNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else
            {
                SQLiteDA.Edit(userSub + "ip", "InProgress", chosendate.Content.ToString(), TextBoxInProgress.Text.ToString(), InProgressNo.Text.ToString());
                Inprogress.ItemsSource = SQLiteDA.GetData(userSub + "ip", "InProgress", chosendate.Content.ToString()).DefaultView;
            }
        }
        
        private void EditDone_Click(object sender, RoutedEventArgs e)
        {
            if (DoneNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else
            {
                SQLiteDA.Edit(userSub + "d", "Done", chosendate.Content.ToString(), TextBoxDone.Text.ToString(), DoneNo.Text.ToString());
                Done.ItemsSource = SQLiteDA.GetData(userSub + "d", "Done", chosendate.Content.ToString()).DefaultView;
            }
        }

        /// <summary>
        /// Deletes existing in database text, chooses row based on entered by user number
        /// Every Delete button works the same with slight diffrence of proper tabels for each button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteToDo_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else
            {
                SQLiteDA.Delete(userSub + "td", chosendate.Content.ToString(), ToDoNo.Text.ToString());
                Todo.ItemsSource = SQLiteDA.GetData(userSub + "td", "ToDo", chosendate.Content.ToString()).DefaultView;
            }
        }


        private void DeleteInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (InProgressNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else
            {
                SQLiteDA.Delete(userSub + "ip", chosendate.Content.ToString(), InProgressNo.Text.ToString());
                Inprogress.ItemsSource = SQLiteDA.GetData(userSub + "ip", "InProgress", chosendate.Content.ToString()).DefaultView;
            }
        }

        private void DeleteDone_Click(object sender, RoutedEventArgs e)
        {
            if (DoneNo.Text == "")
            {
                MessageBox.Show("Looks like something has gone wrong." + "\n" + "Be sure that entered data is correct");
            }
            else
            {
                SQLiteDA.Delete(userSub + "d", chosendate.Content.ToString(), DoneNo.Text.ToString());
                Done.ItemsSource = SQLiteDA.GetData(userSub + "d", "Done", chosendate.Content.ToString()).DefaultView;
            }
        }

        /// <summary>
        /// Blocks entering any other values than numbers into every "No" TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToDoNo_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void InProgressNo_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DoneNo_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

        
    
}
