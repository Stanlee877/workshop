using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{
    /*
     * CodeController
     * 功能：提供各種代碼資料（code）查詢 API，例如：借閱狀態、圖書類別、使用者清單等。
     * 說明：前端會向本 Controller 的各端點發出 POST 請求取得下拉選單或狀態參考資料。
     */
    [Route("api/code")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        /// <summary>
        /// 取得書籍借閱狀態清單
        /// 回傳格式：ApiResult<List<Code>>，Data 欄位為狀態清單
        /// 對應前端：api/code/bookstatus
        /// </summary>
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
                // 發生例外時回傳 500（Problem）
                return Problem();
            }
        }

        /// <summary>
        /// 取得圖書類別清單
        /// 回傳格式：ApiResult<List<Code>>，Data 欄位為類別清單
        /// 對應前端：api/code/bookclass
        /// 注意：CodeService 類別需實作 GetBookClassData()
        /// </summary>
        [Route("bookclass")]
        [HttpPost()]
        public IActionResult GetBookClassData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    // 假設 Service 有此方法，如無請補上
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

        /// <summary>
        /// 取得借閱人（使用者）清單
        /// 回傳格式：ApiResult<List<Code>>，Data 欄位為使用者清單
        /// 對應前端：api/code/user
        /// 注意：CodeService 類別需實作 GetUserData()
        /// </summary>
        [Route("user")]
        [HttpPost()]
        public IActionResult GetUserData()
        {
            try
            {
                CodeService codeService = new CodeService();
                ApiResult<List<Code>> result = new ApiResult<List<Code>>()
                {
                    // 假設 Service 有此方法
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
