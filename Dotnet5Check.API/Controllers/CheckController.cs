using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Dotnet5Check.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ILogger<CheckController> _logger;

        public CheckController(IWebHostEnvironment hostEnvironment, ILogger<CheckController> logger)
        {
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }
        [HttpGet("Forwarded")]
        public object Forwarded()
        {
            var request = HttpContext.Request;
            return new
            {
                headers = request.Headers,
                ip4 = request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(),
                ip6 = request.HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString()
            };
            //var ip = request.Headers["X-Forwarded-For"].FirstOrDefault();
            //_logger.LogError("X-Forwarded-For:  " + JSON.Serialize(request.Headers["X-Forwarded-For"]));
            //_logger.LogError("X-Forwarded-For1:  " + request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString());
            //if (string.IsNullOrEmpty(ip))
            //{
            //    ip = ;
            //}
            //return ip;
        }

        [HttpGet("Captcha")]
        public object Captcha(string code = "验证码", int width = 230, int height = 100)
        {
            Bitmap bitmap = null;
            Graphics g = null;
            MemoryStream ms = null;
            Random random = new();

            Color[] colorArray = { Color.Black, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };

            string bgImagesDir = Path.Combine(_hostEnvironment.WebRootPath, "Captcha/Image");
            string[] bgImagesFiles = Directory.GetFiles(bgImagesDir);

            // 字体来自：https://www.zcool.com.cn/special/zcoolfonts/
            string fontsDir = Path.Combine(_hostEnvironment.WebRootPath, "Captcha/Font");
            string[] fontFiles = new DirectoryInfo(fontsDir)?.GetFiles()
                ?.Where(m => m.Extension.ToLower() == ".ttf")
                ?.Select(m => m.FullName).ToArray();

            int imgIndex = random.Next(bgImagesFiles.Length);
            string randomImgFile = bgImagesFiles[imgIndex];
            var imageStream = Image.FromFile(randomImgFile);

            bitmap = new Bitmap(imageStream, width, height);
            imageStream.Dispose();
            g = Graphics.FromImage(bitmap);
            Color[] penColor = { Color.Red, Color.Green, Color.Blue };
            int code_length = code.Length;
            var words = new List<string>();
            for (int i = 0; i < code_length; i++)
            {
                int colorIndex = random.Next(colorArray.Length);
                var f = new Font("文泉驿正黑", 18);
                if (f is null)
                {
                    int fontIndex = random.Next(fontFiles.Length);
                    f = LoadFont(fontFiles[fontIndex], 18, FontStyle.Regular);
                }
                Brush b = new SolidBrush(colorArray[colorIndex]);
                int _y = random.Next(height);
                if (_y > (height - 30))
                    _y -= 60;

                int _x = width / (i + 1);
                if ((width - _x) < 50)
                {
                    _x = width - 60;
                }
                string word = code.Substring(i, 1);

                _logger.LogWarning($"字体:{f.ToString()}");
                g.DrawString(word, f, b, _x, _y);
            }

            ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            g.Dispose();
            bitmap.Dispose();
            ms.Dispose();
            return new FileContentResult(ms.GetBuffer(), "image/jpeg");
        }

        /// <summary>
        /// 加载字体
        /// </summary>
        /// <param name="path">字体文件路径,包含字体文件名和后缀名</param>
        /// <param name="size">大小</param>
        /// <param name="fontStyle">字形(常规/粗体/斜体/粗斜体)</param>
        private static Font LoadFont(string path, int size, FontStyle fontStyle)
        {
            var pfc = new System.Drawing.Text.PrivateFontCollection();
            pfc.AddFontFile(path);// 字体文件路径
            return new Font(pfc.Families[0], size, fontStyle);
        }
    }
}
