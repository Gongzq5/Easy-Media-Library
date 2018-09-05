using Mid.Model;
using Mid.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Mid
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LikesPage : Page
    {
        FolderViewModel viewmodel;
        private ObservableCollection<Video> likes = new ObservableCollection<Video>();

        public LikesPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewmodel = FolderViewModel.GetInstance();
            likes = viewmodel.LikeVideos;
        }

        private void Likes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewmodel.SelectedVideo = (sender as GridView).SelectedItem as Video;
            if (viewmodel.SelectedVideo == null) return;
            Frame.Navigate(typeof(VideoDetailsPage));
        }

        private void RatingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (standard.SelectedItem.ToString() == "Stars")
            {
                for (int i = 0; i < likes.Count; i++)
                {
                    for (int j = 0; j < likes.Count - 1; j++)
                    {
                        if (likes[j].Stars < likes[j + 1].Stars)
                        {
                            likes.Move(j + 1, j);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < likes.Count; i++)
                {
                    for (int j = 0; j < likes.Count - 1; j++)
                    {
                        string date1 = likes[j].Date.Split('/')[0].PadLeft(4, '0') + likes[j].Date.Split('/')[1].PadLeft(2, '0') + likes[j].Date.Split('/')[2].PadLeft(2, '0');
                        string date2 = likes[j + 1].Date.Split('/')[0].PadLeft(4, '0') + likes[j + 1].Date.Split('/')[1].PadLeft(2, '0') + likes[j + 1].Date.Split('/')[2].PadLeft(2, '0');
                        DateTime dt1 = DateTime.ParseExact(date1, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        DateTime dt2 = DateTime.ParseExact(date2, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                        if (dt1 < dt2)
                        {
                            likes.Move(j + 1, j);
                        }
                    }
                }
            }
        }
    }
}
