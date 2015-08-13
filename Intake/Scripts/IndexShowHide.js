/// <reference path="CustomAutoComplete.js" />
function ticketChange() {
    //alert('Change');
    if ($("#Ticket").val().length > 0 && $('#Ticket_AutoComplete').val().length > 0) {
        //alert('1');
        if ($('#fg7').hasClass('hidden')) {
            //alert('2');
            $('#fg7').removeClass('hidden');
            //alert('3');
        }
    } else {
        //alert('4');
        if ($('#fg7').hasClass('hidden') == false) {
            $('#fg7').addClass('hidden');
        }
        if ($('#Submit').hasClass('disabled') == false) {
            $('#Submit').addClass('disabled');
        }
    }
}

function InitControls() {
    //alert('init');
    $("#VerzoekType").change(function () {
        //alert('Vuur!!');
        //alert($("#VerzoekType").val());
        if ($("#VerzoekType").val() > 0) {
            if ($('#fg3').hasClass('hidden')) {
                //alert('Has Hidden');
                $('#fg3').removeClass('hidden');
                //alert('Hidden removed');
            }
            if ($('#fg4').hasClass('hidden')) {
                //alert('Has Hidden');
                $('#fg4').removeClass('hidden');
                //alert('Hidden removed');
            }
            if ($('#fg5').hasClass('hidden')) {
                //alert('Has Hidden');
                $('#fg5').removeClass('hidden');
                //alert('Hidden removed');
            }
        } else {
            if ($('#fg3').hasClass('hidden') == false) {
                $('#fg3').addClass('hidden');
            }
            if ($('#fg4').hasClass('hidden') == false) {
                $('#fg4').addClass('hidden');
            }
            if ($('#fg5').hasClass('hidden') == false) {
                $('#fg5').addClass('hidden');
            }
            if ($('#fg6').hasClass('hidden') == false) {
                $('#fg6').addClass('hidden');
            }
            if ($('#fg7').hasClass('hidden') == false) {
                $('#fg7').addClass('hidden');
            }
            if ($('#Submit').hasClass('disabled') == false) {
                $('#Submit').addClass('disabled');
            }
        }
    });
    $("#ContactPhone").keypress(function () {
        //alert('Vuur!!');
        //alert($("#Compagny_AutoComplete").val());

        if ($("#Compagny_AutoComplete").val().length > 0 &&
            $("#ContactName").val().length > 0 &&
            $("#ContactPhone").val().length > 0) {
            if ($('#VerzoekType').val() == 1 || $('#VerzoekType').val() == 3) {
                if ($('#fg7').hasClass('hidden')) {
                    $('#fg7').removeClass('hidden');
                }
                if ($('#fg6').hasClass('hidden') == false) {
                    $('#fg6').addClass('hidden');
                }
            } else {
                if ($('#fg6').hasClass('hidden')) {
                    $('#fg6').removeClass('hidden');
                }
            }
        } else {
            if ($('#fg6').hasClass('hidden') == false) {
                $('#fg6').addClass('hidden');
            }
            if ($('#fg7').hasClass('hidden') == false) {
                $('#fg7').addClass('hidden');
            }
            if ($('#Submit').hasClass('disabled') == false) {
                $('#Submit').addClass('disabled');
            }
        }
    });
    $("#ContactName").keypress(function () {
        //alert('Vuur!!');
        if ($("#Compagny_AutoComplete").val().length > 0 &&
            $("#ContactName").val().length > 0 &&
            $("#ContactPhone").val().length > 0) {
            if ($('#VerzoekType').val() == 1 || $('#VerzoekType').val() == 3) {
                if ($('#fg7').hasClass('hidden')) {
                    $('#fg7').removeClass('hidden');
                }
                if ($('#fg6').hasClass('hidden') == false) {
                    $('#fg6').addClass('hidden');
                }
            } else {
                if ($('#fg6').hasClass('hidden')) {
                    $('#fg6').removeClass('hidden');
                }
            }
        } else {
            if ($('#fg6').hasClass('hidden') == false) {
                $('#fg6').addClass('hidden');
            }
            if ($('#fg7').hasClass('hidden') == false) {
                $('#fg7').addClass('hidden');
            }
            if ($('#Submit').hasClass('disabled') == false) {
                $('#Submit').addClass('disabled');
            }
        }
    });
    $("input:hidden[name='Compagny']").change(function () {
        //alert('Vuur!!');
        if ($("#Compagny_AutoComplete").val().length > 0 &&
            $("#ContactName").val().length > 0 &&
            $("#ContactPhone").val().length > 0) {
            if ($('#VerzoekType').val() == 1 || $('#VerzoekType').val() == 3) {
                if ($('#fg7').hasClass('hidden')) {
                    $('#fg7').removeClass('hidden');
                }
                if ($('#fg6').hasClass('hidden') == false) {
                    $('#fg6').addClass('hidden');
                }
            } else {
                if ($('#fg6').hasClass('hidden')) {
                    $('#fg6').removeClass('hidden');
                }
            }
        } else {
            if ($('#fg6').hasClass('hidden') == false) {
                $('#fg6').addClass('hidden');
            }
            if ($('#fg7').hasClass('hidden') == false) {
                $('#fg7').addClass('hidden');
                if ($('#Submit').hasClass('disabled') == false) {
                    $('#Submit').addClass('disabled');
                }
            }
        }
    });

    //$("#Ticket_AutoComplete").change(function (o) {
    //    alert('Vuur!!');
    //    ticketChange
    //});

    $("#Message").keypress(function () {
        //alert('Vuur!!');
        if ($("#Message").val().length > 0) {
            if ($('#Submit').hasClass('disabled')) {
                $('#Submit').removeClass('disabled');
            }
        } else {
            if ($('#Submit').hasClass('disabled') == false) {
                $('#Submit').addClass('disabled');
            }
        }
    });
}
