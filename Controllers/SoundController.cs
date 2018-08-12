using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
using WebApplication.Models;


namespace WebApplication.Controllers
{
    public class SoundController : ApiController
    {
        public int numberOfDevices;
        public static WaveOutEvent[] waveOut;
        public Thread[] t;
        public List<string> list;
        public List<Track> tracks = new List<Track>();
        private static Random rnd = new Random();
        private string path; //Директория со звуковыми файлами


        //Запускает отдельный поток для каждого WaveOutEvent
        //ID - номер устроиства в URL
        //trackid - что воспроизводить (необязательный)
        [HttpGet]
        public void Play(string id, string trackid)
        {
            try
            {
                List<string> deviceNames = new List<string>();
                int location;            //Считываем четвертую строчку из файла конфигурации с количеством устройств вывода
                numberOfDevices = Convert.ToInt32(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                      .ElementAtOrDefault(3));
                //Считываем следующие N названий устройств вывода
                 
                for (int i = 0; i < numberOfDevices; i++)
                {
                    deviceNames.Add(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                    .ElementAtOrDefault(4 + i)
                                    .ToLower());
                }
                //Если входной параметр ID не приводится к целочисленному типу, ищем его в коллекции и
                //присваиваем индекс найденного ID переменной location (хранит deviceNumber)
                if (!(Int32.TryParse(id, out location)))
                {
                    if (deviceNames.Contains(id.ToLower())) { location = deviceNames.IndexOf(id.ToLower()); }
                }
                //Если массива, хранящего WaveoutEvent-ы еще не существует (первый запуск), создаем его
                if (waveOut == null)
                {
                    waveOut = new WaveOutEvent[numberOfDevices];
                }

                //То же самое с потоками
                if (t == null)
                {
                    t = new Thread[numberOfDevices];
                    for (int i = 0; i < numberOfDevices; i++)
                    {
                        t[i] = new Thread(() => StartPlay(location, trackid));
                    }
                }
                //Запускаем поток
                t[location].Start();
            }
            catch (IndexOutOfRangeException)
            {
                if (numberOfDevices == 0)
                {
                    string wmessage = "Неверно сконфигурировано количество выходов в файле конфигурации.";
                    int wcode = 700;
                    string wtype = "exception";
                    Log(wmessage, wcode, wtype);
                    throw;
                }
                else
                {
                    string wmessage = "Неверно указан DeviceNumber.";
                    int wcode = 700;
                    string wtype = "exception";
                    Log(wmessage, wcode, wtype);
                    throw;
                }
            }
            catch (FormatException)
            {
                string wmessage = "Неверно указан DeviceNumber.";
                int wcode = 700;
                string wtype = "exception";
                Log(wmessage, wcode, wtype);
                throw;
            }
            catch (Exception)
            {
                string wmessage = "сосатб.";
                int wcode = 700;
                string wtype = "exception";
                Log(wmessage, wcode, wtype);
                throw;
            }
            
        }


        [HttpGet]
        public void Stop(int id)
        {
            if (waveOut[id] != null && waveOut[id].PlaybackState == PlaybackState.Playing)
            {
                waveOut[id].Stop();
            }
        }

        public void Catalog()
        {
            String[] arr;
            list = new List<String>();
            string path = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt").ElementAtOrDefault(0);
            //считываем строку с директорией треков из конфига
            try
            {
                arr = Directory.GetFiles(path, "*.mp3");
                for (int i = 0; i < arr.Length; i++)
                {
                    list.Add(arr[i]);
                    tracks.Add(new Track() { Id = i + 1, Name = arr[i] });
                }
                //формируем каталог треков
            }
            catch
            {
                string message = "Directory " + path + " not found";
                int code = 700;
                string type = "exсeption";
                Log(message, code, type);
                //обработка исключения, когда указан несуществующий путь к трекам
            }

        }

        [HttpGet]
        public IEnumerable<Track> GetAllTracks()
        {
            Catalog();
            return tracks;
        }

        public IHttpActionResult GetTrack(int id)
        {
            Catalog();
            var track = tracks.FirstOrDefault((p) => p.Id == id);
            if (track == null)
            {
                return NotFound();
            }
            else {
            return Ok(track);
            }
            

        }

        [HttpGet]
        private void StartPlay(int location, string track)
        {
            //Если в данном потоке что-то воспроизводится, останавливаем воспроизведение
            if (waveOut[location] != null)
            {
                waveOut[location].Stop();
            }
            //Получаем список треков
            Catalog();
            //Считываем путь к каталогу с звуками-событиями из файла конфигурации
            string eventCatalogue = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                    .ElementAtOrDefault(2);
            waveOut[location] = new WaveOutEvent();
            waveOut[location].DeviceNumber = location;
            Mp3FileReader mp3Reader;
            //Если trackid не был указан в URL => выбираем случайно
            //Иначе музыкальный файл по номеру или алерт
            switch (track)
            {
                case null:
                    int trackid = rnd.Next(list.Count);
                    mp3Reader = new Mp3FileReader(list[trackid]);
                    break;
                case "alert":
                    mp3Reader = new Mp3FileReader(eventCatalogue + "alert.wav");
                    break;
                case "error":
                    mp3Reader = new Mp3FileReader(eventCatalogue + "alert.wav");
                    break;
                case "gas":
                    mp3Reader = new Mp3FileReader(eventCatalogue + "gas.mp3");
                    break;
                default:
                    trackid = Convert.ToInt32(track);
                    mp3Reader = new Mp3FileReader(list[trackid - 1]);
                    break;
            }
            waveOut[location].Init(mp3Reader);
            waveOut[location].Play();
        }

        public void Log(string message, int code, string type)
        {
            string logpath = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt").ElementAtOrDefault(1);
            //считываем директорию для лога из конфига
            System.IO.File.AppendAllText(logpath +
            DateTime.Now.ToString("yyyyMMdd") + ".log",
            "{" + "  " + "\"date\": \"" + DateTime.Now.ToString("dd.MM.yyyy") + "\", "
            + "  " + "\"time\": \"" + DateTime.Now.ToString("HH:mm:ss") + "\", " +
            "  " + "\"code\": \"" + code + "\", " +
            "  " + "\"type\": \"" + type + "\", " +
            "  " + "\"description\": \"" + message + "\"}\r\n");
            //запись в формате json
        }
    }
}

//http://localhost:55525/api/Sound/play