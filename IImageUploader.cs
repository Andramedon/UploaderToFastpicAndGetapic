
namespace ImageUploaderApp
{
    internal interface IImageUploader
    {
        public Task<List<string>> Upload(string imageFilePath);
    }
}
