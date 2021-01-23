using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace ConsoleApp
{
    static class PersonManager
    {
        public static List<Person> People = new List<Person>();
        public static void Add()
        {
            try
            {
                ENTERNAME:
                Console.WriteLine("Adınızı Daxil Edin:");
                string name = Console.ReadLine();
                if(name == Regex.Match(name,@"^[A-Za-z]+$").ToString())
                {
                    Console.WriteLine("Yaşınızı Daxil Edin");
                    byte age = byte.Parse(Console.ReadLine());

                    Person person = new Person(name, age);

                    People.Add(person);
                    
                }
                else
                {
                    Console.WriteLine("Ad Yalnız Hərflərdən İbarət Ola Bilər...");
                    goto ENTERNAME;
                    //goto evezine burada da Add() etmek olar.
                }
                
            }catch(FormatException)
            {
                //exceptions-lar okaydir, amma yash uchun de regex-den istifade etmek olardi.
                Console.WriteLine("XƏTA!! Xahiş olunur yaşı rəqəmlərlə daxil edin");
                Add();
            }
            catch (OverflowException)
            {
                Console.WriteLine("XƏTA!! Xahiş edirik yaşınızı doğru daxil edin");
                Add();
            }
        }

        public static IEnumerable<Person> SearchPerson()
        {

            Console.WriteLine("Zəhmət Olmasa Nəyə Görə Axtarış Edirsiniz?(a/b)");
            char search = char.Parse(Console.ReadLine());

            if(search == 'a')
            {
                SEARCHNAME:
                Console.WriteLine("Zəhmət Olmasa Adı Daxil Edin:");
                string s_name = Console.ReadLine();
                if(s_name == Regex.Match(s_name, @"^[A-Za-z]+$").ToString())
                {
                    //Burada People listi override olunur. Artıq həmin list-də yalnız axtarış olunan adlar qalacaq, digərləri haqda məlumatlar silinəcək.
                    // Həm də ToList() edildikdə yield return mənası qalmır, çün ki, list RAM-da yer tutur, amma enumerable bir yerdə saxlanılmır, havada return edir.
                    People = People.Where(Person => Person.Name == s_name).ToList();
                    foreach (var item in People)
                    {
                        yield return item;
                    }
                }
                else
                {
                    Console.WriteLine("Ad Yalnız Hərflərdən İbarət Ola Bilər...");
                    goto SEARCHNAME;
                }
            }
            else if(search == 'b')
            {

                Console.WriteLine("Zəhmət Olmasa Yaş Daxil Edin:");
                byte s_age = byte.Parse(Console.ReadLine());
                //bayaqki problem...
                People = People.Where(Person => Person.Age == s_age).ToList();
                foreach (var item in People)
                {
                    yield return item;
                }
            }
            ChoiceMethod();
        }
        public static void SaveList()
        {
            string[] directories = Directory.GetDirectories(Environment.CurrentDirectory);

            BinaryFormatter bf = new BinaryFormatter();
            
            DateTime dt = new DateTime();
            dt = DateTime.Now;
            DirectoryInfo directory = Directory.CreateDirectory($"Saved-{dt.ToString("dd-MM-yyyy")}");
            Console.WriteLine("Faylın Adını Daxil Edin:");
            string f_name = Console.ReadLine();
            int counter = 0;
            foreach (var dr in directories)
            {
                string filename = Path.GetFileName(dr);
                if (File.Exists($@"{filename}/{f_name}.bin"))
                {
                    Console.WriteLine("Bu adda fayl hal-hazırda mövcuddur,yeni fayl adı daxil etmək istəyirsiniz yoxsa ovveride olunsun?(a/b):");
                    char operation = char.Parse(Console.ReadLine());
                    if(operation == 'a')
                    {
                        Console.WriteLine("Faylın Adını Daxil Edin:");
                        string new_f_name = Console.ReadLine();
                        Save(new_f_name);
                        break;

                    }
                    else if(operation == 'b')
                    {
                        Save(f_name);
                        break;
                    }
                }
                if(counter == directories.Length-1)
                {
                    Save(f_name);
                    break;
                }
                counter++;
            }
            //Save(f_name);
            
            void Save(string filename)
            {
                using (FileStream fs = new FileStream($@"{directory.Name}/{filename}.bin", FileMode.OpenOrCreate))
                {
                    bf.Serialize(fs, People.ToArray());
                }
            }
        }
        public static void LoadData()
        {
            Console.WriteLine("Faylın adını daxil edin:");
            string fileName = Console.ReadLine();
            OPERATIONS:
            Console.WriteLine("Override etmək istəyirsiniz yoxsa əlavə etmək istəyirsiniz?(a/b):");
            char operation = char.Parse(Console.ReadLine());
            string searchFile = "";
            //butun folder-leri axtarmaq yox, yalniz Saved-dd-MM-yyyy olan folderlerin ichini axtarmaq lazimdir. 
            //1.bin hem bugunun hem de dunenin folder-inde save oluna biler. Butun variantlari ekrana cixarib sechim vermek lazim idi...
            foreach(var i in Directory.GetDirectories(Environment.CurrentDirectory))
            {
                searchFile = Path.GetFullPath(Path.Combine(i,fileName));
                //bu cur yoxlanish hechzaman null olmur, chunki file adini string kimi yoxlayirsan. 
                //File.Exists() metodundan yararlanmaq lazim idi...
                //Bu hisse duzgun yazilmadigi uchun deserializasiya vaxti exception ata biler.
                if(searchFile == null)
                {
                    break;
                }
            }
            if(searchFile != null)
            {
                BinaryFormatter bf = new BinaryFormatter();
                using (FileStream fs = new FileStream(searchFile + ".bin", FileMode.OpenOrCreate))
                {
                    //birbasha List<Person> kimi convert etmek olar. 
                    Person[] ds = (Person[])bf.Deserialize(fs);
                    if(operation == 'a')
                    {
                        People = ds.ToList();
                    }else if(operation == 'b')
                    {
                        People = People.Concat(ds.ToList()).ToList();
                    }
                    else
                    {
                        Console.WriteLine("Belə bir əməliyyat yoxdur xahiş edirik yenidən sınayın...");
                        goto OPERATIONS;
                    }
                }
            }
        }
        public static void DisplayMenu()
        {
            Console.WriteLine("\n\n1.Əlavə Et\n2.Search Et\n3.Save Et\n4.Load Et\n5.Display Data\n6.Export Et\n7.Exit\n");

            Console.WriteLine("Bir Əməliyyat Seçin:");
            byte operation = byte.Parse(Console.ReadLine());

            switch (operation)
            {
                case 1:
                    Add();
                    DisplayMenu();
                    break;
                case 2:
                    DisplaySearchResult();
                    DisplayMenu();
                    break;
                case 3:
                    SaveList();
                    DisplayMenu();
                    break;
                case 4:
                    LoadData();
                    DisplayMenu();
                    break;
                case 5:
                    DisplayData();
                    DisplayMenu();
                    break;
                case 6:
                    Export();
                    DisplayMenu();
                    break;
                case 7:
                    break;
                default:
                    Console.WriteLine("Belə Bir Əməliyyat Yoxdur");
                    DisplayMenu();
                    break;
            }
        }
        public static void DisplayData()
        {
            foreach(var i in People)
            {
                Console.WriteLine($"{i.Name},{i.Age}");
            }
        }
        public static void Edit()
        {
            Console.WriteLine("Adı yoxsa Yaşı Dəyişmək İstəyirsiniz?(a/b)");
            char c = char.Parse(Console.ReadLine());
            Console.WriteLine("Hansı İstifadəçinin Dəyişmək istəyirsiniz:");
            int p = Int32.Parse(Console.ReadLine());
            if (c == 'a')
            {
                ENTERNEWNAME:
                Console.WriteLine("Yeni Ad Daxil Edin:");
                string new_name = Console.ReadLine();
                if(new_name == Regex.Match(new_name, @"^[A-Za-z]+$").ToString())
                {
                    int counter = 0;
                    foreach(var i in People)
                    {
                        if(p - 1 == counter)
                        {
                            i.Name = new_name;
                        }
                        counter++;
                    }
                    Console.WriteLine("İstifadəçi uğurla yeniləndi...");
                }
                else
                {
                    Console.WriteLine("Ad Yalnız Hərflərdən İbarət Ola Bilər...");
                    goto ENTERNEWNAME;
                }

                
            }else if(c == 'b')
            {
                Console.WriteLine("Yeni Yaş Daxil Edin:");
                byte new_age = byte.Parse(Console.ReadLine());
                int counter = 0;
                foreach (var i in People)
                {
                    if (p - 1 == counter)
                    {
                        i.Age = new_age;
                    }
                    counter++;
                }
                Console.WriteLine("İstifadəçi uğurla yeniləndi...");
            }
            else
            {
                Console.WriteLine("Belə Bir Seçim Mövcud Deyil");
                Edit();
            }
        }
        public static void Delete()
        {
            Console.WriteLine("Hansı İstifadəçini Silmək İstəyirsiniz:");
            int p = Int32.Parse(Console.ReadLine());

            for(int i = 0; i < People.Count;i++)
            {
                if (p - 1 == i)
                {
                    People.Remove(People[i]);
                }
            }


            Console.WriteLine("İstifadəçi uğurla silindi...");
        }
        public static void ChoiceMethod()
        {
            Console.WriteLine("\nBir Əməliyyat Seçin:\n1.Edit Et\n2.Delete Et");
            char choice = char.Parse(Console.ReadLine());
            
            if(choice == '1')
            {
                Edit();
            }else if(choice == '2')
            {
                Delete();
            }
            else
            {
                Console.WriteLine("Belə Bir Əməliyyat Yoxdur");
                ChoiceMethod();
            }
        }
        public static void DisplaySearchResult()
        {
            int counter = 1;
            foreach (var item in SearchPerson())
            {
                Console.WriteLine($"{counter}){item.Name},{item.Age}");
                counter++;
            }
        }
        public static void Export()
        {
            string[] directories = Directory.GetDirectories(Environment.CurrentDirectory);
            Console.WriteLine("Hansı Formatda Export Etmək İstəyirsiniz:");
            Console.WriteLine("\n1) .txt\n2) .xlsx");
            char c = char.Parse(Console.ReadLine());
            Console.WriteLine("Faylın Adını Daxil Edin:");
            string f_name = Console.ReadLine();

            //new DateTime() etmekle yeni obyekt yaradirsan. DateTime.Now ise statikdir. Birbasha bele ede bilersen:
            // DateTime dt = DateTime.Now;

            DateTime dt = new DateTime();
            dt = DateTime.Now;

            
            DirectoryInfo directory = Directory.CreateDirectory($"Exported-{dt.ToString("dd--MM--yyyy")}");
            foreach (var dr in directories)
            {
                 string filename = Path.GetFileName(dr);
                 if(c == '2')
                 {
                    if (File.Exists($@"{filename}/{f_name}.xlsx"))
                    {
                        OPERATION:
                        Console.WriteLine("Bu adda fayl hal-hazırda mövcuddur,yeni fayl adı daxil etmək istəyirsiniz yoxsa ovveride olunsun?(a/b):");
                        char operation = char.Parse(Console.ReadLine());
                        if (operation == 'a')
                        {
                            
                            Console.WriteLine("Yeni fayl adı daxil edin:");
                            string new_f_name = Console.ReadLine();

                            ExportExcel(new_f_name);
                            break;
             
                        }else if(operation == 'b')
                        {
                            ExportExcel(f_name);
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Belə bir əməliyyat yoxdur...");
                            goto OPERATION;
                        }
                    }
                    else
                    {
                        ExportExcel(f_name);
                    }
                 }
            }
            if (c == '1')
            {
                //excel uchun oldugu kimi text uchun fayli override etmek yoxsa yeni ad sechmek kimi yoxlanish yoxdur.
                using (StreamWriter sw = new StreamWriter($@"{directory.Name}/{f_name}.txt", true))
                {
                    foreach (var i in People)
                    {
                        sw.WriteLine(i.Name);
                        sw.WriteLine(i.Age);
                        sw.WriteLine("\n===\n");
                    }
                }
            }
            void ExportExcel(string filename)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Test");

                    worksheet.Cell("A1").Value = "Name";
                    worksheet.Cell("B1").Value = "Age";

                    int counter = 2;
                    foreach (var i in People)
                    {
                        string cellName1 = $"A{counter}";
                        string cellName2 = $"B{counter}";

                        worksheet.Cell(cellName1).Value = i.Name;
                        worksheet.Cell(cellName2).Value = i.Age;
                        counter++;
                    }
                    workbook.SaveAs($@"{directory.Name}/{filename}.xlsx");
                }
            }
        } 
    }
}
