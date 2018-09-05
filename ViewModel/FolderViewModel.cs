using Mid.Model;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Mid.ViewModel
{
    class FolderViewModel
    {
        /* HomeVideos 存储选中HomePage或FolderDetails页面所有文件夹下的所有视频；
         * 当从离开HomePage或离开FolderDetails时，Homevideos清空；
         * 
         * 若离开HomePage或离开FolderDetails而进入另一个HomePage或FolderDetails，
         * 则在清空完后重新存储当前页面所有文件夹下的所有视频。
         */
        private ObservableCollection<Video> homeVideos = new ObservableCollection<Video>();

        //Video 数据库
        static private SQLiteConnection Videoconn;

        //Folder 数据库和Collection
        static private SQLiteConnection Folderconn;
        private ObservableCollection<Folder> folders = new ObservableCollection<Folder>();
        private Folder selectedFolder;

        //收藏的Video Collection
        private ObservableCollection<Video> likeVideos = new ObservableCollection<Video>();

        //历史记录的Video Collection
        private ObservableCollection<Video> histories = new ObservableCollection<Video>();

        //选中的Video，所有页面都用这个作为选中的Video变量
        private Video selectedVideo;

        public ObservableCollection<Video> HomeVideos { get => homeVideos; set => homeVideos = value; }
        public ObservableCollection<Video> LikeVideos { get => likeVideos; set => likeVideos = value; }
        public ObservableCollection<Video> Histories { get => histories; set => histories = value; }
        public ObservableCollection<Folder> Folders { get => folders; set => folders = value; }
        public Video SelectedVideo { get => selectedVideo; set => selectedVideo = value; }
        public Folder SelectedFolder { get => selectedFolder; set => selectedFolder = value; }

        //单例模式
        private static FolderViewModel instance;
        private FolderViewModel()
        {
            LoadVideoDatabase();
            ReadVideoDatabase();
            LoadFolderDatabase();
            ReadFolderDatabase();
        }
        public static FolderViewModel GetInstance()
        {
            if (instance == null)
            {
                instance = new FolderViewModel();
            }
            return instance;
        }

        //Load video database
        public void LoadVideoDatabase()
        {
            // Get a reference to the SQLite database 
            Videoconn = new SQLiteConnection("sqlitepcldemo.db");
            string sql = @"CREATE TABLE IF NOT EXISTS
                            Videos (Token VARCHAR(100) PRIMARY KEY  NOT NULL,
                                    Name VARCHAR(100),
                                    Date VARCHAR(100),
                                    NotePath VARCHAR(100),
                                    Position REAL, 
                                    IfLike INTEGER,
                                    Stars REAL,
                                    HistoryShow INTEGER,
                                    Path VARCHAR(300)
                         );";
            using (var statement = Videoconn.Prepare(sql))
            {
                statement.Step();
            }
        }

        //read data from database to collection
        public async void ReadVideoDatabase()
        {
            using (var statement = Videoconn.Prepare("SELECT * FROM Videos WHERE HistoryShow <> 0"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    string token = (string)statement[0];
                    var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                                FutureAccessList.GetFileAsync(token);
                    BitmapImage thumbi = await Common.GetThumbnailOfVideo(file);
                    histories.Add(new Video((string)statement[0], (string)statement[1],
                                      (string)statement[2], (string)statement[3],
                                      (double)statement[4], (long)statement[5],
                                      (double)statement[6], (long)statement[7], (String)statement[8], thumbi));
                }
            }

            using (var statement = Videoconn.Prepare("SELECT * FROM Videos WHERE IfLike <> 0"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    string token = (string)statement[0];
                    var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                                FutureAccessList.GetFileAsync(token);
                    BitmapImage thumbi = await Common.GetThumbnailOfVideo(file);
                    likeVideos.Add(new Video((string)statement[0], (string)statement[1],
                                      (string)statement[2], (string)statement[3],
                                      (double)statement[4], (long)statement[5],
                                      (double)statement[6], (long)statement[7], (String)statement[8], thumbi));
                }
            }
        }

        /* 当在HomePage点击新视频时调用，所以这时IfLike=0，Stars=0，HistoryShow=1
         * 同时加入数据库和History 
         */
        public async void VideoAdd()
        {
            if (selectedVideo.HistoryShow == 0)
            {
                selectedVideo.HistoryShow = 1;
                var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(selectedVideo.Token);
                BitmapImage thumbi = await Common.GetThumbnailOfVideo(file);
                histories.Add(selectedVideo);

                foreach (var video in likeVideos)
                {
                    if (video.Name == selectedVideo.Name)
                    {
                        selectedVideo.IfLike = 1;
                    }
                }

                if (selectedVideo.IfLike == 0)
                {
                    using (var custstmt = Videoconn.Prepare("INSERT INTO Videos(Token, Name, Date, NotePath, Position, IfLike, Stars, HistoryShow, Path) " +
                                                        " VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)"))
                    {
                        custstmt.Bind(1, selectedVideo.Token);
                        custstmt.Bind(2, selectedVideo.Name);
                        custstmt.Bind(3, selectedVideo.Date);
                        custstmt.Bind(4, selectedVideo.NotePath);
                        custstmt.Bind(5, selectedVideo.Position);
                        custstmt.Bind(6, selectedVideo.IfLike);
                        custstmt.Bind(7, selectedVideo.Stars);
                        custstmt.Bind(8, selectedVideo.HistoryShow);
                        custstmt.Bind(9, selectedVideo.Path);
                        custstmt.Step();
                    }
                }
                else
                {
                    //likeVideos里的Historyshow属性设为1
                    selectedVideo.HistoryShow = 0; // 先修改为0用于查找
                    int index = likeVideos.IndexOf(selectedVideo);
                    likeVideos[index].HistoryShow = 1;
                }
            }
            //让selectedVideo指向Histories的这个元素，防止HomeVideos清空后selectedVideo失效
            //selectedVideo.HistoryShow = 1;
            //int hindex = histories.IndexOf(selectedVideo);
            //selectedVideo = histories[hindex];
        }

        //同时更新数据库，Histories Collection 和 LikeVideo Collection
        public void VideoUpdate(string date, string notePath, double position, double stars, long IfLike)
        {
            using (var custstmt = Videoconn.Prepare
                ("UPDATE Videos SET Date = ?, NotePath = ?, Position = ?,  Stars = ?, IfLike = ? WHERE Token = ?"))
            {
                custstmt.Bind(1, date);
                custstmt.Bind(2, notePath);
                custstmt.Bind(3, position);
                custstmt.Bind(4, stars);
                custstmt.Bind(5, IfLike);
                custstmt.Bind(6, selectedVideo.Token);
                custstmt.Step();
            }
            
            var v = histories.Where(video => video.Name == selectedVideo.Name).First();
            v.Date = date;
            v.NotePath = notePath;
            v.Position = position;
            v.Stars = stars;

            if (selectedVideo.IfLike == 1)
            {
                bool isFound = false;
                foreach (var video in likeVideos)
                {
                    if (video.Name == selectedVideo.Name)
                    {
                        video.Date = date;
                        video.NotePath = notePath;
                        video.Position = position;
                        video.Stars = stars;
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    likeVideos.Add(selectedVideo);
                }
            }
            else
            {
                foreach (var video in likeVideos)
                {
                    if (video.Name == selectedVideo.Name)
                    {
                        likeVideos.Remove(video);
                        break;
                    }
                }
            }
        }

        //设置HisotryShow属性为0， 若IfLike属性同时为0，则从数据库删除
        public void HistoryRemove()
        {
            if (selectedVideo.IfLike == 0)
            {
                using (var statement = Videoconn.Prepare("DELETE FROM Videos WHERE Token = ?"))
                {
                    statement.Bind(1, selectedVideo.Token);
                    statement.Step();
                }
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(SelectedVideo.Token);
                histories.Remove(selectedVideo);
            }
            else
            {
                using (var statement = Videoconn.Prepare("UPDATE Videos SET HistoryShow = ? WHERE Token = ?"))
                {
                    statement.Bind(1, 0);
                    statement.Bind(2, selectedVideo.Token);
                    statement.Step();
                }
                int index = likeVideos.IndexOf(selectedVideo);
                likeVideos[index].HistoryShow = 0;
                histories.Remove(selectedVideo);
            }

            //selectedVideo = null;
        }

        //设置IfLike属性为0， 若HistoryShow属性同时为0，则从数据库删除
        public void IfLikeAddOrRemove()
        {
            if (selectedVideo.IfLike == 1)
            {
                using (var statement = Videoconn.Prepare("UPDATE Videos SET IfLike = ? WHERE Token = ?"))
                {
                    statement.Bind(1, 0);
                    statement.Bind(2, selectedVideo.Token);
                    statement.Step();
                }
                selectedVideo.IfLike = 0;
                likeVideos.Remove(selectedVideo);
            }
            else
            {
                using (var statement = Videoconn.Prepare("UPDATE Videos SET IfLike = ? WHERE Token = ?"))
                {
                    statement.Bind(1, 1);
                    statement.Bind(2, selectedVideo.Token);
                    statement.Step();
                }
                selectedVideo.IfLike = 1;
                likeVideos.Add(selectedVideo);
            }
        }

        public async Task<Video> Search(string fullPath)
        {
            Video video = null;
            using (var statement = Videoconn.Prepare("SELECT * FROM Videos WHERE Path = ?"))
            {
                statement.Bind(1, fullPath);
                while (statement.Step() == SQLiteResult.ROW)
                {
                    string token = (string)statement[0];
                    var file = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                                FutureAccessList.GetFileAsync(token);
                    BitmapImage thumbi = await Common.GetThumbnailOfVideo(file);
                    video = new Video((string)statement[0], (string)statement[1],
                                      (string)statement[2], (string)statement[3],
                                      (double)statement[4], (long)statement[5],
                                      (double)statement[6], (long)statement[7], (String)statement[8], thumbi);
                }
            }
            return video;
        }

        /*****************************Folder********************************/

        //Load Folder database
        public void LoadFolderDatabase()
        {
            // Get a reference to the SQLite database 
            Folderconn = new SQLiteConnection("sqlitepcldemo.db");
            string sql = @"CREATE TABLE IF NOT EXISTS
                            Folders (Token VARCHAR(140) PRIMARY KEY  NOT NULL,
                                    Name VARCHAR(140),
                                    NumOfItem INTEGER
                         );";
            using (var statement = Folderconn.Prepare(sql))
            {
                statement.Step();
            }
        }

        //read data from database to collection
        public void ReadFolderDatabase()
        {
            using (var statement = Folderconn.Prepare("SELECT * FROM Folders"))
            {
                while (statement.Step() == SQLiteResult.ROW)
                {
                    folders.Add(new Folder((string)statement[0], (string)statement[1],
                                      (long)statement[2]));
                }
            }
        }

        public void FolderAdd(String token, String name, long numOfItem)
        {
            folders.Add(new Folder(token, name, numOfItem));
            using (var custstmt = Folderconn.Prepare("INSERT INTO Folders(Token, Name, NumOfItem) VALUES (?, ?, ?)"))
            {
                custstmt.Bind(1, token);
                custstmt.Bind(2, name);
                custstmt.Bind(3, numOfItem);
                custstmt.Step();
            }
        }

        public void FolderRemove()
        {
            folders.Remove(selectedFolder);
            selectedFolder = null;
            using (var statement = Folderconn.Prepare("DELETE FROM Folders WHERE Token = ?"))
            {
                statement.Bind(1, selectedFolder.Token);
                statement.Step();
            }
        }

        public void FolderUpdate(string name, long numOfItem)
        {
            using (var custstmt = Folderconn.Prepare
                ("UPDATE Folders SET Name = ?, NumOfItem = ? WHERE Token = ?"))
            {
                custstmt.Bind(1, name);
                custstmt.Bind(2, numOfItem);
                custstmt.Bind(3, selectedFolder.Token);
                custstmt.Step();
            }

            selectedFolder.Name = name;
            selectedFolder.NumOfItem = numOfItem;
            selectedFolder = null;
        }
    }
}
