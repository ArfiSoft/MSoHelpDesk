using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Intake.Helpers.Autocomplete
{

    public enum AutoCompleteType
    {
        ServiceDeskName,
        Compagny,
        Ticket
    }
    public static class AutoCompleteHelper
    {

        //public static MvcHtmlString Autocomplete(this HtmlHelper helper, string name, string value, string text, string actionUrl, bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        //{
        //    return GetAutocompleteString(helper, name, value, text, AutoCompleteType.None, actionUrl, isRequired, viewhtmlAttributes, onselectfunction: onselectfunction);
        //}
        public static MvcHtmlString Autocomplete(this HtmlHelper helper, string name, string value, string text, AutoCompleteType autoCompleteType, bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        {
            string actionUrl = string.Empty;
            UrlHelper url = new UrlHelper(helper.ViewContext.RequestContext);
            string acpath = url.Content("~/TestAutoComplete/");
            if (autoCompleteType == AutoCompleteType.Compagny)
                actionUrl = acpath + "GetCompagny";
            else if (autoCompleteType == AutoCompleteType.ServiceDeskName)
                actionUrl = acpath + "GetServiceDeskName";
            else if (autoCompleteType == AutoCompleteType.Ticket)
                actionUrl = acpath + "GetTicket";

            return GetAutocompleteString(helper, name, value, text, autoCompleteType, actionUrl, isRequired: isRequired, viewhtmlAttributes: viewhtmlAttributes, onselectfunction: onselectfunction);
        }
        private static MvcHtmlString GetAutocompleteString(HtmlHelper helper, string name, string value, string text, AutoCompleteType autoCompleteType, string actionUrl = "", bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        {
            if (viewhtmlAttributes == null)
                viewhtmlAttributes = new Dictionary<string, object>();

            viewhtmlAttributes.Add("data-autocomplete", true);

            viewhtmlAttributes.Add("data-autocompletetype", autoCompleteType.ToString().ToLower());

            viewhtmlAttributes.Add("data-sourceurl", actionUrl);


            viewhtmlAttributes.Add("data-valuetarget", name);

            if (!string.IsNullOrEmpty(onselectfunction))
            {
                viewhtmlAttributes.Add("data-electfunction", onselectfunction);
            }
            if (isRequired.HasValue && isRequired.Value)
            {
                viewhtmlAttributes.Add("data-val", "true");
                viewhtmlAttributes.Add("data-val-required", name + " is required");
            }

            var hidden = helper.Hidden(name, value);

            var textBox = helper.TextBox(name + "_AutoComplete", text, viewhtmlAttributes);

            var builder = new StringBuilder();

            builder.AppendLine(hidden.ToHtmlString());

            builder.AppendLine(textBox.ToHtmlString());

            return new MvcHtmlString(builder.ToString());
        }
        //public static MvcHtmlString AutocompleteFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string DisplayProperty, string actionUrl, bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        //{
        //    return GetAutocompleteForString(helper, expression, DisplayProperty, AutoCompleteType.None, actionUrl, isRequired, viewhtmlAttributes, onselectfunction: onselectfunction);
        //}
        public static MvcHtmlString AutocompleteFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string DisplayProperty, AutoCompleteType autoCompleteType, bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        {
            string actionUrl = string.Empty;
            UrlHelper url = new UrlHelper(helper.ViewContext.RequestContext);
            string acpath = url.Content("~/TestAutoComplete/");
            if (autoCompleteType == AutoCompleteType.Compagny)
                actionUrl = acpath + "GetCompagny";
            else if (autoCompleteType == AutoCompleteType.ServiceDeskName)
                actionUrl = acpath + "GetServiceDeskName";
            else if (autoCompleteType == AutoCompleteType.Ticket)
                actionUrl = acpath + "GetTicket";

            return GetAutocompleteForString(helper, expression, DisplayProperty, autoCompleteType, actionUrl, isRequired: isRequired, viewhtmlAttributes: viewhtmlAttributes, onselectfunction: onselectfunction);
        }

        private static MvcHtmlString GetAutocompleteForString<TModel, TValue>(HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string DisplayText, AutoCompleteType autoCompleteType, string actionUrl = "", bool? isRequired = false, IDictionary<string, object> viewhtmlAttributes = null, string onselectfunction = "")
        {
            if (viewhtmlAttributes == null)
                viewhtmlAttributes = new Dictionary<string, object>();

            viewhtmlAttributes.Add("data-autocomplete", true);

            viewhtmlAttributes.Add("data-autocompletetype", autoCompleteType.ToString().ToLower());

            viewhtmlAttributes.Add("data-sourceurl", actionUrl);


            if (!string.IsNullOrEmpty(onselectfunction))
            {
                viewhtmlAttributes.Add("data-electfunction", onselectfunction);
            }
            Func<TModel, TValue> method = expression.Compile();
            object value = null;
            if (helper.ViewData.Model != null)
                value = method((TModel)helper.ViewData.Model);

            string modelpropname = ((MemberExpression)expression.Body).ToString();

            modelpropname = modelpropname.Substring(modelpropname.IndexOf('.') + 1);

            viewhtmlAttributes.Add("data-valuetarget", modelpropname);


            if (isRequired.HasValue && isRequired.Value)
            {
                viewhtmlAttributes.Add("data-val", "true");
                viewhtmlAttributes.Add("data-val-required", modelpropname + " is required");
            }

            MvcHtmlString hidden = helper.HiddenFor(expression);

            MvcHtmlString textBox = helper.TextBox(modelpropname + "_AutoComplete", DisplayText, viewhtmlAttributes);

            var builder = new StringBuilder();

            builder.AppendLine(hidden.ToHtmlString());

            builder.AppendLine(textBox.ToHtmlString());

            return new MvcHtmlString(builder.ToString());
        }

    }

}