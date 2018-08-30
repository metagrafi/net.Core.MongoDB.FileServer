using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileServer.Data;
using FileServer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FileServer.Repositories
{
    public class FilesRepository : IFilesRepository
    {
        private readonly FolderDbContext _context = null;

        public FilesRepository(IOptions<MongoSettings> settings)
        {
            _context = new FolderDbContext(settings);
        }

        public async Task<bool> FolderExists(string folderId)
        {
            var filter = Builders<Folder>.Filter.Where(x => x.Id == folderId);
            try
            {
                return await Task.Run(() => _context.GetFolderCollection().Find(filter).Any());

            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
        public async Task<bool> AddFileToFolder(string folderId, FileDescription file)
        {
            try
            {
                var filter = Builders<Folder>.Filter.Where(x => x.Id == folderId);

                var update = Builders<Folder>.Update.PushEach(x => x.Files, new List<FileDescription> { file });

                UpdateResult actionResult = await _context.GetFolderCollection().UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public Task AddFolder(Folder document)
        {
            try
            {
                return _context.GetFolderCollection().InsertOneAsync(document);

            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
