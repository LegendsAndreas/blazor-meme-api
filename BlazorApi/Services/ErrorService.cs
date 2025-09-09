using BlazorApi.Models;

namespace BlazorApi.Services;

public class ErrorService
{
    public class MemeDeletedException : Exception
    {
        public int MemeId { get; }
        public string Name { get; }
        public string Extension { get; }
        public string MimeType { get; }

        public MemeDeletedException(int memeId, string name, string extension, string mimeType)
            : base($"Meme has been deleted! Id: {memeId}, Name: {name}, Extension: {extension}, MimeType: {mimeType}")
        {
            MemeId = memeId;
            Name = name;
            Extension = extension;
            MimeType = mimeType;
        }
    }

}