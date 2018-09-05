using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Mid.Model
{
    public class Folder : INotifyPropertyChanged
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
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        private long numOfItem;
        public long NumOfItem
        {
            get { return this.numOfItem; }
            set
            {
                if (value != numOfItem)
                {
                    numOfItem = value;
                    OnPropertyChanged();
                }
            }

        }

        private String token;
        public String Token
        {
            get { return this.token; }

        }
        public Folder(String token, String name, long num)
        {
            this.name = name;
            this.numOfItem = num;
            this.token = token;
        }
    }
}
