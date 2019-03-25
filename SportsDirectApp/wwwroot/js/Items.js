var usernameLoggedIn = '';
var usernameOrderCreatedBy = '';
var orderId;
//var apiUrl = "https://localhost:44302/api/orderitems";
var apiUrl = "/api/orderitems";
var sort = "id";

$(function () {

    orderId = $("#orderId").text();

    usernameLoggedIn = $("#usernameLoggedIn").text();
    usernameOrderCreatedBy = $("#usernameOrderCreatedBy").text();

    //load grid
    $("#itemsGrid").jqGrid
        ({
            url: apiUrl + "/ItemsByOrderId/" + orderId + "/" + sort,
            datatype: 'json',
            mtype: 'Get',
            colNames: ['Id', 'Price', 'Amount', 'Size', 'Description',
                'Url', 'Date', 'Created By', "Del", "Edit"],
            colModel: [
                {
                    name: "id", sortable: false, width: 40
                },
                { name: "price", sortable: false, formatter: priceFormatter, width: 50, align: "right" },
                { name: "amount", sortable: false, width: 50, align: "center" },
                { name: "size", sortable: false, width: 75, align: "center" },
                { name: "description", sortable: false, width: 180, },
                { name: "url", sortable: false, formatter: linkFormatter, width: 390 },
                { name: "dateCreated", sortable: false, formatter: dateFormatter, width: 125 },
                { name: "createdBy", sortable: false, width: 100, align: "center" },
                { width: 50, sortable: false, formatter: deleteFormatter, align: "center" },
                { width: 50, sortable: false, formatter: editFormatter, align: "center" }
            ],
            height: '100%',
            rowNum: 1000,
            viewrecords: true,
            emptyrecords: 'No records',
            jsonReader: {
                repeatitems: false,
                root: function (obj) { return obj; }
            },
            autowidth: true
        });

    //add item
    $('#btnAddItem').click(function () {

        var date = new Date($.now());
        date.setHours(date.getHours() + 2);

        var item = new Object();
        item.size = $("#txtSize").val();
        item.description = $("#txtDescription").val();
        item.url = $("#txtUrl").val();
        item.Price = $("#txtPrice").val().replace(",", ".");
        item.Amount = $("#selectAmount  option:selected").text();
        item.dateCreated = date;
        item.dateModified = date;
        item.createdBy = usernameLoggedIn;
        item.modifiedBy = usernameLoggedIn;
        item.orderId = orderId;

        var request = $.ajax({
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            url: apiUrl,
            method: "POST",
            data: JSON.stringify(item),
            dataType: "html"
        });

        request.done(function (msg) {
            clearItemData();
            reloadGrid();
        });

        request.fail(function (jqXHR, textStatus) {
            alert("Request failed: " + jqXHR.responseText);
        });

    });

    //delete
    $("#itemsGrid").on("click", ".deleteBtn", function () {

        $(this).prop("disabled", true);

        var id = this.parentElement.parentElement.id;

        var request = $.ajax({
            contentType: "application/json",
            url: apiUrl + "/" + id,
            type: 'DELETE',
            data: { id: id }
        });

        request.done(function (msg) {
            reloadGrid();
        });

        request.fail(function (jqXHR, textStatus) {
            alert("Request failed: " + jqXHR.responseText);

            $(this).prop("disabled", false);
        });

    });

    //edit - open edit modal
    $("#itemsGrid").on("click", ".editBtn", function () {

        var date = new Date($.now());
        date.setHours(date.getHours() + 2);

        var url = apiUrl + "/" + this.parentNode.parentNode.id;

        $.get(url, function (item) {

            $("#editId").text(item.id);
            $("#txtSizeEdit").val(item.size);
            $("#txtDescriptionEdit").val(item.description);
            $("#txtUrlEdit").val(item.url);
            $("#txtPriceEdit").val(item.price);
            $("#selectAmountEdit option:selected").text(item.amount);

            $('#editModal').show();
        });
    });

    //editConfirm
    $("#btnEditItem").on("click", function () {

        var date = new Date($.now());
        date.setHours(date.getHours() + 2);

        var itemId = $("#editId").text();

        var url = apiUrl + "/" + itemId;

        var item = new Object();
        item.id = itemId;
        item.size = $("#txtSizeEdit").val();
        item.description = $("#txtDescriptionEdit").val();
        item.url = $("#txtUrlEdit").val();
        item.price = $("#txtPriceEdit").val().replace(",", ".");
        item.amount = $("#selectAmountEdit option:selected").text();
        item.dateModified = date;
        item.modifiedBy = usernameLoggedIn;
        item.orderId = orderId;

        $.ajax({
            url: url,
            contentType: "application/json",
            type: 'PUT',
            data: JSON.stringify(item),
            success: function (result) {
                $('#editModal').hide();
                reloadGrid();
            },
            error: function (result) {
                alert("Edit fail: " + result);
            }
        });

    });

    //close edit modal
    $('#closeEditModal').on('click', function () {

        $('#editModal').hide();
    });

    //escape modals
    $(document).keyup(function (e) {
        if (e.keyCode === 27) {

            var editModal = $('#editModal');
            if (editModal.is(':visible')) {
                editModal.hide();
            }

            var calcModal = $('#calcModal');
            if (calcModal.is(':visible')) {
                calcModal.hide();
            }
        }
    });

    //calculate and open calc modal
    $("#btnCalculation").on('click', function () {

        $('#modalContentCalc').hide();
        $("#calcLoader").show();
        $('#calcModal').show();

        var url = apiUrl + "/CalculationHtml/" + orderId;

        $.get(url,
            function(html) {

                $("#modalContentCalcData").html(html);
                $("#calcLoader").hide();
                $('#modalContentCalc').show();
            });

    });
    //close calc modal
    $('#closeCalcModal').on('click', function () {

        $('#calcModal').hide();
    });

    //sorting
    $("#jqgh_itemsGrid_id").on("click", function () {
        if (sort === "id")
            sort = "idDesc";
        else sort = "id";
        reloadGrid();

    });
    $("#jqgh_itemsGrid_price").on("click", function () {
        if (sort === "price")
            sort = "priceDesc";
        else sort = "price";
        reloadGrid();

    });
    $("#jqgh_itemsGrid_dateCreated").on("click", function () {
        if (sort === "date")
            sort = "dateDesc";
        else sort = "date";
        reloadGrid();

    });
    $("#jqgh_itemsGrid_createdBy").on("click", function () {
        if (sort === "createdBy")
            sort = "createdByDesc";
        else sort = "createdBy";
        reloadGrid();

    });
});

