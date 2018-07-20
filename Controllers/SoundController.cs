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
        public static WaveOutEvent[] waveOut;
        public Thread[] t;
        public List<string> list;
        public List<Track> tracks = new List<Track>();
        private static Random rnd = new Random();
        private string path; //Директория со звуковыми файлами


        //Запускает отдельный поток для каждого WaveOutEvent
        [HttpGet]
        public void Play(string id, int trackid)
        {
            List<string> deviceNames = new List<string>();
            int location;
            //Считываем третью строчку из файла конфигурации с количеством устройств вывода
            int numberOfDevices = Convert.ToInt32(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                  .ElementAtOrDefault(2));
            //Считываем следующие N названий устройств вывода
            for (int i = 0; i < numberOfDevices; i++)
            {
                deviceNames.Add(File.ReadLines(AppDomain.CurrentDomain.BaseDirectory + "conf.txt")
                                .ElementAtOrDefault(3 + i)
                                .ToLower());
            }
            //Если входной параметр ID не приводится к целочисленному типу, ищем его в коллекции
            //Номер выхода - индекс элемента в коллекции
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
                    t[i] = new Thread(() => StartPlay1(location, trackid));
                }
            }
            //Запускаем поток
            t[location].Start();
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
            //открытие файла параметризации, считывание переменных, закрытие файла
            StreamReader strRead = new StreamReader(AppDomain.CurrentDomain.BaseDirectory+"conf.txt");
            path = strRead.ReadLine();
            strRead.Close();
            arr = Directory.GetFiles(path, "*.mp3");
            for (int i = 0; i < arr.Length; i++)
            {
                list.Add(arr[i]);
                tracks.Add(new Track() {Id = i+1, Name = arr[i] });
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
        private void StartPlay1(int location, int trackid)
        {
            if (waveOut[location] != null)
            {
                waveOut[location].Stop();
            }
            Catalog();
            waveOut[location] = new WaveOutEvent();
            waveOut[location].DeviceNumber = location;
            //int r = rnd.Next(list.Count);
            var mp3Reader1 = new Mp3FileReader(list[trackid-1]);
            waveOut[location].Init(mp3Reader1);
            // }
            waveOut[location].Play();
        }
    }
}

//http://localhost:55525/api/Sound/play