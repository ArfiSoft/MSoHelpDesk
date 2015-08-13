//attached autocomplete widget to all the autocomplete controls
$(document).ready(function () {
    //alert('JS started');
    BindAutoComplete();
    //alert('Binded');
    InitControls();
});


function BindAutoComplete() {
    //alert('0');
    $('[data-autocomplete]').each(function (index, element) {
        var sourceurl = $(element).attr('data-sourceurl');
        var autocompletetype = $(element).attr('data-autocompletetype');
        //Load Lists
        var d = null;
        if (autocompletetype == 'ticket') {
            var org = "";
            var sd = "";
            var text = "";
            d = { Text: text, Org: org, ServiceDesk: sd };
        }
        else {
            d = { searchHint: "" };
        }
        $.ajax({
            url: sourceurl,
            dataType: "json",
            data: d,
            error: function (data) {
                alert('Fout bij het ophalen van de ' + autocompletetype + 's: ' + data);
            }
        });

        $(element).autocomplete({
            source: function (request, response) {
                var d = null;
                if (autocompletetype == 'ticket') {
                    var org = $('#Compagny_AutoComplete').val();
                    var sdName_ac = $('#ServiceDesk_AutoComplete').val();
                    if (sdName_ac == "") {
                        //alert(sdName_ac);
                        sdName_ac = null;
                    }
                    d = { text: request.term, Org: org, ServiceDesk: sdName_ac };
                }
                else {
                    d = { searchHint: request.term };
                }
                $.ajax({
                    url: sourceurl,
                    dataType: "json",
                    data: d,
                    success: function (data) {
                        response($.map(data, function (item) {
                            if (autocompletetype == 'servicedeskname') {
                                return {
                                    label: item.ServiceDeskName,
                                    value: item.ServiceDeskName,
                                    selectedValue: item.ID
                                };
                            }
                            else if (autocompletetype == 'compagny') {
                                return {
                                    label: item.CompagnyName,
                                    value: item.CompagnyName,
                                    selectedValue: item.ID
                                };
                            }
                            else if (autocompletetype == 'ticket') {
                                return {
                                    label: item.Ticket,
                                    value: item.Ticket,
                                    selectedValue: item.ID
                                };
                            }
                            else if (autocompletetype == 'scope') {
                                return {
                                    label: item.ScopeID,
                                    value: item.ScopeID,
                                    selectedValue: item.ID
                                };
                            }
                        }));
                    },
                    error: function (data) {
                        alert('Fout bij het ophalen van de ' + autocompletetype + 's: ' + data);
                    }
                });
            },
            select: function (event, ui) {
                var valuetarget = $(this).attr('data-valuetarget');
                $("input:hidden[name='" + valuetarget + "']").val(ui.item.selectedValue);

                var selectfunc = $(this).attr('data-electfunction');
                if (selectfunc != null && selectfunc.length > 0) {
                    window[selectfunc](event, ui);
                    //funName();
                }
                //    selectfunc(event, ui);
            },
            change: function (event, ui) {
                var valuetarget = $(this).attr('data-valuetarget');

                if (ui.item == null) {
                    $("input:hidden[name='" + valuetarget + "']").val('');
                    $("input[name='" + valuetarget + "_AutoComplete").val('');
                    $("input[name='" + valuetarget + "_AutoComplete").focus();
                }
            },
            //focus: function (event, ui) {
            //    $.ajax({
            //        url: sourceurl,
            //        dataType: "json",
            //        data: "",
            //        error: function (data) {
            //            alert(data);
            //        }
            //    });
            //}
        });
    })
    }