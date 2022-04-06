using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Models
{
    /// <summary>
    /// This is the object that is going to be marshalled into a JSON when using the API
    /// </summary>
    public class ImageObj
    {
        public int Id { get; set; }
        public string? Tag { get; set; }
        public byte[]? Data { get; set; }
    }
}