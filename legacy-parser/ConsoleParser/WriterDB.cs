using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SyslogLogging;

namespace ConsoleParser
{
    // Legacy note:
    // Запись результата парсинга в нормализованную MySQL-базу. Для 2017 года это
    // была рабочая часть production-пайплайна: дедупликация по MD5, пометка старых
    // файлов как неактуальных и параметризованные SQL-запросы.
    public class WriterDB
    {
        static MySqlConnectionStringBuilder mysqlCSB;
        /// <summary>
        /// Формирование строки подключения
        /// </summary>
        public static void SetConnectionString()
        {
            mysqlCSB = new MySqlConnectionStringBuilder();
            mysqlCSB.Server = allsetting.Default.Server;
            //mysqlCSB.Server = "db.example.local";
            mysqlCSB.Database = allsetting.Default.Database;
            //mysqlCSB.Database = "raspisanie";
            mysqlCSB.UserID = allsetting.Default.User;
            mysqlCSB.Password = allsetting.Default.Password;
            mysqlCSB.CharacterSet = "utf8";
        }
        public static string ConnectionString
        {
            get
            {
                if (mysqlCSB == null) //условие нуждается в проверке
                    SetConnectionString();
                return mysqlCSB.ConnectionString;
            }
        }
        /// <summary>
        /// Отправить занятия в базу данных
        /// </summary>
        /// <param name="Input">list С занятиями</param>
        /// <param name="fileName">Путь до файла</param>
        /// <param name="MD5">кеш сумма, для проверки уже наличия этого файла в базе данных</param>
        public static void WriteToDB(List<Zaniatie> Input, string fileName, string MD5)
        {
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                int status = 0;
                if (!File_Read(MD5))
                    foreach (Zaniatie DatStr in Input)
                    {
                        Program.logs.Log(LoggingModule.Severity.Info, "Прогресс: " + ++status + "/" + Input.Count + " (" + Math.Round((double)status/Input.Count, 2)*100 + "%)");

                        string hash;
                        if (Layering(DatStr, out hash))
                        {
                            not_relevant(hash);
                            Program.logs.Log(LoggingModule.Severity.Warn, "Это обновленная версия расписания");
                        }
                        comm.CommandText = "INSERT INTO class(`date`, `group`, `time_start`, `time_stop`, `discipline`, `tip`, `teacher`, `kab`, `subgroup`, `file`, `hash`, `modulas`, `Ready`, `date_of_update`) VALUES (?date, ?class, ?timeStart, ?timeStop, ?discipline, ?type, ?teacher, ?cabinet, ?subgr, ?file, ?hash, ?modulas, ?Ready, ?date_of_update);";
                        comm.Parameters.Add("?date", MySqlDbType.Date).Value = DatStr.Date.Date;
                        comm.Parameters.Add("?class", MySqlDbType.Int32).Value = GetGrupID(DatStr.Gruppa);
                        comm.Parameters.Add("?timeStart", MySqlDbType.Time).Value = DatStr.Time1.TimeOfDay;
                        comm.Parameters.Add("?timeStop", MySqlDbType.Time).Value = DatStr.Time2.TimeOfDay;
                        comm.Parameters.Add("?discipline", MySqlDbType.Int32).Value = GetDiscID(DatStr.PredmetData.Disc);
                        comm.Parameters.Add("?type", MySqlDbType.Int32).Value = GetTypeID(DatStr.PredmetData.Vid);
                        comm.Parameters.Add("?teacher", MySqlDbType.Int32).Value = GetPrepodID(DatStr.PredmetData.Prepod);
                        comm.Parameters.Add("?cabinet", MySqlDbType.Int32).Value = GetKabID(DatStr.PredmetData.Kabinet);
                        int subgroup = DatStr.subgr;
                        if (DatStr.PredmetData.subgr != 3)
                            subgroup = DatStr.PredmetData.subgr;
                        comm.Parameters.Add("?subgr", MySqlDbType.Int32).Value = subgroup;
                        comm.Parameters.Add("?file", MySqlDbType.VarChar).Value = fileName;
                        comm.Parameters.Add("?hash", MySqlDbType.VarChar).Value = MD5;
                        comm.Parameters.Add("?modulas", MySqlDbType.VarChar).Value = DatStr.PredmetData.Unknown_Words;
                        comm.Parameters.Add("?Ready", MySqlDbType.Int16).Value = DatStr.PredmetData.ready;
                        comm.Parameters.Add("?date_of_update", MySqlDbType.DateTime).Value = DateTime.Now;

                        if (DatStr.PredmetData.Unknown_Words != "")
                        {
                            Program.logs.Log(LoggingModule.Severity.Error, "Неизвестность: " + DatStr.PredmetData.Unknown_Words);
                        }
                        comm.ExecuteNonQuery();
                        comm.Parameters.Clear();
                    }
                else
                    Program.logs.Log(LoggingModule.Severity.Info, "Этот файл уже есть в базе данных");
            }
            catch (Exception e)
            {
                Program.logs.LogException("База данных", "Запись", e);
            }
            finally
            {
                Program.logs.Log(LoggingModule.Severity.Info, "Запись в базу данных завершена");
                conn.Close();
            }
        }

