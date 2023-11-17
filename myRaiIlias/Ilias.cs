using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace myRaiIlias
{
    public enum ResourceType
    {
        All, Scorm, File
    }

    public enum ProgressStatus
    {
        NonIscritto = 0,
        InEsecuzione = 1,
        Completato = 2,
        Fallito = 3,
        NonEseguito = 4
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public class LearningChanges
    {
        public int IdCorso { get; set; }
        public ProgressStatus Stato { get; set; }
        public DateTime Data { get; set; }

        public LearningChanges()
        {

        }

        public LearningChanges(int idCorso, ProgressStatus stato, string timeStamp)
        {
            IdCorso = idCorso;
            Stato = stato;
            Data = DateTime.ParseExact(timeStamp, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    public class CourseResource
    {
        public int IdObj { get; set; }
        public List<LearningProgressInfo> LPInfos { get; set; }
        public decimal Perc { get; set; }
    }

    public class Ilias : IDisposable
    {
        private string _base_url;
        private string _client_id;
        private string _account;

        private string _pwd;

        private const string _SAHS_URL_PATTERN = "{0}/ilias.php?baseClass=ilSAHSPresentationGUI&ref_id={1}";
        private const string _FILE_URL_PATTERN = "{0}/goto.php?target=file_{1}_download&client_id={2}";
        private const string _URL_URL_PATTERN = "{0}/ilias.php?baseClass=ilLinkResourceHandlerGUI&ref_id={1}&cmd=calldirectlink";
        private const string _TST_URL_PATTERN = "{0}/ilias.php?ref_id={1}&cmd=infoScreen&cmdClass=ilobjtestgui&cmdNode=1h:pm&baseClass=ilrepositorygui&ref_id={1}";

        private bool _disposed = false;

        private bool _isLoggedIn;
        private string _sid;
        private ILIASSoapWebservice.ILIASSoapWebservice _ws;
        private string _wsUrl;

        public System.Net.NetworkCredential _credentials;

        private Dictionary<string, int> _userMap;

        public Ilias(string baseUrl, string clientId)
        {
            _isLoggedIn = false;
            _sid = null;
            _ws = null;
            _userMap = new Dictionary<string, int>();

            _base_url = baseUrl;
            _client_id = clientId;

            _wsUrl = _base_url + "/webservice/soap/server.php";
            _credentials = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                Logout();

                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Ilias()
        {
            Dispose(false);
        }

        public T Deserializer<T>(string xmlString)
        {
            if (!String.IsNullOrWhiteSpace(xmlString))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    StringReader reader = new StringReader(xmlString);
                    T retObject = (T)serializer.Deserialize(reader);
                    return retObject;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
            else
                return default(T);
        }

        public string Serializer<T>(T obj)
        {
            if (obj != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                using (Utf8StringWriter textWriter = new Utf8StringWriter())
                {
                    serializer.Serialize(textWriter, obj);
                    return textWriter.ToString();
                }
            }
            else
                return "";
        }

        public void SetCredentials(string account, string pwd, string domain)
        {
            _credentials = new System.Net.NetworkCredential(account, pwd, domain);
            _account = account;
        }

        public bool Login(string account, string pwd)
        {
            //SetCredentials(account, pwd, domain);
            _account = account;
            _pwd = pwd;

            if (!_isLoggedIn)
            {
                _ws = new ILIASSoapWebservice.ILIASSoapWebservice();
                _ws.Url = _wsUrl;
                _ws.Credentials = _credentials;
                //_sid = _ws.loginLDAP("ILIASCOLL", "srvruofpo", "");
                _sid = WrapWS(() => _ws.login(_client_id, _account, pwd));
                _isLoggedIn = !String.IsNullOrWhiteSpace(_sid);
            }

            return _isLoggedIn;
        }

        private void Login()
        {
            Login(_account, _pwd);
            //if (!_isLoggedIn)
            //{
            //    _ws = new ILIASSoapWebservice.ILIASSoapWebservice();
            //    _ws.Url = _wsUrl;
            //    _ws.Credentials = _credentials;
            //    //_sid = _ws.loginLDAP("ILIASCOLL", "srvruofpo", "");
            //    _sid = WrapWS(() => _ws.loginLDAP(_client_id, _account + "@ICT.CORP.RAI.IT", ""));
            //    _isLoggedIn = !String.IsNullOrWhiteSpace(_sid);
            //}
        }

        private T WrapWS<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {

                return default(T);
            }
        }

        public void Logout()
        {
            if (_isLoggedIn)
            {
                bool result = WrapWS(() => _ws.logout(_sid));
                if (result)
                {
                    _isLoggedIn = false;
                    _sid = null;
                }
            }
        }

        private bool GetUserId(string pMatricola, out int userId)
        {
            bool result = false;
            userId = 0;

            if (!_userMap.TryGetValue(pMatricola, out userId))
            {
                userId = WrapWS(() => _ws.lookupUser(_sid, pMatricola + "@ict.corp.rai.it"));
                _userMap.Add(pMatricola, userId);
            }

            result = userId > 0;

            return result;
        }

        public bool GetCoursesForUser(out Dictionary<string, Course> dictCourse)
        {
            int userId = _ws.lookupUser(_sid, "admin");

            result objCaller = new result();
            objCaller.colspecs = new colspec[] { new colspec() { idx = "0", name = "user_id" }, new colspec { idx = "1", name = "status" } };
            objCaller.rows = new string[][] { new string[] { userId.ToString(), "4" } };


            string pippo = WrapWS(() => _ws.getCoursesForUser(_sid, Serializer<result>(objCaller)));

            result objResult = new result();
            //objResult = Deserializer<result>(pippo);
            XmlAttributes xx = new XmlAttributes();
            xx.Xmlns = false;

            XmlAttributeOverrides xover = new XmlAttributeOverrides();
            xover.Add(typeof(result), xx);
            xover.Add(typeof(colspec), xx);

            XmlSerializer serializer = new XmlSerializer(typeof(result), xover);
            StringReader reader = new StringReader(pippo);
            objResult = (result)serializer.Deserialize(reader);

            dictCourse = new Dictionary<string, Course>();
            foreach (var row in objResult.rows)
            {
                string strC = row[1].Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
                Course c = Deserializer<Course>(strC);
                dictCourse.Add(row[0], c);
            }

            return true;
        }

        public bool AssignCourseMember(string courseId, string pMatricola, string roleType = "Member")
        {
            int courseIdInt = Convert.ToInt32(courseId);
            return AssignCourseMember(courseIdInt, pMatricola, roleType);
        }

        public bool AssignCourseMember(int courseId, string pMatricola, string roleType = "Member")
        {
            bool result = false;

            Login();
            if (_isLoggedIn)
            {
                int userId = 0;
                if (GetUserId(pMatricola, out userId))
                    result = WrapWS(() => _ws.assignCourseMember(_sid, courseId, userId, roleType));
            }

            return result;
        }

        public bool IsAssignToCourse(string courseId, string pMatricola, out bool isAssigned)
        {
            int courseIdInt = Convert.ToInt32(courseId);
            return IsAssignToCourse(courseIdInt, pMatricola, out isAssigned);
        }

        public bool IsAssignToCourse(int courseId, string pMatricola, out bool isAssigned)
        {
            bool result = false;
            isAssigned = false;

            Login();
            if (_isLoggedIn)
            {
                int userId = 0;
                if (GetUserId(pMatricola, out userId))
                {
                    result = true;
                    isAssigned = WrapWS(() => _ws.isAssignedToCourse(_sid, courseId, userId) > 0);
                }
            }

            return result;
        }

        public bool GetResource(string courseId, ResourceType resType, string pMatricola, out List<string> resource)
        {
            int courseIdInt = Convert.ToInt32(courseId);
            return GetResource(courseIdInt, resType, pMatricola, out resource);
        }

        public bool GetResource(int courseId, ResourceType resType, string pMatricola, out List<string> resource)
        {
            resource = null;
            bool result = false;

            Login();
            if (_isLoggedIn)
            {
                string[] resArray = null;
                switch (resType)
                {
                    case ResourceType.Scorm:
                        resArray = new string[] { "sahs", "tst" };
                        break;
                    default:
                        resArray = new string[] { };
                        break;
                }

                string objectsString = WrapWS(() => _ws.getTreeChilds(_sid, courseId, resArray, 0));
                Objects objects = Deserializer<Objects>(objectsString);
                if (objects != null && objects.Object != null && objects.Object.Count() > 0)
                {
                    result = true;
                    resource = new List<string>();
                    int counter = 0;
                    foreach (Object item in objects.Object)
                    {
                        if (resType == ResourceType.File && item.type == "tst") continue;

                        if (resType != ResourceType.All && item.type == "sahs")
                        {
                            bool isFile = item.References.Any(y => y.Path.Any(x => x.type == "cat" && x.Value.ToLower() == "materiali e documenti"));
                            if (resType == ResourceType.Scorm && isFile
                                || resType == ResourceType.File && !isFile)
                                continue;
                        }

                        counter++;

                        int ordine = counter;

                        string description = item.Description;
                        string vincoli = "";
                        if (item.type == "sahs" && !String.IsNullOrWhiteSpace(description))
                        {
                            Dictionary<string, string> keyValue = description.Split(new string[] { "|" }, StringSplitOptions.None).Skip(1).ToDictionary(x => x.Split(':')[0], y => y.Split(':')[1]);
                            string tmp = "";
                            if (keyValue.TryGetValue("prereq", out tmp))
                            {
                                vincoli = tmp;
                            }
                        }


                        string resStr = String.Format("{0}|{1}|{2}", ordine, item.Title, GetResourceUrl(item));
                        if (resType == ResourceType.File)
                        {
                            resStr += "|" + item.type;
                        }

                        bool complete = false;
                        bool started = false;
                        if ((resType == ResourceType.Scorm || item.type == "tst") && !String.IsNullOrWhiteSpace(pMatricola))
                        {
                            if (item.type != "itgr")
                            {
                            ProgressStatus status;
                            if (item.References != null && item.References.Count() > 0)
                            {
                                int refId = Convert.ToInt32(item.References.First().ref_id);
                                if (GetProgressInfo(refId, pMatricola, out status))
                                {
                                    complete = status == ProgressStatus.Completato;
                                    started = status == ProgressStatus.InEsecuzione;
                                }
                            }
                            }
                            resStr += "|" + (complete ? "1" : "0");
                            resStr += "|" + (started ? "1" : "0");
                        }

                        if (resType == ResourceType.Scorm)
                        {
                            resStr += "|" + vincoli;
                            resStr += "|" + item.References[0].ref_id;
                            resStr += "|" + item.type;
                        }

                        resource.Add(resStr);
                    }
                }
            }

            return result;
        }

        public bool GetResource(int objId, out Objects resource)
        {
            resource = null;
            bool result = false;

            Login();
            if (_isLoggedIn)
            {
                string[] resArray = new string[] { };

                string objectsString = WrapWS(() => _ws.getTreeChilds(_sid, objId, resArray, 0));
                Objects objects = Deserializer<Objects>(objectsString);
                if (objects != null && objects.Object != null && objects.Object.Count() > 0)
                {
                    result = true;
                    resource = objects;
                }
            }

            return result;
        }

        public bool GetObj(int objId)
        {

            bool result = false;

            Login();
            if (_isLoggedIn)
            {
                string[] resArray = new string[] { };

                var objectsString = WrapWS(() => _ws.getObjectByReference(_sid, objId, 0));
            }

            return result;
        }

        private object GetResourceUrl(Object item)
        {
            string refId = item.References != null && item.References.Count() > 0 ? item.References[0].ref_id : "";

            if (!String.IsNullOrWhiteSpace(refId))
            {
                if (item.type == "sahs")
                    return String.Format(_SAHS_URL_PATTERN, _base_url, refId);
                else if (item.type == "file")
                    return String.Format(_FILE_URL_PATTERN, _base_url, refId, _client_id);
                else if (item.type == "webr")
                    return String.Format(_URL_URL_PATTERN, _base_url, refId);
                else if (item.type == "tst")
                    return String.Format(_TST_URL_PATTERN, _base_url, refId);
            }

            return "";
        }

        public bool GetLatestChange(int courseId, string pMatricola)
        {
            bool result = false;

            //TODO

            return result;
        }

        public bool GetProgressInfo(string objId, string pMatricola, out ProgressStatus status)
        {
            int objIdInt = Convert.ToInt32(objId);
            return GetProgressInfo(objIdInt, pMatricola, out status);
        }

        public bool GetProgressInfo(int objId, string pMatricola, out ProgressStatus status)
        {
            bool result = false;
            status = ProgressStatus.NonIscritto;

            Login();
            if (_isLoggedIn)
            {
                //string PIString = WrapWS(() => _ws.getProgressInfo(_sid, objId, new int[] { 0 }));
                //LearningProgressInfo lp = Deserializer<LearningProgressInfo>(PIString);
                //if (lp != null && lp.UserProgress != null && lp.UserProgress.Count() > 0)
                //{
                //    result = true;

                //    int userId = 0;
                //    if (GetUserId(pMatricola, out userId))
                //    {
                //        LearningProgressInfoUser userInfo = lp.UserProgress.FirstOrDefault(x => x.id == userId);
                //        if (userInfo != null && userInfo.statusSpecified)
                //            status = ConvertStatus(userInfo.status);
                //    }
                //}

                int userId = 0;
                if (GetUserId(pMatricola, out userId))
                {
                    var res = WrapWS(() => _ws.getSCORMCompletionStatus(_sid, userId, objId));
                    switch (res)
                    {
                        case "in_progress":
                            result = true;
                            status = ProgressStatus.InEsecuzione;
                            break;
                        case "completed":
                            result = true;
                            status = ProgressStatus.Completato;
                            break;
                        case "not_attempted":
                            result = true;
                            status = ProgressStatus.NonEseguito;
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        public bool GetAllProgress(string objIdStr, out LearningProgressInfo status)
        {
            bool result = false;
            status = null;

            int objId = Convert.ToInt32(objIdStr);

            Login();
            if (_isLoggedIn)
            {
                string PIString = WrapWS(() => _ws.getProgressInfo(_sid, objId, new int[] { 0 }));
                LearningProgressInfo lp = Deserializer<LearningProgressInfo>(PIString);
                if (lp != null && lp.UserProgress != null && lp.UserProgress.Count() > 0)
                {
                    result = true;
                    status = lp;
                }
            }

            return result;
        }

        public bool GetAllResourceProgress(int objId, out CourseResource courseResource)
        {
            bool result = false;

            courseResource = new CourseResource();
            courseResource.IdObj = objId;
            courseResource.LPInfos = new List<LearningProgressInfo>();

            Login();
            if (_isLoggedIn)
            {
                string objectsString = WrapWS(() => _ws.getTreeChilds(_sid, objId, new string[] { }, 0));
                Objects objects = Deserializer<Objects>(objectsString);
                if (objects != null && objects.Object != null && objects.Object.Count() > 0)
                {
                    foreach (var resource in objects.Object)
                    {
                        LearningProgressInfo lpStatus;
                        if (GetAllProgress(resource.References[0].ref_id, out lpStatus))
                        {
                            courseResource.LPInfos.Add(lpStatus);
                        }
                    }

                    if (courseResource.LPInfos != null && courseResource.LPInfos.Count() > 0)
                        courseResource.Perc = 99 / Convert.ToDecimal(courseResource.LPInfos.Count());


                }
                result = true;
            }

            return result;
        }

        private ProgressStatus ConvertStatus(int intStatus)
        {
            ProgressStatus status = ProgressStatus.NonIscritto;

            switch (intStatus)
            {
                case 1:
                    status = ProgressStatus.InEsecuzione;
                    break;
                case 2:
                    status = ProgressStatus.Completato;
                    break;
                case 3:
                    status = ProgressStatus.Fallito;
                    break;
                case 4:
                    status = ProgressStatus.NonEseguito;
                    break;
                default:
                    break;
            }
            return status;
        }

        public bool GetProgressChanges(string courseId, string pMatricola, DateTime? dateRif, out List<LearningChanges> lpChanges)
        {
            int courseIdInt = Convert.ToInt32(courseId);
            return GetProgressChanges(courseIdInt, pMatricola, dateRif, out lpChanges);
        }

        public bool GetLearningProgressChanges(DateTime dateRif, out LPData lpData)
        {
            bool result = false;
            lpData = null;

            Login();
            if (_isLoggedIn)
            {
                string timeStamp = "";
                timeStamp = dateRif.ToString("yyyy-MM-dd HH:mm:ss");

                string lpDataString = WrapWS(() => _ws.getLearningProgressChanges(_sid, timeStamp, true, new string[] { "crs" }));
                lpData = Deserializer<LPData>(lpDataString);
                result = lpData != null && lpData.LPChange != null && lpData.LPChange.Count() > 0;
            }

            return result;
        }

        public bool GetProgressChanges(int courseId, string pMatricola, DateTime? dateRif, out List<LearningChanges> lpChanges)
        {
            bool result = false;
            lpChanges = null;

            Login();
            if (_isLoggedIn)
            {
                int userId = 0;
                if (GetUserId(pMatricola, out userId))
                {
                    string timeStamp = "";
                    if (dateRif.HasValue)
                        timeStamp = dateRif.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    else
                        timeStamp = DateTime.Now.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss");

                    string lpDataString = WrapWS(() => _ws.getLearningProgressChanges(_sid, timeStamp, true, new string[] { "crs" }));
                    LPData lpData = Deserializer<LPData>(lpDataString);
                    if (lpData != null && lpData.LPChange != null && lpData.LPChange.Count() > 0)
                    {
                        result = true;

                        lpChanges = new List<LearningChanges>();

                        foreach (LPDataLPChange change in lpData.LPChange.Where(x => x.UserId == userId && x.RefIds == courseId))
                            lpChanges.Add(new LearningChanges(courseId, ConvertStatus(change.LPStatus), change.Timestamp));

                        if (lpChanges.Count > 0)
                            lpChanges = lpChanges.OrderBy(x => x.Data).ToList();
                    }
                }
            }

            return result;
        }
    }
}

