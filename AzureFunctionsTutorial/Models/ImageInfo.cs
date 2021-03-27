using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFunctionsTutorial.Models
{
    class ImageInfo
    {
        [JsonProperty("imageid")]
        public string imageId { get; set; }
        [JsonProperty("date")]
        public DateTime currentdate { get; set; }
    }
}
