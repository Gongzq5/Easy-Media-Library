using Mid.Model;
using Mid.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Mid
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        FolderViewModel viewModel;
        private ObservableCollection<Video> histories = new ObservableCollection<Video>();

        public HistoryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel = FolderViewModel.GetInstance();
            histories = viewModel.Histories;
        }

        /*
         * 删除histories里相应的item，并对数据库进行操作
         */
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {           
            //var data = (sender as FrameworkElement).DataContext;
            //var item = Historys.ContainerFromItem(data) as ListViewItem;          
            //viewModel.SelectedVideo = item.Content as Video;
            //viewModel.HistoryRemove();
        }

        private void Historys_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedVideo = (sender as ListView).SelectedItem as Video;
            Frame.Navigate(typeof(VideoDetailsPage));
        }

    }
}
