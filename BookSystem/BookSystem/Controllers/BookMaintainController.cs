using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{
    /*
     * BookMaintainController
     * 功能：提供圖書維護相關的 API (新增、查詢、載入明細、更新、刪除、借閱紀錄)
     * 說明：Controller 主要負責接收前端請求並呼叫 `BookService` 執行實際商業邏輯。
     */
    [Route("api/bookmaintain")]
    [ApiController]
    public class BookMaintainController : ControllerBase
    {

        /// <summary>
        /// 新增書籍
        /// 傳入：Book 物件（從前端 Model Binding 取得）
        /// 行為：驗證模型狀態，呼叫 Service 新增書籍，並回傳 ApiResult
        /// </summary>
        [HttpPost]
        [Route("addbook")]
        public IActionResult AddBook(Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    BookService bookService = new BookService();
                    bookService.AddBook(book);
                    return Ok(
                        new ApiResult<string>()
                        {
                            Data = string.Empty,
                            Status = true,
                            Message = string.Empty
                        });
                }
                else
                {
                    // 模型驗證失敗，回傳 400 與錯誤內容
                    return BadRequest(ModelState);
                }

            }
            catch (Exception)
            {
                // 若發生例外，回傳 500
                return Problem();
            }
        }

        /// <summary>
        /// 查詢書籍
        /// 傳入：BookQueryArg（以 [FromBody] 接收 JSON），可包含模糊搜尋條件
        /// 回傳：符合條件的書籍清單（Service 回傳的結果）
        /// 註：前端以 application/json POST 本端點
        /// </summary>
        [HttpPost()]
        [Route("querybook")]
        // 修改重點：前端送 application/json，這裡要用 [FromBody] 才能接到模糊查詢條件
        public IActionResult QueryBook([FromBody] BookQueryArg arg)
        {
            try
            {
                BookService bookService = new BookService();

                // 呼叫 Service 的 QueryBook
                return Ok(bookService.QueryBook(arg));
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        /// <summary>
        /// 取得書籍明細
        /// 傳入：bookId（從 Request Body 傳入）
        /// 回傳：ApiResult 包含 Book 明細
        /// </summary>
        [HttpPost()]
        [Route("loadbook")]
        public IActionResult GetBookById([FromBody] int bookId)
        {
            try
            {
                BookService bookService = new BookService();
                ApiResult<Book> result = new ApiResult<Book>
                {
                    // 呼叫 Service 去撈取資料庫中的書籍明細
                    Data = bookService.GetBookById(bookId),
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
        /// 更新書籍（含狀態與借閱人相關處理）
        /// 傳入：Book 物件
        /// 行為：呼叫 Service 執行更新動作，Service 應處理借閱紀錄的新增/修改
        /// </summary>
        [HttpPost()]
        [Route("updatebook")]
        public IActionResult UpdateBook(Book book)
        {
            try
            {
                BookService bookService = new BookService();

                // 呼叫 Service 更新並處理借閱紀錄
                bookService.UpdateBook(book);

                return Ok(
                        new ApiResult<string>()
                        {
                            Data = string.Empty,
                            Status = true,
                            Message = string.Empty
                        });
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        /// <summary>
        /// 刪除書籍
        /// 傳入：bookId
        /// 行為：刪除前會先檢查書籍借閱狀態，若已借出則不允許刪除
        /// 回傳：ApiResult 表示是否成功
        /// </summary>
        [HttpPost()]
        [Route("deletebook")] // 必須對應前端 script.js 的呼叫
        public IActionResult DeleteBookById([FromBody] int bookId)
        {
            try
            {
                BookService bookService = new BookService();

                // 1. 刪除前檢查：先取得書籍資訊檢查狀態
                var book = bookService.GetBookById(bookId);

                // 2. 檢查借閱狀態 (B:已借出, U:已借出未領)
                if (book.BookStatusId == "B" || book.BookStatusId == "U")
                {
                    // 若已借出，回傳失敗訊息，不執行刪除
                    return Ok(new ApiResult<string>
                    {
                        Data = string.Empty,
                        Status = false,
                        Message = "該書已借出不可刪除"
                    });
                }

                // 3. 檢查通過，真正執行刪除
                bookService.DeleteBookById(bookId);

                return Ok(new ApiResult<string>
                {
                    Data = string.Empty,
                    Status = true,
                    Message = "刪除成功"
                });
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        // TODO:booklendrecord
        /// <summary>
        /// 取得指定書籍的借閱紀錄
        /// 傳入：LendRecordArg (包含 BookId)
        /// 回傳：物件格式為 { data = records }，方便前端直接取用
        /// </summary>
        [HttpPost()]
        [Route("lendrecord")]
        public IActionResult GetLendRecord([FromBody] LendRecordArg arg)
        {
            try
            {
                BookService bookService = new BookService();
                // 呼叫 Service 取得借閱紀錄
                var records = bookService.GetLendRecordByBookId(arg.BookId);

                // 回傳包裝格式：{ data = [...] }
                return Ok(new { data = records });
            }
            catch (Exception)
            {
                return Problem();
            }
        }
    }

    // 用來接借閱紀錄參數的小類別
    public class LendRecordArg
    {
        public int BookId { get; set; }
    }
}
