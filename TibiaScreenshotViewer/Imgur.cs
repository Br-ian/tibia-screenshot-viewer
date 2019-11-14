using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imgur.API;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;

namespace TibiaScreenshotViewer
{
    class Imgur
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task UploadScreenshot(TibiaScreenshot ts)
        {
            try
            {
                var client = new ImgurClient("10dc669cc5f15f7", "9bc4d3435e39d68b35592a86892b2045af355d8e");
                var endpoint = new ImageEndpoint(client);
                IImage image;
                using (var fs = new FileStream(ts.Path, FileMode.Open))
                {
                    image = await endpoint.UploadImageStreamAsync(fs, null, ts.ToString());
                }
                Log.Info("Image uploaded. Image Url: " + image.Link);
            }
            catch (ImgurException imgurEx)
            {
                Log.Info("An error occurred uploading an image to Imgur.");
                Log.Info(imgurEx.Message);
            }
        }
    }
}
