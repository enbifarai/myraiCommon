using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace myRaiGestionale
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/assets/css/styles").Include(
                "~/assets/stylesheets/opensans.css",
                 "~/assets/js/plugins/slick/slick.css",
                 "~/assets/js/plugins/slick/slick-theme.css",
                 "~/assets/js/plugins/bootstrap-datetimepicker/bootstrap-datetimepicker.css",
                 "~/assets/js/plugins/bootstrap-datepaginator/css/bootstrap-datepaginator.css",
                 "~/assets/js/plugins/bootstrap-select/bootstrap-select.css",
                 "~/assets/js/plugins/bootstrap-jasny_mask/jasny-bootstrap.min.css",
                 "~/assets/js/plugins/select2/select2.min.css",
                 "~/assets/js/plugins/select2/select2-bootstrap.min.css",
                 "~/assets/css/bootstrap.min.css",
                   "~/assets/stylesheets/skins/default.css",
                 "~/assets/css/UIRai.css",
                 "~/assets/css/myrai.css",
                 "~/assets/js/plugins/sweetalert2/sweetalert2.css",
                 "~/assets/js/plugins/bootstrap-timepicker/css/bootstrap-timepicker.css",
                 "~/assets/js/plugins/daterangepicker-master/daterangepicker.css",
                 "~/assets/js/plugins/jquery.contatore/jquery.contatore.css",
                 "~/assets/css/fileinput.min.css")); //plugin filenput (upload file)


            bundles.Add(new StyleBundle("~/assets/css/stylesPorto").Include(
                 "~/assets/js/plugins/slick/slick.css",
                 "~/assets/js/plugins/slick/slick-theme.css",
                 "~/assets/js/plugins/bootstrap-datetimepicker/bootstrap-datetimepicker.css",
                 "~/assets/js/plugins/bootstrap-datepaginator/css/bootstrap-datepaginator.css",
                 "~/assets/js/plugins/bootstrap-select/bootstrap-select.css",
                 "~/assets/js/plugins/bootstrap-jasny_mask/jasny-bootstrap.min.css",
                 "~/assets/js/plugins/select2/select2.min.css",
                 "~/assets/js/plugins/select2/select2-bootstrap.min.css",
                 "~/assets/js/plugins/jquery.contatore/jquery.contatore.css",
                 "~/assets/css/bootstrap.min.css",
                    "~/assets/stylesheets/opensans.css",
                    "~/assets/vendor/font-awesome/css/font-awesome.css",
                    "~/assets/vendor/elusive/elusive-icons.css",
                    "~/assets/vendor/simple-line-icons/css/simple-line-icons.css",
                    "~/assets/vendor/magnific-popup/magnific-popup.css",
                    "~/assets/stylesheets/theme.css",
                   "~/assets/vendor/chartist/chartist.css",
                    "~/assets/stylesheets/skins/default.css",
                    "~/assets/stylesheets/skins/extstyle.css",
                    "~/assets/css/myrai.css",
                    "~/assets/vendor/bootstrap-tour/css/bootstrap-tour.css",
                    "~/assets/js/plugins/sweetalert2/sweetalert2.css",
                    "~/assets/vendor/jquery-skeleton-loader/jquery.scheletrone.css",
                    "~/assets/vendor/summernote/summernote.css",
                    "~/assets/js/plugins/intro/introjs.css",
                    "~/assets/js/plugins/jquery-auto-complete/jquery.auto-complete.min.css",
                     "~/assets/js/plugins/bootstrap-timepicker/css/bootstrap-timepicker.css",
                    "~/assets/css/fileinput.min.css",  //plugin filenput (upload file)
                    "~/assets/js/plugins/daterangepicker-master/daterangepicker.css",
                    "~/assets/vendor/themify-icons/themify-icons.css"));


            ScriptBundle sc = new ScriptBundle("~/assets/javascript");

            sc.Include("~/assets/js/core/jquery.min.js");
            sc.Include("~/assets/js/core/bootstrap.min.js");
            sc.Include("~/assets/js/core/jquery.slimscroll.min.js");
            sc.Include("~/assets/js/core/jquery.scrollLock.min.js");
            sc.Include("~/assets/js/core/jquery.appear.min.js");
            sc.Include("~/assets/js/core/jquery.countTo.min.js");
            sc.Include("~/assets/js/core/jquery.placeholder.min.js");
            sc.Include("~/assets/js/core/moment.js");

            sc.Include("~/assets/js/plugins/jquery-ui/jquery-uiNEW.js");

            sc.Include("~/assets/js/plugins/sweetalert2/es6-promise.auto.min.js");
            sc.Include("~/assets/js/plugins/sweetalert2/sweetalert2.js");
            sc.Include("~/assets/js/plugins/bootstrap-datetimepicker/bootstrap-datetimepicker.js");
            sc.Include("~/assets/js/plugins/bootstrap-timepicker/bootstrap-timepicker.js");
            sc.Include("~/assets/js/plugins/bootstrap-datepaginator/js/bootstrap-datepaginator.js");
            sc.Include("~/assets/js/plugins/daterangepicker-master/daterangepicker.js");
            sc.Include("~/assets/js/plugins/bootstrap-select/bootstrap-select.js");
            sc.Include("~/assets/js/plugins/bootstrap-jasny_mask/jasny-bootstrap.min.js");
            sc.Include("~/assets/js/plugins/masked-inputs/jquery.maskedinput.js");
            sc.Include("~/assets/js/plugins/jquery.contatore/jquery.contatore.js");
            
            sc.Include("~/assets/js/core/JSRai.js");
            sc.Include("~/assets/js/myrai.js");
            //sc.Include("~/assets/js/cv_myrai.js");

            sc.Include("~/assets/js/plugins/bootstrap-wizard/jquery.bootstrap.wizard.min.js");
            sc.Include("~/assets/js/pages/form_wizard.js");
            sc.Include("~/assets/js/plugins/select2/select2.full.min.js");
            sc.Include("~/assets/js/plugins/jquery-validation/jquery.validate.min.js");
            sc.Include("~/assets/js/plugins/jquery-validation/jquery.validate.unobtrusive.min.js");
            sc.Include("~/assets/js/plugins/jquery-validation/additional-methods.min.js");
            sc.Include("~/assets/js/plugins/chartJS/chart.bundle.min.js");
            sc.Include("~/assets/js/plugins/fileinput/piexif.min.js"); //plugin filenput (upload file) 
            sc.Include("~/assets/js/plugins/fileinput/sortable.min.js"); //plugin filenput (upload file) 
            sc.Include("~/assets/js/plugins/fileinput/purify.min.js"); //plugin filenput (upload file)

            sc.Include("~/assets/js/fileinput.min.js"); //plugin filenput (upload file)
            sc.Include("~/assets/js/fa.js"); //plugin filenput (upload file)
            sc.Include("~/assets/js/locales/it.js"); //plugin filenput (upload file)
            sc.Include("~/assets/js/plugins/JsonViewer/jquery.json-editor.min.js");
            sc.Orderer = new NonOrderingBundleOrderer();
            bundles.Add(sc);



            ScriptBundle sP = new ScriptBundle("~/assets/javascriptPorto");

            sP.Include("~/assets/vendor/jquery/jquery.js");
            sP.Include("~/assets/vendor/jquery-browser-mobile/jquery.browser.mobile.js");
            sP.Include("~/assets/vendor/bootstrap/js/bootstrap.js");
            sP.Include("~/assets/vendor/nanoscroller/nanoscroller.js");
            sP.Include("~/assets/vendor/bootstrap-datepicker/js/bootstrap-datepicker.js");
            sP.Include("~/assets/vendor/magnific-popup/magnific-popup.js");
            sP.Include("~/assets/vendor/jquery-placeholder/jquery.placeholder.js");
            sP.Include("~/assets/js/core/jquery.slimscroll.min.js");
            sP.Include("~/assets/js/core/jquery.scrollLock.min.js");
            sP.Include("~/assets/js/core/jquery.appear.min.js");
            sP.Include("~/assets/js/core/jquery.countTo.min.js");
            sP.Include("~/assets/js/core/jquery.placeholder.min.js");
            sP.Include("~/assets/js/core/moment.js");
            sP.Include("~/assets/vendor/modernizr/modernizr.js");

            sP.Include("~/assets/js/plugins/jquery-ui/jquery-uiNEW.js");

            sP.Include("~/assets/js/plugins/sweetalert2/es6-promise.auto.min.js");
            sP.Include("~/assets/js/plugins/sweetalert2/sweetalert2.js");
            sP.Include("~/assets/js/plugins/bootstrap-datetimepicker/bootstrap-datetimepicker.js");
            sP.Include("~/assets/js/plugins/bootstrap-timepicker/bootstrap-timepicker.js");
            sP.Include("~/assets/js/plugins/daterangepicker-master/daterangepicker.js");
            sP.Include("~/assets/js/plugins/bootstrap-datepaginator/js/bootstrap-datepaginator.js");
            sP.Include("~/assets/js/plugins/jquery.contatore/jquery.contatore.js");
            sP.Include("~/assets/js/plugins/bootstrap-select/bootstrap-select.js");
            sP.Include("~/assets/js/plugins/bootstrap-jasny_mask/jasny-bootstrap.min.js");
            sP.Include("~/assets/js/plugins/masked-inputs/jquery.maskedinput.js");

            sP.Include("~/assets/js/plugins/bootstrap-wizard/jquery.bootstrap.wizard.min.js");
            sP.Include("~/assets/js/pages/form_wizard.js");
            sP.Include("~/assets/js/core/JSRai.js");
            sP.Include("~/assets/js/myrai.js");
            sP.Include("~/assets/js/myrai2.js");
            sP.Include("~/assets/js/myrai3.js");
            sP.Include("~/assets/js/myraiV2.js");
            sP.Include("~/assets/js/myraiPlace.js");
            sP.Include("~/assets/js/myRaiAnagrafica.js");
            sP.Include("~/assets/js/myHris.js");
            //sP.Include("~/assets/js/cv_myrai.js");
            sP.Include("~/assets/js/myRaiIncentivi.js");
            sP.Include("~/assets/js/myRaiValutazioni.js");
            //sP.Include("~/assets/js/RaiAcademy.js");
            sP.Include("~/assets/js/jquery.easypiechart.js");
            sP.Include("~/assets/vendor/chartist/chartist.js");
            sP.Include("~/assets/js/plugins/select2/select2.full.js");
            sP.Include("~/assets/javascripts/theme.js");
            sP.Include("~/assets/javascripts/theme.custom.js");
            sP.Include("~/assets/javascripts/theme.init.js");
            sP.Include("~/assets/js/plugins/chartJS/chart.bundle.min.js");
            sP.Include("~/assets/js/plugins/cryptojs/components/core.js");
            sP.Include("~/assets/js/plugins/cryptojs/rollups/aes.js");
            sP.Include("~/assets/js/plugins/cryptojs/rollups/sha1.js");
            sP.Include("~/assets/vendor/bootstrap-tour/js/bootstrap-tour.js");
            sP.Include("~/assets/js/plugins/fileinput/piexif.min.js"); //plugin filenput (upload file) 
            sP.Include("~/assets/js/plugins/fileinput/sortable.min.js"); //plugin filenput (upload file) 
            sP.Include("~/assets/js/plugins/fileinput/purify.min.js"); //plugin filenput (upload file)
            sP.Include("~/assets/js/fileinput.min.js"); //plugin filenput (upload file)
            sP.Include("~/assets/js/fa.js"); //plugin filenput (upload file)
            sP.Include("~/assets/js/locales/it.js"); //plugin filenput (upload file)
            sP.Include("~/assets/js/plugins/porto-modal/modal_porto.js");//plugin per il modal in porto admin
            sP.Include("~/assets/vendor/jquery-skeleton-loader/jquery.scheletrone.js");
            sP.Include("~/assets/vendor/summernote/summernote.js");
            sP.Include("~/assets/js/plugins/jquery-auto-complete/jquery.auto-complete.min.js");

            sP.Include("~/assets/js/plugins/jquery-validation/jquery.validate.js");
            sP.Include("~/assets/js/plugins/jquery-validation/jquery.validate.unobtrusive.js");
            sP.Include("~/assets/js/plugins/jquery-validation/additional-methods.min.js");
            sP.Include("~/assets/js/plugins/flot/jquery.flot.js");
            sP.Include("~/assets/js/plugins/flot/jquery.flot.pie.js");
            sP.Include("~/assets/js/plugins/intro/intro.js");
            sP.Include("~/assets/js/plugins/JsonViewer/jquery.json-editor.min.js");

            sP.Include("~/assets/js/myRaiDematerializzazione.js");
            sP.Include("~/assets/js/myRaiDeleghe.js");

            // CF 30/06/2022 --> Import file js con funzioni per gestione della scelta destinazione TFR
            sP.Include("~/assets/js/myRaiSceltaDestinazioneTFR.js");
            // <--

            sP.Orderer = new NonOrderingBundleOrderer();

            bundles.Add(sP);
        }
    }
    class NonOrderingBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }
}