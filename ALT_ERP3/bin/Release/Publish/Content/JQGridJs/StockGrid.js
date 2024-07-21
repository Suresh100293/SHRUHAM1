$(function () {
    $("#grid").jqGrid({
        url: "/StockGrid/GetData",
        datatype: 'json',
        mtype: 'Get',
        colNames: ['RecordKey', 'Field', 'Heading', 'Input'],
        colModel: [
            { key: true, hidden: true, name: 'RecordKey', index: 'RecordKey' },
            { key: true, name: 'Code', index: 'Fld', editable: false, width: 60, editable: true, editoptions: { size: 10 } },
            { key: true, name: 'Qty', index: 'Head', editable: false, width: 60, editable: true, editoptions: { size: 10 } },
            { key: true, name: 'Rate', index: 'FldType', editable: false, width: 60, editable: true, editoptions: { size: 10 } }
        ],
        //editurl: "/StockGrid/Edit",
        pager: jQuery('#pager'),
        rowNum: 500,
        rowList: [50, 100, 200, 500],
        scrollOffset: 0,
        //emptyrecords: 'No records to display',
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
        beforeRequest: function () {
            responsive_jqgrid($(".jqGrid"));
        },
    });
    jQuery("#grid").jqGrid('navGrid', "#pager",
        { edit: false, add: false, del: false, search: false, refresh: false, searchtext: "Search" });

    function getSelectedRow() {
        var grid = $("#grid");
        var rowKey = grid.getGridParam("selrow");

        if (rowKey)
            alert("Selected row primary key is: " + rowKey);
        else
            alert("No rows are selected");
    }


    function responsive_jqgrid(jqgrid) {
        jqgrid.find('.ui-jqgrid').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-view').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-view > div').eq(1).addClass('clear-margin span12').css('width', '').css('min-height', '0').css('display', 'grid');
        jqgrid.find('.ui-jqgrid-view > div').eq(2).addClass('clear-margin span12').css('width', '').css('min-height', '0').css('display', 'grid');
        jqgrid.find('.ui-jqgrid-sdiv').addClass('clear-margin span12').css('width', '');
        jqgrid.find('.ui-jqgrid-pager').addClass('clear-margin span12').css('width', '');
    };
});
