using myRaiHelper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace System.Web.Mvc.Html
{
    public static class HtmlExtensions
    {
        #region Label
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types")]
        public static MvcHtmlString LabelForRequired<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText = "",
            object htmlAttributes = null)
        {
            return LabelHelper(html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression), labelText, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private static MvcHtmlString LabelHelper(HtmlHelper html,
           ModelMetadata metadata, string htmlFieldName, string labelText, Routing.RouteValueDictionary htmlAttributes)
        {
            if (htmlAttributes == null)
            {
                htmlAttributes = new Routing.RouteValueDictionary();
                //new System.Collections.Generic.Dictionary<string, object>();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }

            bool isRequired = false;

            if (metadata.ContainerType != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                .GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Length == 1;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add(
                "for",
                TagBuilder.CreateSanitizedId(
                    html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)
                )
            );

            if (isRequired)
                tag.Attributes.Add("class", "required");

            tag.SetInnerText(labelText);

            foreach (var x in htmlAttributes)
            {
                string val;

                if (tag.Attributes.TryGetValue(x.Key.ToString(), out val))
                {
                    if (x.Key.ToString().Equals("class"))
                    {
                        tag.Attributes[x.Key.ToString()] = string.Format("{0} {1}", val, x.Value.ToString());
                    }
                    else
                    {
                        tag.Attributes[x.Key.ToString()] = val;
                    }
                }
                else
                {
                    tag.Attributes.Add(x.Key.ToString(), x.Value.ToString());
                }
            }

            var output = tag.ToString(TagRenderMode.Normal);

            //if (isRequired)
            //{
            //    var asteriskTag = new TagBuilder("span");
            //    asteriskTag.Attributes.Add("class", "required");
            //    asteriskTag.SetInnerText("*");
            //    output += asteriskTag.ToString(TagRenderMode.Normal);
            //}
            return MvcHtmlString.Create(output);
        }

        public static MvcHtmlString LabelForRequiredObbl<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText = "",
            object htmlAttributes = null)
        {
            return LabelHelperObbl(html,
                ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                ExpressionHelper.GetExpressionText(expression), labelText, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        private static MvcHtmlString LabelHelperObbl(HtmlHelper html,
           ModelMetadata metadata, string htmlFieldName, string labelText, Routing.RouteValueDictionary htmlAttributes)
        {
            if (htmlAttributes == null)
            {
                htmlAttributes = new Routing.RouteValueDictionary();
                //new System.Collections.Generic.Dictionary<string, object>();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                labelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(labelText))
            {
                return MvcHtmlString.Empty;
            }


            bool isRequired = false;

            if (metadata.ContainerType != null)
            {
                isRequired = metadata.ContainerType.GetProperty(metadata.PropertyName)
                                .GetCustomAttributes(typeof(RequiredAttribute), false)
                                .Length == 1;
            }

            TagBuilder tag = new TagBuilder("label");
            tag.Attributes.Add(
                "for",
                TagBuilder.CreateSanitizedId(
                    html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)
                )
            );

            if (isRequired)
            {
                tag.Attributes.Add("class", "requiredobbl");

            }

            tag.SetInnerText(labelText);
            tag.InnerHtml = labelText + " <font color='#d2322d'>*</font>";

            foreach (var x in htmlAttributes)
            {
                string val;

                if (tag.Attributes.TryGetValue(x.Key.ToString(), out val))
                {
                    if (x.Key.ToString().Equals("class"))
                    {
                        tag.Attributes[x.Key.ToString()] = string.Format("{0} {1}", val, x.Value.ToString());
                    }
                    else
                    {
                        tag.Attributes[x.Key.ToString()] = val;
                    }
                }
                else
                {
                    tag.Attributes.Add(x.Key.ToString(), x.Value.ToString());
                }
            }

            var output = tag.ToString(TagRenderMode.Normal);

            //if (isRequired)
            //{
            //    var asteriskTag = new TagBuilder("span");
            //    asteriskTag.Attributes.Add("class", "required");
            //    asteriskTag.SetInnerText("*");
            //    output += asteriskTag.ToString(TagRenderMode.Normal);
            //}
            return MvcHtmlString.Create(output);
        }
        #endregion

        #region Profile
        public static MvcHtmlString ProfileWidget(this HtmlHelper html, string matricola, string nominativo = "", EnumPresenzaDip? presenza = null, string details1 = null, string details2 = null, string caption = null, object htmlImgAttributes = null, object htmlNominativoAttributes = null)
        {
            return ProfileWidgetCreator(html, ref matricola, nominativo, null, presenza, details1, details2, caption, htmlImgAttributes, htmlNominativoAttributes);
        }
        public static MvcHtmlString ProfileWidgetAction(this HtmlHelper html, string matricola, string nominativo, string action, EnumPresenzaDip? presenza = null, string details1 = null, string details2 = null, string caption = null, object htmlImgAttributes = null, object htmlNominativoAttributes = null)
        {
            return ProfileWidgetCreator(html, ref matricola, nominativo, action, presenza, details1, details2, caption, htmlImgAttributes, htmlNominativoAttributes);
        }

        private static MvcHtmlString ProfileWidgetCreator(HtmlHelper html, ref string matricola, string nominativo, string action, EnumPresenzaDip? presenza, string details1, string details2, string caption, object htmlImgAttributes, object htmlNominativoAttributes)
        {
            Routing.RouteValueDictionary attributes = null;
            if (htmlNominativoAttributes != null)
                attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlNominativoAttributes);

            Routing.RouteValueDictionary imgAttributes = null;
            if (htmlImgAttributes != null)
                imgAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlImgAttributes);

            //if (String.IsNullOrWhiteSpace(matricola))
            //    return MvcHtmlString.Empty;

            if (matricola!=null && (matricola.Length > 6 && matricola[0] == '0' || matricola[0] == 'P'))
                matricola = matricola.Substring(1);

            string urlImage = "";

            if (matricola == "M" || matricola == "F")
            {
                urlImage = CommonHelper.GetParametro<string>(EnumParametriSistema.PathImmaginiFittizie);
                urlImage = Path.Combine(urlImage, matricola.ToLower() + "1.png");
                if (urlImage.StartsWith("~")) urlImage = urlImage.Substring(1);
            }
            else if (!String.IsNullOrWhiteSpace(matricola))
                urlImage = CommonHelper.GetUrlFotoExternal(matricola);

            if (details1 != null && details1.ToLower() == "medium" && urlImage != null)
                urlImage = urlImage.Replace("risoluzione=3", "risoluzione=2");

            ProfileTemplate profileTemplate = new ProfileTemplate()
            {
                Matricola = matricola,
                UrlImage = urlImage,
                HasPresence = presenza.HasValue,
                PresenceValue = presenza.GetValueOrDefault(),
                HasCaption = !String.IsNullOrWhiteSpace(caption),
                Caption = caption,
                HasAction = !String.IsNullOrWhiteSpace(action),
                Action = action,
                ProfileName = nominativo.TitleCase(),
                HasDetails1 = !String.IsNullOrWhiteSpace(details1),
                Details1 = details1,
                HasDetails2 = !String.IsNullOrWhiteSpace(details2),
                Details2 = details2
            };

            if (imgAttributes != null)
            {
                foreach (var x in imgAttributes)
                {
                    string val;

                    if (profileTemplate.ImgAttributes.TryGetValue(x.Key.ToString(), out val))
                        profileTemplate.ImgAttributes[x.Key.ToString()] = val;
                    else
                        profileTemplate.ImgAttributes.Add(x.Key.ToString(), x.Value.ToString());
                }
            }

            if (attributes != null)
            {
                foreach (var x in attributes)
                {
                    string val;

                    if (profileTemplate.NameAttributes.TryGetValue(x.Key.ToString(), out val))
                        profileTemplate.NameAttributes[x.Key.ToString()] = val;
                    else
                        profileTemplate.NameAttributes.Add(x.Key.ToString(), x.Value.ToString());
                }
            }

            return html.Partial("~/Views/_RaiDesign/ProfileTemplate.cshtml", profileTemplate);
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

        #region Modal
        public static IDisposable BeginModal(this HtmlHelper htmlHelper, string title)
        {
            string templateUrl = "~/Views/_RaiDesign/ModalTemplateBody.cshtml";

            htmlHelper.ViewContext.Writer.WriteLine(htmlHelper.Partial(templateUrl, new ModalTemplateBody() { OnlyHeader = true, Title = title }));

            return new RenderContainer<ModalTemplateBody>(htmlHelper, templateUrl);
        }

        public static MvcHtmlString RenderModal(this HtmlHelper html, string nome, ModalPosition position = ModalPosition.Right, ModalSize size = ModalSize.Full, RaiAttributes modalAttributes = null, RaiAttributes modalContentAttributes = null, string customDialogSize = null)
        {
            ModalTemplate template = new ModalTemplate
            {
                Name = nome,
                Position = position,
                DialogSize = size,
                ModalAttributes = modalAttributes,
                ModalContentAttributes = modalContentAttributes,
                CustomDialogSize = customDialogSize
            };

            return html.Partial("~/Views/_RaiDesign/ModalTemplate.cshtml", template);
        }
        public static MvcHtmlString RenderModalColumn(this HtmlHelper html, ModalColumnType columnType, string columnWidth, string title, List<SectionTemplate> sections)
        {
            ModalColumn column = new ModalColumn()
            {
                Title = title,
                Type = columnType,
                Width = columnWidth,
                Sections = sections
            };

            return html.Partial("~/Views/_RaiDesign/ModalTemplateColumn.cshtml", column);
        }
        #endregion

        #region RenderOnePageNav
        public static IDisposable BeginOnePageNav(this HtmlHelper htmlHelper, string id, string title)
        {
            string templateUrl = "~/Views/_RaiDesign/OnePageNavigationTemplate.cshtml";
            OnePageTemplate template = new OnePageTemplate()
            {
                Id = id,
                Title = title
            };
            template.OnlyHeader = true;
            htmlHelper.ViewContext.Writer.WriteLine(htmlHelper.Partial(templateUrl, template));

            return new RenderContainer<OnePageTemplate>(htmlHelper, templateUrl);
        }
        #endregion

        #region RenderPanel
        public static IDisposable BeginPanel(this HtmlHelper htmlHelper, PanelType panelType, string title="", bool collapsable = true, List<ActionTemplate> actions = null, SwitchTemplate switchActions = null, string contentId = "", string sectionId = "", RaiAttributes panelAttributes=null)
        {
            string templateUrl = "~/Views/_RaiDesign/PanelTemplate.cshtml";

            SectionTemplate panel = SectionTemplate.CreatePanel(panelType, title, null, null, collapsable, actions, null, switchActions, false, false, contentId, sectionId, panelAttributes);
            panel.OnlyHeader = true;
            htmlHelper.ViewContext.Writer.WriteLine(htmlHelper.Partial(templateUrl, panel));

            return new RenderContainer<SectionTemplate>(htmlHelper, templateUrl);
        }

        public static MvcHtmlString RenderAsyncPanel(this HtmlHelper html, PanelType panelType, string title, string action, string controller, bool collapsable = true, string callback = null, List<ActionTemplate> actions = null, List<ActionTemplate> footerActions = null, SwitchTemplate switchActions = null, string sectionId = "", RaiAttributes panelAttributes = null, object routeValues=null)
        {
            SectionTemplate panel = SectionTemplate.CreateAsyncPanel(panelType, title, action, controller, collapsable, callback, actions, footerActions, switchActions, "", panelAttributes, routeValues);
            return html.Partial("~/Views/_RaiDesign/PanelTemplate.cshtml", panel);
        }

        public static MvcHtmlString RenderPanel(this HtmlHelper html, PanelType panelType, string title, string viewUrl, object model = null, bool collapsable = true, List<ActionTemplate> actions = null, List<ActionTemplate> footerActions = null, SwitchTemplate switchActions = null, bool checkModel = false, bool drawSkeleton = false, string contentId = "", string sectionId = "", RaiAttributes panelAttributes = null)
        {
            SectionTemplate panel = SectionTemplate.CreatePanel(panelType, title, viewUrl, model, collapsable, actions, footerActions, switchActions, checkModel, drawSkeleton, contentId, sectionId, panelAttributes);
            return html.Partial("~/Views/_RaiDesign/PanelTemplate.cshtml", panel);
        }
        public static MvcHtmlString RenderPanel(this HtmlHelper html, SectionTemplate panel)
        {
            return html.Partial("~/Views/_RaiDesign/PanelTemplate.cshtml", panel);
        }
        #endregion

        #region RenderBlock
        public static IDisposable BeginBlock(this HtmlHelper htmlHelper, BlockType blockType, string title, List<ActionTemplate> actions = null, string contentId = null, bool collapsable=false)
        {
            string templateUrl = "~/Views/_RaiDesign/BlockTemplate.cshtml";

            SectionTemplate block = SectionTemplate.CreateBlock(blockType, title, null, null, actions, contentId, collapsable);
            block.OnlyHeader = true;
            htmlHelper.ViewContext.Writer.WriteLine(htmlHelper.Partial(templateUrl, block));

            return new RenderContainer<SectionTemplate>(htmlHelper, templateUrl);
        }
        public static MvcHtmlString RenderAsyncBlock(this HtmlHelper html, BlockType blockType, string title, string action, string controller, string callback = null, List<ActionTemplate> actions = null, bool collapsable = false)
        {
            SectionTemplate block = SectionTemplate.CreateAsyncBlock(blockType, title, action, controller, callback, actions, collapsable);
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        public static MvcHtmlString RenderBlock(this HtmlHelper html, BlockType blockType, string title, string viewUrl, object model = null, List<ActionTemplate> actions = null, string contentId = null, bool collapsable = false)
        {
            SectionTemplate block = SectionTemplate.CreateBlock(blockType, title, viewUrl, model, actions, contentId, collapsable);
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        public static MvcHtmlString RenderBlock(this HtmlHelper html, SectionTemplate block)
        {
            return html.Partial("~/Views/_RaiDesign/BlockTemplate.cshtml", block);
        }
        #endregion

        #region RenderWidget
        public static MvcHtmlString RenderWidget(this HtmlHelper html, WidgetType widgetType, string title, string icon, string mainText, string detail = "", string description = "", string idWidget = "", ActionTemplate action = null, WidgetTextType mainTextType = WidgetTextType.Regular, string label = "", bool mainAction = false)
        {
            WidgetTemplate widget = WidgetTemplate.CreateWidget(widgetType, title, icon, mainText, idWidget, detail, description, action, mainTextType, label, mainAction);
            return html.Partial("~/Views/_RaiDesign/WidgetTemplate.cshtml", widget);
        }
        public static MvcHtmlString RenderWidget(this HtmlHelper html, WidgetTemplate widget)
        {
            return html.Partial("~/Views/_RaiDesign/WidgetTemplate.cshtml", widget);
        }
        #endregion

        #region RenderSelect
        public static MvcHtmlString RaiSelect(this HtmlHelper html, string id, IEnumerable<SelectListItem> listItems, string placeholder = "Seleziona un valore", string searchPlaceholder = "Cerca...", bool expandView = false, bool addElementMultiSelect = false, string onSelectchange = null, bool multiple = false, object attributes = null, bool hideSearch = false, SelectFilterType tipoFiltro = SelectFilterType.Contains, bool showCodeInDropdown = false, bool readOnly = false, string addClass = "", bool required = false)
        {
            string controlId = Guid.NewGuid().ToString();

            Routing.RouteValueDictionary attr = null;
            if (attributes != null)
                attr = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);

            if (attr == null)
                attr = new Routing.RouteValueDictionary();

            object tmp = null;
            if (attr.TryGetValue("data-rai-select", out tmp))
                attr["data-rai-select"] = controlId;
            else
                attr.Add("data-rai-select", controlId);

            if (attr.TryGetValue("style", out tmp))
                attr["style"] += "position:absolute;visibility:hidden;";
            else
                attr.Add("style", "position:absolute;visibility:hidden;");

            if (multiple && !attr.TryGetValue("multiple", out tmp))
                attr.Add("multiple", "multiple");

            string generatedControl = "";
            if (multiple)
                //generatedControl = html.DropDownList(id + "_" + controlId,  new List<SelectListItem>(listItems) , attr).ToHtmlString();
                generatedControl = html.DropDownList(id, new List<SelectListItem>(listItems), attr).ToHtmlString();
            else
                //generatedControl = html.DropDownList(id + "_" + controlId, new List<SelectListItem>(listItems), attr).ToHtmlString();
                generatedControl = html.DropDownList(id, new List<SelectListItem>(listItems), attr).ToHtmlString();

            int rowNum = 5;
            if (attr != null && attr.TryGetValue("size", out object size) && Int32.TryParse((string)size, out int tmpRowNum))
                rowNum = tmpRowNum;

            RaiSelectControl search = new RaiSelectControl()
            {
                ControlID = controlId,
                ControlType = SearchControlType.Select,
                GeneratedControl = generatedControl,
                //Id = id + "_" + controlId,
                Id = id,
                OptionList = listItems,
                Placeholder = placeholder,
                SearchPlaceholder = searchPlaceholder,
                ExpandView = expandView,
                OnSelectChange = onSelectchange,
                MultiSelect = multiple,
                HideSearch = hideSearch,
                TipoFiltro = tipoFiltro,
                ShowCodeInDropdown = showCodeInDropdown,
                ReadOnly = readOnly,
                VisibleRows = rowNum,
                AddElementMultiSelect = addElementMultiSelect,
                AddClassValue = addClass,
                Required = required
            };

            return html.Partial("~/Views/_RaiDesign/SearchControlTemplate.cshtml", search);
        }
        public static MvcHtmlString RaiSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> listItems, string placeholder = "Seleziona un valore", string searchPlaceholder = "Cerca...", bool expandView = false, string onSelectchange = null, bool multiple = false, object attributes = null, bool hideSearch=false, SelectFilterType tipoFiltro = SelectFilterType.Contains, bool showCodeInDropdown = false, bool readOnly = false)
        {
            string controlId = Guid.NewGuid().ToString();
            object selValue = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model;


            Routing.RouteValueDictionary attr = null;
            if (attributes!=null)
                attr = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);


            if (attr == null)
                attr = new Routing.RouteValueDictionary();

            object tmp = null;
            if (attr.TryGetValue("data-rai-select", out tmp))
                attr["data-rai-select"] = controlId;
            else
                attr.Add("data-rai-select", controlId);

            if (attr.TryGetValue("style", out tmp))
                attr["style"] += "position:absolute;visibility:hidden;";
            else
                attr.Add("style", "position:absolute;visibility:hidden;");

            if (multiple && !attr.TryGetValue("multiple", out tmp))
                attr.Add("multiple", "multiple");

            List<SelectListItem> tmpListItem = new List<SelectListItem>(); 
            //if (!listItems.Any(x => x.Value == ""))
            //    tmpListItem.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            if (listItems != null)
            {
                tmpListItem.AddRange(listItems);
            }


            string generatedControl = null;
            if (multiple)
                generatedControl = htmlHelper.ListBoxFor(expression, new MultiSelectList(tmpListItem, "Value", "Text", (object[])selValue), attr).ToHtmlString();
            else
                generatedControl = htmlHelper.DropDownListFor(expression, new SelectList(tmpListItem, "Value", "Text", selValue), attr).ToHtmlString();

            int rowNum = 5;
            if (attr != null && attr.TryGetValue("size", out object size) && Int32.TryParse((string)size, out int tmpRowNum))
                rowNum = tmpRowNum;

            RaiSelectControl search = new RaiSelectControl()
            {
                ControlID = controlId,
                ControlType = SearchControlType.Select,
                GeneratedControl = generatedControl,
                OptionList = tmpListItem,
                Placeholder = placeholder,
                SearchPlaceholder = searchPlaceholder,
                ExpandView = expandView,
                SelectedValue = selValue,
                OnSelectChange = onSelectchange,
                MultiSelect = multiple,
                HideSearch = hideSearch,
                TipoFiltro = tipoFiltro,
                ShowCodeInDropdown = showCodeInDropdown,
                ReadOnly = readOnly,
                VisibleRows = rowNum
            };

            return htmlHelper.Partial("~/Views/_RaiDesign/SearchControlTemplate.cshtml", search);
        }

        public static MvcHtmlString RaiAsyncSelect(this HtmlHelper html, string id, string action, int minCharInput = 3, string placeholder = "Seleziona un valore", string searchPlaceholder = "Cerca...", bool expandView = false, string onSelectchange = null, string getParametersFunction = "", bool hideSearch=false, SelectFilterType tipoFiltro = SelectFilterType.Contains, bool showCodeInDropdown = false, bool readOnly = false, string searchText = "Immetti almeno {0} caratteri per cercare")
        {
            RaiSelectControl search = new RaiSelectControl()
            {
                IsAsync = true,
                ControlID = Guid.NewGuid().ToString(),
                ControlType = SearchControlType.Select,
                Id = id,
                Placeholder = placeholder,
                SearchPlaceholder = searchPlaceholder,
                ExpandView = expandView,
                UrlAction = action,
                MinCharInput = minCharInput,
                SearchText=searchText,
                OnSelectChange = onSelectchange,
                GetParameterFunction = getParametersFunction,
                HideSearch = hideSearch,
                TipoFiltro = tipoFiltro,
                ShowCodeInDropdown = showCodeInDropdown,
                ReadOnly = readOnly,
                VisibleRows = 5
            };

            return html.Partial("~/Views/_RaiDesign/SearchControlTemplate.cshtml", search);
        }

        public static MvcHtmlString RaiAsyncSelectFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string action, int minCharInput = 3, string placeholder = "Seleziona un valore", string searchPlaceholder = "Cerca...", bool expandView = false, string onSelectchange = null, string getParametersFunction = "", object attributes = null, bool hideSearch=false, SelectFilterType tipoFiltro = SelectFilterType.Contains, bool showCodeInDropdown = false, bool readOnly=false, string searchText = "Immetti almeno {0} caratteri per cercare", bool multiple = false)
        {
            string controlId = Guid.NewGuid().ToString();

            Routing.RouteValueDictionary attr = null;
            if (attributes != null)
                attr = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);

            if (attr == null)
                attr = new Routing.RouteValueDictionary();

            object tmp = null;
            if (attr.TryGetValue("data-rai-select", out tmp))
                attr["data-rai-select"] = controlId;
            else
                attr.Add("data-rai-select", controlId);

            if (attr.TryGetValue("style", out tmp))
                attr["style"] = "position:absolute;visibility:hidden;";
            else
                attr.Add("style", "position:absolute;visibility:hidden;");

            object selValue = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData).Model;

            int rowNum = 5;
            if (attr != null && attr.TryGetValue("size", out object size) && Int32.TryParse((string)size, out int tmpRowNum))
                rowNum = tmpRowNum;

            RaiSelectControl search = new RaiSelectControl()
            {
                IsAsync = true,
                ControlID = controlId,
                ControlType = SearchControlType.Select,
                GeneratedControl = htmlHelper.DropDownListFor(expression, new List<SelectListItem>(), attr).ToHtmlString(),
                Placeholder = placeholder,
                SearchPlaceholder = searchPlaceholder,
                ExpandView = expandView,
                UrlAction = action,
                MinCharInput = minCharInput,
                SearchText = searchText,
                SelectedValue = selValue,
                OnSelectChange = onSelectchange,
                GetParameterFunction = getParametersFunction,
                HideSearch = hideSearch,
                TipoFiltro = tipoFiltro,
                ShowCodeInDropdown = showCodeInDropdown,
                ReadOnly = readOnly,
                VisibleRows = rowNum,
                MultiSelect = multiple
            };

            return htmlHelper.Partial("~/Views/_RaiDesign/SearchControlTemplate.cshtml", search);
        }
        #endregion

        #region RenderUploader
        public static MvcHtmlString RaiUploader(this HtmlHelper htmlHelper, string id, UploaderAccept filter=UploaderAccept.None, string customFilter = "", bool multiple = false)
        {
            string accept = "";
            switch (filter)
            {
                case UploaderAccept.None:
                    accept = "";
                    break;
                case UploaderAccept.Image:
                    accept = "image/*";
                    break;
                case UploaderAccept.Video:
                    accept = "video/*";
                    break;
                case UploaderAccept.Audio:
                    accept = "audio/*";
                    break;
                case UploaderAccept.Custom:
                    accept = customFilter;
                    break;
                default:
                    break;
            }
            
            RaiUploader uploader = new RaiUploader()
            {
                Async = false,
                ControlId = Guid.NewGuid().ToString(),
                Id = id,
                Accept = accept,
                Multiple = multiple,
                UploaderType = UploaderType.Classic
            };

            return htmlHelper.Partial("~/Views/_RaiDesign/UploadTemplate.cshtml", uploader);
        }
        public static MvcHtmlString RaiAsyncUploader(this HtmlHelper htmlHelper, string id, string urlUpload, UploaderAccept filter = UploaderAccept.None, string customFilter = "")
        {
            string accept = "";
            switch (filter)
            {
                case UploaderAccept.None:
                    accept = "";
                    break;
                case UploaderAccept.Image:
                    accept = "image/*";
                    break;
                case UploaderAccept.Video:
                    accept = "video/*";
                    break;
                case UploaderAccept.Audio:
                    accept = "audio/*";
                    break;
                case UploaderAccept.Custom:
                    accept = customFilter;
                    break;
                default:
                    break;
            }

            RaiUploader uploader = new RaiUploader()
            {
                Async=true,
                ControlId = Guid.NewGuid().ToString(),
                Id = id,
                Accept = accept,
                Multiple = false,
                UploaderType = UploaderType.Classic,
                Action = urlUpload
            };

            return htmlHelper.Partial("~/Views/_RaiDesign/UploadTemplate.cshtml", uploader);
        }
        #endregion

        public static MvcHtmlString TryAction(this HtmlHelper html, string actionName)
        {
            try
            {
                return html.Action(actionName);
            }
            catch (Exception)
            {
                return new MvcHtmlString("");
            }

        }
    }

    public static class RaiInputExtension
    {
        const string _CHECKBOX_PREFIX = "<div class=\"rai-checkbox\">";
        const string _CHECKBOX_SUFFIX = "<label{0}>{1}</label></div>";

        const string _RADIO_PREFIX = "<div class=\"rai-radio\">";
        const string _RADIO_SUFFIX = "<label{0}>{1}</label></div>";

        private static string AddCheckBoxSuffix(string label = "&nbsp;", string checkBoxName = "", string labelClass = "")
        {
            string otherAttribute = "";
            otherAttribute += !String.IsNullOrWhiteSpace(checkBoxName) ? " for=\"" + checkBoxName + "\"" : "";
            otherAttribute += !String.IsNullOrWhiteSpace(labelClass) ? " class=\"" + labelClass + "\"" : "";

            return String.Format(_CHECKBOX_SUFFIX, otherAttribute, label);
        }

        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name, bool isChecked)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name, isChecked).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name, bool isChecked, object htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name, isChecked, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name, object htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBox(this HtmlHelper htmlHelper, string name, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBox(name, isChecked, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, object htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBoxFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix());
        }
        public static MvcHtmlString RaiCheckBoxLabelFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string label, string labelClass)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression).ToHtmlString() + AddCheckBoxSuffix(label, ExpressionHelper.GetExpressionText(expression), labelClass));
        }
        public static MvcHtmlString RaiCheckBoxLabelFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string label, string labelClass, object htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix(label, ExpressionHelper.GetExpressionText(expression), labelClass));
        }
        public static MvcHtmlString RaiCheckBoxLabelFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, string label, string labelClass, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_CHECKBOX_PREFIX + htmlHelper.CheckBoxFor(expression, htmlAttributes).ToHtmlString() + AddCheckBoxSuffix(label, ExpressionHelper.GetExpressionText(expression), labelClass));
        }


        private static string AddRadioSuffix(string label = "&nbsp;", string radioName = "", string labelClass = "")
        {
            string otherAttribute = "";
            otherAttribute += !String.IsNullOrWhiteSpace(radioName) ? " for=\"" + radioName + "\"" : "";
            otherAttribute += !String.IsNullOrWhiteSpace(labelClass) ? " class=\"" + labelClass + "\"" : "";

            return String.Format(_RADIO_SUFFIX, otherAttribute, label);
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value, isChecked).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked, object htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value, isChecked, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButton(name, value, isChecked, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButtonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButtonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButtonFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value, object htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value, htmlAttributes).ToHtmlString() + AddRadioSuffix());
        }
        public static MvcHtmlString RaiRadioButtonLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value, string label, string labelClass, IDictionary<string, object> htmlAttributes)
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value, htmlAttributes).ToHtmlString() + AddRadioSuffix(label, ExpressionHelper.GetExpressionText(expression).Replace(".", "_"), labelClass));
        }
        public static MvcHtmlString RaiRadioButtonLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value, string label, string labelClass )
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value).ToHtmlString() + AddRadioSuffix(label, ExpressionHelper.GetExpressionText(expression).Replace(".", "_"), labelClass));
        }
        public static MvcHtmlString RaiRadioButtonLabelFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object value, string label, string labelClass, object htmlAttributes )
        {
            return new MvcHtmlString(_RADIO_PREFIX + htmlHelper.RadioButtonFor(expression, value, htmlAttributes).ToHtmlString() + AddRadioSuffix(label, ExpressionHelper.GetExpressionText(expression).Replace(".","_"), labelClass));
        }
    }
}