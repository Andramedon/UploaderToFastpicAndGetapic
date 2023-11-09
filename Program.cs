

namespace ImageUploaderApp
{
    internal class Program
    {

        static void Main(string[] args)
        {
            //========================== Test ============================//
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Enter path to image");
                Console.ResetColor();
                return;
            }

            string imagePath = args[0];
            List<string> bbCodes;

            try
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Uploading the image {imagePath} to getapic.me...");
                GetapicUploader getapicUploader = new GetapicUploader();
                getapicUploader.ThumbnailTitleType = ThumbnailTitleType.TEXT;
                getapicUploader.ThumbnailTitleText = "Press";
                getapicUploader.ThumbnailSize = 100;
                getapicUploader.ImageNeedReduce = true;
                getapicUploader.ImageSizeSide = 700;
                getapicUploader.ImageRotate = 0;
                getapicUploader.ImageQuality = 100;
                bbCodes = getapicUploader.Upload(imagePath).Result;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("==========[BB codes of the uploaded image on getapic.me]==========");
                bbCodes.ForEach(Console.WriteLine);

            
                Console.WriteLine("\n\r\n\r\n\r\n\r");


                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Uploading the image {imagePath} to fastpic.org...");
                FastpicUploader fastpicUploader = new FastpicUploader();
                fastpicUploader.ThumbnailTitleType = ThumbnailTitleType.TEXT;
                fastpicUploader.ThumbnailTitleText = "Press";
                fastpicUploader.ThumbnailSize = 100;
                fastpicUploader.ImageNeedReduce = true;
                fastpicUploader.ImageSizeSide = 700;
                fastpicUploader.ImageRotate = 0;
                fastpicUploader.ImageQuality = 100;    
                bbCodes = fastpicUploader.Upload(imagePath).Result;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("==========[BB codes of the uploaded image on fastpic.org]==========");
                bbCodes.ForEach(Console.WriteLine);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Aaaa, fuck! Error: {ex.Message}");
            }


            Console.ResetColor();
            Console.ReadLine();
        }
    }
}
