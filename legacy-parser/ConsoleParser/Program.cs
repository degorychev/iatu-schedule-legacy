using System;
using System.Data;
using System.IO;
using Excel;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.Net;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using SyslogLogging;


namespace ConsoleParser
{
    public class Program
    {
        WriterDB writer = new WriterDB();
        public string curentDate;
        //static public Dictionary<string, string> dict = SerializableDictionary<string, string>.deserializoving(); //Скорее всего это ОЧЕНЬ не правильно
        static public Dictionary<string, string> dict = new Dictionary<string, string>();//Помнишь я говорил, что словарь это экземпляр ненужного унаследованого класса, так вот, это все таки не так, класс можно выпиливать, но зачем?

        static public LoggingModule logs = new LoggingModule(
           allsetting.Default.Log_Server,            // сервер
           514,                                      // порт
           allsetting.Default.Logging,               // вывод в консоль
           LoggingModule.Severity.Info,             // минимум для отправки (?)
           false,                                    // асинхронная отправка
           true,                                     // таймштамп
           true,                                     // серьезность
           true,                                     // включать хостнейм
           false,                                     // Отступ по глубине стека
           true);

        static void Main(string[] args)
        {
            logs.Log(LoggingModule.Severity.Info ,"Запуск программы, десериализация словаря");
            Create_Dictionary();


            try
            {
                if (args.Length > 0)//Это фича, программу можно запустить с аргументом в виде пути к файлу, и программа отпарсит только его
                {
                    logs.Log(LoggingModule.Severity.Info, "Чтение только одного файла");
                    string file = args[0];
                    int type = Convert.ToInt32(args[1]);
                    List<Zaniatie> Raspisanie = StartRead(file, type);
                    Console.Beep(400, 400);
                    WriterDB.WriteToDB(Raspisanie, file, getMD5(file));
                    if (allsetting.Default.After_Delete_File)
                    {
                        logs.Log(LoggingModule.Severity.Info, "Удаляю файл: " + file);
                        File.Delete(file);
                    }
                }
                else
                {
                    string[] dirs = Directory.GetFiles(allsetting.Default.homedir + "/files/ochn", "*");
                    logs.Log(LoggingModule.Severity.Info, "Количество файлов для парсинга (очники): " + dirs.Length);
                    foreach (string dir in dirs)
                    {
                        string file = dir;
                        List<Zaniatie> Raspisanie = StartRead(file, 1);
                        Console.Beep(400, 400);
                        WriterDB.WriteToDB(Raspisanie, file, getMD5(file));
                        if (allsetting.Default.After_Delete_File)
                        { 
                            string newFile = allsetting.Default.homedir + "/files/old/ochn/" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss") + ".xls";
                            logs.Log(LoggingModule.Severity.Info, "Перемещаю файл: " + file + " в " + newFile);
                            File.Move(file, newFile);
                        }
                    }

                    dirs = Directory.GetFiles(allsetting.Default.homedir + "/files/zaochn", "*");
                    logs.Log(LoggingModule.Severity.Info, "Количество файлов для парсинга (заочники): " + dirs.Length);
                    foreach (string dir in dirs)
                    {
                        string file = dir;
                        List<Zaniatie> Raspisanie = StartRead(file, 2);
                        Console.Beep(400, 400);
                        WriterDB.WriteToDB(Raspisanie, file, getMD5(file));
                        if (allsetting.Default.After_Delete_File)
                        {
                            string newFile = allsetting.Default.homedir + "/files/old/zaochn/" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss") + ".xls";
                            logs.Log(LoggingModule.Severity.Info, "Перемещаю файл: " + file + " в " + newFile);
                            File.Move(file, newFile);
                        }
                    }

                    dirs = Directory.GetFiles(allsetting.Default.homedir + "/files/exam/ochn", "*");
                    logs.Log(LoggingModule.Severity.Info, "Количество файлов для парсинга (экзамены очников): " + dirs.Length);
                    foreach (string dir in dirs)
                    {
                        string file = dir;
                        List<Zaniatie> Raspisanie = StartRead(file, 3);
                        Console.Beep(400, 400);
                        WriterDB.WriteToDB(Raspisanie, file, getMD5(file));
                        if (allsetting.Default.After_Delete_File)
                        {
                            string newFile = allsetting.Default.homedir + "/files/old/zaochn/" + DateTime.Now.ToString("yyyy.MM.dd_HH-mm-ss") + ".xls";
                            logs.Log(LoggingModule.Severity.Info, "Перемещаю файл: " + file + " в " + newFile);
                            File.Move(file, newFile);
                        }
                    }
                }
            }
            catch (Exception e)
            {               
                logs.LogException("Чтение файла", "main", e);
            }

            logs.Log(LoggingModule.Severity.Info, "Чтение завершено");
            //Console.Beep(2000, 2000);
            Console.ReadKey();
        }

