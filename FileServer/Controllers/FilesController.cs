using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Threading.Tasks;
using FileServer.Filters;
using FileServer.Models;
using FileServer.Repositories;
using FileServer.Services;
using FileServer.WebUtils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace FileServer.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private readonly IMongoStreamer _mongoStreamer;
        private MultipartRequestHelper _multipartRequestHelper;
        private readonly ILogger _logger;
        private readonly IFilesRepository _filesRepository;
        private readonly FormOptions _defaultFormOptions = new FormOptions();

        public FilesController(MultipartRequestHelper multipartRequestHelper, 
            IMongoStreamer mongoStreamer,
            ILogger<FilesController> logger,
            IFilesRepository filesRepository)
        {
            _mongoStreamer = mongoStreamer;
            _multipartRequestHelper = multipartRequestHelper;
            _logger = logger;
            _filesRepository = filesRepository;

        }
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] {  "You should implement a listing of directories..." };
        }

        [HttpGet("download-file")]
        public async Task<IActionResult> DownloadFile([FromQuery] string id, [FromQuery] string file)
        {
            try
            {
                var memory = new MemoryStream();
                using (var stream = await _mongoStreamer.Download(id))
                {
                    await stream.CopyToAsync(memory);
                    stream.Close();
                }
                memory.Position = 0;
                return new FileStreamResult(memory, _multipartRequestHelper.ContentTypeFromPath(file));
            }
            catch (Exception e)
            {
                _logger.LogError($"{DateTime.UtcNow} StackTrace:{e.StackTrace}");
                return
                        BadRequest(new
                        {
                            e.Message,
                            e.StackTrace,
                            ErrorDescription = "Could not get the page initial data."
                        });
            }

        }


        [HttpPost("upload-file")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadFile([FromQuery]string folderId)
        {
            if (!_multipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                var createFolder = string.IsNullOrEmpty(folderId) && ! await _filesRepository.FolderExists(folderId);
                if (createFolder)
                {
                    var folder = new Folder
                    {
                        NameId = "My Folder Id",

                    };
                    await _filesRepository.AddFolder(folder);
                    folderId = folder.Id;
                }
                var boundary = _multipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);
                var fileDesc = new FileDescription
                {

                    Url = "api/Files/download-file?id={id}&file={path}"
                };
                FileAccumulator newFile = null;

                newFile = await _mongoStreamer.UploadFileAsync(reader);

                if (newFile != null)
                {
                    fileDesc.Id = newFile.Id;
                    fileDesc.Url = fileDesc.Url.Replace("{id}", newFile.Id).Replace("{path}", Uri.EscapeDataString(newFile.Filename));
                    fileDesc.Name = newFile.Filename;
                    fileDesc.Created_time = DateTime.UtcNow;
                    if (!await _filesRepository.AddFileToFolder(folderId, fileDesc))
                    {
                        
                        return
                                        BadRequest(new
                                        {
                                            ErrorDescription = "Could not upload."
                                        });
                    }
                    return Ok(new { Folder = folderId, FileUrl = fileDesc.Url });
            
                }
                else
                {
                    _logger.LogError($"{DateTime.UtcNow} Upload failed.");
                    return
                              BadRequest(new
                              {
                                  Errors = new { Image = new object[] { new { ErrorMessage = "There is no image to upload" } } },
                                  ErrorDescription = "Could not upload."
                              });
                }


                //return Ok(new { Edge = post_edge, ImageUrl = template.Url });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return
                   BadRequest(new
                   {
                       e.StackTrace,
                       ErrorDescription = "Could not fulfill the request."
                   });
            }
        }
           

  
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
