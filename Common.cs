using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Mid
{
    public static class Common
    {
        /**
         * <summary>get a video's thumbnail by token(or a file)</summary> 
         * <param name="path">the token of the file</param>
         */
        public static async Task<BitmapImage> GetThumbnailOfVideo(String path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);

            return await GetThumbnailOfVideo(file);
        }

        /**
         * <summary>get a video's thumbnail by token(or a file)</summary> 
         * <param name="file">a StorageFile of video</param>
         */
        public static async Task<BitmapImage> GetThumbnailOfVideo(StorageFile file)
        {
            var thumbnail = await file.GetScaledImageAsThumbnailAsync(ThumbnailMode.VideosView);
            BitmapImage bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(thumbnail, randomAccessStream);
            randomAccessStream.Seek(0);

            await bitmapImage.SetSourceAsync(randomAccessStream);

            return bitmapImage;
        }
    }
}
