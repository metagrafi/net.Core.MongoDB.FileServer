using FileServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServer.Repositories
{
    public interface IFilesRepository
    {
        Task AddFolder(Folder document);
        Task<bool> AddFileToFolder(string folderId, FileDescription file);
        Task<bool> FolderExists(string folderId);
    }
}
