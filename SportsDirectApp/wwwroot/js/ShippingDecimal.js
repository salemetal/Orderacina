$(function() {

    $('#Shipping:input').on('input', function (e) {
        $(this).val($(this).val().replace(',', '.'));
    });
})