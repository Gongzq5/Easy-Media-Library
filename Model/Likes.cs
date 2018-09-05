using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mid.Model
{
    class Likes
    {
        private String name;
        public String Name
        {
            get { return this.name; }
        }

        private String date;
        public String Date
        {
            get { return this.date; }
        }

        private int stars;
        public int Stars
        {
            get { return this.stars; }
        }

        public Likes(String name, String date, int stars)
        {
            this.name = name;
            this.date = date;
            this.stars = stars;
        }
    }
}
