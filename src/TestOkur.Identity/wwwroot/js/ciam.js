﻿$ = require('jquery');

$(function () {
    if ($('form').validate) {
        $('form').validate({
            highlight: function(input) {
                $(input).parents('.form-line').addClass('error');
            },
            unhighlight: function(input) {
                $(input).parents('.form-line').removeClass('error');
            },
            errorPlacement: function(error, element) {
                $(element).parents('.input-group').append(error);
            }
        });
    }
});
