using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;

            var count = (UInt16)value;

            return (count > 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visiblity = (Visibility)value;

            return (visiblity == Visibility.Visible) ? 1 : 0;
        }
    }

    public enum Period { Now, Day, Week, Month };

    public delegate void GotPostsEventHandler( object sender, GotPostsEventArgs e );

    public class GotPostsEventArgs : EventArgs {
        public Period period { get; set; }
        public uint len { get; set; }

        public GotPostsEventArgs (Period period, uint len = 0) : base() {
            this.period = period;
            this.len = len;
        }
    }

    [DataContract]
    public class Post
    {
        [DataMember]
        public string Title { get; set; }
        public string Website {get; set; }
        protected string linkURL;
        protected UInt16 likes;
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
        [DataMember]
        public UInt32 TimeShift { get; set; }
        
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
    
    public abstract class AbstractPostsData<T>
    {
        public event GotPostsEventHandler Changed;
        public event GotPostsEventHandler Requested;

        public ObservableCollection<T>[] AllTabs;
        public WebClient Client;
        public uint Epoch;
        public int PortionSize;
        public string[] TabsTitles;
  
        public AbstractPostsData() {
            AllTabs = new ObservableCollection<T>[4];
            AllTabs[0] = new ObservableCollection<T>();
            AllTabs[1] = new ObservableCollection<T>();
            AllTabs[2] = new ObservableCollection<T>();
            AllTabs[3] = new ObservableCollection<T>();
            Client = new WebClient();
            Client.Headers["User-Agent"] = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.5.24 Version/10.53";
            Client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            Epoch = 1292;
            PortionSize = 15;
            TabsTitles = new string[4] { "now", "day", "week", "month" };
        }

        public abstract int GetPosts(Period period);

        public virtual void InitTabs() {
            // add additional tabs control            
            WebClient epochGetter = new WebClient();
            epochGetter.Headers["User-Agent"] = "Opera/9.80 (Windows NT 5.1; U; ru) Presto/2.5.24 Version/10.53";
            epochGetter.DownloadStringCompleted += new DownloadStringCompletedEventHandler(epochGetter_DownloadStringCompleted);
            epochGetter.DownloadStringAsync(new Uri("http://theftimes.com/get_current_epoch"));
        }

        public virtual void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Debug.WriteLine(e.Result);
                string json = "[" +
                                "{\"Comments\":10,\"Description\":\"Правила жизни Илая Уоллака. Есть множество историй, которые не выходят у меня из головы, но вот только я ни одной не помню\",\"ImgURL\":\"https://p.twimg.com/AnDpj9ECEAA4fLA.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Правила жизни Илая Уоллака\",\"LinkURL\":\"http://esquire.ru/\"}," +
                                "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                                "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                                "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                                "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}," +
                                "{\"Comments\":10,\"Description\":\"Генерал Шарль де Голль вернулся к власти в 1958 году, когда IV республика пала, не сумев разобраться с войной в Алжире. Он разобрался, хотя проблемы Франции\",\"ImgURL\":\"http://a7.sphotos.ak.fbcdn.net/hphotos-ak-ash4/420977_383281481684718_100000086086040_1659203_1659002380_n.jpg\",\"Likes\":10,\"PageShares\":10,\"Shares\":10,\"Title\":\"Должно быть иначе\",\"LinkURL\":\"http://kommersant.ru/\"}" +
                              "]";
                Period ActiveTab = (Period)e.UserState;
                MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(e.Result));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(AllTabs[(uint)Period.Now].GetType()); // NowPosts doesn't matter - just first from 4
                ObservableCollection<T> NewPosts = ser.ReadObject(ms) as ObservableCollection<T>;
                AppendPosts(ActiveTab, NewPosts);
                OnChanged(new GotPostsEventArgs(ActiveTab, (uint)NewPosts.Count()));
            }
            else
            {
                Debug.WriteLine(e.Error);
            }
        }

        public virtual void epochGetter_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e) {
            if (e.Error == null) {
                Debug.WriteLine(e.Result);
                string epoch = e.Result;
                this.Epoch = Convert.ToUInt32(epoch);
                GetPosts(Period.Now);
            }
            else
            {
                Debug.WriteLine(e.Error);
            }
        }

        public void AppendPosts(Period period, ObservableCollection<T> NewPosts) {
            ObservableCollection<T> WorkerPosts = AllTabs[(uint)period];
            foreach (T NewPost in NewPosts) {
                WorkerPosts.Add(NewPost);
            }
        }

        public virtual void OnChanged(GotPostsEventArgs e) {
            if (Changed != null)
                Changed(this, e);
        }

        public virtual void OnRequested(GotPostsEventArgs e) {
            if (Requested != null)
                Requested(this, e);
        }
    }

    public class PostsData<T> : AbstractPostsData<T> where T : Post, new() 
    {
        public override int GetPosts(Period period)
        {
            try {
                string EpochStr = Convert.ToString(Epoch);
                int From = AllTabs[(uint)period].Count();
                string FromStr = Convert.ToString(From);
                string ToStr = Convert.ToString(From + PortionSize);
                string TimeShift = TabsTitles[(uint)period];
                string uri = "http://theftimes.com/next_posts?epoch=" + EpochStr + "&from=" + FromStr + "&to=" + ToStr + "&time_shift=" + TimeShift + "&format=json&view_mode=po";
                Client.DownloadStringAsync(new Uri(uri), period);
            }
            catch (Exception) {
                Client.CancelAsync();
            }
            OnRequested(new GotPostsEventArgs(period));
            return 1;
        }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        protected StackPanel[] lastPosts = new StackPanel[4];
        protected uint[] appendedPosts = new uint[4] { 0, 0, 0, 0 };
        protected uint[] loadedPosts = new uint[4] { 0, 0, 0, 0 };
        protected bool[] busyPicsLoader = new bool[4] { true, true, true, true }; // become available after initial GetPosts
        protected bool[] busyJSONLoader = new bool[4] { false, false, false, false };

        protected PostsData<Post> PData = new PostsData<Post>();
        protected ScrollBar sb = null;
        protected ScrollViewer sv = null;
        protected bool alreadyHookedScrollEvents = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            PData.Changed += new GotPostsEventHandler(PData_Changed);
            PData.Requested += new GotPostsEventHandler(PData_Requested);

            PData.InitTabs();

            PostsNow.ItemsSource = PData.AllTabs[(uint)Period.Now];
            PostsDay.ItemsSource = PData.AllTabs[(uint)Period.Day];
        }

        protected void HookScrollEventsTo(ListBox Lb) {
            
            Lb.AddHandler(ListBox.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true);
            sb = (ScrollBar)FindElementRecursive(Lb, typeof(ScrollBar));
            sv = (ScrollViewer)FindElementRecursive(Lb, typeof(ScrollViewer));

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

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Panorama p = sender as Panorama;
            if (loadedPosts[p.SelectedIndex] == 0 && !busyJSONLoader[0] && !busyJSONLoader[1] && !busyJSONLoader[2] && !busyJSONLoader[3])
                PData.GetPosts((Period)p.SelectedIndex);
        }

        private void PostsDay_LayoutUpdated(object sender, EventArgs e) {
            ListBox a = sender as ListBox;
        }

        // Load data for the ViewModel Items
        protected void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            HookScrollEventsTo(PostsNow);
            HookScrollEventsTo(PostsDay);
            //HookScrollEventsTo(PostsWeek);
            //HookScrollEventsTo(PostsMonth);
        }

        public void PData_Changed(object sender, GotPostsEventArgs e)
        {
            busyJSONLoader[(int)e.period] = false;
            appendedPosts[(int)e.period] += e.len;
            if (lastPosts[(int)e.period] != null)
            {
                UIElement pb = FindElementRecursive(lastPosts[(int)e.period] as FrameworkElement, typeof(ProgressBar));
                pb.Visibility = Visibility.Collapsed;
            }
        }

        public void PData_Requested(object sender, GotPostsEventArgs e) {
            busyJSONLoader[(int)e.period] = true;
        }

        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
        }

        protected void postimage_Failed(object sender, ExceptionRoutedEventArgs e)
        {
            UIElement parent = FindParentRecursive(sender as FrameworkElement, typeof(StackPanel));
            UIElement element = sender as UIElement;
            element.Visibility = Visibility.Collapsed;
            TextBlock text = FindElementRecursive(parent as FrameworkElement, typeof(TextBlock), 2) as TextBlock;
            text.Width = 420;
        }

        protected void Item_Loaded(object sender, RoutedEventArgs e)
        {
            Period period;
            ListBox WorkListBox = null;
            PanoramaItem pi = FindParentRecursive(sender as FrameworkElement, typeof(PanoramaItem)) as PanoramaItem;

            if (pi == Day){
                period = Period.Day;
                WorkListBox = PostsDay;
            }
            else if (pi == Week) {
                period = Period.Week;
            }
            else if (pi == Month)
            {
                period = Period.Month;
            }
            else{
                period = Period.Now;
                WorkListBox = PostsNow;
            }

            loadedPosts[(int)period] += 1;
            if (WorkListBox.Items.Count() == loadedPosts[(int)period])
            {
                lastPosts[(int)period] = sender as StackPanel;
                busyPicsLoader[(int)period] = false;
            }
        }
        
        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            // possible states: "CompressionTop", "NoVerticalCompression"

            Period period;
            ListBox WorkListBox = null;
            PanoramaItem pi = FindParentRecursive(sender as FrameworkElement, typeof(PanoramaItem)) as PanoramaItem;

            if (pi == Day)
            {
                period = Period.Day;
                WorkListBox = PostsDay;
            }
            else if (pi == Week)
            {
                period = Period.Week;
            }
            else if (pi == Month)
            {
                period = Period.Month;
            }
            else
            {
                period = Period.Now;
                WorkListBox = PostsNow;
            }

            if (!busyPicsLoader[(int)period])
            {
                if (e.NewState.Name == "CompressionBottom")
                {
                    UIElement pb = FindElementRecursive(lastPosts[(int)period] as FrameworkElement, typeof(ProgressBar));
                    pb.Visibility = Visibility.Visible;
                    busyPicsLoader[(int)period] = true;
                    PData.GetPosts(period);
                }
            }
        }

        private void group_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            // possible states: e.NewState.Name == "Scrolling"; "NotScrolling"
        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType, uint orderNumber = 1)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            uint number = 0;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        number += 1;
                        if (number == orderNumber)
                            return element as UIElement;
                        continue;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType, orderNumber);
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
