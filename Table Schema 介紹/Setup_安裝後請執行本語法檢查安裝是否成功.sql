Use GSSWEB

--執行以下語法若有資料請聯繫班主任
Select * From BOOK_LEND_RECORD Where BOOK_ID Not IN
(Select BOOK_ID From BOOK_DATA)