using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace BookSystem.Model
{
    public class BookService
    {
        private string GetDBConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return config.GetConnectionString("DBConn");
        }

        public List<Book> QueryBook(BookQueryArg arg)
        {
            var result = new List<Book>();
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                    SELECT 
                        A.BOOK_ID AS BookId,
                        A.BOOK_CLASS_ID AS BookClassId,
                        B.BOOK_CLASS_NAME AS BookClassName,
                        A.BOOK_NAME AS BookName,
                        CONVERT(varchar(10), A.BOOK_BOUGHT_DATE, 111) AS BookBoughtDate,
                        A.BOOK_STATUS AS BookStatusId,
                        C.CODE_NAME AS BookStatusName,
                        A.BOOK_KEEPER AS BookKeeperId,
                        M.USER_CNAME AS BookKeeperCname, 
                        M.USER_ENAME AS BookKeeperEname
                    FROM BOOK_DATA AS A
                    INNER JOIN BOOK_CLASS AS B ON A.BOOK_CLASS_ID = B.BOOK_CLASS_ID
                    LEFT JOIN BOOK_CODE AS C ON A.BOOK_STATUS = C.CODE_ID AND C.CODE_TYPE = 'BOOK_STATUS'
                    LEFT JOIN MEMBER_M AS M ON A.BOOK_KEEPER = M.USER_ID
                    WHERE 
                        (@BookName = '' OR A.BOOK_NAME LIKE '%' + @BookName + '%') AND
                        (@BookClassId = '' OR A.BOOK_CLASS_ID = @BookClassId) AND
                        (@BookKeeperId = '' OR A.BOOK_KEEPER = @BookKeeperId) AND
                        (@BookStatusId = '' OR A.BOOK_STATUS = @BookStatusId)
                    ORDER BY A.BOOK_ID ASC";

                var parameters = new
                {
                    BookName = arg.BookName ?? string.Empty,
                    BookClassId = arg.BookClassId ?? string.Empty,
                    BookKeeperId = arg.BookKeeperId ?? string.Empty,
                    BookStatusId = arg.BookStatusId ?? string.Empty
                };

                result = conn.Query<Book>(sql, parameters).ToList();
            }
            return result;
        }

        public void AddBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"
                INSERT INTO BOOK_DATA
                (
                    BOOK_NAME, BOOK_CLASS_ID,
                    BOOK_AUTHOR, BOOK_BOUGHT_DATE,
                    BOOK_PUBLISHER, BOOK_NOTE,
                    BOOK_STATUS, BOOK_KEEPER,
                    BOOK_AMOUNT,
                    CREATE_DATE, CREATE_USER, MODIFY_DATE, MODIFY_USER
                )
                VALUES 
                (
                    @BOOK_NAME, @BOOK_CLASS_ID,
                    @BOOK_AUTHOR, @BOOK_BOUGHT_DATE,
                    @BOOK_PUBLISHER, @BOOK_NOTE,
                    @BOOK_STATUS, @BOOK_KEEPER,
                    0,
                    GETDATE(), 'Admin', GETDATE(), 'Admin'
                )";

                var parameters = new
                {
                    BOOK_NAME = book.BookName,
                    BOOK_CLASS_ID = book.BookClassId,
                    BOOK_AUTHOR = book.BookAuthor,
                    BOOK_BOUGHT_DATE = book.BookBoughtDate,
                    BOOK_PUBLISHER = book.BookPublisher,
                    BOOK_NOTE = book.BookNote,
                    BOOK_STATUS = "A",
                    BOOK_KEEPER = string.Empty
                };

                conn.Execute(sql, parameters);
            }
        }

        public void UpdateBook(Book book)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                try
                {
                    // 1. 先更新書籍基本資料
                    string sql = @"
                        UPDATE BOOK_DATA
                        SET 
                            BOOK_NAME = @BOOK_NAME,
                            BOOK_CLASS_ID = @BOOK_CLASS_ID,
                            BOOK_AUTHOR = @BOOK_AUTHOR,
                            BOOK_BOUGHT_DATE = @BOOK_BOUGHT_DATE,
                            BOOK_PUBLISHER = @BOOK_PUBLISHER,
                            BOOK_NOTE = @BOOK_NOTE,
                            BOOK_STATUS = @BOOK_STATUS,
                            BOOK_KEEPER = @BOOK_KEEPER,
                            MODIFY_DATE = GETDATE(),
                            MODIFY_USER = 'Admin'
                        WHERE BOOK_ID = @BOOK_ID";

                    var parameters = new
                    {
                        BOOK_NAME = book.BookName,
                        BOOK_CLASS_ID = book.BookClassId,
                        BOOK_AUTHOR = book.BookAuthor,
                        BOOK_BOUGHT_DATE = book.BookBoughtDate,
                        BOOK_PUBLISHER = book.BookPublisher,
                        BOOK_NOTE = book.BookNote,
                        BOOK_STATUS = book.BookStatusId,
                        // 如果狀態是 A 或 C，強制清空借閱人 (防呆)
                        BOOK_KEEPER = (book.BookStatusId == "A" || book.BookStatusId == "C") ? string.Empty : book.BookKeeperId,
                        BOOK_ID = book.BookId
                    };

                    conn.Execute(sql, parameters);

                    // 2. 處理借閱紀錄：只有當狀態為 B(已借出) 或 U(已借出未領) 時，才寫入紀錄
                    if (book.BookStatusId == "B" || book.BookStatusId == "U")
                    {
                        // 實務上建議檢查是否已存在相同紀錄，避免重複 Insert (這裡示範強制寫入)
                        sql = @"
                            INSERT INTO BOOK_LEND_RECORD
                            (
                                BOOK_ID, KEEPER_ID, LEND_DATE,
                                CRE_DATE, CRE_USR, MOD_DATE, MOD_USR
                            )
                            VALUES
                            (
                                @BOOK_ID, @KEEPER_ID, GETDATE(),
                                GETDATE(), 'Admin', GETDATE(), 'Admin'
                            )";

                        conn.Execute(sql, new
                        {
                            BOOK_ID = book.BookId,
                            KEEPER_ID = book.BookKeeperId
                        });
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void DeleteBookById(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = @"DELETE FROM BOOK_DATA WHERE BOOK_ID = @BOOK_ID";
                conn.Execute(sql, new { BOOK_ID = bookId });
            }
        }

        public Book GetBookById(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 修改重點：針對日期欄位加上 CONVERT，轉成 yyyy/MM/dd 字串
                string sql = @"
                    SELECT 
                        BOOK_ID AS BookId,
                        BOOK_NAME AS BookName,
                        BOOK_CLASS_ID AS BookClassId,
                        BOOK_AUTHOR AS BookAuthor,
                        CONVERT(varchar(10), BOOK_BOUGHT_DATE, 23) AS BookBoughtDate,
                        BOOK_PUBLISHER AS BookPublisher,
                        BOOK_NOTE AS BookNote,
                        BOOK_STATUS AS BookStatusId,
                        BOOK_KEEPER AS BookKeeperId
                    FROM BOOK_DATA
                    WHERE BOOK_ID = @BookId";

                return conn.QueryFirstOrDefault<Book>(sql, new { BookId = bookId });
            }
        }

        public List<dynamic> GetLendRecordByBookId(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 修改：別名改為小寫駝峰 (camelCase) 以配合前端 Kendo Grid 的 fields 設定
                string sql = @"
                    SELECT 
                        Format(R.LEND_DATE, 'yyyy/MM/dd') AS lendDate,
                        R.KEEPER_ID AS bookKeeperId,
                        M.USER_ENAME AS bookKeeperEname,
                        M.USER_CNAME AS bookKeeperCname
                    FROM BOOK_LEND_RECORD AS R
                    INNER JOIN MEMBER_M AS M ON R.KEEPER_ID = M.USER_ID
                    WHERE R.BOOK_ID = @BookId
                    ORDER BY R.LEND_DATE DESC";

                return conn.Query<dynamic>(sql, new { BookId = bookId }).ToList();
            }
        }
    }
}