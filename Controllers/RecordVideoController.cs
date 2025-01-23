using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using weasewebrtcsavebigfileapi.Models;

namespace weasewebrtcsavebigfileapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordVideoController : ControllerBase
    {
        private readonly IOptions<BigVideoSaveLocation> _bigVideoSaveLocationService;
        public RecordVideoController(IOptions<BigVideoSaveLocation> bigVideoSaveLocationService)
        {
            _bigVideoSaveLocationService = bigVideoSaveLocationService;
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        public void Logla1(string message)
        {
            using (StreamWriter sw = new StreamWriter(_bigVideoSaveLocationService.Value.ErrorLogPath, true))
            {
                sw.WriteLine(string.Format("{0:dd.MM.yyyy HH.mm:ss} ~~ {1}", DateTime.Now, message));
                sw.Dispose();
                sw.Close();
            }
        }

        [DisableRequestSizeLimit]
        [HttpPost("savewebrtcvideo")]
        public async Task<IActionResult> SaveWebRTCVideo([FromForm] string arr, [FromForm] int? leadId, [FromForm] int? agentId)
        {
            try
            {
                Logla1("savewebrtcvideo apiye geldi..");
                byte[] myByteArray = Convert.FromBase64String(arr);
                BinaryWriter Writer = null;
                string DateTimeStr = string.Format("{0:dd-MM-yyy-HH.mmmm}", DateTime.Now);
                var fileName = leadId + "_" + DateTimeStr + ".mp4";
                Logla1("#1");

                var path = _bigVideoSaveLocationService.Value.BigVideoSaveLocationPath;
                Logla1("#2");
                string DateStrNoSlash = string.Format("{0:dd-MM-yyy}", DateTime.Now);
                string DateStr = string.Format("{0}{1:dd-MM-yyy}", "\\", DateTime.Now);
                string FolderName = path + DateStr;
                if (!Directory.Exists(FolderName))
                {
                    Directory.CreateDirectory(FolderName);
                }
                var pathPath = System.IO.Path.Combine(FolderName, fileName);
                try
                {
                    Logla1("#3");
                    Writer = new BinaryWriter(System.IO.File.OpenWrite(pathPath));
                    Writer.Write(myByteArray);
                    Writer.Flush();
                    Writer.Close();
                }
                catch (Exception ex)
                {
                    Logla1("HATA (1): " + ex.InnerException.Message + " " + ex.StackTrace);
                    throw;
                }
                return Ok(true);
            }
            catch (Exception ex)
            {
                Logla1("HATA (2): " + ex.InnerException.Message + " " + ex.StackTrace);
                throw;
            }
        }
    }
}
