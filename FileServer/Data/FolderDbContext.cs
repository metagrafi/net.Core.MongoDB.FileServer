using FileServer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Data
{
    public class FolderDbContext : IMongoDbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IGridFSBucket _bucket = null;
        private List<string> collections = null;

        public FolderDbContext(IOptions<MongoSettings> settings)
        {

            var client = new MongoClient(settings.Value.ConnectionString);

            if (client != null)
            {
                _database = client.GetDatabase(settings.Value.Databases[0]);
                _bucket = new GridFSBucket(_database);
                collections = settings.Value.FileCollections;
            }
        }
        public IMongoCollection<Folder> GetFolderCollection()
        {
            return _database.GetCollection<Folder>(collections[0]);
        }
       
        public IGridFSBucket GetBucket()
        {
            return _bucket;
        }
    }
}
