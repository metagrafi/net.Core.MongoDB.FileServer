using FileServer.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using System.IO;
using FileServer.WebUtils;
using FileServer.Data;

namespace FileServer.Services
{
    public class MongoStreamer : IMongoStreamer
    {

        private readonly IMongoDbContext _context = null;
        private readonly MultipartRequestHelper _requestHelper = null;

        public MongoStreamer(IMongoDbContext context, MultipartRequestHelper requestHelper)
        {
            //TODO inject genericDbContext
            _context = context;
            _requestHelper = requestHelper;
        }
        public async Task<FileAccumulator> UploadFileAsync(MultipartReader reader)
        {
            try
            {
                var filename = Guid.NewGuid().ToString();
                using (var destinationStream = await _context.GetBucket().OpenUploadStreamAsync(filename))
                {
                    var id = destinationStream.Id; // the unique Id of the file being uploaded
                    var section = await reader.ReadNextSectionAsync();

                    while (section != null)
                    {
                        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                        if (hasContentDispositionHeader)
                        {
                            if (_requestHelper.HasFileContentDisposition(contentDisposition))
                            {
                                filename = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).ToString();
                                // write the contents of the file to stream using asynchronous Stream methods
                                await section.Body.CopyToAsync(destinationStream);
                            }
                        }
                        section = await reader.ReadNextSectionAsync();
                    }
                    await destinationStream.CloseAsync(); // optional but recommended so Dispose does not block

                    var extension = Path.GetExtension(filename);
                    var newName = Guid.NewGuid().ToString() + extension;
                    _context.GetBucket().Rename(id, newName);

                    return new FileAccumulator
                    {
                        Id = id.ToString(),
                        Filename = filename
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        public Task<GridFSDownloadStream<ObjectId>> Download(string id)
        {
            try
            {
                var bucket = _context.GetBucket();
                return bucket.OpenDownloadStreamAsync(ObjectId.Parse(id));
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<FormDataUploadAccumulator> UploadFormAsync(MultipartReader reader)
        {
            try
            {
                var section = await reader.ReadNextSectionAsync();
                var filename = Guid.NewGuid().ToString();
                var formAccumulator = new KeyValueAccumulator();
                using (var destinationStream = await _context.GetBucket().OpenUploadStreamAsync(filename))
                {
                    var id = destinationStream.Id; // the unique Id of the file being uploaded

                    while (section != null)
                    {
                        var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                        if (hasContentDispositionHeader)
                        {
                            if (_requestHelper.HasFileContentDisposition(contentDisposition))
                            {
                                filename = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).ToString();
                                // write the contents of the file to stream using asynchronous Stream methods
                                await section.Body.CopyToAsync(destinationStream);
                            }
                            else if (_requestHelper.HasFormDataContentDisposition(contentDisposition))
                            {
                                // Content-Disposition: form-data; name="key"
                                //
                                // value

                                // Do not limit the key name length here because the 
                                // multipart headers length limit is already in effect.
                                var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).ToString();
                                var encoding = _requestHelper.GetEncoding(section);
                                using (var streamReader = new StreamReader(section.Body, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
                                {
                                    // The value length limit is enforced by MultipartBodyLengthLimit
                                    var value = await streamReader.ReadToEndAsync();
                                    if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                    {
                                        value = String.Empty;
                                    }
                                    formAccumulator.Append(key, value);

                                    //if (formAccumulator.ValueCount > 4)//Todo get default value
                                    //{
                                    //    throw new InvalidDataException($"Form key count limit exceeded.");
                                    //}
                                }
                            }
                        }

                        // Drains any remaining section body that has not been consumed and
                        // reads the headers for the next section.
                        section = await reader.ReadNextSectionAsync();
                    }

                    await destinationStream.CloseAsync(); // optional but recommended so Dispose does not block

                    var extension = Path.GetExtension(filename);
                    var newName = Guid.NewGuid().ToString() + extension;
                    _context.GetBucket().Rename(id, newName);
                    return new FormDataUploadAccumulator
                    {
                        FileId = id.ToString(),
                        Filename = newName,
                        Accumulator = formAccumulator
                    };
                }
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

    }
}
