﻿using NAudio.Wave;
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
        public static WaveOutEvent[] waveOuts;
        public static Mp3FileReader mp3Player;
        public Thread[] threads;
        public List<string> list;
        public List<Track> tracks = new List<Track>();
        private static Random rnd = new Random();
        //private string pathToFiles; //Директория со звуковыми файлами


        //Запускает отдельный поток для каждого WaveOutEvent
        //ID - номер устроиства в URL
        //trackid - что воспроизводить (необязательный)
        [HttpGet]
        public void Play(string locationId, string trackId)
        {
            try
            {
                //список имен звуковых выходов (кухня, спальня и т.д.)
                List<string> deviceNames = new List<string>();

                //хранит нужный deviceNumber, в то время как во входной переменной locationId могут быть введены 
                //названия комнат (kitchen, room1, etc.)
                int deviceNumber; 
                
                //Считываем четвертую строчку из файла конфигурации с количеством устройств вывода
                try
                {
                    numberOfDevices = Convert.ToInt32(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                          .ElementAtOrDefault(3));
                }
                #region exceptions
                //Exception появляется, если в файле конфигурации вместо ожидаемого числа устроиств считывается строка
                catch (FormatException)
                {
                    string wmessage = "Неверно сконфигурировано количество выходов в файле конфигурации.";
                    int wcode = 400;
                    string wtype = "error";
                    Log(wmessage, wcode, wtype);
                    //throw;
                }
                #endregion
                //Считываем следующие N названий устройств вывода

                for (int i = 0; i < numberOfDevices; i++)
                {
                    deviceNames.Add(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                    .ElementAtOrDefault(4 + i)
                                    .ToLower());
                }
                //Если входной параметр ID не приводится к целочисленному типу, ищем его в коллекции и
                //присваиваем индекс найденного ID переменной location (хранит deviceNumber)
                if (!(Int32.TryParse(locationId, out deviceNumber)))
                {
                    if (deviceNames.Contains(locationId.ToLower())) { deviceNumber = deviceNames.IndexOf(locationId.ToLower()); }
                }
                //Если массива, хранящего WaveoutEvent-ы еще не существует (первый запуск), создаем его
                if (waveOuts == null)
                {
                    waveOuts = new WaveOutEvent[numberOfDevices];
                }

                //То же самое с потоками
                if (threads == null)
                {
                    threads = new Thread[numberOfDevices];
                    for (int i = 0; i < numberOfDevices; i++)
                    {
                        threads[i] = new Thread(() => StartPlay(deviceNumber, trackId));
                    }
                }

                if (locationId == "all")
                for (int i=0;i<numberOfDevices;i++)
                {
                        threads[i].Start();
                }
                else
                //Запускаем поток
                threads[deviceNumber].Start();
            }
            #region exceptions
            catch (IndexOutOfRangeException)
            {
                //Если numberOfDevices равен нулю, то не удалось считать количество выходов из конфига, иначе неверный ввод
                if (numberOfDevices == 0)
                {
                    string wmessage = "Неверно сконфигурировано количество выходов в файле конфигурации.";
                    int wcode = 400;
                    string wtype = "error";
                    Log(wmessage, wcode, wtype);
                    //throw;
                }
                else
                {
                    string wmessage = "Неверно указан DeviceNumber.";
                    int wcode = 700;
                    string wtype = "exception";
                    Log(wmessage, wcode, wtype);
                    //throw;
                }
            }
            #endregion

        }


        [HttpGet]
        public void Stop(int locationId)
        {
            if (waveOuts[locationId] != null && waveOuts[locationId].PlaybackState == PlaybackState.Playing)
            {
                waveOuts[locationId].Stop();
            }
        }

        public void Catalog()
        {
            String[] arr;
            list = new List<String>();
            string pathToFiles = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt").ElementAtOrDefault(0);
            //считываем строку с директорией треков из конфига
            try
            {
                arr = Directory.GetFiles(pathToFiles, "*.mp3");
                for (int i = 0; i < arr.Length; i++)
                {
                    list.Add(arr[i]);
                    tracks.Add(new Track() { Id = i + 1, Name = arr[i] });
                }
                //формируем каталог треков
            }
            #region exceptions
            catch
            {
                string message = "Directory " + pathToFiles + " not found";
                int code = 700;
                string type = "exсeption";
                Log(message, code, type);
                //обработка исключения, когда указан несуществующий путь к трекам
            }
            #endregion

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
            if (waveOuts[location] != null)
            {
                waveOuts[location].Stop();
            }
            //Получаем список треков
            Catalog();
            //Считываем путь к каталогу с звуками-событиями из файла конфигурации
            string eventCatalogue = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                    .ElementAtOrDefault(2);
            waveOuts[location] = new WaveOutEvent();
            waveOuts[location].DeviceNumber = location;
            Mp3FileReader mp3Reader = null;
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
                    try
                    { 
                    trackid = Convert.ToInt32(track);
                    mp3Reader = new Mp3FileReader(list[trackid - 1]);
                    }
                    #region exceptions
                    catch (FormatException)
                    {
                        string wmessage = "Неверно указан номер трека.";
                        int wcode = 700;
                        string wtype = "exception";
                        Log(wmessage, wcode, wtype);
                        //throw;
                    }
                    #endregion
                    break;
            }
            if (mp3Reader != null)
            {
                waveOuts[location].Init(mp3Reader);
                waveOuts[location].Play();
            }
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