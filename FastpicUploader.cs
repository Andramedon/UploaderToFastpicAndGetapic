using System.Net.Http.Headers;
using System.Text.RegularExpressions;


namespace ImageUploaderApp
{
    internal class FastpicUploader : ImageUploader, IImageUploader
    {

        ////////////////////////////////////////////////////////////////////////////////////////////////
        // Переменные веб формы, передаваемые в POST запросе (взяты из HTML getapic.me)               //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        // check_thumb          //Что изображено на миниатюре, передается всегда                      //
        //                      //text - текст заданный в thumb_text                                  //
        //                      //size - размер загруженного изображения                              //
        //                      //no - никаких дополнительных надписей                                //
        //                                                                                            //
        // thumb_text           //Увеличить - Текст на миниатюре, передается всегда                   //
        // thumb_size           //100 - 300 - Размер миниатюры (ширина), передается всегда            //
        //                                                                                            //
        // check_orig_resize    //Передается если установлен чекбокс необходимости изменения размера  //
        // orig_resize          //Новый размер картинки, не более оригинального, изменяется ширина    //
        // res_select                                                                                 //
        //                                                                                            //
        // check_orig_rotate    //1 - Передается если установлен чекбокс поворота картинки            //
        // orig_rotate          //0, 90, 180, 270 - Значение угла                                     //
        //                                                                                            //
        // check_optimization   //on - Передается если установлен чекбокс изменения качества          //
        //                      //    картинки, указывает нужно ли изменять качество                  //
        // jpeg_quality         //0 - 100 - качество изображения, передается всегда                   //
        //                                                                                            //
        // submit               //Загрузить - кнопка загрузки, передается всегда                      //
        // uploading            //1 - просто отправляется единица, передается всегда                  //
        ////////////////////////////////////////////////////////////////////////////////////////////////



        private const string uploadUrl = "https://fastpic.org/uploadmulti";


        public FastpicUploader() : base()
        {
            
        }

        private MultipartFormDataContent PrepareMultipartFormDataContent(string imageFilePath)
        {
            MultipartFormDataContent multipartFormContent = new MultipartFormDataContent();
            FileInfo fileInfo = new FileInfo(imageFilePath);
            string fileName = fileInfo.Name;

            //Устанавливаем переменные веб формы, передаваемые в POST запросе (взяты из HTML fastpic.org)
            string check_thumb = ThumbnailTitleType.ToString().ToLower();
            string thumb_text = ThumbnailTitleText;
            string thumb_size = ThumbnailSize.ToString();
            
            string check_orig_resize = "1";
            string orig_resize = ImageSizeSide.ToString();
            string res_select = orig_resize;
            string check_orig_rotate = "1";
            string orig_rotate = ImageRotate.ToString();
            string check_optimization = "on";
            string jpeg_quality = ImageQuality.ToString();
            
            string submit = "Загрузить";
            string uploading = "1";

            multipartFormContent.Add(new StringContent(check_thumb), name: "check_thumb"); //Передается всегда
            multipartFormContent.Add(new StringContent(thumb_text), name: "thumb_text"); //Передается всегда
            multipartFormContent.Add(new StringContent(thumb_size), name: "thumb_size"); //Передается всегда

            if (ImageNeedReduce)
            {   //Передается если установлен чекбокс изменения размера картинки
                multipartFormContent.Add(new StringContent(check_orig_resize), name: "check_orig_resize");
            }
            
            multipartFormContent.Add(new StringContent(orig_resize), name: "orig_resize"); //Передается всегда 
            multipartFormContent.Add(new StringContent(res_select), name: "res_select"); //Передается всегда

            if (ImageRotate > 0)
            {   //Передается если установлен чекбокс поворота картинки
                multipartFormContent.Add(new StringContent(check_orig_rotate), name: "check_orig_rotate");
            }            
            
            multipartFormContent.Add(new StringContent(orig_rotate), name: "orig_rotate"); //Передается всегда

            if (ImageQuality < 100)
            {   //Передается если установлен чекбокс изменения качества картинки
                multipartFormContent.Add(new StringContent(check_optimization), name: "check_optimization");
            }
            
            multipartFormContent.Add(new StringContent(jpeg_quality), name: "jpeg_quality"); //Передается всегда
            multipartFormContent.Add(new StringContent(uploading), name: "uploading"); //Передается всегда
            multipartFormContent.Add(new StringContent(submit), name: "submit"); //Передается всегда

            byte[] bytesOfFile = File.ReadAllBytes(imageFilePath);
            ByteArrayContent contentFile = new ByteArrayContent(bytesOfFile);
            contentFile.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            multipartFormContent.Add(contentFile, name: "file[]", fileName: fileName);
            return multipartFormContent;
        }

        private List<string> GetBBCodesFromHtml(string html)
        {
            List<string> bbCodes = new List<string>();
            int beginIndex = html.IndexOf("Ссылка (Direct Link):");
            if (beginIndex == -1)
            {
                return bbCodes;
            }

            //regExp = <input\s+[^>]*?\s*value=(["'])?(.*?)\1    Group 1: (["'])    Group 2: (.*?) = property value
            string tagInputPattern = @"<input\s+[^>]*?\s*value=([""'])?(.*?)\1";
            Regex regex = new Regex(tagInputPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            MatchCollection matches = regex.Matches(html, beginIndex);
            foreach (Match match in matches)
            {
                string code = match.Groups[2].Value;
                bbCodes.Add(code);
            }

            return bbCodes;
        }

        public override async Task<List<string>> Upload(string imageFilePath)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Content = PrepareMultipartFormDataContent(imageFilePath);
            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.Headers.Contains("Refresh"))
            {
                IEnumerable<string> refreshHeaderEnum = response.Headers.GetValues("Refresh");
                string refreshHeader = string.Join("; ", refreshHeaderEnum);
                int indexURL = refreshHeader.IndexOf("url=");
                string refreshURL = refreshHeader.Substring(indexURL + 4);
                string html = await httpClient.GetStringAsync(refreshURL);
                List<string> bbCodes = GetBBCodesFromHtml(html);
                return bbCodes;
            }
            return new List<string>();
        }
    }
}
