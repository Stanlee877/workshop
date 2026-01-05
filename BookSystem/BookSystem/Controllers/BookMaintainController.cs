using BookSystem.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BookSystem.Controllers
{
    [Route("api/bookmaintain")]
    [ApiController]
    public class BookMaintainController : ControllerBase
    {

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
                    return BadRequest(ModelState);
                }

            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [HttpPost()]
        [Route("querybook")]
        // 修改重點：前端送 application/json，這裡要用 [FromBody] 才能接到模糊查詢條件
        public IActionResult QueryBook([FromBody] BookQueryArg arg)
        {
            try
            {
                BookService bookService = new BookService();

                // 呼叫 Service 的 QueryBook (你剛剛已經修好有 LIKE 的那個)
                return Ok(bookService.QueryBook(arg));
            }
            catch (Exception)
            {
                return Problem();
            }
        }

        [HttpPost()]
        [Route("loadbook")]
        public IActionResult GetBookById([FromBody] int bookId)
        {
            try
            {
                BookService bookService = new BookService();
                ApiResult<Book> result = new ApiResult<Book>
                {
                    // TODO:明細畫面結果
                    // 修改重點：呼叫 Service 真正去撈 DB，而不是回傳假資料
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

        // TODO:UpdateBook()
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

        [HttpPost()]
        [Route("deletebook")] // 修正 Route 名稱，必須對應前端 script.js 的呼叫
        public IActionResult DeleteBookById([FromBody] int bookId) // 修正方法名稱
        {
            try
            {
                BookService bookService = new BookService();

                // 1. [新增] 刪除前檢查：先取得書籍資訊檢查狀態
                var book = bookService.GetBookById(bookId);

                // 2. 檢查借閱狀態 (B:已借出, U:已借出未領)
                // 假設 BookStatusId 是儲存狀態代碼的欄位
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
        [HttpPost()]
        [Route("lendrecord")]
        public IActionResult GetLendRecord([FromBody] LendRecordArg arg)
        {
            try
            {
                BookService bookService = new BookService();
                // 呼叫 Service 取得借閱紀錄
                var records = bookService.GetLendRecordByBookId(arg.BookId);

                // 因為你的 ApiResult 泛型可能不支援 List<dynamic>，直接回傳 Ok(records) 或包裝一下
                // 這裡假設前端可以直接吃 { data: [...] } 格式
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
