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
            var t = new Thread(() => StartPlay(id, trackid));
            t.Start();
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
            arr = Directory.GetFiles("C:\\Media", "*.mp3");
            for (int i = 0; i < arr.Length; i++)
            {
                list.Add(arr[i]);
                tracks.Add(new Track() { Id = i + 1, Name = arr[i] });
            }
        }

        public void Log(string message)
        {
            System.IO.File.AppendAllText("D:\\log/" + 
			DateTime.Now.ToString("yyyyMMdd") + ".log", DateTime.Now + " " + message + "\r\n");

        }
        [HttpGet]
        public IEnumerable<Track> GetAllTracks()
        {
            Catalog();
            return tracks;
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
            var mp3Reader = new Mp3FileReader(list[trackid - 1]);
            waveOut.Init(mp3Reader);
            // }
            waveOut.Play();
            string message = "Track №" + trackid + " started on device №" + location;
            Log(message);
        }

    }
}

//http://localhost:55525/api/Sound/play