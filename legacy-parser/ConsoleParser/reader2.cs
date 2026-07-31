using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{
    class reader2
    {
        /// <summary>
        /// Основной ридер расписания
        /// </summary>
        /// <param name="ds"></param>
        public static List<Zaniatie> MainReader(DataSet ds)
        {
            List<Zaniatie> List_Training = new List<Zaniatie>();
            for (int str = FindTime(ds)[0]; str < FindEnd(ds)+2; str += FindHeight(ds))//организация прыжка по клеткам горизонтали на основе ширины
                for (int stl = FindTime(ds)[1]; stl < FindEndStlb(ds); stl += FindWidth(ds))//организация прыжка по клеткам по вертикали на основе высоты
                {
                    string bufer = "";
                    string gruppa = "";
                    string date = "";
                    List<string> Time = new List<string>();

                    for (int width = 0; width < FindWidth(ds); width++)
                        for (int height = 0; height < FindHeight(ds); height++)//считать все клетки, которые мы охватили и занести в буфер
                        {
                            object yach = ds.Tables[0].Rows[str - 1 + height][stl + width].ToString();
                            bufer += yach.ToString() + " ";
                        }

                    bufer = bufer.Replace('\n', ' ');
                    bufer = Regex.Replace(bufer, "[,]+", " ");
                    bufer = Regex.Replace(bufer, @"\s+", " ");
                    bufer = bufer.Trim();

                    if (bufer != "")//если клетка была не пустая, добавляем туда информацию о группе (из строки) и времени (из столбца) 
                    {
                        gruppa = ds.Tables[0].Rows[FindGrupp(ds)[0] - 1][stl].ToString();
                        for (int height = 0; height < FindHeight(ds); height++)
                        {
                            object yach = ds.Tables[0].Rows[str - 1 + height][FindTime(ds)[1] - 1].ToString();
                            Time.Add(yach.ToString().Replace('.', ':'));
                        }
                        date = getDate(ds, str);
                        //Console.WriteLine(bufer + " [" + str + "] [" + (stl + 1) + "]");
                        //Console.WriteLine("Сопроводительная информация: ");
                        //Console.WriteLine(date);
                        //Console.Write(gruppa + " |");
                        //foreach (string time in Time)
                        //    Console.Write(time + "| ");
                        //Console.WriteLine("\n ===========");
                        List_Training.Add(new Zaniatie(gruppa, date, Time, bufer, 1));
                    }
                }
            return List_Training;
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
                for (int i = FindGrupp(ds)[1] - 1; i < ds.Tables[0].Columns.Count; i += FindWidth(ds)) //Более изящное решение
                {
                    DataColumn column = table.Columns[i];

                    string znach = Row[column].ToString();
                    if (znach == "")
                        return i;
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
        private static string getDate(DataSet ds, int str)
        {
            string Previous_Date = "";
            foreach (DataTable table in ds.Tables)//таблица
            {
                int intRow = 0;
                foreach (DataRow row in table.Rows)//строка
                {
                    intRow++;
                    string vozm = row[FindDate(ds) - 1].ToString();
                    string ishod = vozm.Replace('\n', ' ');
                    ishod = Regex.Replace(ishod, "[,]+", " ");
                    ishod = Regex.Replace(ishod, @"\s+", " ");
                    ishod = ishod.Trim();
                    string[] dates = ishod.Split(' ');
                    foreach (string znach in dates)
                    if (isDate(znach))
                    {
                        if (Previous_Date == "")
                            Previous_Date = znach;
                        if (intRow > str)
                            return Previous_Date;
                        else Previous_Date = znach;
                    }
                }
            }
            return Previous_Date;
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
            return 2;
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
        private static int FindDate(DataSet ds)
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
                        string znach = row[column].ToString();
                        if (znach != String.Empty)
                        {
                            string[] slovo = znach.Split(' ');
                            foreach (string str in slovo)
                            {
                                string ishod = str.Replace('\n', ' ');
                                ishod = Regex.Replace(ishod, "[,]+", " ");
                                ishod = Regex.Replace(ishod, @"\s+", " ");
                                ishod = ishod.Trim();
                                if (isDate(ishod) && (!isTime(znach)))
                                {
                                    //Console.WriteLine("Дата в столбце -" + st.ToString());
                                    return st;
                                }
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
            if (Program.dict.TryGetValue(znach.ToLower(), out output))
                if (output == "Группа")
                    return true;
            return false;
        }
        /// <summary>
        /// Проверка на время
        /// </summary>
        /// <param name="znach">Входящая строка</param>
        /// <returns>true если время</returns>
        private static bool isTime(string znach)
        {
            Regex rgx = new Regex(@"([0-1]?\d|2[0-3])(.[0-5]\d)-([0-1]?\d|2[0-3])(.[0-5]\d)");
            bool ff = rgx.IsMatch(znach);
            return rgx.IsMatch(znach);
        }
        /// <summary>
        /// Проверка на дату (иногда принимает ячейки с временем за дату)
        /// </summary>
        /// <param name="znach">Входящая строка</param>
        /// <returns>true если дата</returns>
        private static bool isDate(string znach)
        {
            DateTime OUTdate = new DateTime();
            return DateTime.TryParse(znach, out OUTdate);
        }
    }
}
