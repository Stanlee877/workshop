var areaOption={
    "query":"q",
    "detail":"d"
}

var apiRootUrl="https://localhost:7246/api/";
var state="";

var stateOption={
    "add":"add",
    "update":"update"
}

var defauleBookStatusId="A";

$(function () {
    
    registerRegularComponent();

    var validator = $("#book_detail_area").kendoValidator({
        rules:{
            //日期必填驗證
            dateCheckRule: function(input){
                if (input.is(".date_picker")) {
                    var selector=$("#"+$(input).prop("id"));
                    return selector.data("kendoDatePicker").value();
                }
                return true;
            }
        },
        messages: { 
            //日期驗證訊息
            dateCheckRule: function(input){ return input.attr("data-message_prefix")+"格式有誤";}
          }
        }).data("kendoValidator");


    $("#book_detail_area").kendoWindow({
        width: "1200px",
        title: "新增書籍",
        visible: false,
        modal: true,
        actions: [
            "Close"
        ],
        close: onBookWindowClose
    }).data("kendoWindow").center();

    $("#book_record_area").kendoWindow({
        width: "700px",
        title: "借閱紀錄",
        visible: false,
        modal: true,
        actions: [
            "Close"
        ]
    }).data("kendoWindow").center();
    

    $("#btn_add_book").click(function (e) {
        e.preventDefault();
        state=stateOption.add;

        enableBookDetail(true);
        clear(areaOption.detail);
        setStatusKeepRelation(state);

        $("#btn-save").css("display","");        
        $("#book_detail_area").data("kendoWindow").title("新增書籍");
        $("#book_detail_area").data("kendoWindow").open();
    });


    $("#btn_query").click(function (e) {
        e.preventDefault();
        
        var grid=getBooGrid();
        grid.dataSource.read();
    });

    $("#btn_clear").click(function (e) {
        e.preventDefault();
        clear(areaOption.query);
        //TODO: 清空後重新查詢
        $("#book_grid").data("kendoGrid").dataSource.read();
    });

    $("#btn-save").click(function (e) {
        e.preventDefault();
        if (validator.validate()) {
            switch (state) {
                case "add":
                    addBook();
                    break;
                case "update":
                    updateBook($("#book_id_d").val());
                break;
                default:
                    break;
            }
        }        
    });

    $("#book_grid").kendoGrid({
        dataSource: {
            transport: {
                read: {
                  url: apiRootUrl+"bookmaintain/querybook",
                  dataType: "json",
                  type: "post",
                  contentType: "application/json"
                },
                parameterMap: function (data, type) {
                if (type === "read") {
                    // 這裡把查詢條件包裝成後端看得懂的 JSON 字串
                    var result = {
                        "BookName":$("#book_name_q").val(),
                        //TODO: 補齊傳入參數
                        "BookClassId":$("#book_class_q").data("kendoDropDownList").value(),
                        "BookKeeperId":$("#book_keeper_q").data("kendoDropDownList").value(),
                        "BookStatusId":$("#book_status_q").data("kendoDropDownList").value()
                    };
                    return JSON.stringify(result);
                  }
                }
            },
            schema: {
                 model: {
                    fields: {
                        bookId: { type: "int" },
                        bookClassName: { type: "string" },
                        bookName: { type: "string" },
                        bookBoughtDate: { type: "string" },
                        bookStatusName: { type: "string" },
                        bookKeeperCname: { type: "string" }
                    }
                }
            },
            serverPaging: false,
            pageSize: 20,
        },
        height: 550,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "bookId", title: "書籍編號", width: "10%" },
            { field: "bookClassName", title: "圖書類別", width: "15%" },
            { field: "bookName", title: "書名", width: "30%" ,
              template: "<a style='cursor:pointer; color:blue' onclick='showBookForDetail(event,#:bookId #)'>#: bookName #</a>"
            },
            { field: "bookBoughtDate", title: "購書日期", width: "15%" },
            { field: "bookStatusName", title: "借閱狀態", width: "15%" },
            { field: "bookKeeperCname", title: "借閱人", width: "15%" },
            { command: { text: "借閱紀錄", click: showBookLendRecord }, title: " ", width: "120px" },
            { command: { text: "修改", click: showBookForUpdate }, title: " ", width: "100px" },
            { command: { text: "刪除", click: deleteBook }, title: " ", width: "100px" }
        ]

    });

    $("#book_record_grid").kendoGrid({
        dataSource: {
            data: [],
            schema: {
                model: {
                    fields: {
                        LendDate: { type: "string" },
                        BookKeeperId: { type: "string" },
                        BookKeeperEname: { type: "string" },
                        BookKeeperCname: { type: "string" }
                    }
                }
            },
            pageSize: 20,
        },
        height: 250,
        sortable: true,
        pageable: {
            input: true,
            numeric: false
        },
        columns: [
            { field: "lendDate", title: "借閱日期", width: "10%" },
            { field: "bookKeeperId", title: "借閱人編號", width: "10%" },
            { field: "bookKeeperEname", title: "借閱人英文姓名", width: "15%" },
            { field: "bookKeeperCname", title: "借閱人中文姓名", width: "15%" },
        ]
    });

})

