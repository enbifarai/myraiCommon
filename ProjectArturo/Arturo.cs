using Arturo.Model;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Arturo
{
    public class Arturo
    {
        #region RenderWidget
        public static MvcHtmlString DisegnaWidget(HtmlHelper html, WidgetType widgetType, string title, string icon, string mainText, string detail = "", 
                string description = "", string idWidget = "", ActionTemplate action = null, WidgetTextType mainTextType = WidgetTextType.Regular, string label = "", bool mainAction = false)
        {
            WidgetTemplate widget = WidgetTemplate.CreateWidget(widgetType, title, icon, mainText, idWidget, detail, description, action, mainTextType, label, mainAction);
            return html.Partial("~/Views/_Arturo/WidgetTemplate.cshtml", widget);
        }

        public static MvcHtmlString DisegnaWidget(HtmlHelper html, WidgetTemplate widget)
        {
            return html.Partial("~/Views/_RaiDesign/WidgetTemplate.cshtml", widget);
        }
        #endregion

        #region RenderBlock
        public static IDisposable BeginBlock(HtmlHelper htmlHelper, BlockType blockType, string title, List<ActionTemplate> actions = null, string contentId = null)
        {
            string templateUrl = "~/Views/_RaiDesign/BlockTemplate.cshtml";

            SectionTemplate block = SectionTemplate.CreateBlock(blockType, title, null, null, actions, contentId);
            block.OnlyHeader = true;
            htmlHelper.ViewContext.Writer.WriteLine(htmlHelper.Partial(templateUrl, block));

            return new RenderContainer<SectionTemplate>(htmlHelper, templateUrl);
        }
        public static MvcHtmlString RenderAsyncBlock(HtmlHelper html, BlockType blockType, string title, string action, string controller, string callback = null, List<ActionTemplate> actions = null)
        {
            SectionTemplate block = SectionTemplate.CreateAsyncBlock(blockType, title, action, controller, callback, actions);
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        public static MvcHtmlString RenderBlock(HtmlHelper html, BlockType blockType, string title, string viewUrl, object model = null, List<ActionTemplate> actions = null, string contentId = null)
        {
            SectionTemplate block = SectionTemplate.CreateBlock(blockType, title, viewUrl, model, actions, contentId);
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        public static MvcHtmlString RenderBlock(HtmlHelper html, SectionTemplate block)
        {
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        #endregion

        private class RenderContainer<T> : IDisposable where T : RaiContainer, new()
        {
            private readonly HtmlHelper _htmlHelper;
            private string _viewUrl;

            public RenderContainer(HtmlHelper htmlHelper, string viewUrl)
            {
                _htmlHelper = htmlHelper;
                _viewUrl = viewUrl;
            }

            public void Dispose()
            {
                _htmlHelper.ViewContext.Writer.WriteLine(_htmlHelper.Partial(_viewUrl, new T() { OnlyFooter = true }));
            }
        }
    }
}
