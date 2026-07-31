using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleParser
{
    public class reader1
    {
        /// <summary>
        /// Основной ридер расписания
        /// </summary>
        /// <param name="ds"></param>
        public static List<Zaniatie> MainReader(DataSet ds)
        {
            List<Zaniatie> List_Training = new List<Zaniatie>();
            for (int str = FindTime(ds)[0]; str < FindEnd(ds); str += FindHeight(ds))//организация прыжка по клеткам вертикали на основе высоты
                for (int stl = FindTime(ds)[1]; stl < FindEndStlb(ds); stl += FindWidth(ds))//организация прыжка по клеткам по горизонтали на основе ширины
                {
                    string bufer = "";
                    string gruppa = "";
                    string date = "";
                    int subgroup = 1;
                    List<string> Time = new List<string>();

                    for (int width = 0; width < FindWidth(ds); width++)
                        for (int height = 0; height < FindHeight(ds); height++)//считать все клетки, которые мы охватили и занести в буфер
                        {
                            object yach = ds.Tables[0].Rows[str - 1 + height][stl + width].ToString();
                            bufer += yach.ToString() + " ђ ";//Символ - разделитель 
                        }
                    bufer = bufer.Remove(bufer.LastIndexOf('ђ'));
                    bufer = bufer.Replace('\n', ' ');
                    bufer = Regex.Replace(bufer, "[,]+", " ");
                    bufer = Regex.Replace(bufer, @"\s+", " ");
                    bufer = bufer.Trim();

                    if ((bufer != "ђ") && (bufer.Length>3))//если клетка была не пустая, добавляем туда информацию о группе (из строки) и времени (из столбца) 
                    {
                        gruppa = ds.Tables[0].Rows[FindGrupp(ds)[0] - 1][stl].ToString();
                        if (gruppa == "")
                        {
                            gruppa = FindGroupName(ds, stl);
                            subgroup = 2;
                        }

                        for (int height = 0; height < FindHeight(ds); height++)
                        {
                            object yach = ds.Tables[0].Rows[str - 1 + height][FindTime(ds)[1] - 1].ToString();
                            Time.Add(yach.ToString());
                        }
                        date = getDate(ds, str).ToShortDateString();

                        //Ща будет мега костыль для случаев когда расписание двух подгрупп ебашат в одну клетку
                        if ((bufer.Split().Count(ch => ch.Contains('/')) == 2)&&(!bufer.Contains("УЛК-2")))//если в нашем буфере есть ровно два слеша (Дисциплина/дисциплина; Препод/препод), ПРИ ЭТОМ, ЭТО НЕ РАБОТАЕТ С ФИЗРОЙ!
                        {
                            var newbufer = bufer.Split(new char[] { '/', 'ђ' });

                            string bufer1 = newbufer[0] + " " + newbufer[2];//дисциплина и препод первой подгруппы
                            subgroup = 1;
                            List_Training.Add(new Zaniatie(gruppa, date, Time, bufer1, subgroup));

                            string bufer2 = newbufer[1] + " " + newbufer[3];//дисциплина и препод второй подгруппы
                            subgroup = 2;
                            List_Training.Add(new Zaniatie(gruppa, date, Time, bufer2, subgroup));

                        }
                        else //Если составители адекваты (Ха-ха)
                            List_Training.Add(new Zaniatie(gruppa, date, Time, bufer.Replace("ђ", ""), subgroup));//Заодно удалим разделитель, чтобы не мешался потом
                    }
                }
            List_Training = KostilSubbota(List_Training);//Лучше сюда не лезть, но если есть баги с субботой, то эт скорее всего здесь
            return List_Training;
        }
        /// <summary>
        /// Мега костыль для субботы, где пару пишут на 4 ячейки
        /// </summary>
        /// <param name="input">Входной массив пар</param>
        /// <returns>Выходной массив пар</returns>
        private static List<Zaniatie> KostilSubbota(List<Zaniatie> input)
        {
            List<Zaniatie> output = new List<Zaniatie>();
            Zaniatie bufer = null;
            foreach(var zaniat in input.OrderBy(o=>o.Time1).OrderBy(o=>o.Date).OrderBy(o=>o.Gruppa))//СОРТИРОВКА
            {
                if (zaniat.Date.DayOfWeek == DayOfWeek.Saturday)//ЕСЛИ СУББОТА
                {
                    if (KostilSubbotaCountError(zaniat) == 2) //если не хватает двух вещей
                    {
                        if (bufer != null)
                        {
                            output.AddRange(KostilSubbotaSum(zaniat, bufer));//добавляем чудо
                            bufer = null;
                        }
                        else
                            bufer = zaniat;//Иначе добавляем как есть
                    }
                    else
                        output.Add(zaniat);
                }
                else
                    output.Add(zaniat);//Возвращаем это говно
            }
            return output;//на выходе исправленный массив
        }

        private static int KostilSubbotaCountError(Zaniatie input)
        {
            int count = 0;
            if (input.PredmetData.Kabinet == "")
                count++;
            if (input.PredmetData.Prepod == "")
                count++;
            if (input.PredmetData.Vid == "")
                count++;
            if (input.PredmetData.Disc == "")
                count++;
            return count;
        }

        private static List<Zaniatie> KostilSubbotaSum(Zaniatie input1, Zaniatie input2)
        {
            List<Zaniatie> output = new List<Zaniatie>();
            if((input1.Date!=input2.Date)||(input1.Gruppa!=input2.Gruppa))
            {
                output.Add(input1);
                output.Add(input2);
                return output;
            }
            else
            {
                Zaniatie zan1 = input1;
                Zaniatie zan2 = input2;

                if (zan1.PredmetData.Disc == "")
                    zan1.PredmetData.Disc = input2.PredmetData.Disc;
                if (zan1.PredmetData.Kabinet == "")
                    zan1.PredmetData.Kabinet = input2.PredmetData.Kabinet;
                if (zan1.PredmetData.Prepod == "")
                    zan1.PredmetData.Prepod = input2.PredmetData.Prepod;
                if (zan1.PredmetData.Vid == "")
                    zan1.PredmetData.Vid = input2.PredmetData.Vid;

                if (zan2.PredmetData.Disc == "")
                    zan2.PredmetData.Disc = input1.PredmetData.Disc;
                if (zan2.PredmetData.Kabinet == "")
                    zan2.PredmetData.Kabinet = input1.PredmetData.Kabinet;
                if (zan2.PredmetData.Prepod == "")
                    zan2.PredmetData.Prepod = input1.PredmetData.Prepod;
                if (zan2.PredmetData.Vid == "")
                    zan2.PredmetData.Vid = input1.PredmetData.Vid;

                output.Add(zan1);
                output.Add(zan2);
                return output;
            }
        }

        /// <summary>
        /// Найти группу в заданном столбце
        /// </summary>
        /// <param name="ds">dataset, где требуется найти группу</param>
        /// <param name="stlb">Стобец, группа которого необходимо</param>
        /// <returns>Газвание группы</returns>
        public static string FindGroupName(DataSet ds, int stlb)
        {
            string currentGroup = "";
            DataRow Row = ds.Tables[0].Rows[FindGrupp(ds)[0] - 1];
            foreach (DataTable table in ds.Tables)//таблица
            {
                //Console.WriteLine("Таблица");
                for (int i = FindGrupp(ds)[1] - 1; i < 50; i += FindWidth(ds))
                {
                    DataColumn column = table.Columns[i];
                    string znach = Row[column].ToString();
                    if (znach != "")
                        currentGroup = znach;
                    if (stlb == i)
                        return currentGroup;
                }
            }
            return "Неизвестная группа";
        }


        /// <summary>
        /// Найти последний столбец
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>id столбца</returns>
        private static int FindEndStlb(DataSet ds)
        {
            DataRow Row = ds.Tables[0].Rows[FindGrupp(ds)[0] - 1];

            foreach (DataTable table in ds.Tables)//таблица
            {
                //Console.WriteLine("Таблица");
                for (int i = FindGrupp(ds)[1] - 1; i < 50; i += FindWidth(ds))
                {
                    DataColumn column = table.Columns[i];
                    DataColumn column2 = table.Columns[i+1];

                    string znach = Row[column].ToString();
                    string znach2 = Row[column2].ToString();
                    if ((znach == "") && (znach2 == ""))
                        return i+1;
                }
            }
            return 0;
        }
        /// <summary>
        /// Получить дату по строке
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <param name="str">Строка</param>
        /// <returns>Дата для этой строки</returns>
        private static DateTime getDate(DataSet ds, int str)
        {
            DateTime OutDate=DateTime.MinValue;
            string Previous_Date = "";
            foreach (DataTable table in ds.Tables)//таблица
            {
                int intRow = 0;
                foreach (DataRow row in table.Rows)//строка
                {
                    intRow++;
                    string znach = row[FindDate(ds) - 1].ToString().Trim(); ;
                    if ((znach == "Понедельник") || (znach == "Вторник") || (znach == "Среда") || (znach == "Четверг") || (znach == "Пятница") || (znach == "Суббота"))
                    {
                        if (intRow > str)
                        {
                            if(DateTime.TryParse(Previous_Date, out OutDate))
                                return OutDate;
                            else
                            {
                                try
                                {
                                    DateTime.TryParse(Previous_Date.Substring(0, Previous_Date.Length - 1), out OutDate);
                                }
                                catch
                                {
                                    Console.WriteLine("Костыли - это плохо! ОЧЕНЬ");
                                }
                                return OutDate;
                            }
                        }
                        Previous_Date = table.Rows[intRow][FindDate(ds) - 1].ToString().Trim();
                    }
                    if (isDate(znach))
                    {
                        if (Previous_Date == "")
                            Previous_Date = znach;

                        if (intRow >= str)
                            if (DateTime.TryParse(Previous_Date, out OutDate))
                                return OutDate;
                            else
                            {
                                try
                                {
                                    DateTime.TryParse(Previous_Date.Substring(0, Previous_Date.Length - 1), out OutDate);
                                }
                                catch
                                {
                                    Console.WriteLine("Костыли - это плохо! ОЧЕНЬ");
                                }
                                return OutDate;
                            }
                        else Previous_Date = znach;
                    }
                }
            }

            if (DateTime.TryParse(Previous_Date, out OutDate))
                return OutDate;
            else
            {
                try
                {
                    DateTime.TryParse(Previous_Date.Substring(0, Previous_Date.Length - 1), out OutDate);
                }
                catch
                {
                    Console.WriteLine("Костыли - это плохо! ОЧЕНЬ");
                }
                return OutDate;
            }
        }

        /// <summary>
        /// Найти высоту ячейки
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>Высота</returns>
        private static int FindHeight(DataSet ds)
        {
            return 2;
        }
        /// <summary>
        /// Найти ширину ячейки
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>Ширина</returns>
        private static int FindWidth(DataSet ds)
        {
            return 1;
        }
        /// <summary>
        /// Найти последнюю строку (Основываясь на времени)
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>id последней строки(+1)</returns>
        private static int FindEnd(DataSet ds)
        {
            int output = 0;
            int str = 0;
            int stlb = FindTime(ds)[1];

            foreach (DataTable table in ds.Tables)//таблица
            {
                foreach (DataRow row in table.Rows)//строка
                {
                    str++;
                    string znach = row[stlb - 1].ToString();
                    if (isTime(znach))
                    {
                        output = str;
                    }
                }
            }
            return output;
        }
        /// <summary>
        /// Найти координаты первой клетки с группами (основываясь на словаре)
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>Массив из двух значений строка/столбец</returns>
        private static int[] FindGrupp(DataSet ds)
        {
            foreach (DataTable table in ds.Tables)//таблица
            {
                int str = 0;
                foreach (DataRow row in table.Rows)//строка
                {
                    str++;
                    int stlb = 0;
                    foreach (DataColumn column in table.Columns)//ячейка (столбец)
                    {
                        stlb++;
                        string znach = row[column].ToString();
                        if (znach != String.Empty)
                        {

                            if (isGrup(znach))
                            {
                                //Console.WriteLine("Группа в строке-" + str.ToString());
                                int[] output = new int[] { str, stlb };
                                return output;
                            }
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// Найти координаты первой клетки с временем
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>Массив из двух значений строка/столбец</returns>
        private static int[] FindTime(DataSet ds)
        {
            foreach (DataTable table in ds.Tables)//таблица
            {
                //Console.WriteLine("Таблица");
                int stlb = 0;
                foreach (DataColumn column in table.Columns)//столбец
                {
                    stlb++;
                    int str = 0;
                    foreach (DataRow row in table.Rows)//строка
                    {
                        str++;
                        string znach = row[column].ToString();
                        if (znach != String.Empty)
                        {
                            if (isTime(znach))
                            {
                                //Console.WriteLine("Время в столбце -" + st.ToString());
                                int[] output = new int[] { str, stlb };
                                return output;
                            }

                        }
                    }

                }
            }
            return null;
        }
        /// <summary>
        /// Найти столбец с временем
        /// </summary>
        /// <param name="ds">В каком dataset</param>
        /// <returns>id столбца с временем (+1)</returns>
        public static int FindDate(DataSet ds)
        {
            foreach (DataTable table in ds.Tables)//таблица
            {
                //Console.WriteLine("Таблица");
                int st = 0;
                foreach (DataColumn column in table.Columns)//столбец
                {
                    st++;
                    foreach (DataRow row in table.Rows)//строка
                    {
                        string znach = row[column].ToString().Trim();

                        if (znach != String.Empty)
                        {
                            if (isDate(znach) && (!isTime(znach)))
                            {
                                //Console.WriteLine("Дата в столбце -" + st.ToString());
                                return st;
                            }
                        }
                    }

                }

            }
            return 0;
        }
        /// <summary>
        /// Проверка на группу
        /// </summary>
        /// <param name="znach">Входящая строка</param>
        /// <returns>true если группа</returns>
        private static bool isGrup(string znach)
        {
            string output = "";
            if (Program.dict.TryGetValue(znach.Trim().ToLower(), out output))
                if (output == "Группа")
                    return true;
            return false;
        }
        /// <summary>
        /// Проверка на время
        /// </summary>
        /// <param name="znach">Входящая строка</param>
        /// <returns>true если время</returns>
        public static bool isTime(string znach)
        {
            Regex rgx = new Regex(@"([0-1]{0,1}[0-9]):([0-5]\d)");
            return rgx.IsMatch(znach);
        }
        /// <summary>
        /// Проверка на дату (иногда принимает ячейки с временем за дату) - Больше не принимает,  if (!isTime(znach))-справился
        /// </summary>
        /// <param name="znach">Входящая строка</param>
        /// <returns>true если дата</returns>
        public static bool isDate(string znach)
        {
            if (!isTime(znach))
            {
                DateTime OUTdate = new DateTime();
                bool value = DateTime.TryParse(znach, out OUTdate);
                if (!value)
                    try
                    {
                        value = DateTime.TryParse(znach.Substring(0, znach.Length - 1), out OUTdate); //Новый костыль (убрать точку вконце у 04.09.)
                    }
                    catch
                    {
                        //Этот костыль исправно работает уже 3 недели, значит это было техническое решение
                    }
                return value;
            }
            return false;
        }
    }
}
