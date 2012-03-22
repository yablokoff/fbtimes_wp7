using System;
using System.IO;
using System.Collections;
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
using System.Windows.Controls.Primitives;
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
    public enum Period { Now, Day, Week, Month };

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

        protected void AppendPosts(Period period, ObservableCollection<T> NewPosts) {
            ObservableCollection<T> WorkPosts = null;
            switch (period) { 
                case Period.Now:
                    WorkPosts = NowPosts;
                    break;
                case Period.Day:
                    WorkPosts = DayPosts;
                    break;
                case Period.Week:
                    WorkPosts = WeekPosts;
                    break;
                case Period.Month:
                    WorkPosts = MonthPosts;
                    break;
                default:
                    WorkPosts = NowPosts;
                    break;
            }
            foreach (T NewPost in NewPosts) {
                WorkPosts.Add(NewPost);
            }
        }

        public abstract DataTemplate CreateDataTemplate();
        public abstract int GetPosts(Period period);
    }

    public class PostsPanorama<T> : AbstractPostsPanorama<T> where T : Post, new() 
    {    
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

        public override int GetPosts(Period period)
        {
            string json = "[" +
                            "{\"Comments\":10,\"Description\":\"Правила жизни Илая Уоллака. Есть множество историй, которые не выходят у меня из головы, но вот только я ни одной не помню\",\"ImgURL\":\"https://p.twimg.com/AnDpj9ECEAA4fLA.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Правила жизни Илая Уоллака\",\"LinkURL\":\"http://esquire.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                            "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}" +
                          "]";
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(DayPosts.GetType());
            ObservableCollection<T> NewPosts = ser.ReadObject(ms) as ObservableCollection<T>;
            bool a;
            for (long i = 0; i < 1000000; i++)
                a = false;
            AppendPosts(period, NewPosts);
            return 1;
        }
        
    }

    public partial class MainPage : PhoneApplicationPage
    {
        private PostsPanorama<Post> PPanorama = new PostsPanorama<Post>();
        private ScrollBar sb = null;
        private ScrollViewer sv = null;
        private bool alreadyHookedScrollEvents = false;
        StackPanel lastForDay = new StackPanel();
        uint loadedForDay = 0;

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

            if (PPanorama.GetPosts(Period.Day) == 1) {
                //PostsDay.ItemTemplate = PPanorama.DTemplate;
                PostsDay.ItemsSource = PPanorama.DayPosts;
                /*
                PostsDay.Height = 2500;
                PostsDay.UpdateLayout();
                PostsDay.LayoutUpdated += new EventHandler(PostsDay_LayoutUpdated);*/
                //PostsDay.ScrollIntoView = 
            }

            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.5.24 Version/10.53";
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            try
            {
                client.DownloadStringAsync(new Uri("http://www.theftimes.com/"));
            }
            catch (Exception) {
                client.CancelAsync();
            }
        }

        private void PostsDay_LayoutUpdated(object sender, EventArgs e) {
            ListBox a = sender as ListBox;
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

            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            PostsDay.AddHandler(ListBox.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true);
            sb = (ScrollBar)FindElementRecursive(PostsDay, typeof(ScrollBar));
            sv = (ScrollViewer)FindElementRecursive(PostsDay, typeof(ScrollViewer));

            if (sv != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {
                    VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    if (group != null)
                    {
                        group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                    }
                    VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                    if (vgroup != null)
                    {
                        vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                    }
                }
            }
        }

        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
        }

        private void Item_Loaded(object sender, RoutedEventArgs e)
        {
            loadedForDay += 1;
            if (PostsDay.Items.Count() == loadedForDay) {
                lastForDay = sender as StackPanel;
            }
            /*if (sender as Post == PostsDay.Items.Last() as Post) {
                lastForDay = sender as StackPanel;
            }*/
        }
        
        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            /*if (e.NewState.Name == "CompressionTop")
            {   
            }*/

            if (e.NewState.Name == "CompressionBottom")
            {
                //ListBoxItem a = PostsDay.Items.Last() as ListBoxItem;
                UIElement pb = FindElementRecursive(lastForDay as FrameworkElement, typeof(ProgressBar));
                pb.Visibility = Visibility.Visible;
                PostsDay.UpdateLayout();
                if (PPanorama.GetPosts(Period.Day) == 1){
                    PostsDay.UpdateLayout();
                    pb.Visibility = Visibility.Collapsed;
                }
            }

            /*if (e.NewState.Name == "NoVerticalCompression")
            {
            }*/
        }

        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            /*if (e.NewState.Name == "Scrolling")
            {
                MonthText.Text = "Scrolling!";
            }
            else
            {
                MonthText.Text = "Not Scrolling!";
            }*/
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }

        private UIElement FindParentRecursive(FrameworkElement child, Type targetType)
        {
            FrameworkElement parent = VisualTreeHelper.GetParent(child) as FrameworkElement;
            UIElement parentElem = null;
            if (parent.GetType() == targetType)
                return parent as UIElement;
            else
                parentElem = FindParentRecursive(parent, targetType);
            return parentElem;
        }
        
        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }
    }
}
