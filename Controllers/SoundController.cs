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
        private string path = ConfigurationManager.AppSettings.Get("Path");


        [HttpGet]
        public void Play(int id, int trackid)
        {
            if (id < WaveOut.DeviceCount && id >= -1)
            {
                var t = new Thread(() => StartPlay(id, trackid));
                t.Start();
            }
            else
            {
                string xmessage = "Device not found";
                int xcode = 701;
                string xtype = "exeption";
                Log(xmessage, xcode, xtype);
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
            try
            {
                string path = File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt").ElementAtOrDefault(0);
                //считываем строку с директорией треков из конфига
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
                string message = "Directory not found";
                int code = 700;
                string type = "exeption";
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
            "{\r\n"+ "  " +"\"date\": \""+ DateTime.Now.ToString("dd.MM.yyyy") + "\", " +
            "\r\n" + "  " + "\"time\": \"" + DateTime.Now.ToString("hh:mm") + "\", " +
            "\r\n" + "  " + "\"code\": \"" + code + "\", " +
            "\r\n" + "  " + "\"type\": \"" + type + "\", " +
            "\r\n" + "  " + "\"description\": \"" + message + "\"\r\n}\r\n");
            //вывод в формате json
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
            try
            {
                var mp3Reader = new Mp3FileReader(list[trackid - 1]);
                waveOut.Init(mp3Reader);
                waveOut.Play();
                string message = "Track №" + trackid + " started on device №" + location;
                int code = 100;
                string type = "event";
                Log(message, code, type);
            }
            catch {
                string wmessage = "Incorrect file format";
                int wcode = 400;
                string wtype = "warning";
                Log(wmessage, wcode, wtype);
            }
        }

    }
}
