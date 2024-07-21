$(function () {
    $("#grid").jqGrid({
        url: "/SalesmanMaster/GetAddOnList",
        datatype: 'json',
        mtype: 'Get',
        colNames: ['RecordKey', 'Field', 'Heading', 'Input'],
        colModel: [
            { key: true, hidden: true, name: 'RecordKey', index: 'RecordKey' },
            { key: true, name: 'Fld', index: 'Fld', editable: false },
            { key: true, name: 'Head', index: 'Head', editable: false },
            { key: true, name: 'FldType', index: 'FldType', editable: false }
        ],
        //editurl: "/SalesMan/Edit",
        pager: jQuery('#pager'),
        rowNum: 500,
        rowList: [50, 100, 200, 500],
        scrollOffset: 0,
        emptyrecords: 'No records to display',
        jsonReader: {
            root: "rows",
            page: "page",
            total: "total",
            records: "records",
            repeatitems: false,
            test: "test",
            id: "0"
        },
        height: 350,
        sortorder: 'desc',
        loadonce: true,
        gridview: true,
        viewrecords: true,
        addParams: { useFormatter: true },
        autowidth: true,
        shrinkToFit: true,
        multiselect: false,
        gridComplete: function () {
            var ids = jQuery("#grid").jqGrid('getDataIDs');
            for (var i = 0; i < ids.length; i++) {
                var rowId = ids[i];
                var rowData = jQuery('#grid').jqGrid('getRowData', rowId);
                var test = rowData.FldType;
                if (test == "M") {
                    var be = "<input type ='checkbox' id ='chk' style='display:block;' value='ON'/>";
                    //var be = "<input id='Option' type='checkbox' /><label class='checkbox' for='Option'></label>";
                    jQuery("#grid").jqGrid('setRowData', rowId, { FldType: be });
                }
                if (test == "T") {
                    // var be = "<input type='text' id='result' name='text[]' style='height:40px;width:120px;'/>";
                    var be = "<input id='result" + rowId + "' type='text'class='message' style='width:120px;' />";
                    jQuery("#grid").jqGrid('setRowData', rowId, { FldType: be });
                }
                if (test == "C") {
                    var be = "<select id='myselect'><option value='1'>Volvo</option><option value='2'>Saab</option><option value='3'>Opel</option><option value='4'>Audi</option></select>";
                    jQuery("#grid").jqGrid('setRowData', rowId, { FldType: be });
                }
            }
        },
        //onSelectRow: function (id) {
        //    var lastsel;
        //    var id = $("#grid").getGridParam('selrow');
        //    if (id && id !== lastsel) {
        //        //$('#grid').editRow(id);
        //        if (typeof lastsel !== "undefined") {
        //            $('#grid').restoreRow(lastsel);
        //            alert("fgdhfgfg");
        //        }
        //        lastsel = id;
        onSelectRow: function (id) {
            var lastsel;
            var id = $("#grid").getGridParam('selrow');
            if (id && id !== lastsel) {
                jQuery('#grid').jqGrid('restoreRow', lastsel)
                jQuery('#grid').jqGrid('editRow', id, true);

                lastsel = id;
            }
        },

        beforeRequest: function () {
            responsive_jqgrid($(".jqGrid"));
        },

        //    gridComplete: function(){ 
        //    var ids = jQuery("#grid").jqGrid('getDataIDs');
        //    alert(ids);
        //    for(var i=0;i < ids.length;i++)
        //    {
        //        var cl = ids[i]; 
        //        be = "<input style='height:22px;width:40px;' type='text' value='text1'   />"; 
        //        se = "<input style='height:22px;width:40px;' type='text' value='text2'  />";  jQuery("#grid").jqGrid('setRowData',ids[i],{fldType:be+se}); }
        //}  
    });
    jQuery("#grid").jqGrid('navGrid', "#pager",
            { edit: false, add: false, del: false, search: true, refresh: false, searchtext: "Search" });

    function run() {
        var index = document.getElementById("dropdown").selectedIndex;
        // Get Dropdownlist selected item text
        alert("text=" + document.getElementById("dropdown").options[index].text);
    }

    //function GetSelectedTextValue(dropdown) {
    //    var selectedText = dropdown.options[dropdown.selectedIndex].innerHTML;
    //    var selectedValue = dropdown.value;
    //    alert("Selected Text: " + selectedText + " Value: " + selectedValue);
    //}

    $("#btnSave").click(function () {
        var myarray = new Array();
        var test = jQuery("#grid").jqGrid('getDataIDs');
        for (var i = 0; i < test.length; i++) {
            var rowId = test[i];
            var rowData = jQuery('#grid').jqGrid('getRowData', rowId);
            var abc = rowData.Fld;
            var fldvalues = rowData.RecordKey;
            var textvalue = document.getElementById("result" + rowId).value;
            var y = "|";
            myarray.push(fldvalues, abc, textvalue, y);

        }

        var myJsonString = JSON.stringify(myarray);
        var newObject = JSON.parse(myJsonString);
        var btnvalue = document.getElementById("btnSave").value;
        var controlName = controllerName;
        $.ajax({
            type: 'POST',
            data: { inputvalues: myJsonString, Command: btnvalue },
            datatype: 'JSON',
            traditional: true,
            url: '/' + controlName + '/ExecuteSubmit',
            success: function (data) {

            }
        })

    });
    $("#savestate").click(function () {
        HideDialog();
    });
    function HideDialog() {
        $("#overlay").hide();
        $("#dialog1").fadeOut(300);
    }

    // var mydata = $("#grid").jqGrid("getGridParam", "rows"),
    //  myPrimarySkill = $.map(mydata, function (item) { return item.Fld; });
    // alert(JSON.stringify(myPrimarySkill));
    // var abc = rowData.Fld;
    //alert(abc);
    //jQuery("textarea.message").each(function () {
    //    var thought = $(this).val();

    //});

    // if (rowId == 2) 
    //{
    //$(function () {
    //    var values = $('input[name="text[]"]').map(function () {
    //        return this.value
    //    }).get()
    //    alert(values)
    //});

    //            var textvalue = document.getElementById("result").value;
    //       // }
    //        //if (rowId == 1)
    //       // {
    //            if (chk.checked == true)
    //            {
    //                chktest = 1;
    //            }
    //            if(chk.checked == false) {
    //                chktest = 0;
    //            }
    //        //}
    //            //if (rowId == 3)
    //            //{
    //                var e = document.getElementById("myselect");
    //                var strUser = e.options[e.selectedIndex].text;  
    //           // }
    //            //alert(textvalue);
    //            var url = "/SalesMan/Edit";
    //            var name = textvalue;
    //            var reck = rowId;
    //            var chkvalue = chktest;
    //            var drpvalue = strUser;
    //            //var address = $("#Address").val();
    //            $.post(url, { id: name, reckey: reck, chkval: chkvalue,drpdwnval:drpvalue}, function (data) {
    //              // $("#msg").html(data);
    //            });
    //        }

    //});
    function responsive_jqgrid(jqgrid) {
        jqgrid.find('.ui-jqgrid').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-view').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-view > div').eq(1).addClass('clear-margin span12').css('width', '').css('min-height', '0').css('display', 'grid');
        jqgrid.find('.ui-jqgrid-view > div').eq(2).addClass('clear-margin span12').css('width', '').css('min-height', '0').css('display', 'grid');
        jqgrid.find('.ui-jqgrid-sdiv').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-pager').addClass('clear-margin span12').css('width', '');
        $("#next_pager").find(".ui-icon-seek-next")
    .css({ "background-image": "url('/Content/images/rightarrow.png')", "background-position": "0", "height": "20" });
        $("#last_pager").find(".ui-icon-seek-end")
    .css({ "background-image": "url('/Content/images/rghtarrowend.png')", "background-position": "0", "height": "20" });
        $("#prev_pager").find(".ui-icon-seek-prev")
    .css({ "background-image": "url('/Content/images/leftarrow.png')", "background-position": "0", "height": "20" });
        $("#first_pager").find(".ui-icon-seek-first")
    .css({ "background-image": "url('/Content/images/leftarrowfirst.png')", "background-position": "0", "height": "20" });

    };
});