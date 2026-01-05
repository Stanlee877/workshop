using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // 確保有引用這個
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BookSystem.Model
{
    public class CodeService
    {
        // 取得連線字串 (維持你原本的寫法)
        private string GetDBConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            return config.GetConnectionString("DBConn");
        }

        // 1. 取得借閱狀態 (你原本寫好的)
        public List<Code> GetBookStatusData()
        {
            var result = new List<Code>();
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                string sql = "Select CODE_ID As Value, CODE_NAME As Text From BOOK_CODE Where CODE_TYPE=@CODE_TYPE";
                var parameter = new { CODE_TYPE = "BOOK_STATUS" }; // Dapper 參數簡化寫法
                result = conn.Query<Code>(sql, parameter).ToList();
            }
            return result;
        }

        // 2. [補上] 取得圖書類別 (解決 GetBookClassData 錯誤)
        public List<Code> GetBookClassData()
        {
            var result = new List<Code>();
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 從 BOOK_CLASS 資料表取得類別
                string sql = "SELECT BOOK_CLASS_ID AS Value, BOOK_CLASS_NAME AS Text FROM BOOK_CLASS ORDER BY BOOK_CLASS_ID";
                result = conn.Query<Code>(sql).ToList();
            }
            return result;
        }

        // 3. [補上] 取得借閱人 (解決 GetUserData 錯誤)
        public List<Code> GetUserData()
        {
            var result = new List<Code>();
            using (SqlConnection conn = new SqlConnection(GetDBConnectionString()))
            {
                // 修改：加上 ISNULL(USER_ENAME, '') 確保不會因為 NULL 而整串消失
                string sql = "SELECT USER_ID AS Value, USER_CNAME + '(' + ISNULL(USER_ENAME, '') + ')' AS Text FROM MEMBER_M ORDER BY USER_ID";
                result = conn.Query<Code>(sql).ToList();
            }
            return result;
        }

    }
}