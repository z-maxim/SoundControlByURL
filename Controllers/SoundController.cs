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

        private static WaveOutEvent waveOut;
        public List<string> list;
        public List<Track> tracks = new List<Track>();
        private static Random rnd = new Random();


        [HttpGet]
        public void Play(int id, int trackid)
        {
            if (id < WaveOut.DeviceCount && id >= -1)//проверяем номер введенного устройства
            {
                var t = new Thread(() => StartPlay(id, trackid));
                t.Start();
            }
            else
            {
                string xmessage = "Device not found";
                int xcode = 701;
                string xtype = "exсeption";
                Log(xmessage, xcode, xtype);//записываем сообщение в лог
            }
        }

        [HttpGet]
        public void Stop()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                waveOut.Stop();
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

        public IEnumerable<Track> GetAllTracks()
        {
            Catalog();
            return tracks;
            //функция для вывода всех треков
        }


        [HttpGet]
        private void StartPlay(int location, int trackid)
        {
            if (waveOut != null)//waveOut.PlaybackState == PlaybackState.Playing
            {
                waveOut.Dispose();
            }
            Catalog();
            waveOut = new WaveOutEvent();
            waveOut.DeviceNumber = location;
            //int r = rnd.Next(list.Count);
            try//отлавливаем ошибку с неправильным форматом файла
            {
                var mp3Reader = new Mp3FileReader(list[trackid - 1]);
                waveOut.Init(mp3Reader);
                waveOut.Play();
                string message = "Track №" + trackid + " started on device №" + location;
                int code = 100;
                string type = "event";
                Log(message, code, type);//записываем в лог сообщение о запуске файла
            }
            catch
            {
                string wmessage = "Incorrect file format";
                int wcode = 400;
                string wtype = "warning";
                Log(wmessage, wcode, wtype);
            }
        }

    }
}
