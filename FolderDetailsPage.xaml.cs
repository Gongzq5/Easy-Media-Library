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
using Windows.Media;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.BulkAccess;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.UI.Core;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Mid
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FolderDetailsPage : Page
    {
        private ObservableCollection<Folder> folders;
        private ObservableCollection<Video> videos;

        FolderViewModel viewModel;

        public Folder currentRootFolder = null;

        public FolderDetailsPage()
        {
            this.InitializeComponent();
            /* Just for test */
            folders = new ObservableCollection<Folder>();
            videos = new ObservableCollection<Video>();
        }  

        /* 进入VideoDetailsPage */
        private void Videos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.SelectedVideo = (sender as GridView).SelectedItem as Video;
            Frame.Navigate(typeof(VideoDetailsPage));
        }

        /*
         * 处理空文件夹：FolderDetailsPage 里的 EmptyFolderText 设置为显示
         * 非空文件夹: FolderDetailsPage 里的 EmptyFolderText 设置为隐藏
         */
        private void Folders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Frame.Navigate(typeof(FolderDetailsPage), (sender as GridView).SelectedItem as Folder);
        }

        /// <summary>
        /// 重载函数处理传入参数
        /// </summary>
        /// <param name="e">Folder类型，为即将显示的界面的根目录</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Frame.CanGoBack ?
            //        AppViewBackButtonVisibility.Visible : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;

            //base.OnNavigatedTo(e);
            MainStackPanel.Visibility = Visibility.Collapsed;
            EmptyFolderText.Visibility = Visibility.Visible;

            viewModel = FolderViewModel.GetInstance();

            // 就是currentFolder = e.parameter as folder 然后判断一下不为null
            if (e.Parameter is Folder currentFolder)
            {
                currentRootFolder = currentFolder;
                try
                {
                    folders.Clear();
                    bool wheatherShowEmpty = true;
                    var folder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(currentRootFolder.Token);
                    if (folder != null)
                    {
                        var foldersQuery = folder.CreateFolderQuery();

                        foreach (var eachfolder in await foldersQuery.GetFoldersAsync())
                        {
                            wheatherShowEmpty = false;
                            string token = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(eachfolder);
                            folders.Add(new Folder(token, eachfolder.Name, await GetNumInFolder(eachfolder)));
                        }

                        var filesQuery = new Windows.Storage.Search.QueryOptions();
                        filesQuery.FileTypeFilter.Add(".mp4");
                        filesQuery.FileTypeFilter.Add(".wma");
                        filesQuery.FileTypeFilter.Add(".avi");

                        var query = folder.CreateFileQueryWithOptions(filesQuery);
                        var files = await query.GetFilesAsync();
                        foreach (var file in files)
                        {
                            
                            BitmapImage bitmapImage = await Common.GetThumbnailOfVideo(file);
                            wheatherShowEmpty = false;
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
                    if (!wheatherShowEmpty)
                    {
                        MainStackPanel.Visibility = Visibility.Visible;
                        EmptyFolderText.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {
                    await new MessageDialog("Path is not valid").ShowAsync();
                }
            }
        }

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
    }
}
