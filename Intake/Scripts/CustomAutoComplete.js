//attached autocomplete widget to all the autocomplete controls
$(document).ready(function () {
    alert('JS started');
    BindAutoComplete();
    alert('Binded');
});
function BindAutoComplete() {
    alert('0');
    $('[data-autocomplete]').each(function (index, element) {
        var sourceurl = $(element).attr('data-sourceurl');
        var autocompletetype = $(element).attr('data-autocompletetype');
        //alert('1.'+index);

        $(element).autocomplete({
            source: function (request, response) {
                var d = null;
                if (autocompletetype== 'ticket') {
                    var orgId = -2147483648;
                    var org = $('#Compagny').val()
                    var sdName = $('#ServiceDeskName_AutoComplete').val();
                    //alert('sdName: '+sdName);
                    if (sdName == "") {
                        sdName = null
                    }
                    //alert('OrgId: ' + org);
                    if (org > orgId) {
                        orgId = org;
                    }
                    d = { filter: { text: request.term, OrgId: orgId, ServiceDesk: sdName} }
                }
                else {
                    d = { searchHint: request.term }
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
                        }));
                    },
                    error: function (data) {
                        alert(data);
                    },
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
            }
        });
    });
}