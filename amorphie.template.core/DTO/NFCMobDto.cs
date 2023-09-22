using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.template.core.DTO
{
    public class NFCMobDto
    {
        /// <summary>
        /// Tur
        /// </summary>
        public string? type { get; set; }
        public Data? data { get; set; }
    }

    public class Data
    {
        /// <summary>
        /// IBAN
        /// </summary>
        public string? TEXT_03 { get; set; }

        /// <summary>
        /// Ad Soyad
        /// </summary>
        public string? TEXT_04 { get; set; }
    }
}