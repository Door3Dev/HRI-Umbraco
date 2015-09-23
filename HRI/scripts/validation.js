jQuery.validator.addMethod('uniqueEmail', function (value, element) {
    if (element.previousValue == value)
        return element.previousError;

    var error = false;
    $.ajax({
        url: "/umbraco/api/RegistrationApi/EmailIsInUse",
        data: { email: value },
        async: false,
        success: function (isInUse) {
            element.previousValue = value;
            element.previousError = error = !isInUse;
        }
    });
    return error;
});

jQuery.validator.addMethod('zipCode', function (value, element) {
    var error = false;
    $.ajax({
        url: '/Umbraco/Surface/ComparePlansSurface/ValidateZipCode',
        data: { ZipCode: value },
        async: false,
        success: function (data) {
            error = data == 'true';
        },
        dataType: 'html'
    });
    return error;
});

jQuery.validator.addMethod('phone', function (value, element) {
    // If the phone number is not blank and less than 10 digits
    return value.indexOf('_') < 0;
});

jQuery.validator.addMethod('specialCharacters', function (value, element) {
    var re = /[\`\~\!#\$%\^&\*\(\)\+\{\}\\[\]\?\\\/\>\<]/;
    return !re.test(value);
});

jQuery.extend(jQuery.validator.messages, {
    specialCharacters: 'Special characters are not allowed.',
    uniqueEmail: 'A member with this e-mail address already exists.',
    phone: 'Invalid Phone Number.',
    simpleZipCode: 'Invalid Zip Code.',
    zipCode: 'Invalid Zip Code.'
});