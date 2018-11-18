using System.Collections.Generic;

namespace WebApplication.Models
{
    public class AppConfiguration
    {
        public string MediaPath { get; set; }
        public string LogPath { get; set; }
        public string AlertPath { get; set; }
        public List<string> RoomNames { get; set; }
        public List<Alert> Alerts { get; set; }
    }

    public class Alert
    {
        public string Name { get; set; }
        public string FileName { get; set; }
    }
}