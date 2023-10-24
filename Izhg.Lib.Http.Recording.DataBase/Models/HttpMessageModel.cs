using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.HttpRecording.Models
{
    public class HttpMessageModel
    {
        public int Id { get; set; }
        /// <summary>
        /// Response 
        /// </summary>
        public int Type { get; set; }
        public int Flags { get; set; }
        public int Method { get; set; }
        public byte[] Body { get; set; }
        public string Host { get; set; }
    }
}