/**
 * 當圖書類別改變時,置換圖片
 */
function onClassChange() {
    var dropdown = $("#book_class_d").data("kendoDropDownList");
    
    // 防呆：如果下拉選單還沒產生，就先不處理
    if (!dropdown) return;

    var selectedValue = dropdown.value();

    if(selectedValue === ""){
        $("#book_image_d").attr("src", "image/optional.jpg");
    }else{
        // 確保你的 image 資料夾有對應 ID 的圖片 (例如 Networking.jpg, Database.jpg)
        $("#book_image_d").attr("src", "image/" + selectedValue + ".jpg");
    }
}

/**
 * 當 BookWindow 關閉後要處理的作業
 */
function onBookWindowClose() {
    //清空表單內容
    clear(areaOption.detail);
}

function addBook() { 

    var grid = $("#book_grid").data("kendoGrid").dataSource.data();
    var nextId = 1;
    if (grid.length > 0) {
        var maxId = Math.max.apply(Math, grid.map(function(o) { return o.bookId; }));
        nextId = maxId + 1;
    }
    
    var book = {
        "BookId": nextId, // 自行產生新的 BookId
        //TODO: 補齊欄位值
        "BookName": $("#book_name_d").val(),
        "BookClassId": $("#book_class_d").data("kendoDropDownList").value(),
        "BookClassName": $("#book_class_d").data("kendoDropDownList").text(),
        "BookBoughtDate": kendo.toString($("#book_bought_date_d").data("kendoDatePicker").value(), "yyyy-MM-dd"), // 格式化日期
        "BookStatusId": "A",
        "BookStatusName": "可以借出",
        // 借閱人預設空值
        "BookKeeperId": "",
        "BookKeeperCname": "",
        "BookKeeperEname": "",
        "BookAuthor": $("#book_author_d").val(),
        "BookPublisher": $("#book_publisher_d").val(),
        "BookNote": $("#book_note_d").val()
    }

    console.log("傳送新增資料:", book);

    $.ajax({
        type: "post",
        url: apiRootUrl + "bookmaintain/addbook",
        data: JSON.stringify(book),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            alert("新增成功");
            $("#book_detail_area").data("kendoWindow").close();
            $("#book_grid").data("kendoGrid").dataSource.read();
        },
        error: function (xhr) {
            console.error(xhr.responseText);
            alert("新增發生錯誤,請檢查 Console 錯誤訊息 ");
            }
    });
    
 }

