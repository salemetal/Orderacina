var apiUrl = "/api/orderitems";

$(function () {

    var orderId = $("#orderId").text();

    $("#calcLoader").show();

    var url = apiUrl + "/CalculationHtml/" + orderId;

    $.get(url,
        function(html) {

            $("#calcContent").html(html);
            $("#calcLoader").hide();
            $('#calcContent').show();
        });

});