        public static void Create_Dictionary()
        {
            MySqlConnection conn = new MySqlConnection(WriterDB.ConnectionString); //строку подключения можно вытащить из WriterDB
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;
                logs.Log(LoggingModule.Severity.Info, "Десериализация групп");
                comm.CommandText = "SELECT `naimenovanie` FROM `groups`;";
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    dict.Add(result.ToLower(), "Группа");
                    logs.Log(LoggingModule.Severity.Debug, result);
                }
                MyDataReader.Close();

                logs.Log(LoggingModule.Severity.Info, "Десериализация аудиторий");
                comm.CommandText = "SELECT `auditoria` FROM `auditorii`;";
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    dict.Add(result.ToLower(), "Кабинет");
                    logs.Log(LoggingModule.Severity.Debug, result);
                }
                dict.Add("УК-2", "Кабинет");
                MyDataReader.Close();

                logs.Log(LoggingModule.Severity.Info, "Десериализация видов");
                comm.CommandText = "SELECT `naimenovanie` FROM `tip`;";
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    dict.Add(result.ToLower(), "Вид");
                    logs.Log(LoggingModule.Severity.Debug, result);
                }
                MyDataReader.Close();

                logs.Log(LoggingModule.Severity.Info, "Десериализация занятий");
                comm.CommandText = "SELECT `naimenovanie` FROM `disciplines`;";
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    dict.Add(result.ToLower(), "Предмет");
                    logs.Log(LoggingModule.Severity.Debug, result);
                }
                MyDataReader.Close();

                logs.Log(LoggingModule.Severity.Info, "Десериализация преподавателей");
                comm.CommandText = "SELECT `FIO` FROM `prepodovatel`;";
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    dict.Add(result.ToLowerInvariant(), "Преподаватель");
                    logs.Log(LoggingModule.Severity.Debug, result);
                }
                MyDataReader.Close();
            }
            catch (Exception e)
            {
                logs.LogException("Десериализация словаря", "Create_Dictionary()", e);
            }
            finally
            {
                logs.Log(LoggingModule.Severity.Info, "Десериализация закончена");
                conn.Close();
            }

            //Костыли для заочников, у них вообще самые больные файлы.
            dict.Add("1 подгр.".ToLowerInvariant(), "Дополнительно");//Смотреть файл Yacheyka.cs, функция addValue()
            dict.Add("2 подгр.".ToLowerInvariant(), "Дополнительно");
            dict.Add("1подгр.".ToLowerInvariant(), "Дополнительно");
            dict.Add("2подгр.".ToLowerInvariant(), "Дополнительно");//Работает, не трогай.
        }


        /// <summary>
        /// Начать чтение
        /// </summary>
        /// <param name="FileName">Путь до файла</param>
        /// <returns>Список с занятиями</returns>
        private static List<Zaniatie> StartRead(string FileName, int readertype)
        {
            logs.Log(LoggingModule.Severity.Warn, "Начато чтение файла: " + FileName);

            DataSet ds;
            using (var stream = new FileStream(FileName, FileMode.Open))
            {
                IExcelDataReader reader = null;
                reader = ExcelReaderFactory.CreateBinaryReader(stream);

                if (reader == null)
                    return null;
                ds = reader.AsDataSet();
            }


            List<Zaniatie> output = new List<Zaniatie>();
            switch(readertype)
            {
                case 1:
                    output = reader1.MainReader(ds);
                    break;
                case 2:
                    output = reader2.MainReader(ds);
                    break;
                case 3:
                    output = reader3.MainReader(ds);
                    break;
            }
            return output;
        }

        private static string getMD5(string FileName)
        {
            using (FileStream fs = System.IO.File.OpenRead(FileName))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                return BitConverter.ToString(checkSum).Replace("-", String.Empty);
            }
        }
    }
}
