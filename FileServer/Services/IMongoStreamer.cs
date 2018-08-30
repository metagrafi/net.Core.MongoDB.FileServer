using FileServer.Models;
using Microsoft.AspNetCore.WebUtilities;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Services
{
    
    public interface IMongoStreamer
    {
        Task<GridFSDownloadStream<ObjectId>> Download(string id);
        Task<FileAccumulator> UploadFileAsync(MultipartReader reader);
        Task<FormDataUploadAccumulator> UploadFormAsync(MultipartReader reader);
    }
}