        public static bool database_online()
        {
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }
            return true;
        }

        /// <summary>
        /// Файл уже есть в базе данных
        /// </summary>
        /// <param name="MD5hash"></param>
        /// <returns>true если файл уже встречался в базе данных</returns>
        private static bool File_Read(string MD5hash)
        {
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT COUNT(*) FROM `class` WHERE `hash` = ?md5;";
                comm.Parameters.Add("?md5", MySqlDbType.VarChar).Value = MD5hash;

                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    if (result != "0")
                        return true;
                }
            }
            catch (Exception e)
            {
                Program.logs.LogException("База данных", "Существование файла", e);
            }
            finally
            {
                conn.Close();
            }
            return false;
        }

        /// <summary>
        /// Факт "наслаивания" занятий (Новая версия расписания)
        /// </summary>
        /// <param name="DatStr">Занятие</param>
        /// <param name="MD5">Возвращает хеш неактуального файла</param>
        /// <returns></returns>
        private static bool Layering (Zaniatie DatStr, out string MD5)
        {
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT COUNT(*),`hash` FROM `class` WHERE ((`date` = ?date)&&(`group` = ?class)&&(`time_start` = ?time)&&(`subgroup` = ?subgr)&&(not_relevant = 0));";
                comm.Parameters.Add("?date", MySqlDbType.Date).Value = DatStr.Date;
                comm.Parameters.Add("?class", MySqlDbType.Int32).Value = GetGrupID(DatStr.Gruppa);
                comm.Parameters.Add("?time", MySqlDbType.Time).Value = DatStr.Time1.TimeOfDay;
                int subgroup = DatStr.subgr;
                if (DatStr.PredmetData.subgr != 3)
                    subgroup = DatStr.PredmetData.subgr;
                comm.Parameters.Add("?subgr", MySqlDbType.Int32).Value = subgroup;

                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    if (result != "0")
                    {
                        MD5 = MyDataReader.GetString(1);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Program.logs.LogException("База данных", "Наслоение", e);
            }
            finally
            {
                conn.Close();
            }
            MD5 = "0";
            return false;
        }

        /// <summary>
        /// Установить что файл устарел
        /// </summary>
        /// <param name="hash">хеш файла</param>
        private static void not_relevant(string hash)
        {
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                comm.CommandText = "UPDATE class SET not_relevant = 1 WHERE `hash`= ?md5;";
                comm.Parameters.Add("?md5", MySqlDbType.VarChar).Value = hash;
                comm.ExecuteReader();
            }
            catch(Exception e)
            {
                Program.logs.LogException("База данных", "Файл устарел", e);
            }
            finally
            {
                conn.Close();
            }
        }
    

        /// <summary>
        /// ID преподавателя
        /// </summary>
        /// <param name="FIO">Строка для поиска</param>
        /// <returns>id найденного преподавателя, если не найден - 0</returns>
        public static int GetPrepodID(string FIO)
        {
            Program.logs.Log(LoggingModule.Severity.Debug, "Спросили преподавателя: " + FIO);
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            conn.Open();
            try
            {
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT `ID` FROM `prepodovatel` WHERE FIO = ?search;";
                comm.Parameters.Add("?search", MySqlDbType.VarChar).Value = FIO;
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    int id = 0;
                    if (int.TryParse(result, out id))
                    {
                        MyDataReader.Close();
                        Program.logs.Log(LoggingModule.Severity.Debug, "Ответ: " + id);
                        return id;
                    }
                }
            }
            catch(Exception e)
            {
                Program.logs.LogException("База данных", "ID Преподавателя", e);
            }
            finally
            {
                conn.Close();
            }
            Program.logs.Log(LoggingModule.Severity.Warn, "Преподавателя нет: " + FIO);
            return 0;
        }

        /// <summary>
        /// ID Дисциплины
        /// </summary>
        /// <param name="searchstring">Строка для поиска</param>
        /// <returns>ID Найденной дисциплины, если не найдена - 0</returns>
        public static int GetDiscID(string searchstring)
        {
            Program.logs.Log(LoggingModule.Severity.Debug, "Cпросили дисциплину " + searchstring);
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT `ID` FROM `disciplines` WHERE naimenovanie = ?search;";
                comm.Parameters.Add("?search", MySqlDbType.VarChar).Value = searchstring;
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    int id = 0;
                    if (int.TryParse(result, out id))
                    {
                        MyDataReader.Close();
                        Program.logs.Log(LoggingModule.Severity.Debug, "Ответ: " + id);
                        return id;
                    }
                }
            }
            catch (Exception e)
            {
                Program.logs.LogException("База данных", "ID Дисциплины", e);
            }
            finally
            {
                conn.Close();
            }
            Program.logs.Log(LoggingModule.Severity.Warn, "Дисциплины нет: " + searchstring);
            return 0;
        }

        /// <summary>
        /// ID вида
        /// </summary>
        /// <param name="searchstring">Строка для поиска</param>
        /// <returns>ID найденного вида, если не найден - 0</returns>
        public static int GetTypeID(string searchstring)
        {
            Program.logs.Log(LoggingModule.Severity.Debug, "Спросили тип " + searchstring);
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();
                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT `ID` FROM `tip` WHERE naimenovanie = ?search;";
                comm.Parameters.Add("?search", MySqlDbType.VarChar).Value = searchstring;
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    int id = 0;
                    if (int.TryParse(result, out id))
                    {
                        MyDataReader.Close();
                        Program.logs.Log(LoggingModule.Severity.Debug, "Ответ: " + id);
                        return id;
                    }
                }
            }
            catch(Exception e)
            {
                Program.logs.LogException("База данных", "ID вида", e);
            }
            finally
            {
                conn.Close();
            }
            Program.logs.Log(LoggingModule.Severity.Warn, "Вида нет: " + searchstring);
            return 0;
        }

        /// <summary>
        /// ID Группы
        /// </summary>
        /// <param name="searchstring">Строка для поиска</param>
        /// <returns>ID Найденной группы, если не найдена - 0</returns>
        public static int GetGrupID(string searchstring)
        {
            Program.logs.Log(LoggingModule.Severity.Debug, "Спросили группу " + searchstring);
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();

                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT `ID` FROM `groups` WHERE naimenovanie = ?search;";
                comm.Parameters.Add("?search", MySqlDbType.VarChar).Value = searchstring;
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    int id = 0;
                    if (int.TryParse(result, out id))
                    {
                        MyDataReader.Close();
                        Program.logs.Log(LoggingModule.Severity.Debug, "Ответ: " + id);
                        return id;
                    }
                }
            }
            catch (Exception e)
            {
                Program.logs.LogException("База данных", "ID группы", e);
            }
            finally
            {
                conn.Close();
            }
            Program.logs.Log(LoggingModule.Severity.Warn, "Группы нет: " + searchstring);
            return 0;
        }

        /// <summary>
        /// ID Кабинета
        /// </summary>
        /// <param name="searchstring">Строка для поиска</param>
        /// <returns>ID Найденного кабинета, Если не найден - 0</returns>
        public static int GetKabID(string searchstring)
        {
            Program.logs.Log(LoggingModule.Severity.Debug, "Спросили кабинет" + searchstring);
            MySqlConnection conn = new MySqlConnection(mysqlCSB.ConnectionString);
            try
            {
                conn.Open();

                MySqlCommand comm = conn.CreateCommand();
                MySqlDataReader MyDataReader;

                comm.CommandText = "SELECT `ID` FROM `auditorii` WHERE auditoria = ?search;";
                comm.Parameters.Add("?search", MySqlDbType.VarChar).Value = searchstring;
                MyDataReader = comm.ExecuteReader();
                while (MyDataReader.Read())
                {
                    string result = MyDataReader.GetString(0); //Получаем строку
                    int id = 0;
                    if (int.TryParse(result, out id))
                    {
                        MyDataReader.Close();
                        Program.logs.Log(LoggingModule.Severity.Debug, "Ответ: " + id);
                        return id;
                    }
                }
            }
            catch(Exception e)
            {
                Program.logs.LogException("База данных", "ID кабинета", e);
            }
            finally
            {
                conn.Close();
            }
            Program.logs.Log(LoggingModule.Severity.Warn, "Кабинета нет: " + searchstring);
            return 0;
        }
    }
}