function updateBook(bookId){
    
    //TODO: 取得畫面上相關書籍資料
    var book={
       "BookId": bookId, // 記得帶入 BookId
        "BookName": $("#book_name_d").val(),
        "BookClassId": $("#book_class_d").data("kendoDropDownList").value(),
        "BookClassName": $("#book_class_d").data("kendoDropDownList").text(), 
        "BookBoughtDate": kendo.toString($("#book_bought_date_d").data("kendoDatePicker").value(), "yyyy-MM-dd"),
        "BookStatusId": $("#book_status_d").data("kendoDropDownList").value(),
        "BookStatusName": $("#book_status_d").data("kendoDropDownList").text(),
        "BookKeeperId": $("#book_keeper_d").data("kendoDropDownList").value(),
        "BookKeeperCname": $("#book_keeper_d").data("kendoDropDownList").text(),
        "BookAuthor": $("#book_author_d").val(),
        "BookPublisher": $("#book_publisher_d").val(),
        "BookNote": $("#book_note_d").val()
    }

    $.ajax({
        type: "post",
        url: apiRootUrl + "bookmaintain/updatebook", // 請確認後端 API 路徑
        data: JSON.stringify(book),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            if (response === true || response.status === true) { // 依據後端回傳格式調整
                alert("修改成功");
                $("#book_detail_area").data("kendoWindow").close();
                // 重新整理 Grid 以顯示最新資料
                $("#book_grid").data("kendoGrid").dataSource.read();
            } else {
                alert("修改失敗");
            }
        },
        error: function (xhr) {
            alert("修改發生錯誤: " + xhr.responseText);
        }
    });
   
 }

function deleteBook(e) {
    e.preventDefault();
    var grid = getBooGrid();
    var row = grid.dataItem(e.target.closest("tr"));

    //新增檢查段 假設"B" 或 "U" 代表已借出
    //或判斷 row.bookStatusName =="已借出"
    if (row.bookStatusId === "B" || row.bookStatusId === "U" || row.bookStatusName === "已借出" || row.bookStatusName === "已借出(未領)") {
        alert("已借出書籍不可刪除");
        return;
    }


    if (confirm("確定刪除")) {
        
        $.ajax({
            type: "post",
            url: apiRootUrl+"bookmaintain/deletebook",
            data: JSON.stringify(row.bookId),
            contentType: "application/json",
            dataType: "json",
            success: function (response) {
                if(!response.Status){
                    alert(response.message);
                }else{
                    grid.dataSource.read();
                    alert("刪除成功");
                }
            }
        });
    }
}

/**
 * 顯示圖書明細-for 修改
 * @param {*} e 
 */
// 修改後的 showBookForUpdate (含錯誤偵測)
function showBookForUpdate(e) {
    e.preventDefault();

    state = stateOption.update;
    $("#book_detail_area").data("kendoWindow").title("修改書籍");
    $("#btn-save").css("display", "");

    var grid = getBooGrid();
    var bookId = grid.dataItem(e.target.closest("tr")).bookId;

    enableBookDetail(true);

    // 呼叫後端 API
    bindBook(bookId)
        .then(function() {
            // 資料讀取成功後，才設定狀態並開啟視窗
            setStatusKeepRelation(); 
            $("#book_detail_area").data("kendoWindow").open();
        })
        .fail(function(xhr, status, error) {
            // 如果 API 失敗，會跳出這個警告，幫助你除錯
            console.error("API Error:", xhr.responseText);
            alert("無法開啟修改視窗，原因：" + error + "\n請檢查 F12 Console 錯誤訊息");
        });
}

/**
 * 顯示圖書明細-for 明細(點選Grid書名超連結)
 * @param {*} e 
 */
function showBookForDetail(e,bookId) {
    e.preventDefault();

    state=stateOption.update;
    $("#book_detail_area").data("kendoWindow").title("書籍明細");

    //隱藏存檔按鈕
    $("#btn-save").css("display","none");
    
    //綁定資料
    bindBook(bookId).then(function(){
        onClassChange();

    //設定借閱狀態與借閱人關聯
        setStatusKeepRelation();

    //設定畫面唯讀與否
        enableBookDetail(false);
        $("#book_detail_area").data("kendoWindow").open();
    });
}

/**
 * 設定書籍明細畫面唯讀與否
 * @param {*} enable 
 */
