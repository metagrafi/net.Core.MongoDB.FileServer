using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Models
{
    public class Folder
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string NameId { get; set; }
        public List<FileDescription> Files { get; set; }

        public Folder()
        {
            Files = new List<FileDescription>();
        }
    }
}
