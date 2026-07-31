using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{

    public class Zaniatie
    {
        //static string CurrentG;

        public DateTime Date;
        public DateTime Time1;
        public DateTime Time2;

        public string Gruppa;
        public string TimeStart;
        public string TimeEnd;
        public Yacheyka PredmetData;
        public int subgr;

        //private List<string> time;
        //private string bufer;
        

        public Zaniatie(string gruppa, string date, List<string> time, string bufer, int subgroup)
        {
            Gruppa = gruppa.Trim();
            PredmetData = new Yacheyka(bufer);
            subgr = subgroup;
            TimeStart = time.First().Trim();
            TimeEnd = time.Last().Trim();
            if (TimeEnd.Contains('-'))
                TimeEnd = TimeEnd.Split('-')[1];
            else if (TimeStart.Contains('-'))
                TimeEnd = TimeStart.Split('-')[1];
            TimeStart = TimeStart.Split('-')[0];
            DateTime.TryParse(TimeStart, out Time1);
            DateTime.TryParse(TimeEnd, out Time2);

            if (!DateTime.TryParse(date, out Date))
                Date = DateTime.Parse("1500, 1,1");
        }

        public Zaniatie(string gruppa, DateTime date, List<string> time, string bufer, int subgroup, bool isexam)
        {
            Gruppa = gruppa.Trim();
            PredmetData = new Yacheyka(bufer);
            subgr = subgroup;
            Date = date;

            DateTime.TryParse(PredmetData.Unknown_Words.Replace("ИС", "").Replace(". ", ":").Trim(), out Time1);//Должно помочь
            Time2 = Time1.AddHours(3);
            
        }

        public string getDate()
        {
            return (String.Format("{0, 8}|{1,12}|{2,15}|{3, 15}|{4, 50}|{5, 5}|{6, 20}|{7, 10}|", Date, Gruppa, TimeStart, TimeEnd, PredmetData.Disc, PredmetData.Vid, PredmetData.Prepod, PredmetData.Kabinet));
        }
    }
}
