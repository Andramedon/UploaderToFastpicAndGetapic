using System.Net;

namespace ImageUploaderApp
{
    internal abstract class ImageUploader
    {
        private protected static readonly HttpClientHandler httpClientHandler;
        private protected static readonly HttpClient httpClient;
        
        static ImageUploader()
        {
            httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.Brotli | DecompressionMethods.GZip | DecompressionMethods.None;
            httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
            httpClient.DefaultRequestHeaders.Add("Connection", "Keep-Alive");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html, application/xhtml+xml, */*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
        }


        private ThumbnailTitleType thumbnailTitleType; //Что писать на превьюшке: text, size, filename, no
        private int thumbnailSize;    //Размер превьюшки по горизонтали
        private string thumbnailTitleText; //Текст на превьюшке

        private int imageQuality; //Качество jpeg
        private int imageRotate; //Значение угла: 0, 90, 180, 270
        private bool imageNeedReduce; //Нужно ли изменять размер
        private int imageSizeSide; //Размер стороны, ширина

        public ThumbnailTitleType ThumbnailTitleType
        {
            get { return thumbnailTitleType; }
            set { thumbnailTitleType = value; }
        }

        public int ThumbnailSize
        {
            get { return thumbnailSize; }
            set
            {
                if (value < 100 || value > 300)
                    thumbnailSize = 170;
                else
                    thumbnailSize = value;
            }
        }

        public string ThumbnailTitleText
        {
            get { return thumbnailTitleText; }
            set { thumbnailTitleText = value; }
        }

        public int ImageQuality
        {
            get { return imageQuality; }
            set
            {
                if (value < 1 || value > 100)
                    imageQuality = 100;
                else
                    imageQuality = value;
            }
        }

        public int ImageRotate
        {
            get { return imageRotate; }
            set
            {   //Округляем до ближайшего угла кратного 90 градусов
                value = Math.Abs(value);
                if (value % 90 < 45)
                {
                    imageRotate = (value / 90) * 90;
                }
                else
                {
                    imageRotate = (value / 90) * 90 + 90;
                }
            }
        }

        public bool ImageNeedReduce
        {
            get { return imageNeedReduce; }
            set { imageNeedReduce = value; }
        }

        public int ImageSizeSide
        {
            get { return imageSizeSide; }
            set
            {
                if (value > 0)
                    imageSizeSide = value;
                else
                    imageSizeSide = 500;
            }
        }

        public ImageUploader()
        {
            ThumbnailTitleType = ThumbnailTitleType.SIZE;
            ThumbnailSize = 150;
            ThumbnailTitleText = "Увеличить";

            ImageQuality = 100;
            ImageRotate = 0;
            ImageNeedReduce = true;
            ImageSizeSide = 500;
        }




        public abstract Task<List<string>> Upload(string imageFilePath);

    }
}
