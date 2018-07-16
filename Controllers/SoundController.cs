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
        public static WaveOutEvent waveOut1, waveOut2;
        public List<string> list;
        public List<Track> tracks = new List<Track>();
        private static Random rnd = new Random();
        private string path; //Директория со звуковыми файлами


        [HttpGet]
        public void Play(int id, int trackid)
        {
            //waveOut1 = new WaveOutEvent();
            //waveOut2 = new WaveOutEvent();
            var t1 = new Thread(() => StartPlay1(0, trackid));
            var t2 = new Thread(() => StartPlay2(1, trackid));
            t1.Start();
            t2.Start();
        }

        //[HttpGet]
        //public void Stop(int id)
        //{
        //    if (waveOut[id] != null && waveOut[id].PlaybackState == PlaybackState.Playing)
        //    {
        //        waveOut[id].Stop();
        //    }


        //}

        [HttpGet]
        public void Stop()
        {
            //if (waveOut1 != null && waveOut1.PlaybackState == PlaybackState.Playing)
            //{
                waveOut1.Stop();
                waveOut2.Stop();
            //}


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
            //if (waveOut1 != null)
            //{
            //    waveOut1.Dispose();
            //}
            Catalog();
            waveOut1 = new WaveOutEvent();
            waveOut1.DeviceNumber = location;
            //int r = rnd.Next(list.Count);
            var mp3Reader1 = new Mp3FileReader(list[trackid-1]);
            waveOut1.Init(mp3Reader1);
            // }
            waveOut1.Play();
        }

        [HttpGet]
        private void StartPlay2(int location, int trackid)
        {
            //if (waveOut2 != null)
            //{
            //    waveOut2.Dispose();
            //}
            Catalog();
            waveOut2 = new WaveOutEvent();
            waveOut2.DeviceNumber = location;
            //int r = rnd.Next(list.Count);
            var mp3Reader2 = new Mp3FileReader(list[trackid - 1]);
            waveOut2.Init(mp3Reader2);
            // }
            waveOut2.Play();
        }

    }
}

//http://localhost:55525/api/Sound/play