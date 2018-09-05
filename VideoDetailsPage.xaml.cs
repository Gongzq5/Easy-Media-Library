using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarButtons;
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
    public sealed partial class VideoDetailsPage : Page
    {
        FolderViewModel viewmodel;

        public VideoDetailsPage()
        {
            this.InitializeComponent();
        }

        public void LikeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LikeButton.Content.ToString() == "收藏")
            {
                LikeButton.Content = "取消收藏";
                viewmodel.SelectedVideo.IfLike = 1;
            }
            else
            {
                LikeButton.Content = "收藏";
                viewmodel.SelectedVideo.IfLike = 0;
            }
        }

        // 进入VideoDetailsPage
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadingControl.IsLoading = true;
            viewmodel = FolderViewModel.GetInstance();


            foreach (var video in viewmodel.LikeVideos)
            {
                if (video.Name == viewmodel.SelectedVideo.Name)
                {
                    LikeButton.Content = "取消收藏";
                    break;
                }
            }

            StorageFile VideoFile = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(viewmodel.SelectedVideo.Token);
            if (VideoFile != null)
            {
                var stream = await VideoFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mediaElement.SetSource(stream, VideoFile.ContentType);
            }

            StarsRating.Value = (double)viewmodel.SelectedVideo.Stars;

            if (viewmodel.SelectedVideo.NotePath != "")
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile file = await folder.GetFileAsync(viewmodel.SelectedVideo.Name + ".rtf");
                if (file != null)
                {
                    try
                    {
                        Windows.Storage.Streams.IRandomAccessStream randAccStream =
                                    await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

                        // Load the file into the Document property of the RichEditBox.
                        EditZone.Document.LoadFromStream(Windows.UI.Text.TextSetOptions.FormatRtf, randAccStream);
                    }
                    catch (Exception)
                    {
                        ContentDialog errorDialog = new ContentDialog()
                        {
                            Title = "File open error",
                            Content = "Sorry, I couldn't open the file.",
                            PrimaryButtonText = "Ok"
                        };

                        await errorDialog.ShowAsync();
                    }
                }
            }            
            viewmodel.VideoAdd();
            videoName.Text = viewmodel.SelectedVideo.Name;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            string date = DateTimeOffset.Now.Year.ToString() + "/" +
                           DateTimeOffset.Now.Month.ToString() + "/" +
                           DateTimeOffset.Now.Day.ToString();
            viewmodel.VideoUpdate(date, viewmodel.SelectedVideo.NotePath, (double)(mediaElement.Position.TotalSeconds), viewmodel.SelectedVideo.Stars, viewmodel.SelectedVideo.IfLike);
        }

        private void StarsRating_ValueChanged(RatingControl sender, object args)
        {
            viewmodel.SelectedVideo.Stars = StarsRating.Value;
        }

        private async void CommentSave_Click(object sender, RoutedEventArgs e)
        {
            //Windows.Storage.Pickers.FileSavePicker savePicker = new Windows.Storage.Pickers.FileSavePicker();
            //savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;

            //// Dropdown of file types the user can save the file as
            //savePicker.FileTypeChoices.Add("Rich Text", new List<string>() { ".rtf" });

            //// Default file name if the user does not type one in or select a file to replace
            //savePicker.SuggestedFileName = "New Document";

            //Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();

            Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //StorageFile file;
            //if (await folder.GetFileAsync(viewmodel.SelectedVideo.Name + ".rtf") != null)
            //{
            //    file = await folder.GetFileAsync(viewmodel.SelectedVideo.Name + ".rtf");
            //}
            //else
            //{
            //    file = await folder.CreateFileAsync(viewmodel.SelectedVideo.Name + ".rtf");
            //}
            StorageFile file = await folder.CreateFileAsync(viewmodel.SelectedVideo.Name + ".rtf", CreationCollisionOption.ReplaceExisting);

            if (file != null)
            {
                //using (var filestream = await file.OpenAsync(FileAccessMode.ReadWrite))
                //{
                //    await RandomAccessStream.CopyAsync(imageStream, filestream);
                //}

                // Prevent updates to the remote version of the file until we
                // finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                using (Windows.Storage.Streams.IRandomAccessStream randAccStream =
                    await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
                {
                    EditZone.Document.SaveToStream(Windows.UI.Text.TextGetOptions.FormatRtf, randAccStream);

                }

                // Let Windows know that we're finished changing the file so the
                // other app can update the remote version of the file.
                Windows.Storage.Provider.FileUpdateStatus status = await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    Windows.UI.Popups.MessageDialog errorBox =
                        new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                    await errorBox.ShowAsync();
                }
                viewmodel.SelectedVideo.NotePath = viewmodel.SelectedVideo.Name + ".rtf";               
            }
        }

        private void mediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            mediaElement.Position = TimeSpan.FromSeconds((double)viewmodel.SelectedVideo.Position);
            LoadingControl.IsLoading = false;
        }

        private void EditZone_TextChanged(object sender, RoutedEventArgs e)
        {
            string text = Toolbar.Formatter?.Text;
            Previewer.Text = string.IsNullOrWhiteSpace(text) ? "Nothing to Preview" : text;
        }

        private void View_Click(object sender, RoutedEventArgs e)
        {
            if (EditZone.Visibility == Visibility.Collapsed)
            {
                EditZone.Visibility = Visibility.Visible;
            }
            else
            {
                EditZone.Visibility = Visibility.Collapsed;
            } 
        }
    }
}
