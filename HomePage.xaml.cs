using Mid.Model;
using Mid.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
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
    public sealed partial class HomePage : Page
    {
        FolderViewModel viewModel;
        private ObservableCollection<Folder> folders = new ObservableCollection<Folder>();
        private ObservableCollection<Video> videos = new ObservableCollection<Video>();

        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Frame rootFrame = Window.Current.Content as Frame;

            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = rootFrame.CanGoBack ?
            //    AppViewBackButtonVisibility.Visible : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

            //base.OnNavigatedTo(e);
            viewModel = FolderViewModel.GetInstance();
            // folders拷贝viewmodel中的folders用于与数据库关联
            folders = viewModel.Folders;
            // videos不需要保存到数据库中，每次打开页面都把文件夹下的文件读出来即可
            foreach (var eachfolder in folders)
            {
                showVideoInFolder(eachfolder.Token);
            }         

            // 挂起

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //base.OnNavigatedFrom(e);
        }

        /* 通过文件夹选择器添加文件夹，并加入FutureAccessList */
        private async void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string token = Windows.Storage.AccessCache.StorageApplicationPermissions.
                                FutureAccessList.Add(folder);
                viewModel.FolderAdd(token, folder.Name, await GetNumInFolder(folder));
                // 显示新加入文件夹下的视频
                showVideoInFolder(token);
            }
        }

        /*
         * 处理空文件夹：FolderDetailsPage 里的 EmptyFolderText 设置为显示
         * 非空文件夹: FolderDetailsPage 里的 EmptyFolderText 设置为隐藏
         */
        private void Folders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedFolder = (sender as GridView).SelectedItem as Folder;
            Frame.Navigate(typeof(FolderDetailsPage), (sender as GridView).SelectedItem as Folder);
        }

        /* 进入VideoDetailsPage */
        private void Videos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedVideo = (sender as GridView).SelectedItem as Video;
            Frame.Navigate(typeof(VideoDetailsPage));
        }
        
        /* 计算文件夹下文件个数的辅助函数 */
        private async Task<int> GetNumInFolder(StorageFolder folder)
        {
            int total = 0;
            var foldersQuery = folder.CreateFolderQuery();
            var listOfFolders = await foldersQuery.GetFoldersAsync();
            total += listOfFolders.Count;
            // get the num of folders

            // find mp4, wma, avi vedios
            var filesQueryOptions = new Windows.Storage.Search.QueryOptions();
            filesQueryOptions.FileTypeFilter.Add(".mp4");
            filesQueryOptions.FileTypeFilter.Add(".wma");
            filesQueryOptions.FileTypeFilter.Add(".avi");

            var listOfVideos = await folder.CreateFileQueryWithOptions(filesQueryOptions).GetFilesAsync();
            total += listOfVideos.Count;
            // get the num of videos

            return total;
        }

        /* 显示文件夹下视频的辅助函数 */
        private async void showVideoInFolder(string token)
        {
            var folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
            if (folder != null)
            {
                var foldersQuery = folder.CreateFolderQuery();

                var filesQuery = new Windows.Storage.Search.QueryOptions();
                filesQuery.FileTypeFilter.Add(".mp4");
                filesQuery.FileTypeFilter.Add(".wma");
                filesQuery.FileTypeFilter.Add(".avi");

                var query = folder.CreateFileQueryWithOptions(filesQuery);
                var files = await query.GetFilesAsync();
                foreach (var file in files)
                {
                    //bitmapImage.SetSource(randomAccessStream);
                    BitmapImage bitmapImage = await Common.GetThumbnailOfVideo(file);
                    Video temp = await viewModel.Search(file.Path);
                    string videoToken;
                    string date = DateTimeOffset.Now.Year.ToString() + "/" + DateTimeOffset.Now.Month.ToString() + "/" + DateTimeOffset.Now.Day.ToString();
                    if (temp == null)
                    {
                        videoToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);                     
                        videos.Add(new Video(videoToken, file.Name, date, "", 0, 0, 0, 0, file.Path, bitmapImage));
                    }
                    else
                    {
                        temp.Date = date;
                        videos.Add(temp);
                    }              
                }
            }
        }
    }
}
