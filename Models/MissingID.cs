using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDChecker.Models
{
    public class MissingID
    {
        public string ElementType { get; set; }
        public int LineNumber { get; set; }
        public string XML { get; set; }

        public MissingID(string type, int ln, string xml)
        {
            this.ElementType = type;
            this.LineNumber = ln;
            this.XML = xml;
        }
    }
}