function enableBookDetail(enable) { 

    $("#book_id_d").prop('readonly', !enable);
    $("#book_name_d").prop('readonly', !enable);
    $("#book_author_d").prop('readonly', !enable);
    $("#book_publisher_d").prop('readonly', !enable);
    $("#book_note_d").prop('readonly', !enable);

    if(enable){    
        $("#book_status_d").data("kendoDropDownList").enable(true);
        var datePicker = $("#book_bought_date_d").data("kendoDatePicker");
        if(datePicker) datePicker.enable(true);
    }else{
        $("#book_status_d").data("kendoDropDownList").readonly();
        var datePicker = $("#book_bought_date_d").data("kendoDatePicker");
        if(datePicker) datePicker.readonly();
    }
 }

 /**
  * 繫結書及明細畫面資料
  * @param {*} bookId 
  */
function bindBook(bookId){

    return $.ajax({
        type: "post",
        url: apiRootUrl+"bookmaintain/loadbook",
        data:JSON.stringify(bookId),
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            var book=response.data;
            //TODO: 補齊要綁的資料
            $("#book_id_d").val(book.bookId);
            $("#book_name_d").val(book.bookName);
            $("#book_author_d").val(book.bookAuthor);
            $("#book_publisher_d").val(book.bookPublisher);
            $("#book_note_d").val(book.bookNote);
            $("#book_bought_date_d").data("kendoDatePicker").value(book.bookBoughtDate);
            
            //下拉選單回填
            $("#book_class_d").data("kendoDropDownList").value(book.bookClassId);
            $("#book_keeper_d").data("kendoDropDownList").value(book.bookKeeperId);
            $("#book_status_d").data("kendoDropDownList").value(book.bookStatusId);

            onClassChange();
        },
        error:function(xhr){
            alert(xhr.responseText);
        }
    });    


}

function showBookLendRecord(e) {
    e.preventDefault();
    
    var grid = getBooGrid();
    var row = grid.dataItem(e.target.closest("tr"));
    var bookId = row.bookId; //row.bookId
    //TODO: 完成發 AJAX 和處理後續動作
    $.ajax({
        type: "post", // 或 get，看後端設計
        url: apiRootUrl + "bookmaintain/lendrecord", // 請確認後端 API 路徑，可能是 querylendrecord 之類
        data: JSON.stringify({ "BookId": bookId }), // 傳送 BookId
        contentType: "application/json",
        dataType: "json",
        success: function (response) {
            // 將回傳的資料塞給紀錄的 Grid
            var recordGrid = $("#book_record_grid").data("kendoGrid");
            // 假設回傳格式是 { data: [...] } 或直接是陣列
            var recordData = response.data || response; 
            
            recordGrid.dataSource.data(recordData);

            // 開啟視窗
            $("#book_record_area").data("kendoWindow").open();
        },
        error: function (xhr) {
            alert("讀取紀錄失敗");
        }
    }); 
}

function clear(area) {
    //TODO:補齊要清空的欄位
    switch (area) {
        case "q":
            $("#book_name_q").val("");
            $("#book_status_q").data("kendoDropDownList").select(0);
            $("#book_class_q").data("kendoDropDownList").select(0);
            $("#book_keeper_q").data("kendoDropDownList").select(0);
            break;
    
        case "d":
            $("#book_name_d").val("");
            $("#book_author_d").val("");
            $("#book_publisher_d").val("");
            $("#book_note_d").val("");

            $("#book_status_d").data("kendoDropDownList").select(0);
            $("#book_class_d").data("kendoDropDownList").select(0);
            $("#book_keeper_d").data("kendoDropDownList").select(0);
            $("#book_bought_date_d").data("kendoDatePicker").value(new Date());
            $("#book_image_d").attr("src", "image/optional.jpg");
            onClassChange();
            //清除驗證訊息
            var validator = $("#book_detail_area").data("kendoValidator");
            if (validator) validator.reset();
            break;
        default:
            break;
    }
}
                      