function clearItemData() {

    $("#txtSize").val("");
    $("#txtDescription").val("");
    $("#txtUrl").val("");
    $("#txtPrice").val("");
    $("#selectAmount").prop("selectedIndex", 0);
}

function reloadGrid() {

    var newUrl = apiUrl + "/ItemsByOrderId/" + orderId + "/" + sort;

    jQuery("#itemsGrid").jqGrid().setGridParam({ url: newUrl }).trigger("reloadGrid");

    //  $('#itemsGrid').trigger('reloadGrid');
}

//formaters
function priceFormatter(cellValue, options, rowObject) {

    return parseFloat(cellValue).toFixed(2).replace(".", ",");;
}

function linkFormatter(cellvalue, options, rowObject) {

    return '<a href="' + cellvalue + '">' + cellvalue + '</a>';
}

function deleteFormatter(cellvalue, options, rowObject) {

    if ($('#orderClosed').length) {
        return '';
    }
    else {

        if (usernameLoggedIn === rowObject.createdBy || usernameOrderCreatedBy === usernameLoggedIn || usernameLoggedIn === 'admin@orderacina.hr') {
            return '<button type="button" class="deleteBtn btn btn-warning btn-sm"><span class="glyphicon glyphicon-off" aria-hidden="true"></span></button>';
        }
        return '';
    }
}

function editFormatter(cellvalue, options, rowObject) {

    if ($('#orderClosed').length) {
        return '';
    }
    else {

        if (usernameLoggedIn === rowObject.createdBy || usernameOrderCreatedBy === usernameLoggedIn || usernameLoggedIn === 'admin@orderacina.hr') {
            return '<button type="button" class="editBtn btn btn-info btn-sm"><span class="glyphicon glyphicon-wrench" aria-hidden="true"></span></button>';
        }
        return '';
    }
}

function dateFormatter(cellvalue, options, rowObject) {

    var date = new Date(cellvalue);

    var d = date.getDate().toString();
    if (d.length == 1) {
        d = "0" + d;
    }

    // JavaScript months are 0-11
    var m = (date.getMonth() + 1).toString();
    if (m.length == 1) {
        m = "0" + m;
    }

    var y = date.getFullYear();

    var h = date.getHours().toString();
    if (h.length == 1) {
        h = "0" + h;
    }

    var min = date.getMinutes().toString();
    if (min.length == 1) {
        min = "0" + min;
    }

    return d + "." + m + "." + y + "." + " " + h + ":" + min;
}

