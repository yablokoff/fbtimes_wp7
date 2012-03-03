using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Markup;
using Microsoft.Phone.Controls;

namespace fbtimes_test
{
    [DataContract]
    public class Post
    {
        [DataMember]
        public string Title { get; set; }
        public string Website {get; set; }
        protected string linkURL;
        [DataMember]
        public string LinkURL {
            get 
            {
                return linkURL;
            }
            set 
            {
                linkURL = value;
                try
                {
                    Uri uri = new Uri(linkURL);
                    Website = uri.Host;
                }
                catch (Exception ex) {
                    Website = linkURL;
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ImgURL { get; set; }
        [DataMember]
        public UInt16 Likes { get; set; }
        [DataMember]
        public UInt16 Comments { get; set; }
        [DataMember]
        public UInt16 Shares { get; set; }
        [DataMember]
        public UInt16 PageShares { get; set; }
        
        public Post() { }

        public Post(string title, string website, string description, string ImgUrl = "", UInt16 Likes = 0, UInt16 Comments = 0, UInt16 Shares = 0, UInt16 PageShares = 0)
        {
            this.Title = title;
            this.LinkURL = website;
            this.Description = description;
            this.ImgURL = ImgURL;
            this.Likes = Likes;
            this.Comments = Comments;
            this.Shares = Shares;
            this.PageShares = PageShares;
            this.Website = "";
        }
    }
    
    public abstract class AbstractPostsPanorama<T> {
        public ObservableCollection<T> NowPosts;
        public ObservableCollection<T> DayPosts;
        public ObservableCollection<T> WeekPosts;
        public ObservableCollection<T> MonthPosts;
        public DataTemplate DTemplate;

        public AbstractPostsPanorama() {
            NowPosts = new ObservableCollection<T>();
            DayPosts = new ObservableCollection<T>();
            WeekPosts = new ObservableCollection<T>();
            MonthPosts = new ObservableCollection<T>();
            DTemplate = CreateDataTemplate();
        }
        public abstract DataTemplate CreateDataTemplate();
        public abstract int GetPosts();
    }

    public class PostsPanorama<T> : AbstractPostsPanorama<T> where T : Post, new() {
        public override DataTemplate CreateDataTemplate()
        {
            string xaml = @"
                            <DataTemplate 
                            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                            xmlns:delay=""clr-namespace:Delay;assembly=PhonePerformance""
                            >
                            <Border BorderBrush=""Silver"" BorderThickness=""0,0,0,1"" Padding=""0,0,0,4"">
                            <StackPanel>
                                <HyperlinkButton Content=""{Binding Title}"" Style=""{StaticResource HyperlinkButtonWrappingStyle}"" TargetName=""_blank"" NavigateUri=""{Binding LinkURL}"" Margin=""10,10,0,0"" />
                                <StackPanel Orientation=""Horizontal"" Margin=""0,5,0,0"" Width=""460"" Height=""125"">
                                    <Image Width=""90"" Height=""110"" Stretch=""UniformToFill"" delay:LowProfileImageLoader.UriSource=""{Binding ImgURL}"" Margin=""10,0,0,0"" />
                                    <StackPanel>
                                        <TextBlock Text=""{Binding Website}"" TextWrapping=""Wrap"" Margin=""12,-2,12,0"" Style=""{StaticResource PhoneTextSmallStyle}""/>
                                        <TextBlock Text=""{Binding Description}"" Width=""320"" TextWrapping=""Wrap"" Margin=""12,0,0,0"" Style=""{StaticResource PhoneTextSmallStyle}"" Foreground=""White""/>
                                    </StackPanel>
                                </StackPanel>
                                <StackPanel Orientation=""Horizontal"">
                                    <Image Source=""like.png"" Margin=""10,0,0,0"" />
                                    <TextBlock Text=""{Binding Likes}"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                    <Image Source=""comment.png"" Margin=""7,0,0,0"" />
                                    <TextBlock Text=""{Binding Comments}"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                    <Image Source=""share.png"" Margin=""7,0,0,0"" />
                                    <TextBlock Text=""{Binding Shares}"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                    <Image Source=""share_page.png"" Margin=""7,0,0,0"" />
                                    <TextBlock Text=""{Binding PageShares}"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                    <TextBlock Text=""·"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                    <TextBlock Text=""5ч назад"" Style=""{StaticResource PhoneTextSmallStyle}"" Margin=""5,0,0,0"" />
                                </StackPanel>
                            </StackPanel>
                            </Border>
                        </DataTemplate>";
            DataTemplate dt = (DataTemplate)XamlReader.Load(xaml);
            return dt;
        }

        public override int GetPosts()
        {
            string json = "[" +
                            "{\"Comments\":10,\"Description\":\"Правила жизни Илая Уоллака. Есть множество историй, которые не выходят у меня из головы, но вот только я ни одной не помню\",\"ImgURL\":\"https://p.twimg.com/AnDpj9ECEAA4fLA.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Правила жизни Илая Уоллака\",\"LinkURL\":\"http://esquire.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}" +
                          "]";
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(DayPosts.GetType());
            DayPosts = ser.ReadObject(ms) as ObservableCollection<T>;
            return 1;
        }
        
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            ObservableCollection<Post> DayPosts = new ObservableCollection<Post>();

            ProgressBar loadProgressBar = new ProgressBar();
            loadProgressBar.IsIndeterminate = true;
            loadProgressBar.Style = (Style)Application.Current.Resources["CustomIndeterminateProgressBar"];

            PostsPanorama<Post> PPanorama = new PostsPanorama<Post>();
            if (PPanorama.GetPosts() == 1) {
                PostsDay.ItemTemplate = PPanorama.DTemplate;
                PostsDay.ItemsSource = PPanorama.DayPosts;
            }

            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.5.24 Version/10.53";
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            try
            {
                client.DownloadStringAsync(new Uri("http://www.theftimesvdfvdf.com/"));
            }
            catch (Exception) {
                client.CancelAsync();
            }
        }

        private void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
            if (e.Error == null){
                Debug.WriteLine(e.Result);
                Dispatcher.BeginInvoke(() => { MonthText.Text = e.Result; });
            }
            else {
                Debug.WriteLine(e.Error);
            }
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
    }
}