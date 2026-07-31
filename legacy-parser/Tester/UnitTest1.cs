using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConsoleParser;
using System.Collections.Generic;
using System.Data;

namespace Tester
{
    [TestClass]
    public class Tester_Parser
    {
        [TestMethod]
        public void TestParsing1()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();
            Yacheyka testyacheyka = new Yacheyka("Згуральская Е.Н. 310 Технологии программирования лб.");
            Assert.AreEqual("Згуральская Е. Н.".ToLower(), testyacheyka.Prepod.ToLower());
            Assert.AreEqual("310".ToLower(), testyacheyka.Kabinet.ToLower());
            Assert.AreEqual("Технологии программирования".ToLower(), testyacheyka.Disc.ToLower());
            Assert.AreEqual("лб.".ToLower(), testyacheyka.Vid.ToLower());
        }

        [TestMethod]
        public void TestParsing2()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();
            Yacheyka testyacheyka = new Yacheyka("Чоракаев О.Э. Управление данными 308 пр.");
            Assert.AreEqual("Чоракаев О. Э.".ToLower(), testyacheyka.Prepod.ToLower());
            Assert.AreEqual("308".ToLower(), testyacheyka.Kabinet.ToLower());
            Assert.AreEqual("Управление данными".ToLower(), testyacheyka.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testyacheyka.Vid.ToLower());
        }

        [TestMethod]
        public void TestParsing3()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();
            Yacheyka testyacheyka = new Yacheyka("Черненькая Е.В. Элективные курсы по физической культуре и спорту УК-2 пр.");
            Assert.AreEqual("Черненькая Е. В.".ToLower(), testyacheyka.Prepod.ToLower());
            Assert.AreEqual("УК-2".ToLower(), testyacheyka.Kabinet.ToLower());
            Assert.AreEqual("Элективные курсы по физической культуре и спорту".ToLower(), testyacheyka.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testyacheyka.Vid.ToLower());
        }
        [TestMethod]
        public void TestParsing4()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();
            Yacheyka testyacheyka = new Yacheyka("Дискретная математика, лек. Шишкин В.В.   403");
            Assert.AreEqual("Шишкин В. В.".ToLower(), testyacheyka.Prepod.ToLower());
            Assert.AreEqual("403".ToLower(), testyacheyka.Kabinet.ToLower());
            Assert.AreEqual("Дискретная математика".ToLower(), testyacheyka.Disc.ToLower());
            Assert.AreEqual("лек.".ToLower(), testyacheyka.Vid.ToLower());
        }
        [TestMethod]
        public void TestParsing5()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();
            Yacheyka testyacheyka = new Yacheyka("Иностранный язык, пр. Аль-Дарабсе Е.В. 205");
            Assert.AreEqual("Аль-Дарабсе Е. В.".ToLower(), testyacheyka.Prepod.ToLower());
            Assert.AreEqual("205".ToLower(), testyacheyka.Kabinet.ToLower());
            Assert.AreEqual("Иностранный язык".ToLower(), testyacheyka.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testyacheyka.Vid.ToLower());
        }

        [TestMethod]
        public void TestZaniatie1()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();

            Zaniatie testZamiatie = new Zaniatie("АИСТбд-21", "16 февраля", new List<string> { "12:20", "13:05" }, "Черненькая Е.В. Элективные курсы по физической культуре и спорту УК-2 пр.", 1);
            Assert.AreEqual("АИСТбд-21".ToLower(), testZamiatie.Gruppa.ToLower());
            Assert.AreEqual(new DateTime(2017, 2, 16), testZamiatie.Date);
            Assert.AreEqual(new TimeSpan(12, 20, 0), testZamiatie.Time1.TimeOfDay);
            Assert.AreEqual(new TimeSpan(13, 5, 0), testZamiatie.Time2.TimeOfDay);
            Assert.AreEqual("Черненькая Е. В.".ToLower(), testZamiatie.PredmetData.Prepod.ToLower());
            Assert.AreEqual("УК-2".ToLower(), testZamiatie.PredmetData.Kabinet.ToLower());
            Assert.AreEqual("Элективные курсы по физической культуре и спорту".ToLower(), testZamiatie.PredmetData.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testZamiatie.PredmetData.Vid.ToLower());
        }
        [TestMethod]
        public void TestZaniatie2()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();

            Zaniatie testZamiatie = new Zaniatie("АИСТбд-11", "18 мая", new List<string> { "08:30", "10:05" }, "Чоракаев О.Э. Управление данными 308 пр.", 1);
            Assert.AreEqual("АИСТбд-11".ToLower(), testZamiatie.Gruppa.ToLower());
            Assert.AreEqual(new DateTime(2017, 5, 18), testZamiatie.Date);
            Assert.AreEqual(new TimeSpan(8, 30, 0), testZamiatie.Time1.TimeOfDay);
            Assert.AreEqual(new TimeSpan(10, 5, 0), testZamiatie.Time2.TimeOfDay);
            Assert.AreEqual("Чоракаев О. Э.".ToLower(), testZamiatie.PredmetData.Prepod.ToLower());
            Assert.AreEqual("308".ToLower(), testZamiatie.PredmetData.Kabinet.ToLower());
            Assert.AreEqual("Управление данными".ToLower(), testZamiatie.PredmetData.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testZamiatie.PredmetData.Vid.ToLower());
        }

        [TestMethod]
        public void TestZaniatie3()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();

            Zaniatie testZamiatie = new Zaniatie("АИСТбд-11", "31 мая", new List<string> { "08:30", "10:05" }, "Згуральская Е.Н. 310 Технологии программирования лб.", 1);
            Assert.AreEqual("АИСТбд-11".ToLower(), testZamiatie.Gruppa.ToLower());
            Assert.AreEqual(new DateTime(2017, 5, 31), testZamiatie.Date);
            Assert.AreEqual(new TimeSpan(8, 30, 0), testZamiatie.Time1.TimeOfDay);
            Assert.AreEqual(new TimeSpan(10, 5, 0), testZamiatie.Time2.TimeOfDay);
            Assert.AreEqual("Згуральская Е. Н.".ToLower(), testZamiatie.PredmetData.Prepod.ToLower());
            Assert.AreEqual("310".ToLower(), testZamiatie.PredmetData.Kabinet.ToLower());
            Assert.AreEqual("Технологии программирования".ToLower(), testZamiatie.PredmetData.Disc.ToLower());
            Assert.AreEqual("лб.".ToLower(), testZamiatie.PredmetData.Vid.ToLower());
        }

        [TestMethod]
        public void TestZaniatie4()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();

            Zaniatie testZamiatie = new Zaniatie("АИСТбд-11", "31 мая", new List<string> { "15:50", "16:35" }, "Дискретная математика, лек. Шишкин В.В.   403", 1);
            Assert.AreEqual("АИСТбд-11".ToLower(), testZamiatie.Gruppa.ToLower());
            Assert.AreEqual(new DateTime(2017, 5, 31), testZamiatie.Date);
            Assert.AreEqual(new TimeSpan(15, 50, 0), testZamiatie.Time1.TimeOfDay);
            Assert.AreEqual(new TimeSpan(16, 35, 0), testZamiatie.Time2.TimeOfDay);
            Assert.AreEqual("Шишкин В. В.".ToLower(), testZamiatie.PredmetData.Prepod.ToLower());
            Assert.AreEqual("403".ToLower(), testZamiatie.PredmetData.Kabinet.ToLower());
            Assert.AreEqual("Дискретная математика".ToLower(), testZamiatie.PredmetData.Disc.ToLower());
            Assert.AreEqual("лек.".ToLower(), testZamiatie.PredmetData.Vid.ToLower());
        }

        [TestMethod]
        public void TestZaniatie5()
        {
            Program.dict.Clear();
            Program.Create_Dictionary();

            Zaniatie testZamiatie = new Zaniatie("АИСТбд-11", "31 мая", new List<string> { "15:50", "16:35" }, "Иностранный язык, пр. Аль-Дарабсе Е.В. 205", 1);
            Assert.AreEqual("АИСТбд-11".ToLower(), testZamiatie.Gruppa.ToLower());
            Assert.AreEqual(new DateTime(2017, 5, 31), testZamiatie.Date);
            Assert.AreEqual(new TimeSpan(15, 50, 0), testZamiatie.Time1.TimeOfDay);
            Assert.AreEqual(new TimeSpan(16, 35, 0), testZamiatie.Time2.TimeOfDay);
            Assert.AreEqual("Аль-Дарабсе Е. В.".ToLower(), testZamiatie.PredmetData.Prepod.ToLower());
            Assert.AreEqual("205".ToLower(), testZamiatie.PredmetData.Kabinet.ToLower());
            Assert.AreEqual("Иностранный язык".ToLower(), testZamiatie.PredmetData.Disc.ToLower());
            Assert.AreEqual("пр.".ToLower(), testZamiatie.PredmetData.Vid.ToLower());
        }

        [TestMethod]
        public void TestIsTime1()
        {
            Assert.IsTrue(reader1.isTime("12:20 - 13:05"));
        }

        [TestMethod]
        public void TestIsTime2()
        {
            Assert.IsTrue(reader1.isTime("16:40-17:25"));
        }

        [TestMethod]
        public void TestIsTime3()
        {
            Assert.IsTrue(reader1.isTime("8:30-9:15"));
        }

        [TestMethod]
        public void TestIsTime4()
        {
            Assert.IsTrue(reader1.isTime("9:20-10:05"));
        }

        [TestMethod]
        public void TestIsTime5()
        {
            Assert.IsTrue(reader1.isTime("16:40-17:25"));
        }

        [TestMethod]
        public void TestIsTime6()
        {
            Assert.IsFalse(reader1.isTime("Вторник"));
        }

        [TestMethod]
        public void TestIsTime7()
        {
            Assert.IsFalse(reader1.isTime("16 мая"));
        }
        [TestMethod]
        public void TestIsTime8()
        {
            Assert.IsFalse(reader1.isTime("АИСТбд-11"));
        }
        [TestMethod]
        public void TestIsTime9()
        {
            Assert.IsFalse(reader1.isTime("Теория информац. процессов и систем, лек."));
        }
        [TestMethod]
        public void TestIsTime10()
        {
            Assert.IsFalse(reader1.isTime("Физика"));
        }

        [TestMethod]
        public void TestIsDate1()
        {
            Assert.IsFalse(reader1.isDate("8:30-9:15"));
        }

        [TestMethod]
        public void TestIsDate2()
        {
            Assert.IsFalse(reader1.isDate("9:20-10:05"));
        }

        [TestMethod]
        public void TestIsDate3()
        {
            Assert.IsFalse(reader1.isDate("Вторник"));
        }

        [TestMethod]
        public void TestIsDate4()
        {
            Assert.IsFalse(reader1.isDate("16:40-17:25"));
        }

        [TestMethod]
        public void TestIsDate5()
        {
            Assert.IsFalse(reader1.isDate("12:20-13:05"));
        }

        [TestMethod]
        public void TestIsDate6()
        {
            Assert.IsTrue(reader1.isDate("15 мая"));
        }

        [TestMethod]
        public void TestIsDate7()
        {
            Assert.IsTrue(reader1.isDate("17 мая"));
        }

        [TestMethod]
        public void TestIsDate8()
        {
            Assert.IsTrue(reader1.isDate("18 мая"));
        }

        [TestMethod]
        public void TestIsDate9()
        {
            Assert.IsTrue(reader1.isDate("18 апреля"));
        }

        [TestMethod]
        public void TestIsDate10()
        {
            Assert.IsTrue(reader1.isDate("20 июня"));
        }

        //Дальше reader1

        [TestMethod]
        public void TestFindDate1()
        {
            DataTable table1 = new DataTable("raspisanie");
            table1.Columns.Add("noname");
            table1.Columns.Add("grops");
            table1.Columns.Add("noname2");
            table1.Rows.Add("Тут ничего нет", "АИСТбд-21");
            table1.Rows.Add("16 мая", "");


            DataSet TestDS = new DataSet();
            TestDS.Tables.Add(table1);

            Assert.AreEqual(1, reader1.FindDate(TestDS));
        }
        [TestMethod]
        public void TestFindDate2()
        {
            DataTable table1 = new DataTable("raspisanie");
            table1.Columns.Add("noname");
            table1.Columns.Add("grops");
            table1.Columns.Add("noname2");
            table1.Rows.Add("Тут ничего нет", "АИСТбд-21");
            table1.Rows.Add("jffjm", "", "12 февраля");


            DataSet TestDS = new DataSet();
            TestDS.Tables.Add(table1);

            Assert.AreEqual(3, reader1.FindDate(TestDS));
        }

        [TestMethod]
        public void TestFindDate3()
        {
            DataTable table1 = new DataTable("raspisanie");
            table1.Columns.Add("noname");
            table1.Columns.Add("grops");
            table1.Columns.Add("noname2");
            table1.Rows.Add("Тут ничего нет", "АИСТбд-21");
            table1.Rows.Add("jffjm", "13 июня");


            DataSet TestDS = new DataSet();
            TestDS.Tables.Add(table1);

            Assert.AreEqual(2, reader1.FindDate(TestDS));
        }

        //Дальше writer

        [TestMethod]
        public void TestDataBaseConnection()
        {
            Assert.IsTrue(WriterDB.database_online());
        }

        [TestMethod]
        public void TestGetValueFromDB1_1()
        {
            Assert.AreEqual(2, WriterDB.GetPrepodID("Згуральская Е. Н."));
        }

        [TestMethod]
        public void TestGetValueFromDB1_2()
        {
            Assert.AreEqual(48, WriterDB.GetPrepodID("Чоракаев О. Э."));
        }

        [TestMethod]
        public void TestGetValueFromDB1_3()
        {
            Assert.AreEqual(62, WriterDB.GetPrepodID("Черненькая Е. В."));
        }

        [TestMethod]
        public void TestGetValueFromDB2_1()
        {
            Assert.AreEqual(2, WriterDB.GetDiscID("Технологии Программирования"));
        }
        [TestMethod]
        public void TestGetValueFromDB2_2()
        {
            Assert.AreEqual(45, WriterDB.GetDiscID("Основы программирования"));
        }
        [TestMethod]
        public void TestGetValueFromDB2_3()
        {
            Assert.AreEqual(44, WriterDB.GetDiscID("Управление данными"));
        }

        [TestMethod]
        public void TestGetValueFromDB3_1()
        {
            Assert.AreEqual(2, WriterDB.GetTypeID("лб."));
        }
        [TestMethod]
        public void TestGetValueFromDB3_2()
        {
            Assert.AreEqual(42, WriterDB.GetTypeID("пр."));
        }
        [TestMethod]
        public void TestGetValueFromDB3_3()
        {
            Assert.AreEqual(48, WriterDB.GetTypeID("зачет."));
        }
        [TestMethod]
        public void TestGetValueFromDB4_1()
        {
            Assert.AreEqual(13, WriterDB.GetGrupID("АИСТбд-21"));
        }
        [TestMethod]
        public void TestGetValueFromDB4_2()
        {
            Assert.AreEqual(53, WriterDB.GetGrupID("АИСТбд-11"));
        }
        [TestMethod]
        public void TestGetValueFromDB4_3()
        {
            Assert.AreEqual(99, WriterDB.GetGrupID("АСВсд-21"));
        }
        [TestMethod]
        public void TestGetValueFromDB5_1()
        {
            Assert.AreEqual(2, WriterDB.GetKabID("310"));
        }
        [TestMethod]
        public void TestGetValueFromDB5_2()
        {
            Assert.AreEqual(49, WriterDB.GetKabID("УК-2"));
        }
        [TestMethod]
        public void TestGetValueFromDB5_3()
        {
            Assert.AreEqual(48, WriterDB.GetKabID("401"));
        }
    }
}
