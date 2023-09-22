using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.template.core.DTO
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    public class Data
    {
        public string TEXT_03 { get; set; }
        public string TEXT_04 { get; set; }
    }

   
    public class Message
    {
        public Data data { get; set; }
    }

    public class NFCMobDto
    {
        public string type { get; set; }
        public Message message { get; set; }
    }


}