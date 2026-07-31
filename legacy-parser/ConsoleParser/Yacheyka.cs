using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ConsoleParser
{
    // Legacy note:
    // Этот класс оставлен почти в исходном виде как пример эвристического разбора
    // "визуального" Excel-расписания. Главная идея: найти в строке самое длинное
    // совпадение из доменного словаря, вырезать его и продолжить разбор остатка.
    public class Yacheyka
    {
        public String Prepod;
        public String Disc;
        public String Kabinet;
        public String Vid;

        public int subgr=3;

        public string Unknown_Words;
        public bool ready = false;
        //public String ToString()
        //{
        //    return Prepod + " " + Disc + " " + Vid + " " + Kabinet;
        //}
        public Yacheyka(string inputBufer)
        {
            Prepod = "";
            Disc = "";
            Kabinet = "";
            Vid = "";

            FindD(inputBufer);
        }
        private void addValue(string data, string direction)
        {
            if (direction == "Преподаватель")
            {
                //                Console.WriteLine("Добавлен преподаватель");
                Prepod = NameToLower(data);
            }
            else if (direction == "Предмет")
            {
                //                Console.WriteLine("Добавлен предмет");
                Disc = data;
            }
            else if (direction == "Кабинет")
            {
                //                Console.WriteLine("Добавлен кабинет");
                Kabinet = data;
            }
            else if (direction == "Вид")
            {
                //                Console.WriteLine("Добавлен тип");
                Vid = data;
            }
            else if (direction == "Дополнительно")
            {
                if ((data == "1 подгр.") || (data == "1подгр."))
                    subgr = 1;
                else if ((data == "2 подгр.") || (data == "2подгр."))
                    subgr = 2;
                else subgr = 3;
            }
        }

        /// <summary>
        /// Функция поиска подстроки с разбивкой в класс ячеек
        /// 
        /// В общем, тут основная магия происходит, функция пытается найти САМОЕ ДЛИННОЕ совпадение части выражения из ячейки со значением из словаря
        /// Кароче, она разбивает предложение "Раз Два Три Четыре Пять" на: 
        /// 1 итерация: "Раз Два Три Четыре Пять"
        /// 2 итерация: "Раз Два Три Четыре" и "Два Три Четыре Пять"
        /// 3 итерация: "Раз Два Три" и "Два Три Четыре" и "Три Четыре Пять"
        /// 4 итерация: "Раз Два" и "Два Три" и "Три Четыре" и "Четыре Пять"
        /// 5 итерация: "Раз" и "Два" "Три" и "Четыре" и "Пять"
        /// 
        /// Когда находится совпадение - запускается функция cut, в глобальные переменные записывается что это такое, из начального предложения выдергивается совпадение и функция запускается заново (Да, она рекурсивна, до тех  пор, пока не дойдет до одного слова)
        /// При этом, это помогает не забыть про "Экономика предприятий", найдя предмет только "Экономика"
        /// Однако, "Экономика предприятий" должно быть в словаре, иначе найдется только "Экономика", а "предприятий" запишется в переменную Unknown_Words и далее в базу данных
        /// </summary>
        /// <param name="input">Входная строка</param>
        /// <returns>Истина  если закончили парсить</returns> //Какой то костыль, не помню почему, работает как обычный void, может хотел false возвращать, если произошла какая то ошибка, хз
        private bool FindD(string input)
        {
            input = input.Replace(".", ". ");
            input = input.Replace("(", " (");
            input = input.Replace(")", ") ");
            input = input.Replace("/", "/ ");
            input = Regex.Replace(input, "[,]+", " ");
            input = Regex.Replace(input, @"\s+", " ");
            input = input.Trim();

            Unknown_Words = input;
            if (Unknown_Words == "")
                ready = true;
            
            string[] ms = input.Split(' ');//Массив Слов
            int SubStringCount = ms.Count();
            String bstr = "";
            int perDel = ms.Length + 1;//ПЕРеменная ДЕЛения, не надо тут!
            do
            {
                perDel--;
                int h = 0;
                bstr = "";

                int j = 0;
                for (int ii = 0; ii < SubStringCount; ii++)
                {
                    h++;
                    bstr += ms[ii] + " ";
                    if (h == perDel)
                    {
                        bstr = bstr.Trim();
                        ++j;

                        string output = "";
                        if (Program.dict.TryGetValue(bstr.ToLower().Replace('ё', 'е'), out output))
                        {

                            addValue(bstr, output);

                            FindD(Cut(input, bstr));
                            perDel = int.MinValue;
                            break;
                        }
                        bstr = "";
                        h = 0;
                        ii = ii - perDel + 1;
                    }
                }
            } while (perDel > 1);
            return true;
        }
        private static string Cut(string input, string substring)
        {
            string output = "";
            try
            {
                output = input.Replace(substring, "");
            }
            catch(Exception e)
            {
                Console.WriteLine(input + "НЛО");
            }
            return output;
        }

        /// <summary>
        /// Восстановить ФИО преподавателя
        /// 
        /// Дело в том, что в словаре ФИО преподов всегда в нижнем регистре, это нужно было чтобы придти к какомоту единому варианту, (ибо у заочников преподы например написаны капсом. Люблю файлы заочников ( ͡° ͜ʖ ͡° ))
        /// </summary>
        /// <param name="FIO">ФИО преподавателя в нижнем регистре</param>
        /// <returns>Восставленный вариант ФИО (первые буквы заглавные)</returns>
        private static string NameToLower(string FIO)//Не знаю почему функция называется так, сюдя по содержанию они делает именно это. И да, я не помню где восстанавливается заглавная буква отчества, скорее всего, уже в базе данных.
        {
            char[] prep = FIO.ToLower().ToCharArray();
            prep[0] = Char.ToUpper(prep[0]);
            string[] output = new string(prep).Split(' ');
            if (output.Length > 1)
                output[1] = output[1].ToUpper();
            return String.Join(" ", output);
        }
    }
}
