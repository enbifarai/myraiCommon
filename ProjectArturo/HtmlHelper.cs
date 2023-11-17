using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace Arturo.Model
{
    public enum ModalPosition
    {
        Center,
        Right
    }
    public enum ModalSize
    {
        Full,
        Half,
        Custom
    }
    public enum ModalColumnType
    {
        Main,
        Sub
    }
    public enum SectionType
    {
        Panel,
        Block
    }
    public enum PanelType
    {
        Panel,
        PanelNoPadding,
        SearchPanel,
        NoHeader
    }
    public enum BlockType
    {
        ContentTable
    }
    public enum WidgetType
    {
        Normal,
        Interactive
    }
    public enum WidgetTextType
    {
        Regular,
        Html
    }
    public enum HtmlAttributeType
    {
        None,
        Object,
        Dictionary
    }
    public enum SearchControlType
    {
        Select,
        TextBox
    }

    public class RaiAttributes : List<RaiAttribute>
    {
        public RaiAttributes():base()
        {
            
        }
    }

    public class RaiAttribute
    {
        public RaiAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Name+"=\""+Value+"\"";
        }
    }

    public class RaiContainer
    {
        public RaiContainer()
        {

        }
        public bool OnlyHeader { get; set; }
        public bool OnlyFooter { get; set; }
    }

    public class ModalTemplate
    {
        public string Name { get; set; }
        public ModalPosition Position { get; set; }
        public ModalSize DialogSize { get; set; }
        public string CustomDialogSize { get; set; }

        public RaiAttributes ModalAttributes { get; set; }
        public RaiAttributes ModalContentAttributes { get; set; }
    }
    public class ModalTemplateBody : RaiContainer
    {
        public ModalTemplateBody()
        {

        }
        public string ViewUrl { get; set; }
        public string Title { get; set; }
        public object Model { get; set; }
    }

    public class ModalColumn
    {
        public ModalColumnType Type { get; set; }
        public string Width { get; set; }
        public string Title { get; set; }
        public List<SectionTemplate> Sections { get; set; }
    }

    public class SectionTemplate : RaiContainer
    {
        public SectionTemplate()
        {

        }

        public string SectionId { get; set; }
        public SectionType Type { get; set; }
        public PanelType PanelType { get; set; }
        public BlockType BlockType { get; set; }
        public string ViewUrl { get; set; }
        public string Title { get; set; }
        public bool Async { get; set; }
        public object Model { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string AsyncCallBack { get; set; }
        public bool Collapsable { get; set; }
        public List<ActionTemplate> Actions { get; set; }
        public List<ActionTemplate> FooterActions { get; set; }
        public SwitchTemplate SwitchActions { get; set; }
        public bool CheckModel { get; set; }
        public bool DrawSkeleton { get; set; }
        public string ContentId { get; set; }
        public bool AsyncLoad { get; set; }

        public static SectionTemplate CreateAsyncPanel(PanelType panelType, string title, string action, string controller, bool collapsable = true, string callback = null, List<ActionTemplate> actions = null, List<ActionTemplate> footerActions = null, SwitchTemplate switchActions = null , string sectionId = "" )
        {
            SectionTemplate panel = new SectionTemplate()
            {
                Type = SectionType.Panel,
                PanelType = panelType,
                Async = true,
                Title = title,
                Action = action,
                Controller = controller,
                AsyncCallBack = callback,
                Collapsable = collapsable,
                Actions = actions,
                FooterActions = footerActions,
                SwitchActions = switchActions,
                SectionId = sectionId
            };

            return panel;
        }

        public static SectionTemplate CreatePanel(PanelType panelType, string title, string viewUrl, object model=null, bool collapsable=true, List<ActionTemplate> actions = null, List<ActionTemplate> footerActions = null, SwitchTemplate switchActions=null, bool checkModel = false, bool drawSkeleton = false, string contentId = "",string sectionId="")
        {
            SectionTemplate panel = new SectionTemplate()
            {
                Type = SectionType.Panel,
                PanelType = panelType,
                Title = title,
                ViewUrl = viewUrl,
                Collapsable = collapsable,
                Model = model,
                Actions = actions,
                FooterActions = footerActions,
                SwitchActions = switchActions,
                CheckModel = checkModel,
                DrawSkeleton = drawSkeleton,
                ContentId = contentId,
                SectionId = sectionId
            };

            return panel;
        }

        public static SectionTemplate CreateAsyncBlock(BlockType blockType, string title, string action, string controller, string callback = null, List<ActionTemplate> actions = null)
        {
            SectionTemplate block = new SectionTemplate()
            {
                Type = SectionType.Block,
                Async = true,
                Title = title,
                Action = action,
                Controller = controller,
                BlockType = blockType,
                AsyncCallBack = callback,
                Actions = actions
            };

            return block;
        }

        public static SectionTemplate CreateBlock(BlockType blockType, string title, string viewUrl, object model=null, List<ActionTemplate> actions = null, string contentId = "")
        {
            SectionTemplate block = new SectionTemplate()
            {
                Type = SectionType.Block,
                Title = title,
                ViewUrl = viewUrl,
                BlockType = blockType,
                Model = model,
                Actions = actions,
                ContentId = contentId
            };

            return block;
        }
    }

    public class OnePageTemplate : RaiContainer
    {
        public OnePageTemplate()
        {

        }

        public string Id { get; set; }
        public string Title { get; set; }
    }

    public class ActionTemplate
    {
        public string Id { get; set; }
        public string Href { get; set; }
        public string OnClick { get; set; }
        public string Icon { get; set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public bool Active { get; set; }
        public RaiAttributes Attributes { get; set; }
    }

    public class SwitchTemplate
    {
        public SwitchTemplate()
        {
            Actions = new List<ActionTemplate>();
        }

        public List<ActionTemplate> Actions { get; set; }
    }


    public class ProfileTemplate
    {
        public ProfileTemplate()
        {
            ImgAttributes = new Dictionary<string, string>();
            NameAttributes = new Dictionary<string, string>();
        }

        public string UrlImage { get; set; }

        public bool HasPresence { get; set; }
        public bool PresenceValue { get; set; }

        public bool HasCaption { get; set; }
        public string Caption { get; set; }


        public bool HasAction { get; set; }
        public string Action { get; set; }

        public string ProfileName { get; set; }

        public bool HasDetails1 { get; set; }
        public string Details1 { get; set; }

        public bool HasDetails2 { get; set; }
        public string Details2 { get; set; }

        public Dictionary<string,string> ImgAttributes { get; set; }
        public string GetImgClass()
        {
            if (!ImgAttributes.TryGetValue("class", out string result))
                result = "";
            return result;
        }
        public string GetImgAttributes()
        {
            string result = "";
            foreach (var item in ImgAttributes.Where(x=>x.Key!="class"))
                result += String.Format(" {0}=\"{1}\"", item.Key, item.Value);
            return result;
        }

        public Dictionary<string, string> NameAttributes { get; set; }
        public string GetNameClass()
        {
            if (!NameAttributes.TryGetValue("class", out string result))
                result = "";
            return result;
        }
        public string GetNameAttributes()
        {
            string result = "";
            foreach (var item in NameAttributes.Where(x => x.Key != "class"))
                result += String.Format(" {0}=\"{1}\"", item.Key, item.Value);
            return result;
        }
    }

    public class WidgetTemplate
    {
        public WidgetType Type { get; set; }

        public string Id { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string MainText { get; set; }
        public string Detail { get; set; }
        public string Description { get; set; }
        public bool HasAction { get; set; }
        public ActionTemplate Action { get; set; }
        public WidgetTextType MainTextType { get; set; }
        public string Label { get; set; }
        public bool MainAction { get; set; }

        public static WidgetTemplate CreateWidget(WidgetType type, string title, string icon, string mainText, string id = "", string detail = "", string description = "", ActionTemplate action = null, WidgetTextType mainTextType = WidgetTextType.Regular, string label="", bool mainAction=false)
        {
            WidgetTemplate widget = new WidgetTemplate()
            {
                Id = id,
                Title = title,
                Type = type,
                Icon = icon,
                MainText = mainText,
                Detail = detail,
                Description = description,
                HasAction = action!=null,
                Action = action,
                MainTextType = mainTextType,
                Label = label,
                MainAction = mainAction
            };
            return widget;
        }
    }

    public abstract class RaiBundle
    {
        public string viewPath  ;
        public RaiBundle()
        {
        }

        protected List<string> _path;
        public abstract void AddPath(string path);

        public List<string> GetPath()
        {
            List<string> path = new List<string>();
            path.AddRange(_path);
            return path;
        }
    }

    public class RaiCSSBundle : RaiBundle
    {
        public RaiCSSBundle ( )
        {
            _path = new List<string>( );
            viewPath = "~/Views/_RaiDesign/RenderCSS.cshtml";
        }

        public override void AddPath(string path)
        {
            _path.Add(path);
        }
    }

    public class RaiScriptBundle : RaiBundle
    {
        public RaiScriptBundle ( )
        {
            _path = new List<string>( );
            viewPath = "~/Views/_RaiDesign/RenderScript.cshtml";
        }

        public override void AddPath(string path)
        {
            _path.Add(path);
        }
    }

    public class RaiSelectControl
    {
        public SearchControlType ControlType { get; set; }
        public string ControlID { get; set; }
        public string GeneratedControl { get; set; }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Placeholder { get; set; }
        public string SearchPlaceholder { get; set; }
        public IEnumerable<SelectListItem> OptionList { get; set; }
        public bool ExpandView { get; set; }
        public bool MultiSelect { get; set; }
        public string UrlAction { get; internal set; }
        public int MinCharInput { get; internal set; }
        public bool IsAsync { get; internal set; }

        public object SelectedValue { get; set; }

        public string OnSelectChange { get; set; }

        public string GetParameterFunction { get; set; }
        public string SearchText { get; set; }
        public RaiSelectControl()
        {
        }
    }

    public class RaiInput
    {
        public bool FromFor { get; set; }

        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsChecked { get; set; }
        public HtmlAttributeType AttributeType { get; set; }
        public object ObjectHtmlAttributes { get; set; }
        public IDictionary<string,object> DictionaryHtmlAttributes { get; set; }
        //public Expression<Func<TModel, bool>> Expression { get; set; }
    }
}
