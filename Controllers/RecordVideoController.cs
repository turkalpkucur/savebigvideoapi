using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using weasewebrtcsavebigfileapi.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace weasewebrtcsavebigfileapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecordVideoController : ControllerBase
    {
        private readonly IOptions<Params> _parametersService;
        public RecordVideoController(IOptions<Params> parametersService)
        {
            _parametersService = parametersService;
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        public void Logla1(string message)
        {
            using (StreamWriter sw = new StreamWriter(_parametersService.Value.ErrorLogPath, true))
            {
                sw.WriteLine(string.Format("{0:dd.MM.yyyy HH.mm:ss} ~~ {1}", DateTime.Now, message));
                sw.Dispose();
                sw.Close();
            }
        }

        [DisableRequestSizeLimit]
        [HttpPost("savewebrtcvideo")]
        public async Task<IActionResult> SaveWebRTCVideo([FromForm] string arr, [FromForm] int? leadId, [FromForm] int? agentId, [FromForm] int? firmId)
        {
            try
            {
                if (arr == null || firmId == null || agentId == null || leadId == null)
                {
                    throw new Exception("firmId, agentId , leadId could not be null..");
                }
                string fullPathToBeSent = "";
                Logla1("savewebrtcvideo apiye geldi..");
                byte[] myByteArray = Convert.FromBase64String(arr);
                BinaryWriter Writer = null;
                string DateTimeStr = string.Format("{0:dd-MM-yyy-HH.mmmm}", DateTime.Now);
                var fileName = leadId + "_" + DateTimeStr + ".mp4";
                Logla1("#1");

                var path = _parametersService.Value.BigVideoSaveLocationPath;
                Logla1("#2");
                string DateStrNoSlash = string.Format("{0:dd-MM-yyy}", DateTime.Now);

                string firmFolderName = System.IO.Path.Combine(path, firmId.ToString());
                if (!Directory.Exists(firmFolderName))
                {
                    Directory.CreateDirectory(firmFolderName);
                }

                string leadFolderName = System.IO.Path.Combine(firmFolderName, leadId.ToString());
                if (!Directory.Exists(leadFolderName))
                {
                    Directory.CreateDirectory(leadFolderName);
                }

                string FolderName = System.IO.Path.Combine(firmFolderName, leadFolderName, DateStrNoSlash);

                if (!Directory.Exists(FolderName))
                {
                    Directory.CreateDirectory(FolderName);
                }

                var fullPath = System.IO.Path.Combine(FolderName, fileName);
                try
                {
                    Logla1("#3");
                    Writer = new BinaryWriter(System.IO.File.OpenWrite(fullPath));
                    Writer.Write(myByteArray);
                    Writer.Flush();
                    Writer.Close();
                }
                catch (Exception ex)
                {
                    Logla1("HATA (1): " + ex.InnerException.Message + " " + ex.StackTrace);
                    throw;
                }

                fullPathToBeSent = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", "\\", "webrtcvideos", "\\", firmId, "\\", leadId, "\\", DateStrNoSlash, "\\", fileName);
                fullPathToBeSent = fullPathToBeSent.Replace("\\", "~");
                using StringContent jsonContent = new StringContent(
       JsonSerializer.Serialize(new
       {
           leadId = leadId,
           agentId = agentId,
           fileFullPath = fullPathToBeSent
       }),
       Encoding.UTF8,
       "application/json");

                var client = new HttpClient();
                var result = await client.PostAsync(_parametersService.Value.ApiLink + "/webrtclogs/finalizewebrtclogformeetingendingbyleadandagent", jsonContent);
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