function setStatusKeepRelation() { 
    // 確認全域變數 state 的值 (add 或 update)
    switch (state) {
        case "add":
            // 新增模式：隱藏狀態與借閱人欄位 (因為預設是 A-可借出，且無借閱人)
            $("#book_status_d_col").css("display","none");
            $("#book_keeper_d_col").css("display","none");
        
            // 欄位隱藏後，取消必填屬性避免驗證錯誤
            $("#book_status_d").prop('required',false);
            $("#book_keeper_d").prop('required',false);            
            break;

        case "update":
            // 修改模式：顯示欄位
            $("#book_status_d_col").css("display","");
            $("#book_keeper_d_col").css("display","");
            $("#book_status_d").prop('required',true); // 狀態欄位本身必填

            // 取得目前選取的借閱狀態代碼
            var bookStatusId = $("#book_status_d").data("kendoDropDownList").value();

            // --- 邏輯修正重點 ---
            // 狀態 A (可以借出) 或 C (不可借出) -> 不需要借閱人
            if(bookStatusId == "A" || bookStatusId == "C"){
                // 1. 設定為非必填
                $("#book_keeper_d").prop('required', false);

                // 2. 建議直接 Disable 下拉選單並清空值，避免使用者誤填
                $("#book_keeper_d").data("kendoDropDownList").enable(false);
                $("#book_keeper_d").data("kendoDropDownList").value("");

                // 3. 移除 UI 上的必填星號
                $("#book_keeper_d_label").removeClass("required");
                
                // 4. 清除該欄位可能殘留的驗證錯誤訊息
                var validator = $("#book_detail_area").data("kendoValidator");
                if (validator) {
                    validator.hideMessages($("#book_keeper_d"));
                }
                
            } else {
                // 狀態 B (已借出) 或 U (已借出未領) -> 必須要有借閱人
                
                // 1. 設定為必填
                $("#book_keeper_d").prop('required', true);

                // 2. 啟用下拉選單
                $("#book_keeper_d").data("kendoDropDownList").enable(true);

                // 3. 加上 UI 上的必填星號
                $("#book_keeper_d_label").addClass("required");
            }
            break;
            
        default:
            break;
    }
}

 /**
  * 生成畫面上的 Kendo 控制項
  */
function registerRegularComponent(){

    $("#book_bought_date_d").kendoDatePicker({
        format: "yyyy-MM-dd",
        value: new Date(),
    
    });
    
    // 1. 圖書類別 (搜尋區)
    $("#book_class_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/bookclass", // 請確認你的 Controller 是不是叫 CodeController/GetBookClass
                    type: "post" // 你的後端看起來是用 POST
                }
            },
            schema: { data: "data" }
        }
    });

    // 2. 圖書類別 (新增/修改區)
    $("#book_class_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/bookclass",
                    type: "post"
                }
            },
            schema: { data: "data" }
        },
        change: onClassChange // 綁定圖片更換事件 [cite: 37]
    });

    // 3. 借閱人 (新增/修改區)
    $("#book_keeper_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/user", // 請確認 API 路徑
                    type: "post"
                }
            },
            schema: { data: "data" }
        }
    });

    // 4. 借閱人 (搜尋區)
     $("#book_keeper_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/user",
                    type: "post"
                }
            },
            schema: { data: "data" }
        }
    });
    // 5. 借閱狀態 (搜尋區)
    $("#book_status_q").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/bookstatus", // 確認你的 Controller 路徑
                    type: "post"
                }
            },
            schema: { data: "data" }
        }
    });

    // 6. 借閱狀態 (新增/修改區)
    $("#book_status_d").kendoDropDownList({
        dataTextField: "text",
        dataValueField: "value",
        optionLabel: "請選擇",
        dataSource: {
            transport: {
                read: {
                    dataType: "json",
                    url: apiRootUrl + "code/bookstatus",
                    type: "post"
                }
            },
            schema: { data: "data" }
        },
        change: setStatusKeepRelation // 狀態改變時，要觸發檢查是否顯示借閱人
    });

}

/**
 * 
 * @returns 取得畫面上的 book grid
 */
function getBooGrid(){
    return $("#book_grid").data("kendoGrid");
}