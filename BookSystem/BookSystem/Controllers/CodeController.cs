using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{
    [Route("api/code")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        // 1. 取得借閱狀態 (原本就有的)
        [Route("bookstatus")]
        [HttpPost()]
        public IActionResult GetBookStatusData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    Data = codeService.GetBookStatusData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // 2. 取得圖書類別 (補上的功能)
        // 對應前端: api/code/bookclass
        [Route("bookclass")]
        [HttpPost()]
        public IActionResult GetBookClassData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    // 假設你的 Service 裡有這個方法，如果沒有請在 Service 補上
                    Data = codeService.GetBookClassData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // 3. 取得借閱人 (補上的功能)
        // 對應前端: api/code/user
        [Route("user")]
        [HttpPost()]
        public IActionResult GetUserData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    // 假設你的 Service 裡有這個方法
                    Data = codeService.GetUserData(),
                    Status = true,
                    Message = string.Empty
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return Problem();
            }
        }

    }
}
