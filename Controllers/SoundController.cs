using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.IO;

namespace SoundService
{
    public class SoundController : ApiController
    {

        private static WaveOut waveOut;
        private List<string> list;
        private static Random rnd = new Random();

        [HttpGet]
        public void Play()
        {
            var t = new Thread(() => StartPlay());
            t.Start();
        }

        [HttpGet]
        public void Stop()
        {
            if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
        }

        private void StartPlay()
        {
            if (waveOut == null)
            {
                list = new List<String>();
                String[] arr = Directory.GetFiles("C:\\Media", "*.mp3");
                for (int i = 0; i < arr.Length; i++)
                {
                    list.Add(arr[i]);
                }
                waveOut = new WaveOut();
                int r = rnd.Next(list.Count);
                var mp3Reader = new Mp3FileReader(list[r]);
                waveOut.Init(mp3Reader);
            }
            waveOut.Play();
        }

        //private void Init()
        //{
        //    list = new List<String>();
        //    String[] arr = Directory.GetFiles("C:\\Media", "*.mp3");
        //}
    }
}

//http://localhost:55525/api/Sound/play