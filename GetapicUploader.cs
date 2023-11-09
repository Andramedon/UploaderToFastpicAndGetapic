using System.Net.Http.Headers;
using System.Text.RegularExpressions;


namespace ImageUploaderApp
{
    internal class GetapicUploader : ImageUploader, IImageUploader
    {
        //////////////////////////////////////////////////////////////////////////////////////////
        // Переменные веб формы, передаваемые в POST запросе (взяты из HTML getapic.me)         //
        //////////////////////////////////////////////////////////////////////////////////////////
        // getpreviewsize;     //150 - размер превью                                            //
        //                                                                                      //
        // getpreviewalt;      //N | "" - превью чистое без надписей, передается пустая строка  //
        //                     //S - на превью размер изображения, передается "S"               //
        //                     //Y - на превью надпись, передается сама надпись                 //
        //                                                                                      //
        // needreduce;         //1 | 0 - уменьшать или нет изображение                          //
        // upload_resizeside;  //width | height - уменьшать по ширине или высоте                //
        // getreduceimage;     //500 - размер стороны картинки при уменьшении                   //
        // upload_quality;     //0 - 100 - качество изображения                                 //
        // upload_angle;       //0=0, 1=90, 2=180, 3=270, 4=360 - поворот картинки              //
        // gettypeofdownload;  //Y | N - не понятно что за хрень, передается всегда N           //
        // session;            //loshvnh4jfsnmtc5a42oqd9ku4 - идентификатор сессии              //
        // suid;               //fns5k8pbr7 - идентификатор юзера                               //
        //////////////////////////////////////////////////////////////////////////////////////////
        

        private const string getapic = "https://getapic.me";
        private const string uploadUrl = "https://getapic.me/upload";

        private ThumbnailTitleType thumbnailTitleType; //Что писать на превьюшке: text, size, filename, no
        private int thumbnailSize;    //Размер превьюшки по горизонтали
        private string thumbnailTitleText; //Текст на превьюшке

        private int imageQuality; //Качество jpeg
        private int imageRotate; //Значение угла: 0, 90, 180, 270, 360
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
                if (value < 50 || value > 800)
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

        public GetapicUploader()
        {
            thumbnailTitleType = ThumbnailTitleType.SIZE;
            thumbnailTitleText = "Увеличить";
            thumbnailSize = 150;
            
            imageQuality = 100;
            imageRotate = 0;
            imageNeedReduce = true;
            imageSizeSide = 500;
        }

        private MultipartFormDataContent PrepareMultipartFormDataContent(string imageFilePath, string session, string suid)
        {
            MultipartFormDataContent multipartFormContent = new MultipartFormDataContent();
            FileInfo fileInfo = new FileInfo(imageFilePath);
            string fileName = fileInfo.Name;

            //Устанавливаем переменные веб формы, передаваемые в POST запросе (взяты из HTML getapic.me)
            string getpreviewsize = thumbnailSize.ToString();
            string getpreviewalt = thumbnailTitleType switch {
                                        ThumbnailTitleType.NO => "",
                                        ThumbnailTitleType.SIZE => "S",
                                        ThumbnailTitleType.TEXT => thumbnailTitleText,
                                        ThumbnailTitleType.FILENAME => fileName,
                                        _ => ""
                                   };

            string needreduce = imageNeedReduce ? "1" : "0";
            string upload_resizeside = "width";
            string getreduceimage = imageSizeSide.ToString();
            string upload_quality = imageQuality.ToString();
            string upload_angle = (imageRotate / 90).ToString();
            string gettypeofdownload = "N";

            multipartFormContent.Add(new StringContent(getpreviewsize), name: "getpreviewsize");
            multipartFormContent.Add(new StringContent(getpreviewalt), name: "getpreviewalt");
            multipartFormContent.Add(new StringContent(needreduce), name: "needreduce");
            multipartFormContent.Add(new StringContent(upload_resizeside), name: "upload_resizeside");
            multipartFormContent.Add(new StringContent(getreduceimage), name: "getreduceimage");
            multipartFormContent.Add(new StringContent(upload_quality), name: "upload_quality");
            multipartFormContent.Add(new StringContent(upload_angle), name: "upload_angle");
            multipartFormContent.Add(new StringContent(gettypeofdownload), name: "gettypeofdownload");
            multipartFormContent.Add(new StringContent(session), name: "session");
            multipartFormContent.Add(new StringContent(suid), name: "suid");

            byte[] bytesOfFile = File.ReadAllBytes(imageFilePath);
            ByteArrayContent contentFile = new ByteArrayContent(bytesOfFile);
            contentFile.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            multipartFormContent.Add(contentFile, name: "file[]", fileName: fileName);

            return multipartFormContent;
        }

        private string GetSessionFromHtml(string html)
        {
            //regExp = \(\'input\[name=session\]\'\)\.val\(\'([a-zA-Z0-9]*?)\'\)  Group 1: ([a-zA-Z0-9]*?) = session
            string pattern = @"\(\'input\[name=session\]\'\)\.val\(\'([a-zA-Z0-9]*?)\'\)";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(html);
            if (matches.Count > 0)
            {
                return matches[0].Groups[1].Value;
            }
            return "";
        }

        private string GetSuidFromHtml(string html)
        {
            //regExp = \(\'input\[name=suid\]\'\)\.val\(\'([a-zA-Z0-9]*?)\'\)  Group 1: ([a-zA-Z0-9]*?) = suid
            string pattern = @"\(\'input\[name=suid\]\'\)\.val\(\'([a-zA-Z0-9]*?)\'\)";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(html);
            if (matches.Count > 0)
            {
                return matches[0].Groups[1].Value;
            }
            return "";
        }

        private string GetLinkToBBCodesPage(string html)
        {
            //regExp = \"data\"\:\{\"url\"\:\"(http\:.*?)\"\}  Group 1: (http:.*?) = link
            string pattern = @"\""data\""\:\{\""url\""\:\""(http\:.*?)\""\}";
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(html);
            if (matches.Count > 0)
            {
                string link = matches[0].Groups[1].Value;
                link = link.Replace(@"\", "");
                link = link.Replace("http://www.", "https://");
                return link;
            }
            return "";
        }
        
        private List<string> GetBBCodesFromHtml(string html)
        {
            List<string> bbCodes = new List<string>();
            //regExp = <input\s+[^>]*?\s*value=(["'])?(.*?)\1    Group 1: (["'])    Group 2: (.*?) = property value
            string tagInputPattern = @"<input\s+[^>]*?\s*value=([""'])?(.*?)\1";
            Regex regex = new Regex(tagInputPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(html);
            foreach (Match match in matches)
            {
                string code = match.Groups[2].Value;
                bbCodes.Add(code);
            }

            return bbCodes;
        }

        public async Task<List<string>> Upload(string imageFilePath)
        {
            string html = await httpClient.GetStringAsync(getapic);
            string session = GetSessionFromHtml(html);//Идентификатор сессии
            string suid = GetSuidFromHtml(html); //Идентификатор юзера
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Content = PrepareMultipartFormDataContent(imageFilePath, session, suid);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Accept", "application/json");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string text = await response.Content.ReadAsStringAsync();
            string link = GetLinkToBBCodesPage(text);
            html = await httpClient.GetStringAsync(link);
            List<string> bbCodes = GetBBCodesFromHtml(html);
            return bbCodes;
        }


    }



}
