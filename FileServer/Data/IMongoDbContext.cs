using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Data
{
    public interface IMongoDbContext
    {
        IGridFSBucket GetBucket();
    }
}
