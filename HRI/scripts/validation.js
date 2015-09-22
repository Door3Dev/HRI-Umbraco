jQuery.validator.addMethod('specialCharacters', function (value, element) {
    var re = /[\`\~\!#\$%\^&\*\(\)\+\{\}\\[\]\?\\\/\>\<]/;
    return !re.test(value);
});

jQuery.extend(jQuery.validator.messages, {
    specialCharacters: "Special characters are not allowed."
});