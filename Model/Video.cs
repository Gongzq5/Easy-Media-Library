using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Mid.Model
{
    class Video : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private String name;
        public String Name
        {
            get { return this.name; }
        }

        // format: yyyy/mm/dd
        private String date;
        public String Date
        {
            get { return this.date; }
            set
            {
                if (value != date)
                {
                    date = value;
                    OnPropertyChanged();
                }
            }
        }

        private String token;
        public String Token
        {
            get { return this.token; }
        }

        private String notePath;
        public String NotePath
        {
            get { return this.notePath; }
            set
            {
                if (value != notePath)
                {
                    notePath = value;
                    OnPropertyChanged();
                }
            }
        }

        private double position;
        public double Position
        {
            get { return this.position; }
            set
            {
                if (value != position)
                {
                    position = value;
                    OnPropertyChanged();
                }
            }
        }

        private String path;
        public String Path
        {
            get { return this.path; }
        }

        private long ifLike;
        public long IfLike
        {
            get { return this.ifLike; }
            set
            {
                if (value != ifLike)
                {
                    ifLike = value;
                    OnPropertyChanged();
                }
            }
        }

        private double stars;
        public double Stars
        {
            get { return this.stars; }
            set
            {
                if (value != stars)
                {
                    stars = value;
                    OnPropertyChanged();
                }
            }
        }

        private long historyShow;
        public long HistoryShow
        {
            get { return this.historyShow; }
            set
            {
                if (value != historyShow)
                {
                    historyShow = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapImage thumbi;
        public BitmapImage Thumbi
        {
            get { return this.thumbi; }
            set
            {
                if (value != thumbi)
                {
                    thumbi = value;
                    OnPropertyChanged();
                }
            }
        }

        public Video(String token, String name, String date, String notePath,
                        double position, long ifLike, double stars, long historyShow, String path, BitmapImage thumbi)
        {
            this.name = name;
            this.date = date;
            this.token = token;
            this.notePath = notePath;
            this.position = position;
            this.path = path;
            this.stars = stars;
            this.historyShow = historyShow;
            this.ifLike = ifLike;
            this.thumbi = thumbi;
        }
    }
}
