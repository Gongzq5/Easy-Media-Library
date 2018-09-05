using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mid.Model
{
    class History
    {
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
        }

        public History(String name, String date)
        {
            this.name = name;
            this.date = date;
        }
    }
}
