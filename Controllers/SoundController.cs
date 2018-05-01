using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace WebApplication2.Controllers
{
    public class SoundController : ApiController
    {

        private static WaveOut waveOut;

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
                waveOut.Stop();
        }

        private void StartPlay()
        {
            if (waveOut == null)
            {
                waveOut = new WaveOut();
                var mp3Reader = new Mp3FileReader("C:\\Media\\sb.mp3");
                waveOut.Init(mp3Reader);
            }
            waveOut.Play();
        }
    }
}

//http://localhost:55525/api/Sound/play