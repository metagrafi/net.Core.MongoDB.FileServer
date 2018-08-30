using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Models
{
    public class FileAccumulator
    {
        public string Id { get; set; }
        public string Filename { get; set; }
    }

    public class FormDataUploadAccumulator
    {
        public string FileId { get; set; }
        public string Filename { get; set; }
        public KeyValueAccumulator Accumulator { get; set; }
    }
}
